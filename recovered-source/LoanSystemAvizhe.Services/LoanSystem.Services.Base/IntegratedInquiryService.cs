using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LoanSystem.Core;
using LoanSystem.Core.Configuration;
using LoanSystem.Core.Exceptions;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Models;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Data.Enums;
using LoanSystemAvizhe.Services.Inquiry.FinnoTech;
using LoanSystemAvizhe.Services.Inquiry.Models;
using LoanSystemAvizhe.Services.Inquiry.Models.ApiIR;
using LoanSystemAvizhe.Services.Inquiry.Models.FinnoTech;
using LoanSystemAvizhe.Services.Models.Shahin;
using LoanSystemAvizhe.Services.Sms;
using LoanSystemAvizhe.Services.Sms.KaveNegar;
using LoanSystemAvizhe.Services.Sms.Models.IPPanel;
using LoanSystemAvizhe.Services.Sms.Models.KaveNegar;
using LoanSystemAvizhe.Services.Sms.Models.MelliPayamak;
using LoanSystemAvizhe.Services.Sms.Models.WebOne;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NikanBase.Services.Security;

namespace LoanSystem.Services.Base;

public class IntegratedInquiryService
{
	private record BankAccount(string AccountNumber, string Bank, string Branch);

	private readonly ICustomerService _customerService;

	private readonly ICurrentUserService _currentUserService;

	private readonly IAppSettingsService _appSettingsService;

	private readonly IAppInquiryService _appInquiryService;

	private readonly IInquirySettingsService _inquirySettingsService;

	private readonly AppConfig _appConfig;

	private readonly ApiIRConfig _apiIRConfig;

	private readonly FinnoTechConfig _finnoTechConfig;

	private readonly ShahinConfig _shahinConfig;

	private readonly IHttpContextAccessor _httpContextAccessor;

	private readonly IHostingEnvironment _hostingEnvironment;

	private readonly IServiceProvider _serviceProvider;

	private readonly IMemberService _memberService;

	private readonly IEncryptionService _encryptionService;

	private readonly ILogger<IntegratedInquiryService> _logger;

	private readonly ILogger<ShahinInquiryService> _shahinLogger;

	private IInquiryService _inquiryService;

	private string _contentRootPath;

	public IntegratedInquiryService(ICurrentUserService currentUserService, ILogger<IntegratedInquiryService> logger, ILogger<ShahinInquiryService> shahinLogger, IAppInquiryService appInquiryService, IOptions<AppConfig> appConfig, IAppSettingsService appSettingsService, IOptions<ApiIRConfig> apiIRConfig, IOptions<KaveNegarConfig> kaveNegarConfig, IOptions<IPPanelConfig> ippanelConfig, IOptions<WebOneConfig> webOneConfig, IOptions<FinnoTechConfig> finnoTechConfig, IOptions<ShahinConfig> shahinConfig, ICustomerService customerService, IHttpContextAccessor httpContextAccessor, IHostingEnvironment hostingEnvironment, IEncryptionService encryptionService, IServiceProvider serviceProvider, IInquirySettingsService inquirySettingsService, IMemberService memberService)
	{
		_currentUserService = currentUserService;
		_appInquiryService = appInquiryService;
		_appConfig = appConfig.Value;
		_appSettingsService = appSettingsService;
		_finnoTechConfig = finnoTechConfig.Value;
		_apiIRConfig = apiIRConfig.Value;
		_shahinConfig = shahinConfig.Value;
		_customerService = customerService;
		_httpContextAccessor = httpContextAccessor;
		_hostingEnvironment = hostingEnvironment;
		_serviceProvider = serviceProvider;
		_memberService = memberService;
		_encryptionService = encryptionService;
		_inquirySettingsService = inquirySettingsService;
		_logger = logger;
		_shahinLogger = shahinLogger;
	}

	public async Task<decimal> GetCredit()
	{
		if (_hostingEnvironment.EnvironmentName == "Development")
		{
			return 111111111m;
		}
		decimal credit = 0m;
		AppSettings appSettings = await _appSettingsService.GetAppSettings();
		if (appSettings == null)
		{
			return credit;
		}
		if (appSettings.DefaultSmsPanel == SmsPanel.JetAqsaat)
		{
			return appSettings.SmsCredit;
		}
		await SetInquiryService(appSettings);
		if (_inquiryService == null)
		{
			return credit;
		}
		ResponseInquiry<CreditResult> credit2 = _inquiryService.GetCredit();
		if (credit2.Success)
		{
			decimal.TryParse(credit2?.Key ?? "0", out credit);
		}
		return credit;
	}

	public async Task<AppInquiry> InquiryRequest(InquiryRequestModel model)
	{
		AppInquiry appInquiry = null;
		MemberModel memberModel = null;
		if (model.MemberId != 0L)
		{
			memberModel = new MemberModel((await _memberService.GetById(model.MemberId)) ?? throw new CustomException("مشتری یافت نشد."));
		}
		switch (model.InquiryType)
		{
		case AppInquiryType.شاهکار:
			appInquiry = await Shahkar(new ShahkarModel
			{
				Member = memberModel
			}, model.InquiryType);
			break;
		case AppInquiryType.شاهکار3:
		{
			if (string.IsNullOrWhiteSpace(memberModel.BankAccountsJson))
			{
				throw new CustomException("شماره شبا مشتری یافت نشد.(در نام بانک یا شعبه واژه شبا درج نشده است)");
			}
			BankAccount bankAccount = JsonSerializer.Deserialize<List<BankAccount>>(memberModel.BankAccountsJson, new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true
			})?.FirstOrDefault((BankAccount x) => x.Bank.Contains("شبا") || x.Branch.Contains("شبا"));
			if (bankAccount == null)
			{
				throw new CustomException("شماره شبا مشتری یافت نشد.(در نام بانک یا شعبه واژه شبا درج نشده است)");
			}
			appInquiry = await Shahkar3(new ShahkarModel
			{
				Iban = bankAccount.AccountNumber,
				Member = memberModel
			}, model.InquiryType);
			break;
		}
		case AppInquiryType.ثنا:
			appInquiry = await Sana(new SanaModel
			{
				Member = memberModel
			}, model.InquiryType);
			break;
		case AppInquiryType.چک_برگشتی:
			appInquiry = await UnpaidCheque(new UnpaidChequeModel
			{
				Member = memberModel
			}, model.InquiryType);
			break;
		case AppInquiryType.مشخصات_هویتی:
			appInquiry = await PersonInfo(new PersonInfoModel
			{
				Member = memberModel
			}, model.InquiryType);
			break;
		case AppInquiryType.مشخصات_شبا:
		{
			if (string.IsNullOrWhiteSpace(memberModel.BankAccountsJson))
			{
				throw new CustomException("شماره شبا مشتری یافت نشد.(در نام بانک یا شعبه واژه شبا درج نشده است)");
			}
			BankAccount bankAccount = JsonSerializer.Deserialize<List<BankAccount>>(memberModel.BankAccountsJson, new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true
			})?.FirstOrDefault((BankAccount x) => x.Bank.Contains("شبا") || x.Branch.Contains("شبا"));
			if (bankAccount == null)
			{
				throw new CustomException("شماره شبا مشتری یافت نشد.(در نام بانک یا شعبه واژه شبا درج نشده است)");
			}
			appInquiry = await IbanInfo(new IbanInfoModel
			{
				Iban = bankAccount.AccountNumber,
				MemberId = memberModel.Id
			}, model.InquiryType);
			break;
		}
		case AppInquiryType.مشخصات_کارت_بانکی:
		{
			if (string.IsNullOrWhiteSpace(memberModel.BankAccountsJson))
			{
				throw new CustomException("شماره کارت مشتری یافت نشد.(در نام بانک یا شعبه واژه کارت درج نشده است)");
			}
			BankAccount bankAccount = JsonSerializer.Deserialize<List<BankAccount>>(memberModel.BankAccountsJson, new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true
			})?.FirstOrDefault((BankAccount x) => x.Bank.Contains("کارت") || x.Branch.Contains("کارت"));
			if (bankAccount == null)
			{
				throw new CustomException("شماره کارت مشتری یافت نشد.(در نام بانک یا شعبه واژه کارت درج نشده است)");
			}
			appInquiry = await BankCardInfo(new BankCardInfoModel
			{
				CardNumber = bankAccount.AccountNumber,
				MemberId = memberModel.Id
			}, model.InquiryType);
			break;
		}
		case AppInquiryType.کدپستی:
			if (string.IsNullOrWhiteSpace(memberModel.PostalCode) || memberModel.PostalCode.Length != 10)
			{
				throw new CustomException("کد پستی صحیح نمی باشد.");
			}
			appInquiry = await PostalAddress(new PostalAddressModel
			{
				Member = memberModel
			}, model.InquiryType);
			break;
		case AppInquiryType.پلاک_فعال:
			appInquiry = await ActivePlates(new ActivePlatesModel
			{
				Member = memberModel
			}, model.InquiryType);
			break;
		case AppInquiryType.مطابقت_کدملی_کارت_بانکی:
		{
			if (string.IsNullOrWhiteSpace(memberModel.BankAccountsJson))
			{
				throw new CustomException("شماره کارت مشتری یافت نشد.(در نام بانک یا شعبه واژه کارت بانکی درج نشده است)");
			}
			BankAccount bankAccount = JsonSerializer.Deserialize<List<BankAccount>>(memberModel.BankAccountsJson, new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true
			})?.FirstOrDefault((BankAccount x) => x.Bank.Contains("کارت") || x.Branch.Contains("کارت"));
			if (bankAccount == null)
			{
				throw new CustomException("شماره کارت بانکی مشتری یافت نشد.(در نام بانک یا شعبه واژه کارت درج نشده است)");
			}
			appInquiry = await CardMatch(new CardMatchModel
			{
				CardNumber = bankAccount.AccountNumber,
				Member = memberModel
			}, model.InquiryType);
			break;
		}
		case AppInquiryType.مطابقت_کدملی_شبا:
		{
			if (string.IsNullOrWhiteSpace(memberModel.BankAccountsJson))
			{
				throw new CustomException("شماره شبا مشتری یافت نشد.(در نام بانک یا شعبه واژه شبا درج نشده است)");
			}
			BankAccount bankAccount = JsonSerializer.Deserialize<List<BankAccount>>(memberModel.BankAccountsJson, new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true
			})?.FirstOrDefault((BankAccount x) => x.Bank.Contains("شبا") || x.Branch.Contains("شبا"));
			if (bankAccount == null)
			{
				throw new CustomException("شماره شبا مشتری یافت نشد.(در نام بانک یا شعبه شبا درج نشده است)");
			}
			appInquiry = await IbanMatch(new IbanMatchModel
			{
				Iban = bankAccount.AccountNumber,
				Member = memberModel
			}, model.InquiryType);
			break;
		}
		case AppInquiryType.رنگ_چک_صیادی:
			appInquiry = await ChequeColor(new ChequeColorModel
			{
				Member = memberModel
			}, model.InquiryType);
			break;
		case AppInquiryType.تسیلات_فعال_بانکی:
			appInquiry = await ActiveLoans(new ActiveLoansModel
			{
				Member = memberModel
			}, model.InquiryType);
			break;
		case AppInquiryType.خدمت_سربازی:
			appInquiry = await MilitaryStatus(new MilitaryStatusModel
			{
				Member = memberModel
			}, model.InquiryType);
			break;
		}
		return appInquiry;
	}

	public async Task<AppInquiry> Shahkar(ShahkarModel model, AppInquiryType inquiryType)
	{
		if (_hostingEnvironment.EnvironmentName == "Development")
		{
			return null;
		}
		if (model.Member == null)
		{
			throw new CustomException("اطلاعات مشتری جهت استعلام کامل نمی باشد.");
		}
		AppSettings appSettings = await _appSettingsService.GetAppSettings();
		InquiryPanel inquiryPanel = await SetInquiryService(appSettings);
		if (_inquiryService == null)
		{
			throw new CustomException("سرویس استعلام یافت نشد");
		}
		decimal num = await GetCostByInquiryType(AppInquiryType.شاهکار, inquiryPanel);
		if ((decimal)appSettings.InquiryCredit < num && _appConfig.IsCloud)
		{
			throw new CustomException("میزان اعتبار استعلام کافی نمی باشد.");
		}
		ResponseInquiry<ShahkarResult> responseInquiry = new ResponseInquiry<ShahkarResult>();
		switch (inquiryPanel)
		{
		case InquiryPanel.JetAqsaat_ApiIR:
		case InquiryPanel.Dedicate_ApiIR:
		{
			BaseSendInquiry baseSendInquiry = new SendApiIR_Shahkar
			{
				BirthDate = model.Member.BirthDate.ToPersianDate(),
				Iban = model.Iban,
				Mobile = model.Member.Mobile,
				NationalCode = model.Member.NationalCode
			};
			responseInquiry = ((!(_hostingEnvironment.EnvironmentName == "Development")) ? _inquiryService.Shahkar(baseSendInquiry as SendApiIR_Shahkar) : new ResponseInquiry<ShahkarResult>
			{
				Key = "",
				Message = "",
				Response = new ShahkarResult
				{
					IsMatch = true
				},
				Success = true
			});
			break;
		}
		case InquiryPanel.JetAqsaat_FinnoTech:
		case InquiryPanel.Dedicate_FinnoTech:
		{
			BaseSendInquiry baseSendInquiry = new SendFinnoTech_Shahkar
			{
				TrackId = Guid.NewGuid().ToString(),
				Mobile = model.Member.Mobile,
				NationalCode = model.Member.NationalCode
			};
			responseInquiry = _inquiryService.Shahkar(baseSendInquiry as SendFinnoTech_Shahkar);
			break;
		}
		}
		AppInquiry appInquiry = new AppInquiry
		{
			AppInquiryType = AppInquiryType.شاهکار,
			Date = DateTime.Now,
			Price = num,
			InquiryPanel = inquiryPanel,
			Key = model.Member.Mobile + "," + model.Member.NationalCode,
			MemberId = ((model.Member.Id != 0L) ? new long?(model.Member.Id) : ((long?)null)),
			IsSuccess = responseInquiry.Success,
			PanelResultJson = JsonSerializer.Serialize(responseInquiry.Response, new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			}),
			Result = ((!responseInquiry.Success) ? responseInquiry.Message : (responseInquiry.Response.IsMatch ? "تطابق دارد" : "عدم تطابق"))
		};
		await _appInquiryService.Insert(appInquiry);
		if (appInquiry.IsSuccess)
		{
			await _appSettingsService.SyncChargeInquiryCredit();
		}
		return appInquiry;
	}

	public async Task<AppInquiry> Shahkar3(ShahkarModel model, AppInquiryType inquiryType)
	{
		if (_hostingEnvironment.EnvironmentName == "Development")
		{
			return null;
		}
		if (model.Member == null)
		{
			throw new CustomException("اطلاعات مشتری جهت استعلام کامل نمی باشد.");
		}
		AppSettings appSettings = await _appSettingsService.GetAppSettings();
		InquiryPanel inquiryPanel = await SetInquiryService(appSettings);
		if (_inquiryService == null)
		{
			throw new CustomException("سرویس استعلام یافت نشد");
		}
		decimal num = await GetCostByInquiryType(AppInquiryType.شاهکار3, inquiryPanel);
		if ((decimal)appSettings.InquiryCredit < num && _appConfig.IsCloud)
		{
			throw new CustomException("میزان اعتبار استعلام کافی نمی باشد.");
		}
		ResponseInquiry<ShahkarResult> responseInquiry = new ResponseInquiry<ShahkarResult>();
		if ((uint)(inquiryPanel - 1) <= 1u)
		{
			BaseSendInquiry baseSendInquiry = new SendApiIR_Shahkar
			{
				BirthDate = model.Member.BirthDate.ToPersianDate(),
				Iban = model.Iban,
				Mobile = model.Member.Mobile,
				NationalCode = model.Member.NationalCode
			};
			responseInquiry = _inquiryService.Shahkar3(baseSendInquiry as SendApiIR_Shahkar);
		}
		AppInquiry appInquiry = new AppInquiry
		{
			AppInquiryType = AppInquiryType.شاهکار3,
			Date = DateTime.Now,
			Price = num,
			InquiryPanel = inquiryPanel,
			Key = model.Member.Mobile + "," + model.Member.NationalCode,
			MemberId = ((model.Member.Id != 0L) ? new long?(model.Member.Id) : ((long?)null)),
			IsSuccess = responseInquiry.Success,
			PanelResultJson = JsonSerializer.Serialize(responseInquiry.Response, new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			}),
			Result = ((!responseInquiry.Success) ? responseInquiry.Message : (responseInquiry.Response.IsMatch ? "تطابق دارد" : "عدم تطابق"))
		};
		await _appInquiryService.Insert(appInquiry);
		if (appInquiry.IsSuccess)
		{
			await _appSettingsService.SyncChargeInquiryCredit();
		}
		return appInquiry;
	}

	public async Task<AppInquiry> Sana(SanaModel model, AppInquiryType inquiryType)
	{
		if (_hostingEnvironment.EnvironmentName == "Development")
		{
			return null;
		}
		if (model.Member == null)
		{
			throw new CustomException("اطلاعات مشتری جهت استعلام کامل نمی باشد.");
		}
		AppSettings appSettings = await _appSettingsService.GetAppSettings();
		InquiryPanel inquiryPanel = await SetInquiryService(appSettings);
		if (_inquiryService == null)
		{
			throw new CustomException("سرویس استعلام یافت نشد");
		}
		decimal num = await GetCostByInquiryType(AppInquiryType.ثنا, inquiryPanel);
		if ((decimal)appSettings.InquiryCredit < num && _appConfig.IsCloud)
		{
			throw new CustomException("میزان اعتبار استعلام کافی نمی باشد.");
		}
		ResponseInquiry<SanaResult> responseInquiry = new ResponseInquiry<SanaResult>();
		if ((uint)(inquiryPanel - 1) <= 1u)
		{
			BaseSendInquiry baseSendInquiry = new SendApiIR_Sana
			{
				NationalCode = model.Member.NationalCode,
				IsCompany = model.IsCompany
			};
			responseInquiry = _inquiryService.Sana(baseSendInquiry as SendApiIR_Sana);
		}
		AppInquiry appInquiry = new AppInquiry
		{
			AppInquiryType = AppInquiryType.ثنا,
			Date = DateTime.Now,
			Price = num,
			InquiryPanel = inquiryPanel,
			Key = (model.Member.NationalCode ?? ""),
			MemberId = ((model.Member.Id != 0L) ? new long?(model.Member.Id) : ((long?)null)),
			IsSuccess = responseInquiry.Success,
			PanelResultJson = JsonSerializer.Serialize(responseInquiry.Response, new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			}),
			Result = ((!responseInquiry.Success) ? responseInquiry.Message : (responseInquiry.Response.HasSana ? "ثنا دارد" : "عدم ثنا"))
		};
		await _appInquiryService.Insert(appInquiry);
		if (appInquiry.IsSuccess)
		{
			await _appSettingsService.SyncChargeInquiryCredit();
		}
		return appInquiry;
	}

	public async Task<AppInquiry> UnpaidCheque(UnpaidChequeModel model, AppInquiryType inquiryType)
	{
		if (_hostingEnvironment.EnvironmentName == "Development")
		{
			return null;
		}
		if (model.Member == null)
		{
			throw new CustomException("اطلاعات مشتری جهت استعلام کامل نمی باشد.");
		}
		AppSettings appSettings = await _appSettingsService.GetAppSettings();
		InquiryPanel inquiryPanel = await SetInquiryService(appSettings);
		if (_inquiryService == null)
		{
			throw new CustomException("سرویس استعلام یافت نشد");
		}
		decimal num = await GetCostByInquiryType(inquiryType, inquiryPanel);
		if ((decimal)appSettings.InquiryCredit < num && _appConfig.IsCloud)
		{
			throw new CustomException("میزان اعتبار استعلام کافی نمی باشد.");
		}
		ResponseInquiry<UnpaidChequeResult> responseInquiry = new ResponseInquiry<UnpaidChequeResult>();
		if ((uint)(inquiryPanel - 1) <= 1u)
		{
			BaseSendInquiry baseSendInquiry = new SendApiIR_UnpaidCheque
			{
				NationalCode = model.Member.NationalCode
			};
			responseInquiry = _inquiryService.UnpaidCheque(baseSendInquiry as SendApiIR_UnpaidCheque);
		}
		AppInquiry appInquiry = new AppInquiry
		{
			AppInquiryType = inquiryType,
			Date = DateTime.Now,
			Price = num,
			InquiryPanel = inquiryPanel,
			Key = (model.Member.NationalCode ?? ""),
			MemberId = ((model.Member.Id != 0L) ? new long?(model.Member.Id) : ((long?)null)),
			IsSuccess = responseInquiry.Success,
			PanelResultJson = JsonSerializer.Serialize(responseInquiry.Response, new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			}),
			Result = ((!responseInquiry.Success) ? responseInquiry.Message : ((responseInquiry.Response.Count == 0) ? "چک برگشتی ندارد" : $"تعداد {responseInquiry.Response.Count} چک برگشتی دارد."))
		};
		await _appInquiryService.Insert(appInquiry);
		if (appInquiry.IsSuccess)
		{
			await _appSettingsService.SyncChargeInquiryCredit();
		}
		return appInquiry;
	}

	public async Task<AppInquiry> PersonInfo(PersonInfoModel model, AppInquiryType inquiryType)
	{
		if (_hostingEnvironment.EnvironmentName == "Development")
		{
			return null;
		}
		if (model.Member == null)
		{
			throw new CustomException("اطلاعات مشتری جهت استعلام کامل نمی باشد.");
		}
		AppSettings appSettings = await _appSettingsService.GetAppSettings();
		InquiryPanel inquiryPanel = await SetInquiryService(appSettings);
		if (_inquiryService == null)
		{
			throw new CustomException("سرویس استعلام یافت نشد");
		}
		decimal num = await GetCostByInquiryType(inquiryType, inquiryPanel);
		if ((decimal)appSettings.InquiryCredit < num && _appConfig.IsCloud)
		{
			throw new CustomException("میزان اعتبار استعلام کافی نمی باشد.");
		}
		ResponseInquiry<PersonInfoResult> responseInquiry = new ResponseInquiry<PersonInfoResult>();
		if ((uint)(inquiryPanel - 1) <= 1u)
		{
			BaseSendInquiry baseSendInquiry = new SendApiIR_PersonInfo
			{
				NationalCode = model.Member.NationalCode,
				BirthDate = model.Member.BirthDate.ToPersianDate()
			};
			responseInquiry = ((!(_hostingEnvironment.EnvironmentName == "Development")) ? _inquiryService.PersonInfo(baseSendInquiry as SendApiIR_PersonInfo) : new ResponseInquiry<PersonInfoResult>
			{
				Key = "",
				Message = "",
				Response = new PersonInfoResult
				{
					Alive = true,
					FatherName = "محمد",
					FirstName = "ماهان",
					Gender = 1,
					LastName = "اصغری",
					NationalCode = "23032015165"
				},
				Success = true
			});
		}
		AppInquiry appInquiry = new AppInquiry
		{
			AppInquiryType = inquiryType,
			Date = DateTime.Now,
			Price = num,
			InquiryPanel = inquiryPanel,
			Key = model.Member.NationalCode + "," + model.Member.BirthDate.ToPersianDate(),
			MemberId = ((model.Member.Id != 0L) ? new long?(model.Member.Id) : ((long?)null)),
			IsSuccess = responseInquiry.Success,
			PanelResultJson = JsonSerializer.Serialize(responseInquiry.Response, new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			}),
			Result = ((!responseInquiry.Success) ? responseInquiry.Message : ("نام: " + responseInquiry.Response.FirstName + ", نام خانوادگی: " + responseInquiry.Response.LastName + ", نام پدر: " + responseInquiry.Response.FatherName + ", قید حیات: " + (responseInquiry.Response.Alive ? "بلی" : "خیر") + ", جنسیت: " + ((responseInquiry.Response.Gender == 1) ? "مرد" : "زن")))
		};
		await _appInquiryService.Insert(appInquiry);
		if (appInquiry.IsSuccess)
		{
			await _appSettingsService.SyncChargeInquiryCredit();
		}
		return appInquiry;
	}

	public async Task<AppInquiry> IbanInfo(IbanInfoModel model, AppInquiryType inquiryType)
	{
		if (_hostingEnvironment.EnvironmentName == "Development")
		{
			return null;
		}
		AppSettings appSettings = await _appSettingsService.GetAppSettings();
		InquiryPanel inquiryPanel = await SetInquiryService(appSettings);
		if (_inquiryService == null)
		{
			throw new CustomException("سرویس استعلام یافت نشد");
		}
		decimal num = await GetCostByInquiryType(inquiryType, inquiryPanel);
		if ((decimal)appSettings.InquiryCredit < num && _appConfig.IsCloud)
		{
			throw new CustomException("میزان اعتبار استعلام کافی نمی باشد.");
		}
		ResponseInquiry<IbanInfoResult> responseInquiry = new ResponseInquiry<IbanInfoResult>();
		switch (inquiryPanel)
		{
		case InquiryPanel.JetAqsaat_ApiIR:
		case InquiryPanel.Dedicate_ApiIR:
		{
			BaseSendInquiry baseSendInquiry = new SendApiIR_IbanInfo
			{
				Iban = model.Iban
			};
			responseInquiry = ((!(_hostingEnvironment.EnvironmentName == "Development")) ? _inquiryService.IbanInfo(baseSendInquiry as SendApiIR_IbanInfo) : new ResponseInquiry<IbanInfoResult>
			{
				Key = "",
				Message = "",
				Response = new IbanInfoResult
				{
					Active = true,
					BankName = "بانک شهر",
					Name = "علی اصغری"
				},
				Success = true
			});
			break;
		}
		case InquiryPanel.JetAqsaat_Shahin:
		case InquiryPanel.Dedicate_Shahin:
		{
			BaseSendInquiry baseSendInquiry = new ShahinIbanInfoRequestModel
			{
				sourceAccount = model.Iban
			};
			responseInquiry = _inquiryService.IbanInfo(baseSendInquiry as ShahinIbanInfoRequestModel);
			break;
		}
		}
		AppInquiry appInquiry = new AppInquiry
		{
			AppInquiryType = inquiryType,
			Date = DateTime.Now,
			Price = num,
			InquiryPanel = inquiryPanel,
			Key = (model.Iban ?? ""),
			MemberId = model.MemberId,
			IsSuccess = responseInquiry.Success,
			PanelResultJson = JsonSerializer.Serialize(responseInquiry.Response, new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			}),
			Result = ((!responseInquiry.Success) ? responseInquiry.Message : ("نام: " + responseInquiry.Response.Name + ", بانک: " + responseInquiry.Response.BankName + ", حساب فعال: " + (responseInquiry.Response.Active ? "بلی" : "خیر")))
		};
		await _appInquiryService.Insert(appInquiry);
		if (appInquiry.IsSuccess)
		{
			await _appSettingsService.SyncChargeInquiryCredit();
		}
		return appInquiry;
	}

	public async Task<AppInquiry> BankCardInfo(BankCardInfoModel model, AppInquiryType inquiryType)
	{
		if (_hostingEnvironment.EnvironmentName == "Development")
		{
			return null;
		}
		AppSettings appSettings = await _appSettingsService.GetAppSettings();
		InquiryPanel inquiryPanel = await SetInquiryService(appSettings);
		if (_inquiryService == null)
		{
			throw new CustomException("سرویس استعلام یافت نشد");
		}
		decimal num = await GetCostByInquiryType(inquiryType, inquiryPanel);
		if ((decimal)appSettings.InquiryCredit < num && _appConfig.IsCloud)
		{
			throw new CustomException("میزان اعتبار استعلام کافی نمی باشد.");
		}
		ResponseInquiry<BankCardInfoResult> responseInquiry = new ResponseInquiry<BankCardInfoResult>();
		switch (inquiryPanel)
		{
		case InquiryPanel.JetAqsaat_ApiIR:
		case InquiryPanel.Dedicate_ApiIR:
		{
			BaseSendInquiry baseSendInquiry = new SendApiIR_BankCardInfo
			{
				CardNumber = model.CardNumber
			};
			responseInquiry = ((!(_hostingEnvironment.EnvironmentName == "Development")) ? _inquiryService.BankCardInfo(baseSendInquiry as SendApiIR_BankCardInfo) : new ResponseInquiry<BankCardInfoResult>
			{
				Key = "",
				Message = "",
				Response = new BankCardInfoResult
				{
					Name = "علی اصغری",
					AccountNumber = "31200216516516",
					Iban = "IR202156516516516516516"
				},
				Success = true
			});
			break;
		}
		case InquiryPanel.JetAqsaat_Shahin:
		case InquiryPanel.Dedicate_Shahin:
		{
			BaseSendInquiry baseSendInquiry = new ShahinBankCardInfoRequestModel
			{
				sourceAccount = model.CardNumber
			};
			responseInquiry = _inquiryService.BankCardInfo(baseSendInquiry as ShahinBankCardInfoRequestModel);
			break;
		}
		}
		AppInquiry appInquiry = new AppInquiry
		{
			AppInquiryType = inquiryType,
			Date = DateTime.Now,
			Price = num,
			InquiryPanel = inquiryPanel,
			Key = (model.CardNumber ?? ""),
			MemberId = model.MemberId,
			IsSuccess = responseInquiry.Success,
			PanelResultJson = JsonSerializer.Serialize(responseInquiry.Response, new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			}),
			Result = ((!responseInquiry.Success) ? responseInquiry.Message : ("نام: " + responseInquiry.Response.Name + ", شبا: " + responseInquiry.Response.Iban + ", شماره حساب: " + responseInquiry.Response.AccountNumber))
		};
		await _appInquiryService.Insert(appInquiry);
		if (appInquiry.IsSuccess)
		{
			await _appSettingsService.SyncChargeInquiryCredit();
		}
		return appInquiry;
	}

	public async Task<AppInquiry> PostalAddress(PostalAddressModel model, AppInquiryType inquiryType)
	{
		if (_hostingEnvironment.EnvironmentName == "Development")
		{
			return null;
		}
		if (model.Member == null)
		{
			throw new CustomException("اطلاعات مشتری جهت استعلام کامل نمی باشد.");
		}
		AppSettings appSettings = await _appSettingsService.GetAppSettings();
		InquiryPanel inquiryPanel = await SetInquiryService(appSettings);
		if (_inquiryService == null)
		{
			throw new CustomException("سرویس استعلام یافت نشد");
		}
		decimal num = await GetCostByInquiryType(inquiryType, inquiryPanel);
		if ((decimal)appSettings.InquiryCredit < num && _appConfig.IsCloud)
		{
			throw new CustomException("میزان اعتبار استعلام کافی نمی باشد.");
		}
		ResponseInquiry<PostalAddressResult> responseInquiry = new ResponseInquiry<PostalAddressResult>();
		if ((uint)(inquiryPanel - 1) <= 1u)
		{
			BaseSendInquiry baseSendInquiry = new SendApiIR_PostalAddress
			{
				PostalCode = model.Member.PostalCode
			};
			responseInquiry = ((!(_hostingEnvironment.EnvironmentName == "Development")) ? _inquiryService.PostalAddress(baseSendInquiry as SendApiIR_PostalAddress) : new ResponseInquiry<PostalAddressResult>
			{
				Key = "",
				Message = "",
				Response = new PostalAddressResult
				{
					Province = "فارس",
					City = "شهر",
					Street = "بلوار استقلال خیابان بعثت",
					Number = "22"
				},
				Success = true
			});
		}
		AppInquiry appInquiry = new AppInquiry
		{
			AppInquiryType = inquiryType,
			Date = DateTime.Now,
			Price = num,
			InquiryPanel = inquiryPanel,
			Key = (model.Member.PostalCode ?? ""),
			MemberId = ((model.Member.Id != 0L) ? new long?(model.Member.Id) : ((long?)null)),
			IsSuccess = responseInquiry.Success,
			PanelResultJson = JsonSerializer.Serialize(responseInquiry.Response, new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			}),
			Result = ((!responseInquiry.Success) ? responseInquiry.Message : ("استان: " + responseInquiry.Response.Province + ", شهر: " + responseInquiry.Response.City + ", بخش: " + responseInquiry.Response.Town + ", منطقه: " + responseInquiry.Response.District + ", خیابان: " + responseInquiry.Response.Street + " " + responseInquiry.Response.Street2 + ", کوچه: " + responseInquiry.Response.Number + ", طبقه: " + responseInquiry.Response.Floor + ", نام ساختمان: " + responseInquiry.Response.BuildingName))
		};
		await _appInquiryService.Insert(appInquiry);
		if (appInquiry.IsSuccess)
		{
			await _appSettingsService.SyncChargeInquiryCredit();
		}
		return appInquiry;
	}

	public async Task<AppInquiry> ActivePlates(ActivePlatesModel model, AppInquiryType inquiryType)
	{
		if (_hostingEnvironment.EnvironmentName == "Development")
		{
			return null;
		}
		if (model.Member == null)
		{
			throw new CustomException("اطلاعات مشتری جهت استعلام کامل نمی باشد.");
		}
		AppSettings appSettings = await _appSettingsService.GetAppSettings();
		InquiryPanel inquiryPanel = await SetInquiryService(appSettings);
		if (_inquiryService == null)
		{
			throw new CustomException("سرویس استعلام یافت نشد");
		}
		decimal num = await GetCostByInquiryType(inquiryType, inquiryPanel);
		if ((decimal)appSettings.InquiryCredit < num && _appConfig.IsCloud)
		{
			throw new CustomException("میزان اعتبار استعلام کافی نمی باشد.");
		}
		ResponseInquiry<List<ActivePlatesResult>> responseInquiry = new ResponseInquiry<List<ActivePlatesResult>>();
		if ((uint)(inquiryPanel - 1) <= 1u)
		{
			BaseSendInquiry baseSendInquiry = new SendApiIR_ActivePlates
			{
				NationalCode = model.Member.NationalCode,
				Mobile = model.Member.Mobile
			};
			responseInquiry = _inquiryService.ActivePlates(baseSendInquiry as SendApiIR_ActivePlates);
		}
		AppInquiry appInquiry = new AppInquiry
		{
			AppInquiryType = inquiryType,
			Date = DateTime.Now,
			Price = num,
			InquiryPanel = inquiryPanel,
			Key = model.Member.Mobile + "," + model.Member.NationalCode,
			MemberId = ((model.Member.Id != 0L) ? new long?(model.Member.Id) : ((long?)null)),
			IsSuccess = responseInquiry.Success,
			PanelResultJson = JsonSerializer.Serialize(responseInquiry.Response, new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			}),
			Result = ((!responseInquiry.Success) ? responseInquiry.Message : string.Join(',', responseInquiry.Response.Select((ActivePlatesResult x, int ind) => "[" + x.Plate + "]").ToList()))
		};
		await _appInquiryService.Insert(appInquiry);
		if (appInquiry.IsSuccess)
		{
			await _appSettingsService.SyncChargeInquiryCredit();
		}
		return appInquiry;
	}

	public async Task<AppInquiry> CardMatch(CardMatchModel model, AppInquiryType inquiryType)
	{
		if (_hostingEnvironment.EnvironmentName == "Development")
		{
			return null;
		}
		if (model.Member == null)
		{
			throw new CustomException("اطلاعات مشتری جهت استعلام کامل نمی باشد.");
		}
		AppSettings appSettings = await _appSettingsService.GetAppSettings();
		InquiryPanel inquiryPanel = await SetInquiryService(appSettings);
		if (_inquiryService == null)
		{
			throw new CustomException("سرویس استعلام یافت نشد");
		}
		decimal num = await GetCostByInquiryType(AppInquiryType.مطابقت_کدملی_کارت_بانکی, inquiryPanel);
		if ((decimal)appSettings.InquiryCredit < num && _appConfig.IsCloud)
		{
			throw new CustomException("میزان اعتبار استعلام کافی نمی باشد.");
		}
		ResponseInquiry<CardMatchResult> responseInquiry = new ResponseInquiry<CardMatchResult>();
		switch (inquiryPanel)
		{
		case InquiryPanel.JetAqsaat_ApiIR:
		case InquiryPanel.Dedicate_ApiIR:
		{
			BaseSendInquiry baseSendInquiry = new SendApiIR_CardMatch
			{
				BirthDate = model.Member.BirthDate.ToPersianDate(),
				NationalCode = model.Member.NationalCode,
				CardNumber = model.CardNumber
			};
			responseInquiry = _inquiryService.CardMatch(baseSendInquiry as SendApiIR_CardMatch);
			break;
		}
		case InquiryPanel.JetAqsaat_FinnoTech:
		case InquiryPanel.Dedicate_FinnoTech:
		{
			BaseSendInquiry baseSendInquiry = new SendFinnoTech_CardMatch
			{
				NationalCode = model.Member.NationalCode,
				CardNumber = model.CardNumber
			};
			responseInquiry = _inquiryService.CardMatch(baseSendInquiry as SendFinnoTech_CardMatch);
			break;
		}
		}
		AppInquiry appInquiry = new AppInquiry
		{
			AppInquiryType = inquiryType,
			Date = DateTime.Now,
			Price = num,
			InquiryPanel = inquiryPanel,
			Key = model.CardNumber + "," + model.Member.NationalCode,
			MemberId = ((model.Member.Id != 0L) ? new long?(model.Member.Id) : ((long?)null)),
			IsSuccess = responseInquiry.Success,
			PanelResultJson = JsonSerializer.Serialize(responseInquiry.Response, new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			}),
			Result = ((!responseInquiry.Success) ? responseInquiry.Message : (responseInquiry.Response.IsMatch ? "تطابق دارد" : "عدم تطابق"))
		};
		await _appInquiryService.Insert(appInquiry);
		if (appInquiry.IsSuccess)
		{
			await _appSettingsService.SyncChargeInquiryCredit();
		}
		return appInquiry;
	}

	public async Task<AppInquiry> IbanMatch(IbanMatchModel model, AppInquiryType inquiryType)
	{
		if (_hostingEnvironment.EnvironmentName == "Development")
		{
			return null;
		}
		if (model.Member == null)
		{
			throw new CustomException("اطلاعات مشتری جهت استعلام کامل نمی باشد.");
		}
		AppSettings appSettings = await _appSettingsService.GetAppSettings();
		InquiryPanel inquiryPanel = await SetInquiryService(appSettings);
		if (_inquiryService == null)
		{
			throw new CustomException("سرویس استعلام یافت نشد");
		}
		decimal num = await GetCostByInquiryType(AppInquiryType.مطابقت_کدملی_کارت_بانکی, inquiryPanel);
		if ((decimal)appSettings.InquiryCredit < num && _appConfig.IsCloud)
		{
			throw new CustomException("میزان اعتبار استعلام کافی نمی باشد.");
		}
		ResponseInquiry<IbanMatchResult> responseInquiry = new ResponseInquiry<IbanMatchResult>();
		switch (inquiryPanel)
		{
		case InquiryPanel.JetAqsaat_ApiIR:
		case InquiryPanel.Dedicate_ApiIR:
		{
			BaseSendInquiry baseSendInquiry = new SendApiIR_IbanMatch
			{
				BirthDate = model.Member.BirthDate.ToPersianDate(),
				NationalCode = model.Member.NationalCode,
				Iban = model.Iban
			};
			responseInquiry = ((!(_hostingEnvironment.EnvironmentName == "Development")) ? _inquiryService.IbanMatch(baseSendInquiry as SendApiIR_IbanMatch) : new ResponseInquiry<IbanMatchResult>
			{
				Key = "",
				Message = "",
				Response = new IbanMatchResult
				{
					IsMatch = true
				},
				Success = true
			});
			break;
		}
		case InquiryPanel.JetAqsaat_FinnoTech:
		case InquiryPanel.Dedicate_FinnoTech:
		{
			BaseSendInquiry baseSendInquiry = new SendFinnoTech_IbanMatch
			{
				NationalCode = model.Member.NationalCode,
				Iban = model.Iban
			};
			responseInquiry = _inquiryService.IbanMatch(baseSendInquiry as SendFinnoTech_IbanMatch);
			break;
		}
		}
		AppInquiry appInquiry = new AppInquiry
		{
			AppInquiryType = inquiryType,
			Date = DateTime.Now,
			Price = num,
			InquiryPanel = inquiryPanel,
			Key = model.Iban + "," + model.Member.NationalCode,
			MemberId = ((model.Member.Id != 0L) ? new long?(model.Member.Id) : ((long?)null)),
			IsSuccess = responseInquiry.Success,
			PanelResultJson = JsonSerializer.Serialize(responseInquiry.Response, new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			}),
			Result = ((!responseInquiry.Success) ? responseInquiry.Message : (responseInquiry.Response.IsMatch ? "تطابق دارد" : "عدم تطابق"))
		};
		await _appInquiryService.Insert(appInquiry);
		if (appInquiry.IsSuccess)
		{
			await _appSettingsService.SyncChargeInquiryCredit();
		}
		return appInquiry;
	}

	public async Task<AppInquiry> ChequeColor(ChequeColorModel model, AppInquiryType inquiryType)
	{
		if (_hostingEnvironment.EnvironmentName == "Development")
		{
			return null;
		}
		if (model.Member == null)
		{
			throw new CustomException("اطلاعات مشتری جهت استعلام کامل نمی باشد.");
		}
		AppSettings appSettings = await _appSettingsService.GetAppSettings();
		InquiryPanel inquiryPanel = await SetInquiryService(appSettings);
		if (_inquiryService == null)
		{
			throw new CustomException("سرویس استعلام یافت نشد");
		}
		decimal num = await GetCostByInquiryType(inquiryType, inquiryPanel);
		if ((decimal)appSettings.InquiryCredit < num && _appConfig.IsCloud)
		{
			throw new CustomException("میزان اعتبار استعلام کافی نمی باشد.");
		}
		ResponseInquiry<ChequeColorResult> responseInquiry = new ResponseInquiry<ChequeColorResult>();
		if ((uint)(inquiryPanel - 1) <= 1u)
		{
			BaseSendInquiry baseSendInquiry = new SendApiIR_ChequeColor
			{
				NationalCode = model.Member.NationalCode,
				IsCompany = false
			};
			responseInquiry = _inquiryService.ChequeColor(baseSendInquiry as SendApiIR_ChequeColor);
		}
		AppInquiry appInquiry = new AppInquiry
		{
			AppInquiryType = inquiryType,
			Date = DateTime.Now,
			Price = num,
			InquiryPanel = inquiryPanel,
			Key = (model.Member.NationalCode ?? ""),
			MemberId = ((model.Member.Id != 0L) ? new long?(model.Member.Id) : ((long?)null)),
			IsSuccess = responseInquiry.Success,
			PanelResultJson = JsonSerializer.Serialize(responseInquiry.Response, new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			}),
			Result = ((!responseInquiry.Success) ? responseInquiry.Message : responseInquiry.Response.ChequeColor)
		};
		await _appInquiryService.Insert(appInquiry);
		if (appInquiry.IsSuccess)
		{
			await _appSettingsService.SyncChargeInquiryCredit();
		}
		return appInquiry;
	}

	public async Task<AppInquiry> ActiveLoans(ActiveLoansModel model, AppInquiryType inquiryType)
	{
		if (_hostingEnvironment.EnvironmentName == "Development")
		{
			return null;
		}
		if (model.Member == null)
		{
			throw new CustomException("اطلاعات مشتری جهت استعلام کامل نمی باشد.");
		}
		AppSettings appSettings = await _appSettingsService.GetAppSettings();
		InquiryPanel inquiryPanel = await SetInquiryService(appSettings);
		if (_inquiryService == null)
		{
			throw new CustomException("سرویس استعلام یافت نشد");
		}
		decimal num = await GetCostByInquiryType(inquiryType, inquiryPanel);
		if ((decimal)appSettings.InquiryCredit < num && _appConfig.IsCloud)
		{
			throw new CustomException("میزان اعتبار استعلام کافی نمی باشد.");
		}
		ResponseInquiry<ActiveLoansResult> responseInquiry = new ResponseInquiry<ActiveLoansResult>();
		if ((uint)(inquiryPanel - 1) <= 1u)
		{
			BaseSendInquiry baseSendInquiry = new SendApiIR_ActiveLoans
			{
				NationalCode = model.Member.NationalCode
			};
			responseInquiry = _inquiryService.ActiveLoans(baseSendInquiry as SendApiIR_ActiveLoans);
		}
		AppInquiry appInquiry = new AppInquiry
		{
			AppInquiryType = inquiryType,
			Date = DateTime.Now,
			Price = num,
			InquiryPanel = inquiryPanel,
			Key = (model.Member.NationalCode ?? ""),
			MemberId = ((model.Member.Id != 0L) ? new long?(model.Member.Id) : ((long?)null)),
			IsSuccess = responseInquiry.Success,
			PanelResultJson = JsonSerializer.Serialize(responseInquiry.Response, new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			}),
			Result = ((!responseInquiry.Success) ? responseInquiry.Message : string.Format("تعداد تسهیلات: {0}, مجموع تسهیلات: {1}, میزان کل بدهی: {2}, سررسید گذشته: {3}, مبلغ کل معوق: {4}", responseInquiry.Response.Count, responseInquiry.Response.TotalAmount.ToString("N0"), responseInquiry.Response.DebtTotalAmount.ToString("N0"), responseInquiry.Response.PastExpiredTotalAmount.ToString("N0"), responseInquiry.Response.DeferredTotalAmount.ToString("N0")))
		};
		await _appInquiryService.Insert(appInquiry);
		if (appInquiry.IsSuccess)
		{
			await _appSettingsService.SyncChargeInquiryCredit();
		}
		return appInquiry;
	}

	public async Task<AppInquiry> MilitaryStatus(MilitaryStatusModel model, AppInquiryType inquiryType)
	{
		if (_hostingEnvironment.EnvironmentName == "Development")
		{
			return null;
		}
		if (model.Member == null)
		{
			throw new CustomException("اطلاعات مشتری جهت استعلام کامل نمی باشد.");
		}
		AppSettings appSettings = await _appSettingsService.GetAppSettings();
		InquiryPanel inquiryPanel = await SetInquiryService(appSettings);
		if (_inquiryService == null)
		{
			throw new CustomException("سرویس استعلام یافت نشد");
		}
		decimal num = await GetCostByInquiryType(inquiryType, inquiryPanel);
		if ((decimal)appSettings.InquiryCredit < num && _appConfig.IsCloud)
		{
			throw new CustomException("میزان اعتبار استعلام کافی نمی باشد.");
		}
		ResponseInquiry<MilitaryStatusResult> responseInquiry = new ResponseInquiry<MilitaryStatusResult>();
		if ((uint)(inquiryPanel - 1) <= 1u)
		{
			BaseSendInquiry baseSendInquiry = new SendApiIR_MilitaryStatus
			{
				NationalCode = model.Member.NationalCode
			};
			responseInquiry = _inquiryService.MilitaryStatus(baseSendInquiry as SendApiIR_MilitaryStatus);
		}
		AppInquiry appInquiry = new AppInquiry
		{
			AppInquiryType = inquiryType,
			Date = DateTime.Now,
			Price = num,
			InquiryPanel = inquiryPanel,
			Key = (model.Member.NationalCode ?? ""),
			MemberId = ((model.Member.Id != 0L) ? new long?(model.Member.Id) : ((long?)null)),
			IsSuccess = responseInquiry.Success,
			PanelResultJson = JsonSerializer.Serialize(responseInquiry.Response, new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			}),
			Result = ((!responseInquiry.Success) ? responseInquiry.Message : (responseInquiry.Response.Qualified ? "خدمت سربازی انجام شده است" : "خدمت سربازی انجام نشده است"))
		};
		await _appInquiryService.Insert(appInquiry);
		if (appInquiry.IsSuccess)
		{
			await _appSettingsService.SyncChargeInquiryCredit();
		}
		return appInquiry;
	}

	public async Task<AppInquiry> ChequeInfo(ChequeInfoModel model, AppInquiryType inquiryType)
	{
		if (_hostingEnvironment.EnvironmentName == "Development")
		{
			return null;
		}
		if (model.Member == null)
		{
			throw new CustomException("اطلاعات مشتری جهت استعلام کامل نمی باشد.");
		}
		if (string.IsNullOrWhiteSpace(model.ChequeID))
		{
			throw new CustomException("اطلاعات شماره چک کامل نمی باشد.");
		}
		AppSettings appSettings = await _appSettingsService.GetAppSettings();
		InquiryPanel inquiryPanel = await SetInquiryService(appSettings);
		if (_inquiryService == null)
		{
			throw new CustomException("سرویس استعلام یافت نشد");
		}
		decimal num = await GetCostByInquiryType(inquiryType, inquiryPanel);
		if ((decimal)appSettings.InquiryCredit < num && _appConfig.IsCloud)
		{
			throw new CustomException("میزان اعتبار استعلام کافی نمی باشد.");
		}
		ResponseInquiry<ChequeInfoResult> responseInquiry = new ResponseInquiry<ChequeInfoResult>();
		if ((uint)(inquiryPanel - 1) <= 1u)
		{
			BaseSendInquiry baseSendInquiry = new SendApiIR_ChequeInfo
			{
				ChequeID = model.ChequeID
			};
			responseInquiry = _inquiryService.ChequeInfo(baseSendInquiry as SendApiIR_ChequeInfo);
		}
		AppInquiry appInquiry = new AppInquiry
		{
			AppInquiryType = inquiryType,
			Date = DateTime.Now,
			Price = num,
			InquiryPanel = inquiryPanel,
			Key = (model.ChequeID ?? ""),
			MemberId = ((model.Member.Id != 0L) ? new long?(model.Member.Id) : ((long?)null)),
			IsSuccess = responseInquiry.Success,
			PanelResultJson = JsonSerializer.Serialize(responseInquiry.Response, new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			}),
			Result = ((!responseInquiry.Success) ? responseInquiry.Message : ("شبا: " + responseInquiry.Response.Iban + ", تاریخ سررسید: " + responseInquiry.Response.IssuedDate + ", تاریخ انقضا: " + responseInquiry.Response.ExpirationDate + ", شماره سریال: " + responseInquiry.Response.SeriesNumber + ", شماره سری: " + responseInquiry.Response.SeriesNumber + ", نوع چک: " + responseInquiry.Response.ChequeType + ", کد شعبه: " + responseInquiry.Response.BranchCode + ", صاحب حساب: " + responseInquiry.Response.Name))
		};
		await _appInquiryService.Insert(appInquiry);
		if (appInquiry.IsSuccess)
		{
			await _appSettingsService.SyncChargeInquiryCredit();
		}
		return appInquiry;
	}

	private async Task<decimal> GetCostByInquiryType(AppInquiryType appInquiryType, InquiryPanel inquiryPanel)
	{
		if (!_appConfig.IsCloud)
		{
			return 0m;
		}
		decimal cost = 0m;
		InquirySettings inquirySettings = (await _inquirySettingsService.GetByCondition((InquirySettings x) => (int)x.AppInquiryType == (int)appInquiryType))?.FirstOrDefault();
		if (inquirySettings == null)
		{
			return 0m;
		}
		switch (inquiryPanel)
		{
		case InquiryPanel.JetAqsaat_ApiIR:
			cost = inquirySettings.ApiIR_PriceTo;
			cost += cost * 10m / 100m;
			break;
		}
		return cost;
	}

	private async Task<InquiryPanel> SetInquiryService(AppSettings appSettings = null)
	{
		InquiryPanel inquiryPanel = InquiryPanel.Unknown;
		if (_appConfig != null)
		{
			if (appSettings == null && !_appConfig.IsCloud)
			{
				appSettings = await _appSettingsService.GetAppSettings();
			}
			AppSettingsModel appSettingsModel = new AppSettingsModel(appSettings);
			switch (_appConfig.IsCloud ? _appConfig.DefaultInquiryPanel : appSettings?.DefaultInquiryPanel.ToString())
			{
			case "JetAqsaat_ApiIR":
			case "Dedicate_ApiIR":
			case "ApiIR":
				if (_apiIRConfig != null)
				{
					if (_appConfig.IsCloud)
					{
						_inquiryService = new ApiIRService(new ApiIRConfig
						{
							ApiKey = _apiIRConfig.ApiKey,
							ApiToken = _apiIRConfig.ApiToken
						});
						inquiryPanel = InquiryPanel.JetAqsaat_ApiIR;
					}
					else
					{
						_inquiryService = new ApiIRService(appSettingsModel.ApiIRConfig);
						inquiryPanel = appSettings.DefaultInquiryPanel;
					}
				}
				break;
			case "JetAqsaat_FinnoTech":
			case "Dedicate_FinnoTech":
			case "FinnoTech":
				if (_finnoTechConfig != null)
				{
					if (_appConfig.IsCloud)
					{
						_inquiryService = new FinnoTechService(_finnoTechConfig);
						inquiryPanel = InquiryPanel.JetAqsaat_FinnoTech;
					}
					else
					{
						_inquiryService = new FinnoTechService(appSettingsModel.FinnoTechConfig);
						inquiryPanel = appSettings.DefaultInquiryPanel;
					}
				}
				break;
			case "Dedicate_Shahin":
				if (_shahinConfig != null)
				{
					if (_appConfig.IsCloud)
					{
						_inquiryService = new ShahinInquiryService(_shahinLogger, _shahinConfig);
						(_inquiryService as ShahinInquiryService).SetWebHostRoot(_contentRootPath);
						inquiryPanel = InquiryPanel.JetAqsaat_Shahin;
					}
					else
					{
						_inquiryService = new ShahinInquiryService(_shahinLogger, _shahinConfig);
						(_inquiryService as ShahinInquiryService).SetWebHostRoot(_contentRootPath);
						inquiryPanel = InquiryPanel.Dedicate_Shahin;
					}
				}
				break;
			}
		}
		return inquiryPanel;
	}

	public void SetWebHostRoot(string contentRootPath)
	{
		_contentRootPath = contentRootPath;
	}
}
