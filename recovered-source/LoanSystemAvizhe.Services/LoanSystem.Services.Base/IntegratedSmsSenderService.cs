using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Hangfire;
using LoanSystem.Core;
using LoanSystem.Core.Configuration;
using LoanSystem.Core.Exceptions;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Models;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Data.Enums;
using LoanSystemAvizhe.Services.Sms;
using LoanSystemAvizhe.Services.Sms.Bale;
using LoanSystemAvizhe.Services.Sms.IPPanel;
using LoanSystemAvizhe.Services.Sms.KaveNegar;
using LoanSystemAvizhe.Services.Sms.MelliPayamak;
using LoanSystemAvizhe.Services.Sms.Models.Bale;
using LoanSystemAvizhe.Services.Sms.Models.IPPanel;
using LoanSystemAvizhe.Services.Sms.Models.KaveNegar;
using LoanSystemAvizhe.Services.Sms.Models.MelliPayamak;
using LoanSystemAvizhe.Services.Sms.Models.WebOne;
using LoanSystemAvizhe.Services.Sms.WebOne;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using NikanBase.Services.Security;
using UAParser;

namespace LoanSystem.Services.Base;

public class IntegratedSmsSenderService
{
	private readonly ICustomerService _customerService;

	private readonly ICurrentUserService _currentUserService;

	private readonly IAppSettingsService _appSettingsService;

	private readonly IAppSmsService _appSmsService;

	private readonly AppConfig _appConfig;

	private readonly MelliPayamakConfig _melliPayamakConfig;

	private readonly KaveNegarConfig _kaveNegarConfig;

	private readonly IPPanelConfig _ippanelConfig;

	private readonly WebOneConfig _webOneConfig;

	private readonly BaleConfig _baleConfig;

	private readonly BaleApiService _baleApiService;

	private readonly IHttpContextAccessor _httpContextAccessor;

	private readonly IHostingEnvironment _hostingEnvironment;

	private readonly IServiceProvider _serviceProvider;

	private readonly IEncryptionService _encryptionService;

	private readonly ILogger<IntegratedSmsSenderService> _logger;

	private IPayamakService _payamakService;

	private const int persianPageSplitLength = 67;

	private const int laghvLength = 6;

	public IntegratedSmsSenderService(ICurrentUserService currentUserService, ILogger<IntegratedSmsSenderService> logger, IAppSmsService appSmsService, IOptions<AppConfig> appConfig, IAppSettingsService appSettingsService, IOptions<MelliPayamakConfig> melliPayamakConfig, IOptions<KaveNegarConfig> kaveNegarConfig, IOptions<IPPanelConfig> ippanelConfig, IOptions<WebOneConfig> webOneConfig, IOptions<BaleConfig> baleConfig, ICustomerService customerService, IHttpContextAccessor httpContextAccessor, IHostingEnvironment hostingEnvironment, IEncryptionService encryptionService, IServiceProvider serviceProvider)
	{
		_currentUserService = currentUserService;
		_appSmsService = appSmsService;
		_appConfig = appConfig.Value;
		_appSettingsService = appSettingsService;
		_melliPayamakConfig = melliPayamakConfig.Value;
		_kaveNegarConfig = kaveNegarConfig.Value;
		_baleConfig = baleConfig.Value;
		_ippanelConfig = ippanelConfig.Value;
		_webOneConfig = webOneConfig.Value;
		_customerService = customerService;
		_httpContextAccessor = httpContextAccessor;
		_hostingEnvironment = hostingEnvironment;
		_serviceProvider = serviceProvider;
		_encryptionService = encryptionService;
		_logger = logger;
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
		await SetPayamakService(appSettings);
		if (_payamakService == null)
		{
			return credit;
		}
		ResponsePayamak credit2 = _payamakService.GetCredit();
		if (credit2.Success)
		{
			decimal.TryParse(credit2?.Key ?? "0", out credit);
		}
		return credit;
	}

	public async Task<int> GetTodaySuccessSmsCount()
	{
		return await _appSmsService.GetCountByCondition((AppSms x) => (int)x.Status == 2 && x.Date.Date == DateTime.Now.Date, false);
	}

	public async Task SendOnlineMoneyTransferOTP(string to, string code, decimal amount, Guid customerId)
	{
		if (_hostingEnvironment.EnvironmentName == "Development")
		{
			return;
		}
		_currentUserService.CustomerId = customerId;
		SmsPanel smsPanel = await SetPayamakService();
		if (_payamakService == null)
		{
			throw new CustomException("سرویس پیامک یافت نشد");
		}
		if (string.IsNullOrEmpty(to))
		{
			throw new CustomException("شماره موبایل دریافت کننده ثبت نشده است.");
		}
		BaseSendPayamak baseSendPayamak = null;
		ResponsePayamak sendResult = new ResponsePayamak();
		switch (smsPanel)
		{
		case SmsPanel.IPPanel:
		case SmsPanel.JetAqsaat_IPPanel:
		{
			string onlineMoneyTransferOTP3 = _ippanelConfig.OnlineMoneyTransferOTP;
			SendIPPanelPattern sendIPPanelPattern = new SendIPPanelPattern();
			sendIPPanelPattern.To = to;
			sendIPPanelPattern.Text = "کد تائیدیه انتقال وجه: " + code + "\r\nمبلغ: " + (amount * 10m).ToString("N0") + " ریال\r\n" + _appConfig.FullTitle;
			baseSendPayamak = sendIPPanelPattern;
			if (!string.IsNullOrEmpty(onlineMoneyTransferOTP3))
			{
				(baseSendPayamak as SendIPPanelPattern).Variable = new ExpandoObject();
				(baseSendPayamak as SendIPPanelPattern).Variable.TryAdd("Code", code);
				(baseSendPayamak as SendIPPanelPattern).Variable.TryAdd("Amount", (amount * 10m).ToString("N0"));
				sendResult = _payamakService.SendByPatternIPPanel(baseSendPayamak as SendIPPanelPattern, onlineMoneyTransferOTP3);
			}
			else
			{
				sendResult = _payamakService.Send(baseSendPayamak);
			}
			break;
		}
		case SmsPanel.KaveNegar:
		case SmsPanel.JetAqsaat_KaveNegar:
		{
			string onlineMoneyTransferOTP2 = _kaveNegarConfig.OnlineMoneyTransferOTP;
			SendKaveNegar sendKaveNegar = new SendKaveNegar();
			sendKaveNegar.To = to;
			sendKaveNegar.Text = "کد تائیدیه انتقال وجه: " + code + "\r\nمبلغ: " + (amount * 10m).ToString("N0") + " ریال\r\n" + _appConfig.FullTitle;
			baseSendPayamak = sendKaveNegar;
			if (!string.IsNullOrEmpty(onlineMoneyTransferOTP2))
			{
				(baseSendPayamak as SendKaveNegar).Token1 = code;
				(baseSendPayamak as SendKaveNegar).Token2 = (amount * 10m).ToString("N0");
				sendResult = _payamakService.SendByLookupKaveNegar(baseSendPayamak as SendKaveNegar, onlineMoneyTransferOTP2);
			}
			else
			{
				sendResult = _payamakService.Send(baseSendPayamak);
			}
			break;
		}
		case SmsPanel.MelliPayamak:
		case SmsPanel.JetAqsaat_MelliPayamak:
		{
			int onlineMoneyTransferOTP4 = _melliPayamakConfig.OnlineMoneyTransferOTP;
			SendMelliPayamak sendMelliPayamak = new SendMelliPayamak();
			sendMelliPayamak.To = to;
			sendMelliPayamak.Text = "کد تائیدیه انتقال وجه: " + code + "\r\nمبلغ: " + (amount * 10m).ToString("N0") + " ریال\r\n" + _appConfig.FullTitle;
			baseSendPayamak = sendMelliPayamak;
			if (onlineMoneyTransferOTP4 != 0)
			{
				baseSendPayamak.Text = code + ";" + (amount * 10m).ToString("N0");
				sendResult = _payamakService.SendByBaseNumber(baseSendPayamak, onlineMoneyTransferOTP4);
			}
			else
			{
				sendResult = _payamakService.Send(baseSendPayamak);
			}
			break;
		}
		case SmsPanel.WebOne:
		{
			string onlineMoneyTransferOTP = _webOneConfig.OnlineMoneyTransferOTP;
			SendWebOnePattern sendWebOnePattern = new SendWebOnePattern();
			sendWebOnePattern.To = to;
			sendWebOnePattern.Text = "کد تائیدیه انتقال وجه: " + code + "\r\nمبلغ: " + (amount * 10m).ToString("N0") + " ریال\r\n" + _appConfig.FullTitle;
			baseSendPayamak = sendWebOnePattern;
			if (!string.IsNullOrWhiteSpace(onlineMoneyTransferOTP))
			{
				baseSendPayamak.Text = code + ";" + (amount * 10m).ToString("N0");
				sendResult = _payamakService.SendByPatternWebOne(baseSendPayamak as SendWebOnePattern, onlineMoneyTransferOTP);
			}
			else
			{
				sendResult = _payamakService.Send(baseSendPayamak);
			}
			break;
		}
		case SmsPanel.Bale:
		{
			SendBale sendBale = new SendBale();
			sendBale.To = to;
			sendBale.Text = "کد تائیدیه انتقال وجه: " + code + "\r\nمبلغ: " + (amount * 10m).ToString("N0") + " ریال\r\n" + _appConfig.FullTitle;
			baseSendPayamak = sendBale;
			sendResult = _payamakService.Send(baseSendPayamak);
			break;
		}
		}
		AppSms appSms = new AppSms
		{
			AppSmsType = AppSmsType.OnlineMoneyTransferOtp,
			Date = DateTime.Now,
			Price = 0m,
			Status = (sendResult.Success ? AppSmsStatus.Pending : AppSmsStatus.Failed),
			Text = baseSendPayamak.Text,
			SmsPatternTitle = "کد تاییدیه انتقال وجه آنلاین",
			To = to,
			EntityType = "",
			EntityId = Guid.Empty,
			SmsPanel = smsPanel,
			SMSKey = sendResult.Key,
			Tag = code
		};
		await _appSmsService.Insert(appSms);
		if (sendResult.Success)
		{
			await _appSettingsService.SyncChargeSmsCredit();
			BackgroundJob.Schedule(() => CheckDelivery(sendResult.Key, appSms.Id, (Guid)_currentUserService.CustomerId, _appConfig.IsCloud), TimeSpan.FromMinutes(30.0));
		}
	}

	public async Task SendNewConsultantNotification(string name)
	{
		SmsPanel smsPanel = await SetPayamakService(new AppSettings
		{
			DefaultSmsPanel = SmsPanel.JetAqsaat
		});
		if (_payamakService != null && !string.IsNullOrEmpty(_appConfig.AdminPhone))
		{
			switch (smsPanel)
			{
			case SmsPanel.IPPanel:
			case SmsPanel.JetAqsaat_IPPanel:
			{
				BaseSendPayamak sendPayamak = new SendIPPanelPattern
				{
					To = _appConfig.AdminPhone,
					Text = "درخواست مشاوره جدید توسط " + name + " در سایت جت اقساط ثبت شده است"
				};
				_payamakService.Send(sendPayamak);
				break;
			}
			case SmsPanel.KaveNegar:
			case SmsPanel.JetAqsaat_KaveNegar:
			{
				BaseSendPayamak sendPayamak = new SendKaveNegar
				{
					To = _appConfig.AdminPhone,
					Text = "درخواست مشاوره جدید توسط " + name + " در سایت جت اقساط ثبت شده است"
				};
				_payamakService.Send(sendPayamak);
				break;
			}
			case SmsPanel.MelliPayamak:
			case SmsPanel.JetAqsaat_MelliPayamak:
			{
				BaseSendPayamak sendPayamak = new SendMelliPayamak
				{
					To = _appConfig.AdminPhone,
					Text = "درخواست مشاوره جدید توسط " + name + " در سایت جت اقساط ثبت شده است"
				};
				_payamakService.Send(sendPayamak);
				break;
			}
			case SmsPanel.Bale:
			{
				BaseSendPayamak sendPayamak = new SendBale
				{
					To = _appConfig.AdminPhone,
					Text = "درخواست مشاوره جدید توسط " + name + " در سایت جت اقساط ثبت شده است"
				};
				_payamakService.Send(sendPayamak);
				break;
			}
			case SmsPanel.WebOne:
				break;
			}
		}
	}

	public async Task SendNewCustomerOrderNotification(CustomerOrderModel model, Guid customerId)
	{
		if (_hostingEnvironment.EnvironmentName == "Development")
		{
			return;
		}
		_currentUserService.CustomerId = customerId;
		Customer customer = await _customerService.GetById(customerId);
		AppSettings appSettings = await _appSettingsService.GetAppSettings();
		SmsPanel smsPanel = await SetPayamakService(appSettings);
		if (_payamakService == null)
		{
			throw new CustomException("سرویس پیامک یافت نشد");
		}
		if (appSettings?.NewOrderSmsPattern == null)
		{
			throw new CustomException("قالب پیامک اطلاع سفارش جدید به مدیر تعیین نشده است.");
		}
		string to = customer.Mobile;
		if (string.IsNullOrEmpty(to))
		{
			throw new CustomException("شماره موبایل دریافت کننده ثبت نشده است.");
		}
		bool isPatternBase = appSettings.NewOrderSmsPattern.IsPatternBase && !string.IsNullOrEmpty(appSettings.NewOrderSmsPattern.PatternBaseCode);
		string text = await ExtractTextFromPattern(model, appSettings.NewOrderSmsPattern.Pattern, isPatternBase, customer);
		decimal num = Math.Ceiling(((decimal)text.Length + 6m) / 67m) * (decimal)_appConfig.BaseSmsPrice;
		if (smsPanel.ToString().ToLower().Contains("jet") && (decimal)appSettings.SmsCredit < num && _appConfig.IsCloud)
		{
			throw new CustomException("میزان اعتبار شارژ کافی نمی باشد.");
		}
		BaseSendPayamak baseSendPayamak = null;
		ResponsePayamak sendResult = new ResponsePayamak();
		switch (smsPanel)
		{
		case SmsPanel.IPPanel:
		case SmsPanel.JetAqsaat_IPPanel:
			baseSendPayamak = new SendIPPanelPattern
			{
				To = to,
				Text = text
			};
			if (isPatternBase)
			{
				PrepareIPPanelKeys(baseSendPayamak as SendIPPanelPattern, text, appSettings.NewOrderSmsPattern.Pattern, appSettings.NewOrderSmsPattern.IppanelKeysJson);
				sendResult = _payamakService.SendByPatternIPPanel(baseSendPayamak as SendIPPanelPattern, appSettings.NewOrderSmsPattern.PatternBaseCode);
			}
			else
			{
				sendResult = _payamakService.Send(baseSendPayamak);
			}
			break;
		case SmsPanel.KaveNegar:
		case SmsPanel.JetAqsaat_KaveNegar:
			baseSendPayamak = new SendKaveNegar
			{
				To = to,
				Text = text
			};
			if (isPatternBase)
			{
				PrepareKavehTokens(baseSendPayamak as SendKaveNegar, text);
				sendResult = _payamakService.SendByLookupKaveNegar(baseSendPayamak as SendKaveNegar, appSettings.NewOrderSmsPattern.PatternBaseCode);
			}
			else
			{
				sendResult = _payamakService.Send(baseSendPayamak);
			}
			break;
		case SmsPanel.MelliPayamak:
		case SmsPanel.JetAqsaat_MelliPayamak:
		{
			baseSendPayamak = new SendMelliPayamak
			{
				To = to,
				Text = text
			};
			if (isPatternBase && int.TryParse(appSettings.NewOrderSmsPattern.PatternBaseCode, out var result))
			{
				PrepareMelliPayamakTextPattern(baseSendPayamak as SendMelliPayamak, text);
				sendResult = _payamakService.SendByBaseNumber(baseSendPayamak, result);
			}
			else
			{
				sendResult = _payamakService.Send(baseSendPayamak);
			}
			break;
		}
		case SmsPanel.WebOne:
			baseSendPayamak = new SendWebOnePattern
			{
				To = to,
				Text = text
			};
			if (isPatternBase)
			{
				sendResult = _payamakService.SendByPatternWebOne(baseSendPayamak as SendWebOnePattern, appSettings.NewOrderSmsPattern.PatternBaseCode);
			}
			else
			{
				sendResult = _payamakService.Send(baseSendPayamak);
			}
			break;
		case SmsPanel.Bale:
			baseSendPayamak = new SendBale
			{
				To = to,
				Text = text
			};
			sendResult = _payamakService.Send(baseSendPayamak);
			break;
		}
		AppSms appSms = new AppSms
		{
			AppSmsType = AppSmsType.NewOrder,
			Date = DateTime.Now,
			Price = num,
			Status = (sendResult.Success ? AppSmsStatus.Pending : AppSmsStatus.Failed),
			Text = baseSendPayamak.Text,
			SmsPatternTitle = appSettings.NewOrderSmsPattern.Title,
			To = to,
			EntityType = "CustomerOrder",
			EntityId = model.Id,
			SmsPanel = smsPanel,
			SMSKey = sendResult.Key,
			Tag = ""
		};
		await _appSmsService.Insert(appSms);
		if (sendResult.Success)
		{
			await _appSettingsService.SyncChargeSmsCredit();
			BackgroundJob.Schedule(() => CheckDelivery(sendResult.Key, appSms.Id, (Guid)_currentUserService.CustomerId, _appConfig.IsCloud), TimeSpan.FromMinutes(30.0));
		}
	}

	public async Task SendRegisterationCode(string to, string code)
	{
		SmsPanel smsPanel = await SetPayamakService(new AppSettings
		{
			DefaultSmsPanel = SmsPanel.JetAqsaat
		});
		if (_payamakService == null || string.IsNullOrEmpty(to))
		{
			return;
		}
		switch (smsPanel)
		{
		case SmsPanel.IPPanel:
		case SmsPanel.JetAqsaat_IPPanel:
		{
			string regTemplate = _ippanelConfig.RegTemplate;
			BaseSendPayamak sendPayamak = new SendIPPanelPattern
			{
				To = to,
				Text = "رمز ورود به اپلیکیشن : " + code + "\r\n" + _appConfig.FullTitle
			};
			if (!string.IsNullOrEmpty(regTemplate))
			{
				sendPayamak.Text = code;
				(sendPayamak as SendIPPanelPattern).Variable = new ExpandoObject();
				(sendPayamak as SendIPPanelPattern).Variable.TryAdd("Code", code);
				_payamakService.SendByPatternIPPanel(sendPayamak as SendIPPanelPattern, regTemplate);
			}
			else
			{
				_payamakService.Send(sendPayamak);
			}
			break;
		}
		case SmsPanel.WebOne:
		{
			string regTemplate = _webOneConfig.RegTemplate;
			BaseSendPayamak sendPayamak = new SendWebOnePattern
			{
				To = to,
				Text = "رمز ورود به اپلیکیشن : " + code + "\r\n" + _appConfig.FullTitle
			};
			if (!string.IsNullOrEmpty(regTemplate))
			{
				sendPayamak.Text = code;
				_payamakService.SendByPatternWebOne(sendPayamak as SendWebOnePattern, regTemplate);
			}
			else
			{
				_payamakService.Send(sendPayamak);
			}
			break;
		}
		case SmsPanel.KaveNegar:
		case SmsPanel.JetAqsaat_KaveNegar:
		{
			string regTemplate2 = _kaveNegarConfig.RegTemplate;
			BaseSendPayamak sendPayamak = new SendKaveNegar
			{
				To = to,
				Text = "رمز ورود به اپلیکیشن : " + code + "\r\n" + _appConfig.FullTitle,
				Token1 = code
			};
			if (!string.IsNullOrEmpty(regTemplate2))
			{
				sendPayamak.Text = code;
				(sendPayamak as SendKaveNegar).Token1 = code;
				_payamakService.SendByLookupKaveNegar(sendPayamak as SendKaveNegar, regTemplate2);
			}
			else
			{
				_payamakService.Send(sendPayamak);
			}
			break;
		}
		case SmsPanel.MelliPayamak:
		case SmsPanel.JetAqsaat_MelliPayamak:
		{
			int regBaseNumber = _melliPayamakConfig.RegBaseNumber;
			BaseSendPayamak sendPayamak = new SendMelliPayamak
			{
				To = to,
				Text = "رمز ورود به اپلیکیشن : " + code + "\r\n" + _appConfig.FullTitle
			};
			if (regBaseNumber != 0)
			{
				sendPayamak.Text = code;
				_payamakService.SendByBaseNumber(sendPayamak, regBaseNumber);
			}
			else
			{
				_payamakService.Send(sendPayamak);
			}
			break;
		}
		case SmsPanel.Bale:
		{
			BaseSendPayamak sendPayamak = new SendBale
			{
				To = to,
				Text = "رمز ورود به اپلیکیشن : " + code + "\r\n" + _appConfig.FullTitle
			};
			_payamakService.Send(sendPayamak);
			break;
		}
		}
	}

	public async Task SendMemberRegisterationCode(string to, string code, Guid customerId, bool insertAppSms = false)
	{
		if (_hostingEnvironment.EnvironmentName == "Development")
		{
			return;
		}
		_currentUserService.CustomerId = customerId;
		AppSettings appSettings = await _appSettingsService.GetAppSettings();
		SmsPanel smsPanel = await SetPayamakService(appSettings);
		if (_payamakService == null)
		{
			throw new CustomException("سرویس پیامک یافت نشد");
		}
		if (appSettings?.RegisterOtpSmsPattern == null)
		{
			throw new CustomException("قالب پیامک رمز تاییدیه تعیین نشده است.");
		}
		if (string.IsNullOrEmpty(to))
		{
			throw new CustomException("شماره موبایل دریافت کننده ثبت نشده است.");
		}
		bool isPatternBase = appSettings.RegisterOtpSmsPattern.IsPatternBase && !string.IsNullOrEmpty(appSettings.RegisterOtpSmsPattern.PatternBaseCode);
		RegisterOtpModel registerOtpModel = new RegisterOtpModel
		{
			Code = code,
			Date = DateTime.Now,
			Mobile = to
		};
		string text = await ExtractTextFromPattern(registerOtpModel, appSettings.RegisterOtpSmsPattern.Pattern, isPatternBase);
		BaseSendPayamak baseSendPayamak = null;
		decimal num = Math.Ceiling(((decimal)text.Length + 6m) / 67m) * (decimal)_appConfig.BaseSmsPrice;
		if (smsPanel.ToString().ToLower().Contains("jet") && (decimal)appSettings.SmsCredit < num && _appConfig.IsCloud)
		{
			throw new CustomException("میزان اعتبار شارژ کافی نمی باشد.");
		}
		ResponsePayamak sendResult = new ResponsePayamak();
		switch (smsPanel)
		{
		case SmsPanel.IPPanel:
		case SmsPanel.JetAqsaat_IPPanel:
			baseSendPayamak = new SendIPPanelPattern
			{
				To = to,
				Text = text
			};
			if (isPatternBase)
			{
				PrepareIPPanelKeys(baseSendPayamak as SendIPPanelPattern, text, appSettings.RegisterOtpSmsPattern.Pattern, appSettings.RegisterOtpSmsPattern.IppanelKeysJson);
				sendResult = _payamakService.SendByPatternIPPanel(baseSendPayamak as SendIPPanelPattern, appSettings.RegisterOtpSmsPattern.PatternBaseCode);
			}
			else
			{
				sendResult = _payamakService.Send(baseSendPayamak);
			}
			break;
		case SmsPanel.WebOne:
			baseSendPayamak = new SendWebOnePattern
			{
				To = to,
				Text = text
			};
			if (isPatternBase)
			{
				sendResult = _payamakService.SendByPatternWebOne(baseSendPayamak as SendWebOnePattern, appSettings.RegisterOtpSmsPattern.PatternBaseCode);
			}
			else
			{
				sendResult = _payamakService.Send(baseSendPayamak);
			}
			break;
		case SmsPanel.KaveNegar:
		case SmsPanel.JetAqsaat_KaveNegar:
			baseSendPayamak = new SendKaveNegar
			{
				To = to,
				Text = text
			};
			if (isPatternBase)
			{
				PrepareKavehTokens(baseSendPayamak as SendKaveNegar, text);
				sendResult = _payamakService.SendByLookupKaveNegar(baseSendPayamak as SendKaveNegar, appSettings.RegisterOtpSmsPattern.PatternBaseCode);
			}
			else
			{
				sendResult = _payamakService.Send(baseSendPayamak);
			}
			break;
		case SmsPanel.MelliPayamak:
		case SmsPanel.JetAqsaat_MelliPayamak:
		{
			baseSendPayamak = new SendMelliPayamak
			{
				To = to,
				Text = text
			};
			if (isPatternBase && int.TryParse(appSettings.RegisterOtpSmsPattern.PatternBaseCode, out var result))
			{
				PrepareMelliPayamakTextPattern(baseSendPayamak as SendMelliPayamak, text);
				sendResult = _payamakService.SendByBaseNumber(baseSendPayamak, result);
			}
			else
			{
				sendResult = _payamakService.Send(baseSendPayamak);
			}
			break;
		}
		case SmsPanel.Bale:
			baseSendPayamak = new SendBale
			{
				To = to,
				Text = text
			};
			sendResult = _payamakService.Send(baseSendPayamak);
			break;
		}
		if (!insertAppSms)
		{
			return;
		}
		AppSms appSms = new AppSms
		{
			AppSmsType = AppSmsType.VerifyCode,
			Date = DateTime.Now,
			Price = num,
			Status = (sendResult.Success ? AppSmsStatus.Pending : AppSmsStatus.Failed),
			Text = baseSendPayamak.Text,
			SmsPatternTitle = "کد تاییدیه شماره موبایل",
			To = to,
			EntityType = "",
			EntityId = Guid.Empty,
			SmsPanel = smsPanel,
			SMSKey = sendResult.Key,
			Tag = code
		};
		await _appSmsService.Insert(appSms);
		if (sendResult.Success)
		{
			await _appSettingsService.SyncChargeSmsCredit();
			BackgroundJob.Schedule(() => CheckDelivery(sendResult.Key, appSms.Id, (Guid)_currentUserService.CustomerId, _appConfig.IsCloud), TimeSpan.FromMinutes(30.0));
		}
	}

	public async Task SendDigitalSignOtp(string to, string code, UserLoanRequestAuthorizationItemModel model, Guid customerId, bool insertAppSms = false)
	{
		if (_hostingEnvironment.EnvironmentName == "Development")
		{
			return;
		}
		_currentUserService.CustomerId = customerId;
		AppSettings appSettings = await _appSettingsService.GetAppSettings();
		SmsPanel smsPanel = await SetPayamakService(appSettings);
		if (_payamakService == null)
		{
			throw new CustomException("سرویس پیامک یافت نشد");
		}
		if (appSettings?.DigitalSignOtpSmsPattern == null)
		{
			throw new CustomException("قالب پیامک رمز تاییدیه تعیین نشده است.");
		}
		if (string.IsNullOrEmpty(to))
		{
			throw new CustomException("شماره موبایل دریافت کننده ثبت نشده است.");
		}
		bool isPatternBase = appSettings.DigitalSignOtpSmsPattern.IsPatternBase && !string.IsNullOrEmpty(appSettings.DigitalSignOtpSmsPattern.PatternBaseCode);
		string text = await ExtractTextFromPattern(model, code, appSettings.DigitalSignOtpSmsPattern.Pattern, isPatternBase);
		BaseSendPayamak baseSendPayamak = null;
		decimal num = Math.Ceiling(((decimal)text.Length + 6m) / 67m) * (decimal)_appConfig.BaseSmsPrice;
		if (smsPanel.ToString().ToLower().Contains("jet") && (decimal)appSettings.SmsCredit < num && _appConfig.IsCloud)
		{
			throw new CustomException("میزان اعتبار شارژ کافی نمی باشد.");
		}
		ResponsePayamak sendResult = new ResponsePayamak();
		switch (smsPanel)
		{
		case SmsPanel.IPPanel:
		case SmsPanel.JetAqsaat_IPPanel:
			baseSendPayamak = new SendIPPanelPattern
			{
				To = to,
				Text = text
			};
			if (isPatternBase)
			{
				PrepareIPPanelKeys(baseSendPayamak as SendIPPanelPattern, text, appSettings.DigitalSignOtpSmsPattern.Pattern, appSettings.DigitalSignOtpSmsPattern.IppanelKeysJson);
				sendResult = _payamakService.SendByPatternIPPanel(baseSendPayamak as SendIPPanelPattern, appSettings.DigitalSignOtpSmsPattern.PatternBaseCode);
			}
			else
			{
				sendResult = _payamakService.Send(baseSendPayamak);
			}
			break;
		case SmsPanel.WebOne:
			baseSendPayamak = new SendWebOnePattern
			{
				To = to,
				Text = text
			};
			if (isPatternBase)
			{
				sendResult = _payamakService.SendByPatternWebOne(baseSendPayamak as SendWebOnePattern, appSettings.DigitalSignOtpSmsPattern.PatternBaseCode);
			}
			else
			{
				sendResult = _payamakService.Send(baseSendPayamak);
			}
			break;
		case SmsPanel.KaveNegar:
		case SmsPanel.JetAqsaat_KaveNegar:
			baseSendPayamak = new SendKaveNegar
			{
				To = to,
				Text = text
			};
			if (isPatternBase)
			{
				PrepareKavehTokens(baseSendPayamak as SendKaveNegar, text);
				sendResult = _payamakService.SendByLookupKaveNegar(baseSendPayamak as SendKaveNegar, appSettings.DigitalSignOtpSmsPattern.PatternBaseCode);
			}
			else
			{
				sendResult = _payamakService.Send(baseSendPayamak);
			}
			break;
		case SmsPanel.MelliPayamak:
		case SmsPanel.JetAqsaat_MelliPayamak:
		{
			baseSendPayamak = new SendMelliPayamak
			{
				To = to,
				Text = text
			};
			if (isPatternBase && int.TryParse(appSettings.DigitalSignOtpSmsPattern.PatternBaseCode, out var result))
			{
				PrepareMelliPayamakTextPattern(baseSendPayamak as SendMelliPayamak, text);
				sendResult = _payamakService.SendByBaseNumber(baseSendPayamak, result);
			}
			else
			{
				sendResult = _payamakService.Send(baseSendPayamak);
			}
			break;
		}
		case SmsPanel.Bale:
			baseSendPayamak = new SendBale
			{
				To = to,
				Text = text
			};
			sendResult = _payamakService.Send(baseSendPayamak);
			break;
		}
		if (!insertAppSms)
		{
			return;
		}
		AppSms appSms = new AppSms
		{
			AppSmsType = AppSmsType.DigitalSignOtp,
			Date = DateTime.Now,
			Price = num,
			Status = (sendResult.Success ? AppSmsStatus.Pending : AppSmsStatus.Failed),
			Text = baseSendPayamak.Text,
			SmsPatternTitle = "کد تاییدیه امضای قرارداد",
			To = to,
			EntityType = "",
			EntityId = Guid.Empty,
			SmsPanel = smsPanel,
			SMSKey = sendResult.Key,
			Tag = code
		};
		await _appSmsService.Insert(appSms);
		if (sendResult.Success)
		{
			await _appSettingsService.SyncChargeSmsCredit();
			BackgroundJob.Schedule(() => CheckDelivery(sendResult.Key, appSms.Id, (Guid)_currentUserService.CustomerId, _appConfig.IsCloud), TimeSpan.FromMinutes(30.0));
		}
	}

	public async Task SendThirdPersonDigitalSignOtp(string to, string code, ThirdPersonAssuranceAuthorizationItemModel model, Guid customerId, bool insertAppSms = false)
	{
		if (_hostingEnvironment.EnvironmentName == "Development")
		{
			return;
		}
		_currentUserService.CustomerId = customerId;
		AppSettings appSettings = await _appSettingsService.GetAppSettings();
		SmsPanel smsPanel = await SetPayamakService(appSettings);
		if (_payamakService == null)
		{
			throw new CustomException("سرویس پیامک یافت نشد");
		}
		if (appSettings?.ThirdPersonDigitalSignOtpSmsPattern == null)
		{
			throw new CustomException("قالب پیامک رمز تاییدیه تعیین نشده است.");
		}
		if (string.IsNullOrEmpty(to))
		{
			throw new CustomException("شماره موبایل دریافت کننده ثبت نشده است.");
		}
		bool isPatternBase = appSettings.ThirdPersonDigitalSignOtpSmsPattern.IsPatternBase && !string.IsNullOrEmpty(appSettings.ThirdPersonDigitalSignOtpSmsPattern.PatternBaseCode);
		string text = await ExtractTextFromPattern(model, code, appSettings.ThirdPersonDigitalSignOtpSmsPattern.Pattern, isPatternBase);
		BaseSendPayamak baseSendPayamak = null;
		decimal num = Math.Ceiling(((decimal)text.Length + 6m) / 67m) * (decimal)_appConfig.BaseSmsPrice;
		if (smsPanel.ToString().ToLower().Contains("jet") && (decimal)appSettings.SmsCredit < num && _appConfig.IsCloud)
		{
			throw new CustomException("میزان اعتبار شارژ کافی نمی باشد.");
		}
		ResponsePayamak sendResult = new ResponsePayamak();
		switch (smsPanel)
		{
		case SmsPanel.IPPanel:
		case SmsPanel.JetAqsaat_IPPanel:
			baseSendPayamak = new SendIPPanelPattern
			{
				To = to,
				Text = text
			};
			if (isPatternBase)
			{
				PrepareIPPanelKeys(baseSendPayamak as SendIPPanelPattern, text, appSettings.ThirdPersonDigitalSignOtpSmsPattern.Pattern, appSettings.DigitalSignOtpSmsPattern.IppanelKeysJson);
				sendResult = _payamakService.SendByPatternIPPanel(baseSendPayamak as SendIPPanelPattern, appSettings.ThirdPersonDigitalSignOtpSmsPattern.PatternBaseCode);
			}
			else
			{
				sendResult = _payamakService.Send(baseSendPayamak);
			}
			break;
		case SmsPanel.WebOne:
			baseSendPayamak = new SendWebOnePattern
			{
				To = to,
				Text = text
			};
			if (isPatternBase)
			{
				sendResult = _payamakService.SendByPatternWebOne(baseSendPayamak as SendWebOnePattern, appSettings.ThirdPersonDigitalSignOtpSmsPattern.PatternBaseCode);
			}
			else
			{
				sendResult = _payamakService.Send(baseSendPayamak);
			}
			break;
		case SmsPanel.KaveNegar:
		case SmsPanel.JetAqsaat_KaveNegar:
			baseSendPayamak = new SendKaveNegar
			{
				To = to,
				Text = text
			};
			if (isPatternBase)
			{
				PrepareKavehTokens(baseSendPayamak as SendKaveNegar, text);
				sendResult = _payamakService.SendByLookupKaveNegar(baseSendPayamak as SendKaveNegar, appSettings.ThirdPersonDigitalSignOtpSmsPattern.PatternBaseCode);
			}
			else
			{
				sendResult = _payamakService.Send(baseSendPayamak);
			}
			break;
		case SmsPanel.MelliPayamak:
		case SmsPanel.JetAqsaat_MelliPayamak:
		{
			baseSendPayamak = new SendMelliPayamak
			{
				To = to,
				Text = text
			};
			if (isPatternBase && int.TryParse(appSettings.ThirdPersonDigitalSignOtpSmsPattern.PatternBaseCode, out var result))
			{
				PrepareMelliPayamakTextPattern(baseSendPayamak as SendMelliPayamak, text);
				sendResult = _payamakService.SendByBaseNumber(baseSendPayamak, result);
			}
			else
			{
				sendResult = _payamakService.Send(baseSendPayamak);
			}
			break;
		}
		case SmsPanel.Bale:
			baseSendPayamak = new SendBale
			{
				To = to,
				Text = text
			};
			sendResult = _payamakService.Send(baseSendPayamak);
			break;
		}
		if (!insertAppSms)
		{
			return;
		}
		AppSms appSms = new AppSms
		{
			AppSmsType = AppSmsType.ThirdPersonDigitalSignOtp,
			Date = DateTime.Now,
			Price = num,
			Status = (sendResult.Success ? AppSmsStatus.Pending : AppSmsStatus.Failed),
			Text = baseSendPayamak.Text,
			SmsPatternTitle = "کد تاییدیه امضای قرارداد ضامن",
			To = to,
			EntityType = "",
			EntityId = Guid.Empty,
			SmsPanel = smsPanel,
			SMSKey = sendResult.Key,
			Tag = code
		};
		await _appSmsService.Insert(appSms);
		if (sendResult.Success)
		{
			await _appSettingsService.SyncChargeSmsCredit();
			BackgroundJob.Schedule(() => CheckDelivery(sendResult.Key, appSms.Id, (Guid)_currentUserService.CustomerId, _appConfig.IsCloud), TimeSpan.FromMinutes(30.0));
		}
	}

	public async Task SendLoginOTPCode(string to, string otpCode, Guid customerId, bool insertAppSms = false)
	{
		if (_hostingEnvironment.EnvironmentName == "Development")
		{
			return;
		}
		_currentUserService.CustomerId = customerId;
		AppSettings appSettings = await _appSettingsService.GetAppSettings();
		SmsPanel smsPanel = await SetPayamakService(appSettings);
		if (_payamakService == null)
		{
			throw new CustomException("سرویس پیامک یافت نشد");
		}
		if (appSettings?.RegisterOtpSmsPattern == null)
		{
			throw new CustomException("قالب پیامک رمز تاییدیه تعیین نشده است.");
		}
		if (string.IsNullOrEmpty(to))
		{
			throw new CustomException("شماره موبایل دریافت کننده ثبت نشده است.");
		}
		bool isPatternBase = appSettings.RegisterOtpSmsPattern.IsPatternBase && !string.IsNullOrEmpty(appSettings.RegisterOtpSmsPattern.PatternBaseCode);
		RegisterOtpModel registerOtpModel = new RegisterOtpModel
		{
			Code = otpCode,
			Date = DateTime.Now,
			Mobile = to
		};
		string text = await ExtractTextFromPattern(registerOtpModel, appSettings.RegisterOtpSmsPattern.Pattern, isPatternBase);
		BaseSendPayamak baseSendPayamak = null;
		decimal num = Math.Ceiling(((decimal)text.Length + 6m) / 67m) * (decimal)_appConfig.BaseSmsPrice;
		if (smsPanel.ToString().ToLower().Contains("jet") && (decimal)appSettings.SmsCredit < num && _appConfig.IsCloud)
		{
			throw new CustomException("میزان اعتبار شارژ کافی نمی باشد.");
		}
		ResponsePayamak sendResult = new ResponsePayamak();
		switch (smsPanel)
		{
		case SmsPanel.IPPanel:
		case SmsPanel.JetAqsaat_IPPanel:
			baseSendPayamak = new SendIPPanelPattern
			{
				To = to,
				Text = text
			};
			if (isPatternBase)
			{
				PrepareIPPanelKeys(baseSendPayamak as SendIPPanelPattern, text, appSettings.RegisterOtpSmsPattern.Pattern, appSettings.RegisterOtpSmsPattern.IppanelKeysJson);
				sendResult = _payamakService.SendByPatternIPPanel(baseSendPayamak as SendIPPanelPattern, appSettings.RegisterOtpSmsPattern.PatternBaseCode);
			}
			else
			{
				sendResult = _payamakService.Send(baseSendPayamak);
			}
			break;
		case SmsPanel.WebOne:
			baseSendPayamak = new SendWebOnePattern
			{
				To = to,
				Text = text
			};
			if (isPatternBase)
			{
				sendResult = _payamakService.SendByPatternWebOne(baseSendPayamak as SendWebOnePattern, appSettings.RegisterOtpSmsPattern.PatternBaseCode);
			}
			else
			{
				sendResult = _payamakService.Send(baseSendPayamak);
			}
			break;
		case SmsPanel.KaveNegar:
		case SmsPanel.JetAqsaat_KaveNegar:
			baseSendPayamak = new SendKaveNegar
			{
				To = to,
				Text = text
			};
			if (isPatternBase)
			{
				PrepareKavehTokens(baseSendPayamak as SendKaveNegar, text);
				sendResult = _payamakService.SendByLookupKaveNegar(baseSendPayamak as SendKaveNegar, appSettings.RegisterOtpSmsPattern.PatternBaseCode);
			}
			else
			{
				sendResult = _payamakService.Send(baseSendPayamak);
			}
			break;
		case SmsPanel.MelliPayamak:
		case SmsPanel.JetAqsaat_MelliPayamak:
		{
			baseSendPayamak = new SendMelliPayamak
			{
				To = to,
				Text = text
			};
			if (isPatternBase && int.TryParse(appSettings.RegisterOtpSmsPattern.PatternBaseCode, out var result))
			{
				PrepareMelliPayamakTextPattern(baseSendPayamak as SendMelliPayamak, text);
				sendResult = _payamakService.SendByBaseNumber(baseSendPayamak, result);
			}
			else
			{
				sendResult = _payamakService.Send(baseSendPayamak);
			}
			break;
		}
		case SmsPanel.Bale:
			baseSendPayamak = new SendBale
			{
				To = to,
				Text = text
			};
			sendResult = _payamakService.Send(baseSendPayamak);
			break;
		}
		if (!insertAppSms)
		{
			return;
		}
		AppSms appSms = new AppSms
		{
			AppSmsType = AppSmsType.LoginOTP,
			Date = DateTime.Now,
			Price = Math.Ceiling(((decimal)text.Length + 6m) / 67m) * (decimal)_appConfig.BaseSmsPrice,
			Status = (sendResult.Success ? AppSmsStatus.Pending : AppSmsStatus.Failed),
			Text = baseSendPayamak.Text,
			SmsPatternTitle = "کد تاییدیه شماره موبایل",
			To = to,
			EntityType = "",
			EntityId = Guid.Empty,
			SmsPanel = smsPanel,
			SMSKey = sendResult.Key,
			Tag = otpCode
		};
		await _appSmsService.Insert(appSms);
		if (sendResult.Success)
		{
			await _appSettingsService.SyncChargeSmsCredit();
			BackgroundJob.Schedule(() => CheckDelivery(sendResult.Key, appSms.Id, (Guid)_currentUserService.CustomerId, _appConfig.IsCloud), TimeSpan.FromMinutes(30.0));
		}
	}

	public async Task<AppSms> SendWalletCharge(WalletChargeModel model)
	{
		if (_hostingEnvironment.EnvironmentName == "Development")
		{
			return null;
		}
		AppSettings appSettings = await _appSettingsService.GetAppSettings();
		SmsPanel smsPanel = await SetPayamakService(appSettings);
		if (_payamakService == null)
		{
			throw new CustomException("سرویس پیامک یافت نشد");
		}
		if (appSettings?.WalletChargeSmsPattern == null)
		{
			throw new CustomException("قالب پیامک شارژ کیف پول تنظیم نشده است.");
		}
		string to = model.Member.Mobile ?? "";
		if (string.IsNullOrEmpty(to))
		{
			throw new CustomException("شماره موبایل دریافت کننده ثبت نشده است.");
		}
		bool isPatternBase = appSettings.WalletChargeSmsPattern.IsPatternBase && !string.IsNullOrEmpty(appSettings.WalletChargeSmsPattern.PatternBaseCode);
		string text = await ExtractTextFromPattern(model, appSettings.WalletChargeSmsPattern.Pattern, isPatternBase);
		decimal num = Math.Ceiling(((decimal)text.Length + 6m) / 67m) * (decimal)_appConfig.BaseSmsPrice;
		if (smsPanel.ToString().ToLower().Contains("jet") && (decimal)appSettings.SmsCredit < num && _appConfig.IsCloud)
		{
			throw new CustomException("میزان اعتبار شارژ کافی نمی باشد.");
		}
		ResponsePayamak sendResult = new ResponsePayamak();
		switch (smsPanel)
		{
		case SmsPanel.IPPanel:
		case SmsPanel.JetAqsaat_IPPanel:
		{
			BaseSendPayamak sendPayamak = new SendIPPanelPattern
			{
				To = to,
				Text = text
			};
			if (isPatternBase)
			{
				PrepareIPPanelKeys(sendPayamak as SendIPPanelPattern, text, appSettings.WalletChargeSmsPattern.Pattern, appSettings.WalletChargeSmsPattern.IppanelKeysJson);
				sendResult = _payamakService.SendByPatternIPPanel(sendPayamak as SendIPPanelPattern, appSettings.WalletChargeSmsPattern.PatternBaseCode);
			}
			else
			{
				sendResult = _payamakService.Send(sendPayamak);
			}
			break;
		}
		case SmsPanel.WebOne:
		{
			BaseSendPayamak sendPayamak = new SendWebOnePattern
			{
				To = to,
				Text = text
			};
			if (isPatternBase)
			{
				sendResult = _payamakService.SendByPatternWebOne(sendPayamak as SendWebOnePattern, appSettings.WalletChargeSmsPattern.PatternBaseCode);
			}
			else
			{
				sendResult = _payamakService.Send(sendPayamak);
			}
			break;
		}
		case SmsPanel.KaveNegar:
		case SmsPanel.JetAqsaat_KaveNegar:
		{
			BaseSendPayamak sendPayamak = new SendKaveNegar
			{
				To = to,
				Text = text
			};
			if (isPatternBase)
			{
				PrepareKavehTokens(sendPayamak as SendKaveNegar, text);
				sendResult = _payamakService.SendByLookupKaveNegar(sendPayamak as SendKaveNegar, appSettings.WalletChargeSmsPattern.PatternBaseCode);
			}
			else
			{
				sendResult = _payamakService.Send(sendPayamak);
			}
			break;
		}
		case SmsPanel.MelliPayamak:
		case SmsPanel.JetAqsaat_MelliPayamak:
		{
			BaseSendPayamak sendPayamak = new SendMelliPayamak
			{
				To = to,
				Text = text
			};
			if (isPatternBase && int.TryParse(appSettings.WalletChargeSmsPattern.PatternBaseCode, out var result))
			{
				PrepareMelliPayamakTextPattern(sendPayamak as SendMelliPayamak, text);
				sendResult = _payamakService.SendByBaseNumber(sendPayamak, result);
			}
			else
			{
				sendResult = _payamakService.Send(sendPayamak);
			}
			break;
		}
		case SmsPanel.Bale:
		{
			BaseSendPayamak sendPayamak = new SendBale
			{
				To = to,
				Text = text
			};
			sendResult = _payamakService.Send(sendPayamak);
			break;
		}
		}
		AppSms appSms = new AppSms
		{
			AppSmsType = AppSmsType.WalletCharge,
			Date = DateTime.Now,
			Price = num,
			Status = (sendResult.Success ? AppSmsStatus.Pending : AppSmsStatus.Failed),
			Text = text,
			SmsPatternTitle = appSettings.WalletChargeSmsPattern.Title,
			To = to,
			EntityType = "",
			EntityId = Guid.Empty,
			SmsPanel = smsPanel,
			SMSKey = sendResult.Key
		};
		await _appSmsService.Insert(appSms);
		if (sendResult.Success)
		{
			await _appSettingsService.SyncChargeSmsCredit();
			BackgroundJob.Schedule(() => CheckDelivery(sendResult.Key, appSms.Id, (Guid)_currentUserService.CustomerId, _appConfig.IsCloud), TimeSpan.FromMinutes(30.0));
		}
		return appSms;
	}

	public async Task<AppSms> SendAgentFactorPaymentLink(AgentFactorModel model)
	{
		if (_hostingEnvironment.EnvironmentName == "Development")
		{
			return null;
		}
		AppSettings appSettings = await _appSettingsService.GetAppSettings();
		SmsPanel smsPanel = await SetPayamakService(appSettings);
		if (_payamakService == null)
		{
			throw new CustomException("سرویس پیامک یافت نشد");
		}
		if (appSettings?.AgentFactorPaymentLinkSmsPattern == null)
		{
			throw new CustomException("قالب پیامک لینک پرداخت صورتحساب تنظیم نشده است.");
		}
		string to = model.Member.Mobile ?? "";
		if (string.IsNullOrEmpty(to))
		{
			throw new CustomException("شماره موبایل دریافت کننده ثبت نشده است.");
		}
		bool isPatternBase = appSettings.AgentFactorPaymentLinkSmsPattern.IsPatternBase && !string.IsNullOrEmpty(appSettings.AgentFactorPaymentLinkSmsPattern.PatternBaseCode);
		string text = await ExtractTextFromPattern(model, appSettings.AgentFactorPaymentLinkSmsPattern.Pattern, isPatternBase);
		decimal num = Math.Ceiling(((decimal)text.Length + 6m) / 67m) * (decimal)_appConfig.BaseSmsPrice;
		if (smsPanel.ToString().ToLower().Contains("jet") && (decimal)appSettings.SmsCredit < num && _appConfig.IsCloud)
		{
			throw new CustomException("میزان اعتبار شارژ کافی نمی باشد.");
		}
		ResponsePayamak sendResult = new ResponsePayamak();
		switch (smsPanel)
		{
		case SmsPanel.IPPanel:
		case SmsPanel.JetAqsaat_IPPanel:
		{
			BaseSendPayamak sendPayamak = new SendIPPanelPattern
			{
				To = to,
				Text = text
			};
			if (isPatternBase)
			{
				PrepareIPPanelKeys(sendPayamak as SendIPPanelPattern, text, appSettings.AgentFactorPaymentLinkSmsPattern.Pattern, appSettings.AgentFactorPaymentLinkSmsPattern.IppanelKeysJson);
				sendResult = _payamakService.SendByPatternIPPanel(sendPayamak as SendIPPanelPattern, appSettings.AgentFactorPaymentLinkSmsPattern.PatternBaseCode);
			}
			else
			{
				sendResult = _payamakService.Send(sendPayamak);
			}
			break;
		}
		case SmsPanel.WebOne:
		{
			BaseSendPayamak sendPayamak = new SendWebOnePattern
			{
				To = to,
				Text = text
			};
			if (isPatternBase)
			{
				sendResult = _payamakService.SendByPatternWebOne(sendPayamak as SendWebOnePattern, appSettings.AgentFactorPaymentLinkSmsPattern.PatternBaseCode);
			}
			else
			{
				sendResult = _payamakService.Send(sendPayamak);
			}
			break;
		}
		case SmsPanel.KaveNegar:
		case SmsPanel.JetAqsaat_KaveNegar:
		{
			BaseSendPayamak sendPayamak = new SendKaveNegar
			{
				To = to,
				Text = text
			};
			if (isPatternBase)
			{
				PrepareKavehTokens(sendPayamak as SendKaveNegar, text);
				sendResult = _payamakService.SendByLookupKaveNegar(sendPayamak as SendKaveNegar, appSettings.AgentFactorPaymentLinkSmsPattern.PatternBaseCode);
			}
			else
			{
				sendResult = _payamakService.Send(sendPayamak);
			}
			break;
		}
		case SmsPanel.MelliPayamak:
		case SmsPanel.JetAqsaat_MelliPayamak:
		{
			BaseSendPayamak sendPayamak = new SendMelliPayamak
			{
				To = to,
				Text = text
			};
			if (isPatternBase && int.TryParse(appSettings.AgentFactorPaymentLinkSmsPattern.PatternBaseCode, out var result))
			{
				PrepareMelliPayamakTextPattern(sendPayamak as SendMelliPayamak, text);
				sendResult = _payamakService.SendByBaseNumber(sendPayamak, result);
			}
			else
			{
				sendResult = _payamakService.Send(sendPayamak);
			}
			break;
		}
		case SmsPanel.Bale:
		{
			BaseSendPayamak sendPayamak = new SendBale
			{
				To = to,
				Text = text
			};
			sendResult = _payamakService.Send(sendPayamak);
			break;
		}
		}
		AppSms appSms = new AppSms
		{
			AppSmsType = AppSmsType.AgentFactorPaymentLink,
			Date = DateTime.Now,
			Price = num,
			Status = (sendResult.Success ? AppSmsStatus.Pending : AppSmsStatus.Failed),
			Text = text,
			SmsPatternTitle = appSettings.AgentFactorPaymentLinkSmsPattern.Title,
			To = to,
			EntityType = "AgentFactor",
			EntityId = model.Id,
			SmsPanel = smsPanel,
			SMSKey = sendResult.Key
		};
		await _appSmsService.Insert(appSms);
		if (sendResult.Success)
		{
			await _appSettingsService.SyncChargeSmsCredit();
			BackgroundJob.Schedule(() => CheckDelivery(sendResult.Key, appSms.Id, (Guid)_currentUserService.CustomerId, _appConfig.IsCloud), TimeSpan.FromMinutes(30.0));
		}
		return appSms;
	}

	public async Task<AppSms> SendDigitalSignLink(UserLoanRequestAuthorizationItemModel model)
	{
		if (_hostingEnvironment.EnvironmentName == "Development")
		{
			return null;
		}
		AppSettings appSettings = await _appSettingsService.GetAppSettings();
		SmsPanel smsPanel = await SetPayamakService(appSettings);
		if (_payamakService == null)
		{
			throw new CustomException("سرویس پیامک یافت نشد");
		}
		if (appSettings?.DigitalSignLinkSmsPattern == null)
		{
			throw new CustomException("قالب پیامک لینک قرارداد تنظیم نشده است.");
		}
		string to = model.UserLoanRequest?.MemberModel?.Mobile ?? "";
		if (string.IsNullOrEmpty(to))
		{
			throw new CustomException("شماره موبایل دریافت کننده ثبت نشده است.");
		}
		bool isPatternBase = appSettings.DigitalSignLinkSmsPattern.IsPatternBase && !string.IsNullOrEmpty(appSettings.DigitalSignLinkSmsPattern.PatternBaseCode);
		string text = await ExtractTextFromPattern(model, "", appSettings.DigitalSignLinkSmsPattern.Pattern, isPatternBase);
		decimal num = Math.Ceiling(((decimal)text.Length + 6m) / 67m) * (decimal)_appConfig.BaseSmsPrice;
		if (smsPanel.ToString().ToLower().Contains("jet") && (decimal)appSettings.SmsCredit < num && _appConfig.IsCloud)
		{
			throw new CustomException("میزان اعتبار شارژ کافی نمی باشد.");
		}
		ResponsePayamak sendResult = new ResponsePayamak();
		switch (smsPanel)
		{
		case SmsPanel.IPPanel:
		case SmsPanel.JetAqsaat_IPPanel:
		{
			BaseSendPayamak sendPayamak = new SendIPPanelPattern
			{
				To = to,
				Text = text
			};
			if (isPatternBase)
			{
				PrepareIPPanelKeys(sendPayamak as SendIPPanelPattern, text, appSettings.DigitalSignLinkSmsPattern.Pattern, appSettings.DigitalSignLinkSmsPattern.IppanelKeysJson);
				sendResult = _payamakService.SendByPatternIPPanel(sendPayamak as SendIPPanelPattern, appSettings.DigitalSignLinkSmsPattern.PatternBaseCode);
			}
			else
			{
				sendResult = _payamakService.Send(sendPayamak);
			}
			break;
		}
		case SmsPanel.WebOne:
		{
			BaseSendPayamak sendPayamak = new SendWebOnePattern
			{
				To = to,
				Text = text
			};
			if (isPatternBase)
			{
				sendResult = _payamakService.SendByPatternWebOne(sendPayamak as SendWebOnePattern, appSettings.DigitalSignLinkSmsPattern.PatternBaseCode);
			}
			else
			{
				sendResult = _payamakService.Send(sendPayamak);
			}
			break;
		}
		case SmsPanel.KaveNegar:
		case SmsPanel.JetAqsaat_KaveNegar:
		{
			BaseSendPayamak sendPayamak = new SendKaveNegar
			{
				To = to,
				Text = text
			};
			if (isPatternBase)
			{
				PrepareKavehTokens(sendPayamak as SendKaveNegar, text);
				sendResult = _payamakService.SendByLookupKaveNegar(sendPayamak as SendKaveNegar, appSettings.DigitalSignLinkSmsPattern.PatternBaseCode);
			}
			else
			{
				sendResult = _payamakService.Send(sendPayamak);
			}
			break;
		}
		case SmsPanel.MelliPayamak:
		case SmsPanel.JetAqsaat_MelliPayamak:
		{
			BaseSendPayamak sendPayamak = new SendMelliPayamak
			{
				To = to,
				Text = text
			};
			if (isPatternBase && int.TryParse(appSettings.DigitalSignLinkSmsPattern.PatternBaseCode, out var result))
			{
				PrepareMelliPayamakTextPattern(sendPayamak as SendMelliPayamak, text);
				sendResult = _payamakService.SendByBaseNumber(sendPayamak, result);
			}
			else
			{
				sendResult = _payamakService.Send(sendPayamak);
			}
			break;
		}
		case SmsPanel.Bale:
		{
			BaseSendPayamak sendPayamak = new SendBale
			{
				To = to,
				Text = text
			};
			sendResult = _payamakService.Send(sendPayamak);
			break;
		}
		}
		AppSms appSms = new AppSms
		{
			AppSmsType = AppSmsType.DigitalSignLink,
			Date = DateTime.Now,
			Price = num,
			Status = (sendResult.Success ? AppSmsStatus.Pending : AppSmsStatus.Failed),
			Text = text,
			SmsPatternTitle = appSettings.DigitalSignLinkSmsPattern.Title,
			To = to,
			EntityType = "UserLoanRequestAuthorizationItem",
			EntityId = model.Id,
			SmsPanel = smsPanel,
			SMSKey = sendResult.Key
		};
		await _appSmsService.Insert(appSms);
		if (sendResult.Success)
		{
			await _appSettingsService.SyncChargeSmsCredit();
			BackgroundJob.Schedule(() => CheckDelivery(sendResult.Key, appSms.Id, (Guid)_currentUserService.CustomerId, _appConfig.IsCloud), TimeSpan.FromMinutes(30.0));
		}
		return appSms;
	}

	public async Task<AppSms> SendThirdPersonDigitalSignLink(ThirdPersonAssuranceAuthorizationItemModel model)
	{
		if (_hostingEnvironment.EnvironmentName == "Development")
		{
			return null;
		}
		AppSettings appSettings = await _appSettingsService.GetAppSettings();
		SmsPanel smsPanel = await SetPayamakService(appSettings);
		if (_payamakService == null)
		{
			throw new CustomException("سرویس پیامک یافت نشد");
		}
		if (appSettings?.ThirdPersonDigitalSignLinkSmsPattern == null)
		{
			throw new CustomException("قالب پیامک لینک قرارداد تنظیم نشده است.");
		}
		string to = model.UserLoanRequestAuthorizationItem?.UserLoanRequest?.MemberModel?.Mobile ?? model.UserLoan?.MemberModel?.Mobile ?? "";
		if (string.IsNullOrEmpty(to))
		{
			throw new CustomException("شماره موبایل دریافت کننده ثبت نشده است.");
		}
		bool isPatternBase = appSettings.ThirdPersonDigitalSignLinkSmsPattern.IsPatternBase && !string.IsNullOrEmpty(appSettings.ThirdPersonDigitalSignLinkSmsPattern.PatternBaseCode);
		string text = await ExtractTextFromPattern(model, "", appSettings.ThirdPersonDigitalSignLinkSmsPattern.Pattern, isPatternBase);
		decimal num = Math.Ceiling(((decimal)text.Length + 6m) / 67m) * (decimal)_appConfig.BaseSmsPrice;
		if (smsPanel.ToString().ToLower().Contains("jet") && (decimal)appSettings.SmsCredit < num && _appConfig.IsCloud)
		{
			throw new CustomException("میزان اعتبار شارژ کافی نمی باشد.");
		}
		ResponsePayamak sendResult = new ResponsePayamak();
		switch (smsPanel)
		{
		case SmsPanel.IPPanel:
		case SmsPanel.JetAqsaat_IPPanel:
		{
			BaseSendPayamak sendPayamak = new SendIPPanelPattern
			{
				To = to,
				Text = text
			};
			if (isPatternBase)
			{
				PrepareIPPanelKeys(sendPayamak as SendIPPanelPattern, text, appSettings.ThirdPersonDigitalSignLinkSmsPattern.Pattern, appSettings.ThirdPersonDigitalSignLinkSmsPattern.IppanelKeysJson);
				sendResult = _payamakService.SendByPatternIPPanel(sendPayamak as SendIPPanelPattern, appSettings.ThirdPersonDigitalSignLinkSmsPattern.PatternBaseCode);
			}
			else
			{
				sendResult = _payamakService.Send(sendPayamak);
			}
			break;
		}
		case SmsPanel.WebOne:
		{
			BaseSendPayamak sendPayamak = new SendWebOnePattern
			{
				To = to,
				Text = text
			};
			if (isPatternBase)
			{
				sendResult = _payamakService.SendByPatternWebOne(sendPayamak as SendWebOnePattern, appSettings.ThirdPersonDigitalSignLinkSmsPattern.PatternBaseCode);
			}
			else
			{
				sendResult = _payamakService.Send(sendPayamak);
			}
			break;
		}
		case SmsPanel.KaveNegar:
		case SmsPanel.JetAqsaat_KaveNegar:
		{
			BaseSendPayamak sendPayamak = new SendKaveNegar
			{
				To = to,
				Text = text
			};
			if (isPatternBase)
			{
				PrepareKavehTokens(sendPayamak as SendKaveNegar, text);
				sendResult = _payamakService.SendByLookupKaveNegar(sendPayamak as SendKaveNegar, appSettings.ThirdPersonDigitalSignLinkSmsPattern.PatternBaseCode);
			}
			else
			{
				sendResult = _payamakService.Send(sendPayamak);
			}
			break;
		}
		case SmsPanel.MelliPayamak:
		case SmsPanel.JetAqsaat_MelliPayamak:
		{
			BaseSendPayamak sendPayamak = new SendMelliPayamak
			{
				To = to,
				Text = text
			};
			if (isPatternBase && int.TryParse(appSettings.ThirdPersonDigitalSignLinkSmsPattern.PatternBaseCode, out var result))
			{
				PrepareMelliPayamakTextPattern(sendPayamak as SendMelliPayamak, text);
				sendResult = _payamakService.SendByBaseNumber(sendPayamak, result);
			}
			else
			{
				sendResult = _payamakService.Send(sendPayamak);
			}
			break;
		}
		case SmsPanel.Bale:
		{
			BaseSendPayamak sendPayamak = new SendBale
			{
				To = to,
				Text = text
			};
			sendResult = _payamakService.Send(sendPayamak);
			break;
		}
		}
		AppSms appSms = new AppSms
		{
			AppSmsType = AppSmsType.ThirdPersonDigitalSignLink,
			Date = DateTime.Now,
			Price = num,
			Status = (sendResult.Success ? AppSmsStatus.Pending : AppSmsStatus.Failed),
			Text = text,
			SmsPatternTitle = appSettings.ThirdPersonDigitalSignLinkSmsPattern.Title,
			To = to,
			EntityType = "ThirdPersonAssuranceAuthorizationItem",
			EntityId = model.Id,
			SmsPanel = smsPanel,
			SMSKey = sendResult.Key
		};
		await _appSmsService.Insert(appSms);
		if (sendResult.Success)
		{
			await _appSettingsService.SyncChargeSmsCredit();
			BackgroundJob.Schedule(() => CheckDelivery(sendResult.Key, appSms.Id, (Guid)_currentUserService.CustomerId, _appConfig.IsCloud), TimeSpan.FromMinutes(30.0));
		}
		return appSms;
	}

	public async Task<AppSms> SendDigitalSignComplete(UserLoanRequestAuthorizationItemModel model)
	{
		if (_hostingEnvironment.EnvironmentName == "Development")
		{
			return null;
		}
		AppSettings appSettings = await _appSettingsService.GetAppSettings();
		SmsPanel smsPanel = await SetPayamakService(appSettings);
		if (_payamakService == null)
		{
			throw new CustomException("سرویس پیامک یافت نشد");
		}
		if (appSettings?.DigitalSignCompleteSmsPattern == null)
		{
			throw new CustomException("قالب پیامک تکمیل قرارداد تنظیم نشده است.");
		}
		string to = model.UserLoanRequest?.MemberModel?.Mobile ?? "";
		if (string.IsNullOrEmpty(to))
		{
			throw new CustomException("شماره موبایل دریافت کننده ثبت نشده است.");
		}
		bool isPatternBase = appSettings.DigitalSignCompleteSmsPattern.IsPatternBase && !string.IsNullOrEmpty(appSettings.DigitalSignCompleteSmsPattern.PatternBaseCode);
		string text = await ExtractTextFromPattern(model, "", appSettings.DigitalSignCompleteSmsPattern.Pattern, isPatternBase);
		decimal num = Math.Ceiling(((decimal)text.Length + 6m) / 67m) * (decimal)_appConfig.BaseSmsPrice;
		if (smsPanel.ToString().ToLower().Contains("jet") && (decimal)appSettings.SmsCredit < num && _appConfig.IsCloud)
		{
			throw new CustomException("میزان اعتبار شارژ کافی نمی باشد.");
		}
		ResponsePayamak sendResult = new ResponsePayamak();
		switch (smsPanel)
		{
		case SmsPanel.IPPanel:
		case SmsPanel.JetAqsaat_IPPanel:
		{
			BaseSendPayamak sendPayamak = new SendIPPanelPattern
			{
				To = to,
				Text = text
			};
			if (isPatternBase)
			{
				PrepareIPPanelKeys(sendPayamak as SendIPPanelPattern, text, appSettings.DigitalSignCompleteSmsPattern.Pattern, appSettings.DigitalSignCompleteSmsPattern.IppanelKeysJson);
				sendResult = _payamakService.SendByPatternIPPanel(sendPayamak as SendIPPanelPattern, appSettings.DigitalSignCompleteSmsPattern.PatternBaseCode);
			}
			else
			{
				sendResult = _payamakService.Send(sendPayamak);
			}
			break;
		}
		case SmsPanel.WebOne:
		{
			BaseSendPayamak sendPayamak = new SendWebOnePattern
			{
				To = to,
				Text = text
			};
			if (isPatternBase)
			{
				sendResult = _payamakService.SendByPatternWebOne(sendPayamak as SendWebOnePattern, appSettings.DigitalSignCompleteSmsPattern.PatternBaseCode);
			}
			else
			{
				sendResult = _payamakService.Send(sendPayamak);
			}
			break;
		}
		case SmsPanel.KaveNegar:
		case SmsPanel.JetAqsaat_KaveNegar:
		{
			BaseSendPayamak sendPayamak = new SendKaveNegar
			{
				To = to,
				Text = text
			};
			if (isPatternBase)
			{
				PrepareKavehTokens(sendPayamak as SendKaveNegar, text);
				sendResult = _payamakService.SendByLookupKaveNegar(sendPayamak as SendKaveNegar, appSettings.DigitalSignCompleteSmsPattern.PatternBaseCode);
			}
			else
			{
				sendResult = _payamakService.Send(sendPayamak);
			}
			break;
		}
		case SmsPanel.MelliPayamak:
		case SmsPanel.JetAqsaat_MelliPayamak:
		{
			BaseSendPayamak sendPayamak = new SendMelliPayamak
			{
				To = to,
				Text = text
			};
			if (isPatternBase && int.TryParse(appSettings.DigitalSignCompleteSmsPattern.PatternBaseCode, out var result))
			{
				PrepareMelliPayamakTextPattern(sendPayamak as SendMelliPayamak, text);
				sendResult = _payamakService.SendByBaseNumber(sendPayamak, result);
			}
			else
			{
				sendResult = _payamakService.Send(sendPayamak);
			}
			break;
		}
		case SmsPanel.Bale:
		{
			BaseSendPayamak sendPayamak = new SendBale
			{
				To = to,
				Text = text
			};
			sendResult = _payamakService.Send(sendPayamak);
			break;
		}
		}
		AppSms appSms = new AppSms
		{
			AppSmsType = AppSmsType.DigitalSignComplete,
			Date = DateTime.Now,
			Price = num,
			Status = (sendResult.Success ? AppSmsStatus.Pending : AppSmsStatus.Failed),
			Text = text,
			SmsPatternTitle = appSettings.DigitalSignCompleteSmsPattern.Title,
			To = to,
			EntityType = "UserLoanRequestAuthorizationItem",
			EntityId = model.Id,
			SmsPanel = smsPanel,
			SMSKey = sendResult.Key
		};
		await _appSmsService.Insert(appSms);
		if (sendResult.Success)
		{
			await _appSettingsService.SyncChargeSmsCredit();
			BackgroundJob.Schedule(() => CheckDelivery(sendResult.Key, appSms.Id, (Guid)_currentUserService.CustomerId, _appConfig.IsCloud), TimeSpan.FromMinutes(30.0));
		}
		return appSms;
	}

	public async Task<AppSms> SendThirdPersonDigitalSignComplete(ThirdPersonAssuranceAuthorizationItemModel model)
	{
		if (_hostingEnvironment.EnvironmentName == "Development")
		{
			return null;
		}
		AppSettings appSettings = await _appSettingsService.GetAppSettings();
		SmsPanel smsPanel = await SetPayamakService(appSettings);
		if (_payamakService == null)
		{
			throw new CustomException("سرویس پیامک یافت نشد");
		}
		if (appSettings?.ThirdPersonDigitalSignCompleteSmsPattern == null)
		{
			throw new CustomException("قالب پیامک تکمیل قرارداد تنظیم نشده است.");
		}
		string to = model.UserLoanRequestAuthorizationItem?.UserLoanRequest?.MemberModel?.Mobile ?? model.UserLoan?.MemberModel?.Mobile ?? "";
		if (string.IsNullOrEmpty(to))
		{
			throw new CustomException("شماره موبایل دریافت کننده ثبت نشده است.");
		}
		bool isPatternBase = appSettings.ThirdPersonDigitalSignCompleteSmsPattern.IsPatternBase && !string.IsNullOrEmpty(appSettings.ThirdPersonDigitalSignCompleteSmsPattern.PatternBaseCode);
		string text = await ExtractTextFromPattern(model, "", appSettings.ThirdPersonDigitalSignCompleteSmsPattern.Pattern, isPatternBase);
		decimal num = Math.Ceiling(((decimal)text.Length + 6m) / 67m) * (decimal)_appConfig.BaseSmsPrice;
		if (smsPanel.ToString().ToLower().Contains("jet") && (decimal)appSettings.SmsCredit < num && _appConfig.IsCloud)
		{
			throw new CustomException("میزان اعتبار شارژ کافی نمی باشد.");
		}
		ResponsePayamak sendResult = new ResponsePayamak();
		switch (smsPanel)
		{
		case SmsPanel.IPPanel:
		case SmsPanel.JetAqsaat_IPPanel:
		{
			BaseSendPayamak sendPayamak = new SendIPPanelPattern
			{
				To = to,
				Text = text
			};
			if (isPatternBase)
			{
				PrepareIPPanelKeys(sendPayamak as SendIPPanelPattern, text, appSettings.DigitalSignCompleteSmsPattern.Pattern, appSettings.DigitalSignCompleteSmsPattern.IppanelKeysJson);
				sendResult = _payamakService.SendByPatternIPPanel(sendPayamak as SendIPPanelPattern, appSettings.DigitalSignCompleteSmsPattern.PatternBaseCode);
			}
			else
			{
				sendResult = _payamakService.Send(sendPayamak);
			}
			break;
		}
		case SmsPanel.WebOne:
		{
			BaseSendPayamak sendPayamak = new SendWebOnePattern
			{
				To = to,
				Text = text
			};
			if (isPatternBase)
			{
				sendResult = _payamakService.SendByPatternWebOne(sendPayamak as SendWebOnePattern, appSettings.DigitalSignCompleteSmsPattern.PatternBaseCode);
			}
			else
			{
				sendResult = _payamakService.Send(sendPayamak);
			}
			break;
		}
		case SmsPanel.KaveNegar:
		case SmsPanel.JetAqsaat_KaveNegar:
		{
			BaseSendPayamak sendPayamak = new SendKaveNegar
			{
				To = to,
				Text = text
			};
			if (isPatternBase)
			{
				PrepareKavehTokens(sendPayamak as SendKaveNegar, text);
				sendResult = _payamakService.SendByLookupKaveNegar(sendPayamak as SendKaveNegar, appSettings.DigitalSignCompleteSmsPattern.PatternBaseCode);
			}
			else
			{
				sendResult = _payamakService.Send(sendPayamak);
			}
			break;
		}
		case SmsPanel.MelliPayamak:
		case SmsPanel.JetAqsaat_MelliPayamak:
		{
			BaseSendPayamak sendPayamak = new SendMelliPayamak
			{
				To = to,
				Text = text
			};
			if (isPatternBase && int.TryParse(appSettings.DigitalSignCompleteSmsPattern.PatternBaseCode, out var result))
			{
				PrepareMelliPayamakTextPattern(sendPayamak as SendMelliPayamak, text);
				sendResult = _payamakService.SendByBaseNumber(sendPayamak, result);
			}
			else
			{
				sendResult = _payamakService.Send(sendPayamak);
			}
			break;
		}
		case SmsPanel.Bale:
		{
			BaseSendPayamak sendPayamak = new SendBale
			{
				To = to,
				Text = text
			};
			sendResult = _payamakService.Send(sendPayamak);
			break;
		}
		}
		AppSms appSms = new AppSms
		{
			AppSmsType = AppSmsType.ThirdPersonDigitalSignComplete,
			Date = DateTime.Now,
			Price = num,
			Status = (sendResult.Success ? AppSmsStatus.Pending : AppSmsStatus.Failed),
			Text = text,
			SmsPatternTitle = appSettings.ThirdPersonDigitalSignCompleteSmsPattern.Title,
			To = to,
			EntityType = "ThirdPersonAssuranceAuthorizationItem",
			EntityId = model.Id,
			SmsPanel = smsPanel,
			SMSKey = sendResult.Key
		};
		await _appSmsService.Insert(appSms);
		if (sendResult.Success)
		{
			await _appSettingsService.SyncChargeSmsCredit();
			BackgroundJob.Schedule(() => CheckDelivery(sendResult.Key, appSms.Id, (Guid)_currentUserService.CustomerId, _appConfig.IsCloud), TimeSpan.FromMinutes(30.0));
		}
		return appSms;
	}

	public async Task<AppSms> SendNewUserLoan(UserLoanModel userLoan)
	{
		if (_hostingEnvironment.EnvironmentName == "Development")
		{
			return null;
		}
		AppSettings appSettings = await _appSettingsService.GetAppSettings();
		SmsPanel smsPanel = await SetPayamakService(appSettings);
		if (_payamakService == null)
		{
			throw new CustomException("سرویس پیامک یافت نشد");
		}
		if (appSettings?.NewUserLoanSmsPattern == null)
		{
			throw new CustomException("قالب پیامک ثبت پرونده تنظیم نشده است.");
		}
		string to = userLoan?.MemberModel?.Mobile ?? "";
		if (string.IsNullOrEmpty(to))
		{
			throw new CustomException("شماره موبایل دریافت کننده ثبت نشده است.");
		}
		bool isPatternBase = appSettings.NewUserLoanSmsPattern.IsPatternBase && !string.IsNullOrEmpty(appSettings.NewUserLoanSmsPattern.PatternBaseCode);
		string text = await ExtractTextFromPattern(userLoan, appSettings.NewUserLoanSmsPattern.Pattern, isPatternBase);
		decimal num = Math.Ceiling(((decimal)text.Length + 6m) / 67m) * (decimal)_appConfig.BaseSmsPrice;
		if (smsPanel.ToString().ToLower().Contains("jet") && (decimal)appSettings.SmsCredit < num && _appConfig.IsCloud)
		{
			throw new CustomException("میزان اعتبار شارژ کافی نمی باشد.");
		}
		ResponsePayamak sendResult = new ResponsePayamak();
		switch (smsPanel)
		{
		case SmsPanel.IPPanel:
		case SmsPanel.JetAqsaat_IPPanel:
		{
			BaseSendPayamak sendPayamak = new SendIPPanelPattern
			{
				To = to,
				Text = text
			};
			if (isPatternBase)
			{
				PrepareIPPanelKeys(sendPayamak as SendIPPanelPattern, text, appSettings.NewUserLoanSmsPattern.Pattern, appSettings.NewUserLoanSmsPattern.IppanelKeysJson);
				sendResult = _payamakService.SendByPatternIPPanel(sendPayamak as SendIPPanelPattern, appSettings.NewUserLoanSmsPattern.PatternBaseCode);
			}
			else
			{
				sendResult = _payamakService.Send(sendPayamak);
			}
			break;
		}
		case SmsPanel.WebOne:
		{
			BaseSendPayamak sendPayamak = new SendWebOnePattern
			{
				To = to,
				Text = text
			};
			if (isPatternBase)
			{
				sendResult = _payamakService.SendByPatternWebOne(sendPayamak as SendWebOnePattern, appSettings.NewUserLoanSmsPattern.PatternBaseCode);
			}
			else
			{
				sendResult = _payamakService.Send(sendPayamak);
			}
			break;
		}
		case SmsPanel.KaveNegar:
		case SmsPanel.JetAqsaat_KaveNegar:
		{
			BaseSendPayamak sendPayamak = new SendKaveNegar
			{
				To = to,
				Text = text
			};
			if (isPatternBase)
			{
				PrepareKavehTokens(sendPayamak as SendKaveNegar, text);
				sendResult = _payamakService.SendByLookupKaveNegar(sendPayamak as SendKaveNegar, appSettings.NewUserLoanSmsPattern.PatternBaseCode);
			}
			else
			{
				sendResult = _payamakService.Send(sendPayamak);
			}
			break;
		}
		case SmsPanel.MelliPayamak:
		case SmsPanel.JetAqsaat_MelliPayamak:
		{
			BaseSendPayamak sendPayamak = new SendMelliPayamak
			{
				To = to,
				Text = text
			};
			if (isPatternBase && int.TryParse(appSettings.NewUserLoanSmsPattern.PatternBaseCode, out var result))
			{
				PrepareMelliPayamakTextPattern(sendPayamak as SendMelliPayamak, text);
				sendResult = _payamakService.SendByBaseNumber(sendPayamak, result);
			}
			else
			{
				sendResult = _payamakService.Send(sendPayamak);
			}
			break;
		}
		case SmsPanel.Bale:
		{
			BaseSendPayamak sendPayamak = new SendBale
			{
				To = to,
				Text = text
			};
			sendResult = _payamakService.Send(sendPayamak);
			break;
		}
		}
		AppSms appSms = new AppSms
		{
			AppSmsType = AppSmsType.NewUserLoan,
			Date = DateTime.Now,
			Price = num,
			Status = (sendResult.Success ? AppSmsStatus.Pending : AppSmsStatus.Failed),
			Text = text,
			SmsPatternTitle = appSettings.NewUserLoanSmsPattern.Title,
			To = to,
			EntityType = "UserLoan",
			EntityId = userLoan.Id,
			SmsPanel = smsPanel,
			SMSKey = sendResult.Key
		};
		await _appSmsService.Insert(appSms);
		if (sendResult.Success)
		{
			await _appSettingsService.SyncChargeSmsCredit();
			BackgroundJob.Schedule(() => CheckDelivery(sendResult.Key, appSms.Id, (Guid)_currentUserService.CustomerId, _appConfig.IsCloud), TimeSpan.FromMinutes(30.0));
		}
		return appSms;
	}

	public async Task<AppSms> SendThanks(LoanPaymentModel loanPayment)
	{
		if (_hostingEnvironment.EnvironmentName == "Development")
		{
			return null;
		}
		AppSettings appSettings = await _appSettingsService.GetAppSettings();
		SmsPanel smsPanel = await SetPayamakService(appSettings);
		if (_payamakService == null)
		{
			throw new CustomException("سرویس پیامک یافت نشد");
		}
		if (appSettings?.ThanksPaymentSmsPattern == null)
		{
			throw new CustomException("قالب پیامک تشکر تنظیم نشده است.");
		}
		string to = loanPayment?.UserLoanModel?.MemberModel?.Mobile ?? "";
		if (string.IsNullOrEmpty(to))
		{
			throw new CustomException("شماره موبایل دریافت کننده ثبت نشده است.");
		}
		bool isPatternBase = appSettings.ThanksPaymentSmsPattern.IsPatternBase && !string.IsNullOrEmpty(appSettings.ThanksPaymentSmsPattern.PatternBaseCode);
		string text = await ExtractTextFromPattern(loanPayment, null, appSettings.ThanksPaymentSmsPattern.Pattern, isPatternBase);
		decimal num = Math.Ceiling(((decimal)text.Length + 6m) / 67m) * (decimal)_appConfig.BaseSmsPrice;
		if (smsPanel.ToString().ToLower().Contains("jet") && (decimal)appSettings.SmsCredit < num && _appConfig.IsCloud)
		{
			throw new CustomException("میزان اعتبار شارژ کافی نمی باشد.");
		}
		ResponsePayamak sendResult = new ResponsePayamak();
		switch (smsPanel)
		{
		case SmsPanel.IPPanel:
		case SmsPanel.JetAqsaat_IPPanel:
		{
			BaseSendPayamak sendPayamak = new SendIPPanelPattern
			{
				To = to,
				Text = text
			};
			if (isPatternBase)
			{
				PrepareIPPanelKeys(sendPayamak as SendIPPanelPattern, text, appSettings.ThanksPaymentSmsPattern.Pattern, appSettings.ThanksPaymentSmsPattern.IppanelKeysJson);
				sendResult = _payamakService.SendByPatternIPPanel(sendPayamak as SendIPPanelPattern, appSettings.ThanksPaymentSmsPattern.PatternBaseCode);
			}
			else
			{
				sendResult = _payamakService.Send(sendPayamak);
			}
			break;
		}
		case SmsPanel.WebOne:
		{
			BaseSendPayamak sendPayamak = new SendWebOnePattern
			{
				To = to,
				Text = text
			};
			if (isPatternBase)
			{
				sendResult = _payamakService.SendByPatternWebOne(sendPayamak as SendWebOnePattern, appSettings.ThanksPaymentSmsPattern.PatternBaseCode);
			}
			else
			{
				sendResult = _payamakService.Send(sendPayamak);
			}
			break;
		}
		case SmsPanel.KaveNegar:
		case SmsPanel.JetAqsaat_KaveNegar:
		{
			BaseSendPayamak sendPayamak = new SendKaveNegar
			{
				To = to,
				Text = text
			};
			if (isPatternBase)
			{
				PrepareKavehTokens(sendPayamak as SendKaveNegar, text);
				sendResult = _payamakService.SendByLookupKaveNegar(sendPayamak as SendKaveNegar, appSettings.ThanksPaymentSmsPattern.PatternBaseCode);
			}
			else
			{
				sendResult = _payamakService.Send(sendPayamak);
			}
			break;
		}
		case SmsPanel.MelliPayamak:
		case SmsPanel.JetAqsaat_MelliPayamak:
		{
			BaseSendPayamak sendPayamak = new SendMelliPayamak
			{
				To = to,
				Text = text
			};
			if (isPatternBase && int.TryParse(appSettings.ThanksPaymentSmsPattern.PatternBaseCode, out var result))
			{
				PrepareMelliPayamakTextPattern(sendPayamak as SendMelliPayamak, text);
				sendResult = _payamakService.SendByBaseNumber(sendPayamak, result);
			}
			else
			{
				sendResult = _payamakService.Send(sendPayamak);
			}
			break;
		}
		case SmsPanel.Bale:
		{
			BaseSendPayamak sendPayamak = new SendBale
			{
				To = to,
				Text = text
			};
			sendResult = _payamakService.Send(sendPayamak);
			break;
		}
		}
		AppSms appSms = new AppSms
		{
			AppSmsType = AppSmsType.Thanks,
			Date = DateTime.Now,
			Price = num,
			Status = (sendResult.Success ? AppSmsStatus.Pending : AppSmsStatus.Failed),
			Text = text,
			SmsPatternTitle = appSettings.ThanksPaymentSmsPattern.Title,
			To = to,
			EntityType = "UserLoanPayment",
			EntityId = loanPayment.Id,
			SmsPanel = smsPanel,
			SMSKey = sendResult.Key
		};
		await _appSmsService.Insert(appSms);
		if (sendResult.Success)
		{
			await _appSettingsService.SyncChargeSmsCredit();
			BackgroundJob.Schedule(() => CheckDelivery(sendResult.Key, appSms.Id, (Guid)_currentUserService.CustomerId, _appConfig.IsCloud), TimeSpan.FromMinutes(30.0));
		}
		return appSms;
	}

	public async Task<AppSms> SendDueDate(LoanPaymentModel loanPayment)
	{
		if (_hostingEnvironment.EnvironmentName == "Development")
		{
			return null;
		}
		AppSettings appSettings = await _appSettingsService.GetAppSettings();
		SmsPanel smsPanel = await SetPayamakService(appSettings);
		if (_payamakService == null)
		{
			throw new CustomException("سرویس پیامک یافت نشد");
		}
		if (appSettings?.DueDateRememberSmsPattern == null)
		{
			throw new CustomException("قالب پیامک یادآوری سرسید تنظیم نشده است.");
		}
		string to = loanPayment?.UserLoanModel?.MemberModel?.Mobile ?? "";
		if (string.IsNullOrEmpty(to))
		{
			throw new CustomException("شماره موبایل دریافت کننده ثبت نشده است.");
		}
		bool isPatternBase = appSettings.DueDateRememberSmsPattern.IsPatternBase && !string.IsNullOrEmpty(appSettings.DueDateRememberSmsPattern.PatternBaseCode);
		string text = await ExtractTextFromPattern(loanPayment, null, appSettings.DueDateRememberSmsPattern.Pattern, isPatternBase);
		decimal num = Math.Ceiling(((decimal)text.Length + 6m) / 67m) * (decimal)_appConfig.BaseSmsPrice;
		if (smsPanel.ToString().ToLower().Contains("jet") && (decimal)appSettings.SmsCredit < num && _appConfig.IsCloud)
		{
			throw new CustomException("میزان اعتبار شارژ کافی نمی باشد.");
		}
		ResponsePayamak sendResult = new ResponsePayamak();
		switch (smsPanel)
		{
		case SmsPanel.IPPanel:
		case SmsPanel.JetAqsaat_IPPanel:
		{
			BaseSendPayamak sendPayamak = new SendIPPanelPattern
			{
				To = to,
				Text = text
			};
			if (isPatternBase)
			{
				PrepareIPPanelKeys(sendPayamak as SendIPPanelPattern, text, appSettings.DueDateRememberSmsPattern.Pattern, appSettings.DueDateRememberSmsPattern.IppanelKeysJson);
				sendResult = _payamakService.SendByPatternIPPanel(sendPayamak as SendIPPanelPattern, appSettings.DueDateRememberSmsPattern.PatternBaseCode);
			}
			else
			{
				sendResult = _payamakService.Send(sendPayamak);
			}
			break;
		}
		case SmsPanel.WebOne:
		{
			BaseSendPayamak sendPayamak = new SendWebOnePattern
			{
				To = to,
				Text = text
			};
			if (isPatternBase)
			{
				sendResult = _payamakService.SendByPatternWebOne(sendPayamak as SendWebOnePattern, appSettings.DueDateRememberSmsPattern.PatternBaseCode);
			}
			else
			{
				sendResult = _payamakService.Send(sendPayamak);
			}
			break;
		}
		case SmsPanel.KaveNegar:
		case SmsPanel.JetAqsaat_KaveNegar:
		{
			BaseSendPayamak sendPayamak = new SendKaveNegar
			{
				To = to,
				Text = text
			};
			if (isPatternBase)
			{
				PrepareKavehTokens(sendPayamak as SendKaveNegar, text);
				sendResult = _payamakService.SendByLookupKaveNegar(sendPayamak as SendKaveNegar, appSettings.DueDateRememberSmsPattern.PatternBaseCode);
			}
			else
			{
				sendResult = _payamakService.Send(sendPayamak);
			}
			break;
		}
		case SmsPanel.MelliPayamak:
		case SmsPanel.JetAqsaat_MelliPayamak:
		{
			BaseSendPayamak sendPayamak = new SendMelliPayamak
			{
				To = to,
				Text = text
			};
			if (isPatternBase && int.TryParse(appSettings.DueDateRememberSmsPattern.PatternBaseCode, out var result))
			{
				PrepareMelliPayamakTextPattern(sendPayamak as SendMelliPayamak, text);
				sendResult = _payamakService.SendByBaseNumber(sendPayamak, result);
			}
			else
			{
				sendResult = _payamakService.Send(sendPayamak);
			}
			break;
		}
		case SmsPanel.Bale:
		{
			BaseSendPayamak sendPayamak = new SendBale
			{
				To = to,
				Text = text
			};
			sendResult = _payamakService.Send(sendPayamak);
			break;
		}
		}
		AppSms appSms = new AppSms
		{
			AppSmsType = AppSmsType.DueDate,
			Date = DateTime.Now,
			Price = num,
			Status = (sendResult.Success ? AppSmsStatus.Pending : AppSmsStatus.Failed),
			Text = text,
			SmsPatternTitle = appSettings.DueDateRememberSmsPattern.Title,
			To = to,
			EntityType = "UserLoanPayment",
			EntityId = loanPayment.Id,
			SmsPanel = smsPanel,
			SMSKey = sendResult.Key
		};
		await _appSmsService.Insert(appSms);
		if (sendResult.Success)
		{
			await _appSettingsService.SyncChargeSmsCredit();
			BackgroundJob.Schedule(() => CheckDelivery(sendResult.Key, appSms.Id, (Guid)_currentUserService.CustomerId, _appConfig.IsCloud), TimeSpan.FromMinutes(30.0));
		}
		return appSms;
	}

	public async Task<AppSms> SendOverDueDate(LoanPaymentModel loanPayment, List<LoanPaymentModel> otherLoanPayments)
	{
		if (_hostingEnvironment.EnvironmentName == "Development")
		{
			return null;
		}
		AppSettings appSettings = await _appSettingsService.GetAppSettings();
		SmsPanel smsPanel = await SetPayamakService(appSettings);
		if (_payamakService == null)
		{
			throw new CustomException("سرویس پیامک یافت نشد");
		}
		if (appSettings?.OverDueDateSmsPattern == null)
		{
			throw new CustomException("قالب پیامک معوق تنظیم نشده است.");
		}
		string to = loanPayment?.UserLoanModel?.MemberModel?.Mobile ?? "";
		if (string.IsNullOrEmpty(to))
		{
			throw new CustomException("شماره موبایل دریافت کننده ثبت نشده است.");
		}
		bool isPatternBase = appSettings.OverDueDateSmsPattern.IsPatternBase && !string.IsNullOrEmpty(appSettings.OverDueDateSmsPattern.PatternBaseCode);
		string text = await ExtractTextFromPattern(loanPayment, otherLoanPayments, appSettings.OverDueDateSmsPattern.Pattern, isPatternBase);
		decimal num = Math.Ceiling(((decimal)text.Length + 6m) / 67m) * (decimal)_appConfig.BaseSmsPrice;
		if (smsPanel.ToString().ToLower().Contains("jet") && (decimal)appSettings.SmsCredit < num && _appConfig.IsCloud)
		{
			throw new CustomException("میزان اعتبار شارژ کافی نمی باشد.");
		}
		ResponsePayamak sendResult = new ResponsePayamak();
		switch (smsPanel)
		{
		case SmsPanel.IPPanel:
		case SmsPanel.JetAqsaat_IPPanel:
		{
			BaseSendPayamak sendPayamak = new SendIPPanelPattern
			{
				To = to,
				Text = text
			};
			if (isPatternBase)
			{
				PrepareIPPanelKeys(sendPayamak as SendIPPanelPattern, text, appSettings.OverDueDateSmsPattern.Pattern, appSettings.OverDueDateSmsPattern.IppanelKeysJson);
				sendResult = _payamakService.SendByPatternIPPanel(sendPayamak as SendIPPanelPattern, appSettings.OverDueDateSmsPattern.PatternBaseCode);
			}
			else
			{
				sendResult = _payamakService.Send(sendPayamak);
			}
			break;
		}
		case SmsPanel.WebOne:
		{
			BaseSendPayamak sendPayamak = new SendWebOnePattern
			{
				To = to,
				Text = text
			};
			if (isPatternBase)
			{
				sendResult = _payamakService.SendByPatternWebOne(sendPayamak as SendWebOnePattern, appSettings.OverDueDateSmsPattern.PatternBaseCode);
			}
			else
			{
				sendResult = _payamakService.Send(sendPayamak);
			}
			break;
		}
		case SmsPanel.KaveNegar:
		case SmsPanel.JetAqsaat_KaveNegar:
		{
			BaseSendPayamak sendPayamak = new SendKaveNegar
			{
				To = to,
				Text = text
			};
			if (isPatternBase)
			{
				PrepareKavehTokens(sendPayamak as SendKaveNegar, text);
				sendResult = _payamakService.SendByLookupKaveNegar(sendPayamak as SendKaveNegar, appSettings.OverDueDateSmsPattern.PatternBaseCode);
			}
			else
			{
				sendResult = _payamakService.Send(sendPayamak);
			}
			break;
		}
		case SmsPanel.MelliPayamak:
		case SmsPanel.JetAqsaat_MelliPayamak:
		{
			BaseSendPayamak sendPayamak = new SendMelliPayamak
			{
				To = to,
				Text = text
			};
			if (isPatternBase && int.TryParse(appSettings.OverDueDateSmsPattern.PatternBaseCode, out var result))
			{
				PrepareMelliPayamakTextPattern(sendPayamak as SendMelliPayamak, text);
				sendResult = _payamakService.SendByBaseNumber(sendPayamak, result);
			}
			else
			{
				sendResult = _payamakService.Send(sendPayamak);
			}
			break;
		}
		case SmsPanel.Bale:
		{
			BaseSendPayamak sendPayamak = new SendBale
			{
				To = to,
				Text = text
			};
			sendResult = _payamakService.Send(sendPayamak);
			break;
		}
		}
		AppSms appSms = new AppSms
		{
			AppSmsType = AppSmsType.OverDueDate,
			Date = DateTime.Now,
			Price = num,
			Status = (sendResult.Success ? AppSmsStatus.Pending : AppSmsStatus.Failed),
			Text = text,
			SmsPatternTitle = appSettings.OverDueDateSmsPattern.Title,
			To = to,
			EntityType = "UserLoanPayment",
			EntityId = loanPayment.Id,
			SmsPanel = smsPanel,
			SMSKey = sendResult.Key
		};
		await _appSmsService.Insert(appSms);
		if (sendResult.Success)
		{
			await _appSettingsService.SyncChargeSmsCredit();
			BackgroundJob.Schedule(() => CheckDelivery(sendResult.Key, appSms.Id, (Guid)_currentUserService.CustomerId, _appConfig.IsCloud), TimeSpan.FromMinutes(30.0));
		}
		return appSms;
	}

	public async Task<AppSms> SendBeforeDueDate(LoanPaymentModel loanPayment)
	{
		if (_hostingEnvironment.EnvironmentName == "Development")
		{
			return null;
		}
		AppSettings appSettings = await _appSettingsService.GetAppSettings();
		SmsPanel smsPanel = await SetPayamakService(appSettings);
		if (_payamakService == null)
		{
			throw new CustomException("سرویس پیامک یافت نشد");
		}
		if (appSettings?.BeforeDueDateSmsPattern == null)
		{
			throw new CustomException("قالب پیامک سررسید پیش از موعد تنظیم نشده است.");
		}
		string to = loanPayment?.UserLoanModel?.MemberModel?.Mobile ?? "";
		if (string.IsNullOrEmpty(to))
		{
			throw new CustomException("شماره موبایل دریافت کننده ثبت نشده است.");
		}
		bool isPatternBase = appSettings.BeforeDueDateSmsPattern.IsPatternBase && !string.IsNullOrEmpty(appSettings.BeforeDueDateSmsPattern.PatternBaseCode);
		string text = await ExtractTextFromPattern(loanPayment, null, appSettings.BeforeDueDateSmsPattern.Pattern, isPatternBase);
		decimal num = Math.Ceiling(((decimal)text.Length + 6m) / 67m) * (decimal)_appConfig.BaseSmsPrice;
		if (smsPanel.ToString().ToLower().Contains("jet") && (decimal)appSettings.SmsCredit < num && _appConfig.IsCloud)
		{
			throw new CustomException("میزان اعتبار شارژ کافی نمی باشد.");
		}
		ResponsePayamak sendResult = new ResponsePayamak();
		switch (smsPanel)
		{
		case SmsPanel.IPPanel:
		case SmsPanel.JetAqsaat_IPPanel:
		{
			BaseSendPayamak sendPayamak = new SendIPPanelPattern
			{
				To = to,
				Text = text
			};
			if (isPatternBase)
			{
				PrepareIPPanelKeys(sendPayamak as SendIPPanelPattern, text, appSettings.BeforeDueDateSmsPattern.Pattern, appSettings.BeforeDueDateSmsPattern.IppanelKeysJson);
				sendResult = _payamakService.SendByPatternIPPanel(sendPayamak as SendIPPanelPattern, appSettings.BeforeDueDateSmsPattern.PatternBaseCode);
			}
			else
			{
				sendResult = _payamakService.Send(sendPayamak);
			}
			break;
		}
		case SmsPanel.WebOne:
		{
			BaseSendPayamak sendPayamak = new SendWebOnePattern
			{
				To = to,
				Text = text
			};
			if (isPatternBase)
			{
				sendResult = _payamakService.SendByPatternWebOne(sendPayamak as SendWebOnePattern, appSettings.BeforeDueDateSmsPattern.PatternBaseCode);
			}
			else
			{
				sendResult = _payamakService.Send(sendPayamak);
			}
			break;
		}
		case SmsPanel.KaveNegar:
		case SmsPanel.JetAqsaat_KaveNegar:
		{
			BaseSendPayamak sendPayamak = new SendKaveNegar
			{
				To = to,
				Text = text
			};
			if (isPatternBase)
			{
				PrepareKavehTokens(sendPayamak as SendKaveNegar, text);
				sendResult = _payamakService.SendByLookupKaveNegar(sendPayamak as SendKaveNegar, appSettings.BeforeDueDateSmsPattern.PatternBaseCode);
			}
			else
			{
				sendResult = _payamakService.Send(sendPayamak);
			}
			break;
		}
		case SmsPanel.MelliPayamak:
		case SmsPanel.JetAqsaat_MelliPayamak:
		{
			BaseSendPayamak sendPayamak = new SendMelliPayamak
			{
				To = to,
				Text = text
			};
			if (isPatternBase && int.TryParse(appSettings.BeforeDueDateSmsPattern.PatternBaseCode, out var result))
			{
				PrepareMelliPayamakTextPattern(sendPayamak as SendMelliPayamak, text);
				sendResult = _payamakService.SendByBaseNumber(sendPayamak, result);
			}
			else
			{
				sendResult = _payamakService.Send(sendPayamak);
			}
			break;
		}
		case SmsPanel.Bale:
		{
			BaseSendPayamak sendPayamak = new SendBale
			{
				To = to,
				Text = text
			};
			sendResult = _payamakService.Send(sendPayamak);
			break;
		}
		}
		AppSms appSms = new AppSms
		{
			AppSmsType = AppSmsType.BeforeDueDate,
			Date = DateTime.Now,
			Price = num,
			Status = (sendResult.Success ? AppSmsStatus.Pending : AppSmsStatus.Failed),
			Text = text,
			SmsPatternTitle = appSettings.BeforeDueDateSmsPattern.Title,
			To = to,
			EntityType = "UserLoanPayment",
			EntityId = loanPayment.Id,
			SmsPanel = smsPanel,
			SMSKey = sendResult.Key
		};
		await _appSmsService.Insert(appSms);
		if (sendResult.Success)
		{
			await _appSettingsService.SyncChargeSmsCredit();
			BackgroundJob.Schedule(() => CheckDelivery(sendResult.Key, appSms.Id, (Guid)_currentUserService.CustomerId, _appConfig.IsCloud), TimeSpan.FromMinutes(30.0));
		}
		return appSms;
	}

	public async Task<AppSmsStatus> CheckDelivery(string recId, Guid appSmsId, Guid customerId, bool isCloud, bool continueCheck = true)
	{
		if (_httpContextAccessor.HttpContext == null)
		{
			_httpContextAccessor.HttpContext = new DefaultHttpContext();
			_currentUserService.CustomerId = customerId;
		}
		AppSms appSms = await _appSmsService.GetById(appSmsId, "Customer");
		if (appSms == null)
		{
			return AppSmsStatus.Pending;
		}
		AppSettings appSettings = await _appSettingsService.GetAppSettings();
		appSettings.DefaultSmsPanel = appSms.SmsPanel;
		await SetPayamakService(appSettings);
		ResponsePayamak responsePayamak = _payamakService.CheckDelivery(recId);
		if (responsePayamak != null && responsePayamak.Success)
		{
			_httpContextAccessor.HttpContext = new DefaultHttpContext();
			_httpContextAccessor.HttpContext.Request.Headers.Add(CurrentUserService.Customer_header_key, customerId.ToString());
			if (responsePayamak.Key == AppSmsStatus.Failed.ToString() && appSms.Status != AppSmsStatus.Failed)
			{
				appSms.Status = AppSmsStatus.Failed;
				await _appSmsService.Update(appSms);
				if (appSms.SmsPanel.ToString().ToLower().Contains("jet") & isCloud)
				{
					await _appSettingsService.SyncChargeSmsCredit();
				}
				return AppSmsStatus.Failed;
			}
			if (responsePayamak.Key == AppSmsStatus.Delivered.ToString() && appSms.Status != AppSmsStatus.Delivered)
			{
				appSms.Status = AppSmsStatus.Delivered;
				await _appSmsService.Update(appSms);
				return AppSmsStatus.Delivered;
			}
			if ((responsePayamak.Key == AppSmsStatus.Pending.ToString()) & continueCheck)
			{
				if (appSms.DeliverCheckRetryCount < 6)
				{
					appSms.DeliverCheckRetryCount++;
					await _appSmsService.Update(appSms);
					BackgroundJob.Schedule(delay: TimeSpan.FromMinutes(appSms.DeliverCheckRetryCount switch
					{
						1 => 30, 
						2 => 60, 
						3 => 90, 
						4 => 180, 
						5 => 360, 
						6 => 720, 
						_ => 30, 
					}), methodCall: () => CheckDelivery(recId, appSmsId, customerId, isCloud, continueCheck));
					return AppSmsStatus.Pending;
				}
				appSms.Status = AppSmsStatus.Failed;
				await _appSmsService.Update(appSms);
				return AppSmsStatus.Failed;
			}
		}
		return AppSmsStatus.Pending;
	}

	public async Task<AppSms> ReSend(AppSms appSms)
	{
		if (_hostingEnvironment.EnvironmentName == "Development")
		{
			return null;
		}
		AppSettings appSettings = await _appSettingsService.GetAppSettings();
		SmsPanel smsPanel = await SetPayamakService(appSettings);
		if (_payamakService == null)
		{
			throw new CustomException("سرویس پیامک یافت نشد");
		}
		if (string.IsNullOrEmpty(appSms.To))
		{
			throw new CustomException("شماره موبایل دریافت کننده ثبت نشده است.");
		}
		BaseSendPayamak sendPayamak = null;
		decimal num = Math.Ceiling((decimal)appSms.Text.Length / 67m) * (decimal)_appConfig.BaseSmsPrice;
		if ((decimal)appSettings.SmsCredit < num && _appConfig.IsCloud)
		{
			throw new CustomException("میزان اعتبار شارژ کافی نمی باشد.");
		}
		switch (smsPanel)
		{
		case SmsPanel.IPPanel:
		case SmsPanel.JetAqsaat_IPPanel:
			sendPayamak = new SendIPPanelPattern
			{
				To = appSms.To,
				Text = appSms.Text
			};
			break;
		case SmsPanel.WebOne:
			sendPayamak = new SendWebOnePattern
			{
				To = appSms.To,
				Text = appSms.Text
			};
			break;
		case SmsPanel.KaveNegar:
		case SmsPanel.JetAqsaat_KaveNegar:
			sendPayamak = new SendKaveNegar
			{
				To = appSms.To,
				Text = appSms.Text
			};
			break;
		case SmsPanel.MelliPayamak:
		case SmsPanel.JetAqsaat_MelliPayamak:
			sendPayamak = new SendMelliPayamak
			{
				To = appSms.To,
				Text = appSms.Text
			};
			break;
		case SmsPanel.Bale:
			sendPayamak = new SendBale
			{
				To = appSms.To,
				Text = appSms.Text
			};
			_payamakService.Send(sendPayamak);
			break;
		}
		ResponsePayamak sendResult = _payamakService.Send(sendPayamak);
		AppSms newAppSms = new AppSms
		{
			AppSmsType = appSms.AppSmsType,
			Date = DateTime.Now,
			Price = num,
			Status = (sendResult.Success ? AppSmsStatus.Pending : AppSmsStatus.Failed),
			Text = appSms.Text,
			SmsPatternTitle = appSms.SmsPatternTitle,
			To = appSms.To,
			EntityType = appSms.EntityType,
			EntityId = appSms.EntityId,
			SmsPanel = smsPanel,
			SMSKey = sendResult.Key,
			Tag = appSms.Tag
		};
		await _appSmsService.Insert(newAppSms);
		if (sendResult.Success)
		{
			await _appSettingsService.SyncChargeSmsCredit();
			BackgroundJob.Schedule(() => CheckDelivery(sendResult.Key, newAppSms.Id, (Guid)_currentUserService.CustomerId, _appConfig.IsCloud), TimeSpan.FromMinutes(30.0));
		}
		return newAppSms;
	}

	public async Task<AppSms> SendChangeLoanRequestStatus(UserLoanRequestModel userLoanRequestModel)
	{
		if (_hostingEnvironment.EnvironmentName == "Development")
		{
			return null;
		}
		AppSettings appSettings = await _appSettingsService.GetAppSettings();
		SmsPanel smsPanel = await SetPayamakService(appSettings);
		if (_payamakService == null)
		{
			throw new CustomException("سرویس پیامک یافت نشد");
		}
		if (userLoanRequestModel?.Status?.SmsPattern == null)
		{
			throw new CustomException("قالب پیامک تغییر وضعیت یافت نشد.");
		}
		string to = userLoanRequestModel?.MemberModel?.Mobile ?? "";
		if (string.IsNullOrEmpty(to))
		{
			throw new CustomException("شماره موبایل دریافت کننده ثبت نشده است.");
		}
		bool isPatternBase = userLoanRequestModel.Status.SmsPattern.IsPatternBase && !string.IsNullOrEmpty(userLoanRequestModel.Status.SmsPattern.PatternBaseCode);
		string text = await ExtractTextFromPattern(userLoanRequestModel, userLoanRequestModel.Status.SmsPattern.Pattern, isPatternBase);
		decimal num = Math.Ceiling(((decimal)text.Length + 6m) / 67m) * (decimal)_appConfig.BaseSmsPrice;
		if (smsPanel.ToString().ToLower().Contains("jet") && (decimal)appSettings.SmsCredit < num && _appConfig.IsCloud)
		{
			throw new CustomException("میزان اعتبار شارژ کافی نمی باشد.");
		}
		ResponsePayamak sendResult = new ResponsePayamak();
		switch (smsPanel)
		{
		case SmsPanel.IPPanel:
		case SmsPanel.JetAqsaat_IPPanel:
		{
			BaseSendPayamak sendPayamak = new SendIPPanelPattern
			{
				To = to,
				Text = text
			};
			if (isPatternBase)
			{
				PrepareIPPanelKeys(sendPayamak as SendIPPanelPattern, text, userLoanRequestModel.Status.SmsPattern.Pattern, userLoanRequestModel.Status.SmsPattern.IppanelKeysJson);
				sendResult = _payamakService.SendByPatternIPPanel(sendPayamak as SendIPPanelPattern, userLoanRequestModel.Status.SmsPattern.PatternBaseCode);
			}
			else
			{
				sendResult = _payamakService.Send(sendPayamak);
			}
			break;
		}
		case SmsPanel.WebOne:
		{
			BaseSendPayamak sendPayamak = new SendWebOnePattern
			{
				To = to,
				Text = text
			};
			if (isPatternBase)
			{
				sendResult = _payamakService.SendByPatternWebOne(sendPayamak as SendWebOnePattern, userLoanRequestModel.Status.SmsPattern.PatternBaseCode);
			}
			else
			{
				sendResult = _payamakService.Send(sendPayamak);
			}
			break;
		}
		case SmsPanel.KaveNegar:
		case SmsPanel.JetAqsaat_KaveNegar:
		{
			BaseSendPayamak sendPayamak = new SendKaveNegar
			{
				To = to,
				Text = text
			};
			if (isPatternBase)
			{
				PrepareKavehTokens(sendPayamak as SendKaveNegar, text);
				sendResult = _payamakService.SendByLookupKaveNegar(sendPayamak as SendKaveNegar, userLoanRequestModel.Status.SmsPattern.PatternBaseCode);
			}
			else
			{
				sendResult = _payamakService.Send(sendPayamak);
			}
			break;
		}
		case SmsPanel.MelliPayamak:
		case SmsPanel.JetAqsaat_MelliPayamak:
		{
			BaseSendPayamak sendPayamak = new SendMelliPayamak
			{
				To = to,
				Text = text
			};
			if (isPatternBase && int.TryParse(userLoanRequestModel.Status.SmsPattern.PatternBaseCode, out var result))
			{
				PrepareMelliPayamakTextPattern(sendPayamak as SendMelliPayamak, text);
				sendResult = _payamakService.SendByBaseNumber(sendPayamak, result);
			}
			else
			{
				sendResult = _payamakService.Send(sendPayamak);
			}
			break;
		}
		case SmsPanel.Bale:
		{
			BaseSendPayamak sendPayamak = new SendBale
			{
				To = to,
				Text = text
			};
			sendResult = _payamakService.Send(sendPayamak);
			break;
		}
		}
		AppSms appSms = new AppSms
		{
			AppSmsType = AppSmsType.ChangeStatus,
			Date = DateTime.Now,
			Price = num,
			Status = (sendResult.Success ? AppSmsStatus.Pending : AppSmsStatus.Failed),
			Text = text,
			SmsPatternTitle = userLoanRequestModel.Status.SmsPattern.Title,
			To = to,
			EntityType = "UserLoanRequest",
			EntityId = userLoanRequestModel.Id,
			SmsPanel = smsPanel,
			SMSKey = sendResult.Key
		};
		await _appSmsService.Insert(appSms);
		if (sendResult.Success)
		{
			await _appSettingsService.SyncChargeSmsCredit();
			BackgroundJob.Schedule(() => CheckDelivery(sendResult.Key, appSms.Id, (Guid)_currentUserService.CustomerId, _appConfig.IsCloud), TimeSpan.FromMinutes(30.0));
		}
		return appSms;
	}

	public async Task<AppSms> SendChangeLoanRequestAgentStatus(UserLoanRequestModel userLoanRequestModel)
	{
		if (_hostingEnvironment.EnvironmentName == "Development")
		{
			return null;
		}
		AppSettings appSettings = await _appSettingsService.GetAppSettings();
		SmsPanel smsPanel = await SetPayamakService(appSettings);
		if (_payamakService == null)
		{
			throw new CustomException("سرویس پیامک یافت نشد");
		}
		if (userLoanRequestModel?.AgentStatus?.SmsPattern == null)
		{
			throw new CustomException("قالب پیامک تغییر وضعیت نماینده یافت نشد.");
		}
		string to = userLoanRequestModel?.AgentModel?.Mobile ?? "";
		if (string.IsNullOrEmpty(to))
		{
			throw new CustomException("شماره موبایل دریافت کننده ثبت نشده است.");
		}
		bool isPatternBase = userLoanRequestModel.AgentStatus.SmsPattern.IsPatternBase && !string.IsNullOrEmpty(userLoanRequestModel.AgentStatus.SmsPattern.PatternBaseCode);
		string text = await ExtractTextFromPattern(userLoanRequestModel, userLoanRequestModel.AgentStatus.SmsPattern.Pattern, isPatternBase);
		decimal num = Math.Ceiling(((decimal)text.Length + 6m) / 67m) * (decimal)_appConfig.BaseSmsPrice;
		if (smsPanel.ToString().ToLower().Contains("jet") && (decimal)appSettings.SmsCredit < num && _appConfig.IsCloud)
		{
			throw new CustomException("میزان اعتبار شارژ کافی نمی باشد.");
		}
		ResponsePayamak sendResult = new ResponsePayamak();
		switch (smsPanel)
		{
		case SmsPanel.IPPanel:
		case SmsPanel.JetAqsaat_IPPanel:
		{
			BaseSendPayamak sendPayamak = new SendIPPanelPattern
			{
				To = to,
				Text = text
			};
			if (isPatternBase)
			{
				PrepareIPPanelKeys(sendPayamak as SendIPPanelPattern, text, userLoanRequestModel.AgentStatus.SmsPattern.Pattern, userLoanRequestModel.AgentStatus.SmsPattern.IppanelKeysJson);
				sendResult = _payamakService.SendByPatternIPPanel(sendPayamak as SendIPPanelPattern, userLoanRequestModel.AgentStatus.SmsPattern.PatternBaseCode);
			}
			else
			{
				sendResult = _payamakService.Send(sendPayamak);
			}
			break;
		}
		case SmsPanel.WebOne:
		{
			BaseSendPayamak sendPayamak = new SendWebOnePattern
			{
				To = to,
				Text = text
			};
			if (isPatternBase)
			{
				sendResult = _payamakService.SendByPatternWebOne(sendPayamak as SendWebOnePattern, userLoanRequestModel.AgentStatus.SmsPattern.PatternBaseCode);
			}
			else
			{
				sendResult = _payamakService.Send(sendPayamak);
			}
			break;
		}
		case SmsPanel.KaveNegar:
		case SmsPanel.JetAqsaat_KaveNegar:
		{
			BaseSendPayamak sendPayamak = new SendKaveNegar
			{
				To = to,
				Text = text
			};
			if (isPatternBase)
			{
				PrepareKavehTokens(sendPayamak as SendKaveNegar, text);
				sendResult = _payamakService.SendByLookupKaveNegar(sendPayamak as SendKaveNegar, userLoanRequestModel.AgentStatus.SmsPattern.PatternBaseCode);
			}
			else
			{
				sendResult = _payamakService.Send(sendPayamak);
			}
			break;
		}
		case SmsPanel.MelliPayamak:
		case SmsPanel.JetAqsaat_MelliPayamak:
		{
			BaseSendPayamak sendPayamak = new SendMelliPayamak
			{
				To = to,
				Text = text
			};
			if (isPatternBase && int.TryParse(userLoanRequestModel.AgentStatus.SmsPattern.PatternBaseCode, out var result))
			{
				PrepareMelliPayamakTextPattern(sendPayamak as SendMelliPayamak, text);
				sendResult = _payamakService.SendByBaseNumber(sendPayamak, result);
			}
			else
			{
				sendResult = _payamakService.Send(sendPayamak);
			}
			break;
		}
		case SmsPanel.Bale:
		{
			BaseSendPayamak sendPayamak = new SendBale
			{
				To = to,
				Text = text
			};
			sendResult = _payamakService.Send(sendPayamak);
			break;
		}
		}
		AppSms appSms = new AppSms
		{
			AppSmsType = AppSmsType.ChangeAgentStatus,
			Date = DateTime.Now,
			Price = num,
			Status = (sendResult.Success ? AppSmsStatus.Pending : AppSmsStatus.Failed),
			Text = text,
			SmsPatternTitle = userLoanRequestModel.AgentStatus.SmsPattern.Title,
			To = to,
			EntityType = "UserLoanRequest",
			EntityId = userLoanRequestModel.Id,
			SmsPanel = smsPanel,
			SMSKey = sendResult.Key
		};
		await _appSmsService.Insert(appSms);
		if (sendResult.Success)
		{
			await _appSettingsService.SyncChargeSmsCredit();
			BackgroundJob.Schedule(() => CheckDelivery(sendResult.Key, appSms.Id, (Guid)_currentUserService.CustomerId, _appConfig.IsCloud), TimeSpan.FromMinutes(30.0));
		}
		return appSms;
	}

	public async Task<AppSms> SendWalletWithdrawOtp(AgentFactorModel agentFactorModel, string code)
	{
		if (_hostingEnvironment.EnvironmentName == "Development")
		{
			return null;
		}
		AppSettings appSettings = await _appSettingsService.GetAppSettings();
		SmsPanel smsPanel = await SetPayamakService(appSettings);
		if (_payamakService == null)
		{
			throw new CustomException("سرویس پیامک یافت نشد");
		}
		if (appSettings.WalletWithdrawOtpSmsPattern == null)
		{
			throw new CustomException("قالب پیامک کد تائیدیه برداشت از کیف پول یافت نشد.");
		}
		string to = agentFactorModel?.Member?.Mobile ?? "";
		if (string.IsNullOrEmpty(to))
		{
			throw new CustomException("شماره موبایل دریافت کننده ثبت نشده است.");
		}
		bool isPatternBase = appSettings.WalletWithdrawOtpSmsPattern.IsPatternBase && !string.IsNullOrEmpty(appSettings.WalletWithdrawOtpSmsPattern.PatternBaseCode);
		string text = await ExtractTextFromPattern(agentFactorModel, appSettings.WalletWithdrawOtpSmsPattern.Pattern, isPatternBase, code);
		decimal num = Math.Ceiling(((decimal)text.Length + 6m) / 67m) * (decimal)_appConfig.BaseSmsPrice;
		if (smsPanel.ToString().ToLower().Contains("jet") && (decimal)appSettings.SmsCredit < num && _appConfig.IsCloud)
		{
			throw new CustomException("میزان اعتبار شارژ کافی نمی باشد.");
		}
		ResponsePayamak sendResult = new ResponsePayamak();
		switch (smsPanel)
		{
		case SmsPanel.IPPanel:
		case SmsPanel.JetAqsaat_IPPanel:
		{
			BaseSendPayamak sendPayamak = new SendIPPanelPattern
			{
				To = to,
				Text = text
			};
			if (isPatternBase)
			{
				PrepareIPPanelKeys(sendPayamak as SendIPPanelPattern, text, appSettings.WalletWithdrawOtpSmsPattern.Pattern, appSettings.WalletWithdrawOtpSmsPattern.IppanelKeysJson);
				sendResult = _payamakService.SendByPatternIPPanel(sendPayamak as SendIPPanelPattern, appSettings.WalletWithdrawOtpSmsPattern.PatternBaseCode);
			}
			else
			{
				sendResult = _payamakService.Send(sendPayamak);
			}
			break;
		}
		case SmsPanel.WebOne:
		{
			BaseSendPayamak sendPayamak = new SendWebOnePattern
			{
				To = to,
				Text = text
			};
			if (isPatternBase)
			{
				sendResult = _payamakService.SendByPatternWebOne(sendPayamak as SendWebOnePattern, appSettings.WalletWithdrawOtpSmsPattern.PatternBaseCode);
			}
			else
			{
				sendResult = _payamakService.Send(sendPayamak);
			}
			break;
		}
		case SmsPanel.KaveNegar:
		case SmsPanel.JetAqsaat_KaveNegar:
		{
			BaseSendPayamak sendPayamak = new SendKaveNegar
			{
				To = to,
				Text = text
			};
			if (isPatternBase)
			{
				PrepareKavehTokens(sendPayamak as SendKaveNegar, text);
				sendResult = _payamakService.SendByLookupKaveNegar(sendPayamak as SendKaveNegar, appSettings.WalletWithdrawOtpSmsPattern.PatternBaseCode);
			}
			else
			{
				sendResult = _payamakService.Send(sendPayamak);
			}
			break;
		}
		case SmsPanel.MelliPayamak:
		case SmsPanel.JetAqsaat_MelliPayamak:
		{
			BaseSendPayamak sendPayamak = new SendMelliPayamak
			{
				To = to,
				Text = text
			};
			if (isPatternBase && int.TryParse(appSettings.WalletWithdrawOtpSmsPattern.PatternBaseCode, out var result))
			{
				PrepareMelliPayamakTextPattern(sendPayamak as SendMelliPayamak, text);
				sendResult = _payamakService.SendByBaseNumber(sendPayamak, result);
			}
			else
			{
				sendResult = _payamakService.Send(sendPayamak);
			}
			break;
		}
		case SmsPanel.Bale:
		{
			BaseSendPayamak sendPayamak = new SendBale
			{
				To = to,
				Text = text
			};
			sendResult = _payamakService.Send(sendPayamak);
			break;
		}
		}
		AppSms appSms = new AppSms
		{
			AppSmsType = AppSmsType.WalletWithdrawOtp,
			Date = DateTime.Now,
			Price = num,
			Status = (sendResult.Success ? AppSmsStatus.Pending : AppSmsStatus.Failed),
			Text = text,
			SmsPatternTitle = appSettings.WalletWithdrawOtpSmsPattern.Title,
			To = to,
			EntityType = "AgentFactor",
			EntityId = agentFactorModel.Id,
			SmsPanel = smsPanel,
			SMSKey = sendResult.Key,
			Tag = code
		};
		await _appSmsService.Insert(appSms);
		if (sendResult.Success)
		{
			await _appSettingsService.SyncChargeSmsCredit();
			BackgroundJob.Schedule(() => CheckDelivery(sendResult.Key, appSms.Id, (Guid)_currentUserService.CustomerId, _appConfig.IsCloud), TimeSpan.FromMinutes(30.0));
		}
		return appSms;
	}

	public async Task<AppSms> SendWalletWithdrawNotification(AgentFactorModel agentFactorModel)
	{
		if (_hostingEnvironment.EnvironmentName == "Development")
		{
			return null;
		}
		AppSettings appSettings = await _appSettingsService.GetAppSettings();
		SmsPanel smsPanel = await SetPayamakService(appSettings);
		if (_payamakService == null)
		{
			throw new CustomException("سرویس پیامک یافت نشد");
		}
		if (appSettings.WalletWithdrawNotificationSmsPattern == null)
		{
			throw new CustomException("قالب پیامک برداشت از کیف پول یافت نشد.");
		}
		string to = agentFactorModel?.Member?.Mobile ?? "";
		if (string.IsNullOrEmpty(to))
		{
			throw new CustomException("شماره موبایل دریافت کننده ثبت نشده است.");
		}
		bool isPatternBase = appSettings.WalletWithdrawNotificationSmsPattern.IsPatternBase && !string.IsNullOrEmpty(appSettings.WalletWithdrawNotificationSmsPattern.PatternBaseCode);
		string text = await ExtractTextFromPattern(agentFactorModel, appSettings.WalletWithdrawNotificationSmsPattern.Pattern, isPatternBase, null);
		decimal num = Math.Ceiling(((decimal)text.Length + 6m) / 67m) * (decimal)_appConfig.BaseSmsPrice;
		if (smsPanel.ToString().ToLower().Contains("jet") && (decimal)appSettings.SmsCredit < num && _appConfig.IsCloud)
		{
			throw new CustomException("میزان اعتبار شارژ کافی نمی باشد.");
		}
		ResponsePayamak sendResult = new ResponsePayamak();
		switch (smsPanel)
		{
		case SmsPanel.IPPanel:
		case SmsPanel.JetAqsaat_IPPanel:
		{
			BaseSendPayamak sendPayamak = new SendIPPanelPattern
			{
				To = to,
				Text = text
			};
			if (isPatternBase)
			{
				PrepareIPPanelKeys(sendPayamak as SendIPPanelPattern, text, appSettings.WalletWithdrawNotificationSmsPattern.Pattern, appSettings.WalletWithdrawNotificationSmsPattern.IppanelKeysJson);
				sendResult = _payamakService.SendByPatternIPPanel(sendPayamak as SendIPPanelPattern, appSettings.WalletWithdrawNotificationSmsPattern.PatternBaseCode);
			}
			else
			{
				sendResult = _payamakService.Send(sendPayamak);
			}
			break;
		}
		case SmsPanel.WebOne:
		{
			BaseSendPayamak sendPayamak = new SendWebOnePattern
			{
				To = to,
				Text = text
			};
			if (isPatternBase)
			{
				sendResult = _payamakService.SendByPatternWebOne(sendPayamak as SendWebOnePattern, appSettings.WalletWithdrawNotificationSmsPattern.PatternBaseCode);
			}
			else
			{
				sendResult = _payamakService.Send(sendPayamak);
			}
			break;
		}
		case SmsPanel.KaveNegar:
		case SmsPanel.JetAqsaat_KaveNegar:
		{
			BaseSendPayamak sendPayamak = new SendKaveNegar
			{
				To = to,
				Text = text
			};
			if (isPatternBase)
			{
				PrepareKavehTokens(sendPayamak as SendKaveNegar, text);
				sendResult = _payamakService.SendByLookupKaveNegar(sendPayamak as SendKaveNegar, appSettings.WalletWithdrawNotificationSmsPattern.PatternBaseCode);
			}
			else
			{
				sendResult = _payamakService.Send(sendPayamak);
			}
			break;
		}
		case SmsPanel.MelliPayamak:
		case SmsPanel.JetAqsaat_MelliPayamak:
		{
			BaseSendPayamak sendPayamak = new SendMelliPayamak
			{
				To = to,
				Text = text
			};
			if (isPatternBase && int.TryParse(appSettings.WalletWithdrawNotificationSmsPattern.PatternBaseCode, out var result))
			{
				PrepareMelliPayamakTextPattern(sendPayamak as SendMelliPayamak, text);
				sendResult = _payamakService.SendByBaseNumber(sendPayamak, result);
			}
			else
			{
				sendResult = _payamakService.Send(sendPayamak);
			}
			break;
		}
		case SmsPanel.Bale:
		{
			BaseSendPayamak sendPayamak = new SendBale
			{
				To = to,
				Text = text
			};
			sendResult = _payamakService.Send(sendPayamak);
			break;
		}
		}
		AppSms appSms = new AppSms
		{
			AppSmsType = AppSmsType.WalletWithdrawNotification,
			Date = DateTime.Now,
			Price = num,
			Status = (sendResult.Success ? AppSmsStatus.Pending : AppSmsStatus.Failed),
			Text = text,
			SmsPatternTitle = appSettings.WalletWithdrawNotificationSmsPattern.Title,
			To = to,
			EntityType = "AgentFactor",
			EntityId = agentFactorModel.Id,
			SmsPanel = smsPanel,
			SMSKey = sendResult.Key
		};
		await _appSmsService.Insert(appSms);
		if (sendResult.Success)
		{
			await _appSettingsService.SyncChargeSmsCredit();
			BackgroundJob.Schedule(() => CheckDelivery(sendResult.Key, appSms.Id, (Guid)_currentUserService.CustomerId, _appConfig.IsCloud), TimeSpan.FromMinutes(30.0));
		}
		return appSms;
	}

	public async Task<bool> TrySendLogin(LoginRegisterModel loginRegisterModel, Guid customerId)
	{
		try
		{
			if (_hostingEnvironment.EnvironmentName == "Development")
			{
				return false;
			}
			_currentUserService.CustomerId = customerId;
			AppSettings appSettings = await _appSettingsService.GetAppSettings();
			SmsPanel smsPanel = await SetPayamakService(appSettings);
			if (_payamakService == null)
			{
				return false;
			}
			if (appSettings?.LoginSmsPattern == null)
			{
				return false;
			}
			string to = loginRegisterModel.Mobile;
			if (string.IsNullOrEmpty(to))
			{
				return false;
			}
			bool isPatternBase = appSettings.LoginSmsPattern.IsPatternBase && !string.IsNullOrEmpty(appSettings.LoginSmsPattern.PatternBaseCode);
			string text = await ExtractTextFromPattern(loginRegisterModel, appSettings.LoginSmsPattern.Pattern, isPatternBase);
			decimal num = Math.Ceiling(((decimal)text.Length + 6m) / 67m) * (decimal)_appConfig.BaseSmsPrice;
			if (smsPanel.ToString().ToLower().Contains("jet") && (decimal)appSettings.SmsCredit < num && _appConfig.IsCloud)
			{
				throw new CustomException("میزان اعتبار شارژ کافی نمی باشد.");
			}
			ResponsePayamak sendResult = new ResponsePayamak();
			switch (smsPanel)
			{
			case SmsPanel.IPPanel:
			case SmsPanel.JetAqsaat_IPPanel:
			{
				BaseSendPayamak sendPayamak = new SendIPPanelPattern
				{
					To = to,
					Text = text
				};
				if (isPatternBase)
				{
					PrepareIPPanelKeys(sendPayamak as SendIPPanelPattern, text, appSettings.LoginSmsPattern.Pattern, appSettings.LoginSmsPattern.IppanelKeysJson);
					sendResult = _payamakService.SendByPatternIPPanel(sendPayamak as SendIPPanelPattern, appSettings.LoginSmsPattern.PatternBaseCode);
				}
				else
				{
					sendResult = _payamakService.Send(sendPayamak);
				}
				break;
			}
			case SmsPanel.WebOne:
			{
				BaseSendPayamak sendPayamak = new SendWebOnePattern
				{
					To = to,
					Text = text
				};
				if (isPatternBase)
				{
					sendResult = _payamakService.SendByPatternWebOne(sendPayamak as SendWebOnePattern, appSettings.LoginSmsPattern.PatternBaseCode);
				}
				else
				{
					sendResult = _payamakService.Send(sendPayamak);
				}
				break;
			}
			case SmsPanel.KaveNegar:
			case SmsPanel.JetAqsaat_KaveNegar:
			{
				BaseSendPayamak sendPayamak = new SendKaveNegar
				{
					To = to,
					Text = text
				};
				if (isPatternBase)
				{
					PrepareKavehTokens(sendPayamak as SendKaveNegar, text);
					sendResult = _payamakService.SendByLookupKaveNegar(sendPayamak as SendKaveNegar, appSettings.LoginSmsPattern.PatternBaseCode);
				}
				else
				{
					sendResult = _payamakService.Send(sendPayamak);
				}
				break;
			}
			case SmsPanel.MelliPayamak:
			case SmsPanel.JetAqsaat_MelliPayamak:
			{
				BaseSendPayamak sendPayamak = new SendMelliPayamak
				{
					To = to,
					Text = text
				};
				if (isPatternBase && int.TryParse(appSettings.LoginSmsPattern.PatternBaseCode, out var result))
				{
					PrepareMelliPayamakTextPattern(sendPayamak as SendMelliPayamak, text);
					sendResult = _payamakService.SendByBaseNumber(sendPayamak, result);
				}
				else
				{
					sendResult = _payamakService.Send(sendPayamak);
				}
				break;
			}
			case SmsPanel.Bale:
			{
				BaseSendPayamak sendPayamak = new SendBale
				{
					To = to,
					Text = text
				};
				sendResult = _payamakService.Send(sendPayamak);
				break;
			}
			}
			AppSms appSms = new AppSms
			{
				AppSmsType = AppSmsType.Login,
				Date = DateTime.Now,
				Price = num,
				Status = (sendResult.Success ? AppSmsStatus.Pending : AppSmsStatus.Failed),
				Text = text,
				SmsPatternTitle = appSettings.LoginSmsPattern.Title,
				To = to,
				SmsPanel = smsPanel,
				SMSKey = sendResult.Key
			};
			await _appSmsService.Insert(appSms);
			if (sendResult.Success)
			{
				await _appSettingsService.SyncChargeSmsCredit();
				BackgroundJob.Schedule(() => CheckDelivery(sendResult.Key, appSms.Id, (Guid)_currentUserService.CustomerId, _appConfig.IsCloud), TimeSpan.FromMinutes(30.0));
			}
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	public async Task<bool> TrySendRegister(LoginRegisterModel loginRegisterModel, Guid customerId)
	{
		try
		{
			if (_hostingEnvironment.EnvironmentName == "Development")
			{
				return false;
			}
			_currentUserService.CustomerId = customerId;
			AppSettings appSettings = await _appSettingsService.GetAppSettings();
			SmsPanel smsPanel = await SetPayamakService();
			if (_payamakService == null)
			{
				return false;
			}
			if (appSettings?.RegisterSmsPattern == null)
			{
				return false;
			}
			string to = loginRegisterModel.Mobile;
			if (string.IsNullOrEmpty(to))
			{
				return false;
			}
			bool isPatternBase = appSettings.RegisterSmsPattern.IsPatternBase && !string.IsNullOrEmpty(appSettings.RegisterSmsPattern.PatternBaseCode);
			string text = await ExtractTextFromPattern(loginRegisterModel, appSettings.RegisterSmsPattern.Pattern, isPatternBase);
			decimal num = Math.Ceiling(((decimal)text.Length + 6m) / 67m) * (decimal)_appConfig.BaseSmsPrice;
			if (smsPanel.ToString().ToLower().Contains("jet") && (decimal)appSettings.SmsCredit < num && _appConfig.IsCloud)
			{
				throw new CustomException("میزان اعتبار شارژ کافی نمی باشد.");
			}
			ResponsePayamak sendResult = new ResponsePayamak();
			switch (smsPanel)
			{
			case SmsPanel.IPPanel:
			case SmsPanel.JetAqsaat_IPPanel:
			{
				BaseSendPayamak sendPayamak = new SendIPPanelPattern
				{
					To = to,
					Text = text
				};
				if (isPatternBase)
				{
					PrepareIPPanelKeys(sendPayamak as SendIPPanelPattern, text, appSettings.RegisterSmsPattern.Pattern, appSettings.RegisterSmsPattern.IppanelKeysJson);
					sendResult = _payamakService.SendByPatternIPPanel(sendPayamak as SendIPPanelPattern, appSettings.RegisterSmsPattern.PatternBaseCode);
				}
				else
				{
					sendResult = _payamakService.Send(sendPayamak);
				}
				break;
			}
			case SmsPanel.WebOne:
			{
				BaseSendPayamak sendPayamak = new SendWebOnePattern
				{
					To = to,
					Text = text
				};
				if (isPatternBase)
				{
					sendResult = _payamakService.SendByPatternWebOne(sendPayamak as SendWebOnePattern, appSettings.RegisterSmsPattern.PatternBaseCode);
				}
				else
				{
					sendResult = _payamakService.Send(sendPayamak);
				}
				break;
			}
			case SmsPanel.KaveNegar:
			case SmsPanel.JetAqsaat_KaveNegar:
			{
				BaseSendPayamak sendPayamak = new SendKaveNegar
				{
					To = to,
					Text = text
				};
				if (isPatternBase)
				{
					PrepareKavehTokens(sendPayamak as SendKaveNegar, text);
					sendResult = _payamakService.SendByLookupKaveNegar(sendPayamak as SendKaveNegar, appSettings.RegisterSmsPattern.PatternBaseCode);
				}
				else
				{
					sendResult = _payamakService.Send(sendPayamak);
				}
				break;
			}
			case SmsPanel.MelliPayamak:
			case SmsPanel.JetAqsaat_MelliPayamak:
			{
				BaseSendPayamak sendPayamak = new SendMelliPayamak
				{
					To = to,
					Text = text
				};
				if (isPatternBase && int.TryParse(appSettings.RegisterSmsPattern.PatternBaseCode, out var result))
				{
					PrepareMelliPayamakTextPattern(sendPayamak as SendMelliPayamak, text);
					sendResult = _payamakService.SendByBaseNumber(sendPayamak, result);
				}
				else
				{
					sendResult = _payamakService.Send(sendPayamak);
				}
				break;
			}
			case SmsPanel.Bale:
			{
				BaseSendPayamak sendPayamak = new SendBale
				{
					To = to,
					Text = text
				};
				sendResult = _payamakService.Send(sendPayamak);
				break;
			}
			}
			AppSms appSms = new AppSms
			{
				AppSmsType = AppSmsType.Register,
				Date = DateTime.Now,
				Price = num,
				Status = (sendResult.Success ? AppSmsStatus.Pending : AppSmsStatus.Failed),
				Text = text,
				SmsPatternTitle = appSettings.RegisterSmsPattern.Title,
				To = to,
				Tag = loginRegisterModel.User_Id.ToString(),
				EntityType = "User",
				SmsPanel = smsPanel,
				SMSKey = sendResult.Key
			};
			await _appSmsService.Insert(appSms);
			if (sendResult.Success)
			{
				await _appSettingsService.SyncChargeSmsCredit();
				BackgroundJob.Schedule(() => CheckDelivery(sendResult.Key, appSms.Id, (Guid)_currentUserService.CustomerId, _appConfig.IsCloud), TimeSpan.FromMinutes(30.0));
			}
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	private async Task<string> ExtractTextFromPattern(UserLoanModel userLoan, string pattern, bool isPatternBase)
	{
		List<string> list = new List<string> { "{نام مشتری}", "{نام فروشگاه}", "{مبلغ هر قسط}", "{شماره وام}", "{نوع وام}", "{تاریخ شروع}", "{مبلغ وام}", "{بازپرداخت با بهره}", "{تعداد اقساط}", "{مبلغ باقیمانده}" };
		foreach (string tag in list)
		{
			switch (tag)
			{
			case "{نام مشتری}":
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + userLoan?.MemberModel.FullName + "}") : userLoan?.MemberModel.FullName);
				break;
			case "{نام فروشگاه}":
			{
				Customer customer = await _customerService.GetById(_currentUserService.CustomerId.Value);
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + customer.Name + "}") : customer.Name);
				break;
			}
			case "{مبلغ هر قسط}":
			{
				string text = ((double)userLoan.Installment).ToString("N0");
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + text + "}") : text);
				break;
			}
			case "{شماره وام}":
			{
				string text = (userLoan?.Code).ToString() ?? "";
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + text + "}") : text);
				break;
			}
			case "{نوع وام}":
			{
				string text = userLoan?.LoanModel.Title;
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + text + "}") : text);
				break;
			}
			case "{تاریخ شروع}":
			{
				string text = userLoan?.StartDate_Persian.Date ?? "";
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + text + "}") : text);
				break;
			}
			case "{مبلغ وام}":
			{
				string text = ((double)(userLoan?.Amount).Value).ToString("N0");
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + text + "}") : text);
				break;
			}
			case "{مبلغ باقیمانده}":
			{
				string text = ((double)(userLoan?.Remain).Value).ToString("N0");
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + text + "}") : text);
				break;
			}
			case "{بازپرداخت با بهره}":
			{
				string text = ((double)(userLoan?.PayableAmount).Value).ToString("N0");
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + text + "}") : text);
				break;
			}
			case "{تعداد اقساط}":
			{
				string text = userLoan?.Month.ToString();
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + text + "}") : text);
				break;
			}
			}
		}
		return pattern;
	}

	private async Task<string> ExtractTextFromPattern(UserLoanRequestAuthorizationItemModel authorizeModel, string code, string pattern, bool isPatternBase)
	{
		List<string> list = new List<string>
		{
			"{رمز تاییدیه}", "{تاریخ}", "{تاریخ و ساعت}", "{نام مشتری}", "{کد ملی مشتری}", "{نام فروشگاه}", "{نام تجاری نماینده}", "{شماره درخواست}", "{نام طرح وام}", "{مبلغ درخواست وام}",
			"{تعداد قسط}", "{مبلغ هر قسط}", "{تاریخ درخواست}", "{لینک قرارداد}", "{بخش متغیر لینک قرارداد}"
		};
		foreach (string tag in list)
		{
			switch (tag)
			{
			case "{رمز تاییدیه}":
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + code + "}") : (code ?? ""));
				break;
			case "{تاریخ و ساعت}":
			{
				PersianDate persianDate = DateTime.Now.ToPersianDateObject();
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + persianDate.Date + "} {" + persianDate.Time + "}") : (persianDate.Date + " " + persianDate.Time));
				break;
			}
			case "{تاریخ}":
			{
				PersianDate persianDate = DateTime.Now.ToPersianDateObject();
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + persianDate.Date + "}") : persianDate.Date);
				break;
			}
			case "{نام مشتری}":
			{
				string text2 = authorizeModel?.UserLoanRequest?.MemberModel?.FullName ?? "";
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + text2 + "}") : text2);
				break;
			}
			case "{کد ملی مشتری}":
			{
				string text2 = authorizeModel?.UserLoanRequest?.MemberModel?.NationalCode ?? "";
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + text2 + "}") : text2);
				break;
			}
			case "{نام فروشگاه}":
			{
				string text2 = (await _customerService.GetById(_currentUserService.CustomerId.Value))?.Name ?? "";
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + text2 + "}") : text2);
				break;
			}
			case "{نام تجاری نماینده}":
			{
				string text2 = authorizeModel?.UserLoanRequest?.AgentModel?.BusinessName ?? "";
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + text2 + "}") : text2);
				break;
			}
			case "{شماره درخواست}":
			{
				string text2 = authorizeModel?.UserLoanRequest?.Code.ToString() ?? "";
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + text2 + "}") : text2);
				break;
			}
			case "{نام طرح وام}":
			{
				string text2 = authorizeModel?.UserLoanRequest?.LoanModel?.Title ?? "";
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + text2 + "}") : text2);
				break;
			}
			case "{مبلغ درخواست وام}":
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + authorizeModel.UserLoanRequest.Amount.ToString("N0") + "}") : authorizeModel.UserLoanRequest.Amount.ToString("N0"));
				break;
			case "{تعداد قسط}":
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + authorizeModel.UserLoanRequest.InstallmentCount + "}") : authorizeModel.UserLoanRequest.InstallmentCount.ToString());
				break;
			case "{مبلغ هر قسط}":
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + authorizeModel.UserLoanRequest.Installment.ToString("N0") + "}") : authorizeModel.UserLoanRequest.Installment.ToString("N0"));
				break;
			case "{تاریخ درخواست}":
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + authorizeModel.UserLoanRequest.RequestDate_Persian.Date + "}") : authorizeModel.UserLoanRequest.RequestDate_Persian.Date);
				break;
			case "{لینک قرارداد}":
			{
				Customer customer = await _customerService.GetById(_currentUserService.CustomerId.Value);
				string text = _appConfig.BaseUrl.TrimEnd('/') + $"/dgs/{customer.Code}/{authorizeModel.Code}";
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + text + "}") : text);
				break;
			}
			case "{بخش متغیر لینک قرارداد}":
			{
				string text = $"/dgs/{(await _customerService.GetById(_currentUserService.CustomerId.Value)).Code}/{authorizeModel.Code}";
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + text + "}") : text);
				break;
			}
			}
		}
		return pattern;
	}

	private async Task<string> ExtractTextFromPattern(ThirdPersonAssuranceAuthorizationItemModel authorizeModel, string code, string pattern, bool isPatternBase)
	{
		List<string> list = new List<string>
		{
			"{رمز تاییدیه}", "{تاریخ}", "{تاریخ و ساعت}", "{نام مشتری}", "{کد ملی مشتری}", "{نام فروشگاه}", "{نام تجاری نماینده}", "{شماره درخواست}", "{نام طرح وام}", "{مبلغ درخواست وام}",
			"{تعداد قسط}", "{مبلغ هر قسط}", "{تاریخ درخواست}", "{لینک قرارداد}", "{بخش متغیر لینک قرارداد}", "{نام و نام خانوادگی ضامن}", "{کد ملی ضامن}", "{موبایل ضامن}"
		};
		foreach (string tag in list)
		{
			switch (tag)
			{
			case "{رمز تاییدیه}":
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + code + "}") : (code ?? ""));
				break;
			case "{تاریخ و ساعت}":
			{
				PersianDate persianDate = DateTime.Now.ToPersianDateObject();
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + persianDate.Date + "} {" + persianDate.Time + "}") : (persianDate.Date + " " + persianDate.Time));
				break;
			}
			case "{تاریخ}":
			{
				PersianDate persianDate = DateTime.Now.ToPersianDateObject();
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + persianDate.Date + "}") : persianDate.Date);
				break;
			}
			case "{نام مشتری}":
			{
				string text = "";
				if (authorizeModel?.UserLoanRequestAuthorizationItem != null)
				{
					text = authorizeModel?.UserLoanRequestAuthorizationItem?.UserLoanRequest?.MemberModel?.FullName ?? "";
				}
				else if (authorizeModel?.UserLoan != null)
				{
					text = authorizeModel?.UserLoan?.MemberModel?.FullName ?? "";
				}
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + text + "}") : text);
				break;
			}
			case "{کد ملی مشتری}":
			{
				string text = "";
				if (authorizeModel?.UserLoanRequestAuthorizationItem != null)
				{
					text = authorizeModel?.UserLoanRequestAuthorizationItem?.UserLoanRequest?.MemberModel?.NationalCode ?? "";
				}
				else if (authorizeModel?.UserLoan != null)
				{
					text = authorizeModel.UserLoan?.MemberModel?.NationalCode ?? "";
				}
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + text + "}") : text);
				break;
			}
			case "{نام فروشگاه}":
			{
				string text = (await _customerService.GetById(_currentUserService.CustomerId.Value))?.Name ?? "";
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + text + "}") : text);
				break;
			}
			case "{نام تجاری نماینده}":
			{
				string text = "";
				if (authorizeModel?.UserLoanRequestAuthorizationItem != null)
				{
					text = authorizeModel?.UserLoanRequestAuthorizationItem?.UserLoanRequest?.AgentModel?.BusinessName ?? "";
				}
				else if (authorizeModel?.UserLoan != null)
				{
					text = authorizeModel?.UserLoan?.AgentModel?.BusinessName ?? "";
				}
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + text + "}") : text);
				break;
			}
			case "{شماره درخواست}":
			{
				string text = "";
				if (authorizeModel?.UserLoanRequestAuthorizationItem != null)
				{
					text = authorizeModel?.UserLoanRequestAuthorizationItem?.UserLoanRequest?.Code.ToString() ?? "";
				}
				else if (authorizeModel?.UserLoan != null)
				{
					text = authorizeModel?.UserLoan?.Code.ToString() ?? "";
				}
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + text + "}") : text);
				break;
			}
			case "{نام طرح وام}":
			{
				string text = "";
				if (authorizeModel?.UserLoanRequestAuthorizationItem != null)
				{
					text = authorizeModel?.UserLoanRequestAuthorizationItem?.UserLoanRequest?.LoanModel?.Title ?? "";
				}
				else if (authorizeModel?.UserLoan != null)
				{
					text = authorizeModel?.UserLoan?.LoanModel?.Title ?? "";
				}
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + text + "}") : text);
				break;
			}
			case "{مبلغ درخواست وام}":
				if (authorizeModel?.UserLoanRequestAuthorizationItem != null)
				{
					pattern = pattern.Replace(tag, isPatternBase ? ("{" + authorizeModel.UserLoanRequestAuthorizationItem.UserLoanRequest.Amount.ToString("N0") + "}") : authorizeModel.UserLoanRequestAuthorizationItem.UserLoanRequest.Amount.ToString("N0"));
				}
				else if (authorizeModel?.UserLoan != null)
				{
					pattern = pattern.Replace(tag, isPatternBase ? ("{" + authorizeModel.UserLoan.Amount.ToString("N0") + "}") : authorizeModel.UserLoan.Amount.ToString("N0"));
				}
				break;
			case "{تعداد قسط}":
				if (authorizeModel?.UserLoanRequestAuthorizationItem != null)
				{
					pattern = pattern.Replace(tag, isPatternBase ? ("{" + authorizeModel?.UserLoanRequestAuthorizationItem.UserLoanRequest.InstallmentCount.ToString() + "}") : authorizeModel.UserLoanRequestAuthorizationItem.UserLoanRequest.InstallmentCount.ToString());
				}
				else if (authorizeModel?.UserLoan != null)
				{
					pattern = pattern.Replace(tag, isPatternBase ? ("{" + authorizeModel?.UserLoan.Month.ToString() + "}") : authorizeModel.UserLoan.Month.ToString());
				}
				break;
			case "{مبلغ هر قسط}":
				if (authorizeModel?.UserLoanRequestAuthorizationItem != null)
				{
					pattern = pattern.Replace(tag, isPatternBase ? ("{" + authorizeModel?.UserLoanRequestAuthorizationItem.UserLoanRequest.Installment.ToString("N0") + "}") : authorizeModel.UserLoanRequestAuthorizationItem.UserLoanRequest.Installment.ToString("N0"));
				}
				else if (authorizeModel?.UserLoan != null)
				{
					pattern = pattern.Replace(tag, isPatternBase ? ("{" + authorizeModel?.UserLoan.Installment.ToString("N0") + "}") : authorizeModel.UserLoan.Installment.ToString("N0"));
				}
				break;
			case "{تاریخ درخواست}":
				if (authorizeModel?.UserLoanRequestAuthorizationItem != null)
				{
					pattern = pattern.Replace(tag, isPatternBase ? ("{" + authorizeModel?.UserLoanRequestAuthorizationItem.UserLoanRequest.RequestDate_Persian.Date + "}") : authorizeModel.UserLoanRequestAuthorizationItem.UserLoanRequest.RequestDate_Persian.Date);
				}
				else if (authorizeModel?.UserLoan != null)
				{
					pattern = pattern.Replace(tag, isPatternBase ? ("{" + authorizeModel?.UserLoan.StartDate_Persian.Date + "}") : authorizeModel.UserLoan.StartDate_Persian.Date);
				}
				break;
			case "{لینک قرارداد}":
			{
				Customer customer = await _customerService.GetById(_currentUserService.CustomerId.Value);
				string text2 = _appConfig.BaseUrl.TrimEnd('/') + $"/tpdgs/{customer.Code}/{authorizeModel.Code}";
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + text2 + "}") : text2);
				break;
			}
			case "{بخش متغیر لینک قرارداد}":
			{
				string text2 = $"/tpdgs/{(await _customerService.GetById(_currentUserService.CustomerId.Value)).Code}/{authorizeModel.Code}";
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + text2 + "}") : text2);
				break;
			}
			case "{نام و نام خانوادگی ضامن}":
			{
				string text = authorizeModel?.User?.FullName ?? "";
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + text + "}") : text);
				break;
			}
			case "{کد ملی ضامن}":
			{
				string text = authorizeModel?.User?.NationalCode ?? "";
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + text + "}") : text);
				break;
			}
			case "{موبایل ضامن}":
			{
				string text = authorizeModel?.User?.Mobile ?? "";
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + text + "}") : text);
				break;
			}
			}
		}
		return pattern;
	}

	private async Task<string> ExtractTextFromPattern(AgentFactorModel agentFactor, string pattern, bool isPatternBase, string code)
	{
		foreach (string item in new List<string> { "{نام تجاری نماینده}", "{نام مشتری}", "{تاریخ}", "{مبلغ}", "{مانده اعتبار کیف پول}", "{کد تائیدیه}", "{کد رهگیری}" })
		{
			switch (item)
			{
			case "{کد رهگیری}":
			{
				string text = agentFactor.Code.ToString();
				pattern = pattern.Replace(item, isPatternBase ? ("{" + text + "}") : text);
				break;
			}
			case "{کد تائیدیه}":
			{
				string text = code.ToString();
				pattern = pattern.Replace(item, isPatternBase ? ("{" + text + "}") : text);
				break;
			}
			case "{نام مشتری}":
				pattern = pattern.Replace(item, isPatternBase ? ("{" + agentFactor?.Member.FullName + "}") : agentFactor?.Member.FullName);
				break;
			case "{نام تجاری نماینده}":
				pattern = pattern.Replace(item, isPatternBase ? ("{" + agentFactor?.Agent.BusinessName + "}") : agentFactor?.Agent.BusinessName);
				break;
			case "{تاریخ}":
			{
				string text = agentFactor?.CreateAt_Persian.Date ?? "";
				pattern = pattern.Replace(item, isPatternBase ? ("{" + text + "}") : text);
				break;
			}
			case "{مبلغ}":
			{
				string text = ((double)(agentFactor?.Amount).Value).ToString("N0");
				pattern = pattern.Replace(item, isPatternBase ? ("{" + text + "}") : text);
				break;
			}
			case "{مانده اعتبار کیف پول}":
			{
				string text = agentFactor.Member.Credit.ToString("N0");
				pattern = pattern.Replace(item, isPatternBase ? ("{" + text + "}") : text);
				break;
			}
			}
		}
		return pattern;
	}

	private async Task<string> ExtractTextFromPattern(LoanPaymentModel loanPayment, List<LoanPaymentModel> otherLoanPayments, string pattern, bool isPatternBase)
	{
		List<string> list = new List<string>
		{
			"{نام مشتری}", "{نام فروشگاه}", "{مبلغ قسط}", "{مبلغ مانده قسط}", "{شماره قسط}", "{مبلغ واریز}", "{مبلغ آخرین واریز}", "{تاریخ واریز}", "{مانده وام}", "{لینک پرداخت}",
			"{بخش متغیر لینک پرداخت}", "{شماره پیگیری تراکنش}"
		};
		foreach (string tag in list)
		{
			switch (tag)
			{
			case "{نام مشتری}":
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + loanPayment?.UserLoanModel?.MemberModel.FullName + "}") : loanPayment?.UserLoanModel?.MemberModel.FullName);
				break;
			case "{نام فروشگاه}":
			{
				Customer customer = await _customerService.GetById(_currentUserService.CustomerId.Value);
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + customer.Name + "}") : customer.Name);
				break;
			}
			case "{مبلغ قسط}":
			{
				string val = ((double)loanPayment.InstallmentAmount).ToString("N0");
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + val + "}") : val);
				break;
			}
			case "{مبلغ مانده قسط}":
			{
				string val = Math.Max(loanPayment.InstallmentAmount - loanPayment.SumAmount, 0m).ToString("N0");
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + val + "}") : val);
				break;
			}
			case "{شماره قسط}":
			{
				string val = (loanPayment?.InstallmentNumber).ToString() ?? "";
				if (otherLoanPayments?.Any() ?? false)
				{
					val = val + "," + string.Join(',', (from x in otherLoanPayments
						orderby x.TargetMonth descending
						select x.InstallmentNumber.ToString()).ToList());
				}
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + val + "}") : val);
				break;
			}
			case "{مبلغ آخرین واریز}":
			{
				LoanPaymentOtherModel loanPaymentOtherModel = loanPayment.LoanPaymentOthers?.OrderByDescending((LoanPaymentOtherModel x) => x.SaveDate).FirstOrDefault();
				string val = ((loanPaymentOtherModel != null) ? loanPaymentOtherModel.Amount.ToString("N0") : (loanPayment?.SumAmount ?? 0m).ToString("N0"));
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + val + "}") : val);
				break;
			}
			case "{مبلغ واریز}":
			{
				string val = ((double)(loanPayment?.SumAmount).Value).ToString("N0");
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + val + "}") : val);
				break;
			}
			case "{تاریخ واریز}":
			{
				string val = loanPayment?.SaveDate_Persian.Date ?? "";
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + val + "}") : val);
				break;
			}
			case "{مانده وام}":
			{
				string val = ((double)(loanPayment?.UserLoanModel.Remain).Value).ToString("N0");
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + val + "}") : val);
				break;
			}
			case "{لینک پرداخت}":
			{
				Customer customer = await _customerService.GetById(_currentUserService.CustomerId.Value);
				string text = _appConfig.BaseUrl.TrimEnd('/') + "/pop/" + customer.Code + "/" + loanPayment.Code;
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + text + "}") : text);
				break;
			}
			case "{بخش متغیر لینک پرداخت}":
			{
				string text = (await _customerService.GetById(_currentUserService.CustomerId.Value)).Code + "/" + loanPayment.Code;
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + text + "}") : text);
				break;
			}
			case "شماره پیگیری تراکنش":
			{
				string val = "";
				if (loanPayment.PaymentMethod == PaymentMethods.Online)
				{
					using IServiceScope scope = _serviceProvider.CreateScope();
					MemberPurchase memberPurchase = await scope.ServiceProvider.GetRequiredService<MemberPurchaseService>().GetByUserLoanPaymentId(loanPayment.Id);
					if (memberPurchase != null)
					{
						MemberPurchaseModel memberPurchaseModel = new MemberPurchaseModel(memberPurchase);
						val = memberPurchaseModel.TransactionCode;
					}
				}
				else
				{
					val = loanPayment.Code.ToString();
				}
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + val + "}") : val);
				break;
			}
			}
		}
		return pattern;
	}

	private async Task<string> ExtractTextFromPattern(UserLoanRequestModel loanRequestModel, string pattern, bool isPatternBase)
	{
		List<string> list = new List<string>
		{
			"{نام مشتری}", "{نام فروشگاه}", "{مبلغ هر قسط}", "{مبلغ درخواستی}", "{تعداد قسط}", "{تاریخ درخواست}", "{نوع وام}", "{شماره درخواست}", "{تاریخ تغییر وضعیت}", "{نام نماینده}",
			"{وضعیت جدید}", "{وضعیت جدید نماینده}", "{سررسید واریز نماینده}", "{توسط نماینده با لیبل}"
		};
		foreach (string tag in list)
		{
			switch (tag)
			{
			case "{نام مشتری}":
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + loanRequestModel?.MemberModel.FullName + "}") : loanRequestModel?.MemberModel.FullName);
				break;
			case "{نام فروشگاه}":
			{
				Customer customer = await _customerService.GetById(_currentUserService.CustomerId.Value);
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + customer.Name + "}") : customer.Name);
				break;
			}
			case "{مبلغ هر قسط}":
			{
				string text = ((double)(loanRequestModel?.Installment).Value).ToString("N0");
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + text + "}") : text);
				break;
			}
			case "{مبلغ درخواستی}":
			{
				string text = (loanRequestModel?.Amount).ToString() ?? "";
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + text + "}") : text);
				break;
			}
			case "{تعداد قسط}":
			{
				string text = ((double)(loanRequestModel?.InstallmentCount).Value).ToString("N0");
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + text + "}") : text);
				break;
			}
			case "{تاریخ درخواست}":
			{
				string text = loanRequestModel?.RequestDate_Persian.Date ?? "";
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + text + "}") : text);
				break;
			}
			case "{نوع وام}":
			{
				string text = loanRequestModel?.LoanModel?.Title;
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + text + "}") : text);
				break;
			}
			case "{شماره درخواست}":
			{
				string text = loanRequestModel?.Code.ToString();
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + text + "}") : text);
				break;
			}
			case "{تاریخ تغییر وضعیت}":
			{
				string text = DateTime.Now.ToPersianDateObject().Date;
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + text + "}") : text);
				break;
			}
			case "{نام نماینده}":
			{
				string text = loanRequestModel?.AgentModel?.BusinessName ?? "";
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + text + "}") : text);
				break;
			}
			case "{توسط نماینده با لیبل}":
			{
				string text = loanRequestModel?.AgentModel?.BusinessName ?? "";
				if (!string.IsNullOrEmpty(text))
				{
					text = "توسط نماینده " + text;
				}
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + text + "}") : text);
				break;
			}
			case "{وضعیت جدید}":
			{
				string text = loanRequestModel?.Str_Status;
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + text + "}") : text);
				break;
			}
			case "{وضعیت جدید نماینده}":
			{
				string text = loanRequestModel?.Str_AgentStatus;
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + text + "}") : text);
				break;
			}
			case "{سررسید واریز نماینده}":
			{
				string text = ((loanRequestModel == null || loanRequestModel.UserLoan?.SubAgentDueDate.HasValue != true || !(loanRequestModel?.UserLoan?.SubAgentDueDate != DateTime.MinValue)) ? ((loanRequestModel?.UserLoan?.AgentDueDate_Persian.Date ?? "") ?? "") : ((loanRequestModel?.UserLoan?.SubAgentDueDate_Persian.Date ?? "") ?? ""));
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + text + "}") : text);
				break;
			}
			}
		}
		return pattern;
	}

	private async Task<string> ExtractTextFromPattern(LoginRegisterModel loginRegisterModel, string pattern, bool isPatternBase)
	{
		List<string> list = new List<string> { "{نام و نام خانوادگی}", "{نام کاربری}", "{تاریخ}", "{تاریخ و ساعت}", "{آی پی}", "{مرورگر}", "{نام فروشگاه}", "{نام تجاری نماینده}" };
		foreach (string tag in list)
		{
			switch (tag)
			{
			case "{نام و نام خانوادگی}":
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + loginRegisterModel.User_FullName + "}") : loginRegisterModel.User_FullName);
				break;
			case "{نام کاربری}":
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + loginRegisterModel.User_UserName + "}") : loginRegisterModel.User_UserName);
				break;
			case "{تاریخ و ساعت}":
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + loginRegisterModel.PersianDate.Date + "_" + loginRegisterModel.PersianDate.Time + "}") : (loginRegisterModel.PersianDate.Date + " " + loginRegisterModel.PersianDate.Time));
				break;
			case "{تاریخ}":
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + loginRegisterModel.PersianDate.Date + "}") : loginRegisterModel.PersianDate.Date);
				break;
			case "{آی پی}":
			{
				string text = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + text + "}") : text);
				break;
			}
			case "{مرورگر}":
			{
				StringValues stringValues = _httpContextAccessor.HttpContext.Request.Headers["User-Agent"];
				ClientInfo clientInfo = Parser.GetDefault().Parse(stringValues);
				string text = clientInfo.UA.Family + " " + clientInfo.OS.Family;
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + text + "}") : text);
				break;
			}
			case "{نام فروشگاه}":
			{
				Customer customer = await _customerService.GetById(_currentUserService.CustomerId.Value);
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + customer.Name + "}") : customer.Name);
				break;
			}
			case "{نام تجاری نماینده}":
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + loginRegisterModel.Agent_BusinessName + "}") : (loginRegisterModel.Agent_BusinessName ?? ""));
				break;
			}
		}
		return pattern;
	}

	private async Task<string> ExtractTextFromPattern(RegisterOtpModel registerOtpModel, string pattern, bool isPatternBase)
	{
		List<string> list = new List<string> { "{رمز تاییدیه}", "{تاریخ}", "{تاریخ و ساعت}", "{آی پی}", "{مرورگر}", "{نام فروشگاه}" };
		foreach (string tag in list)
		{
			switch (tag)
			{
			case "{رمز تاییدیه}":
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + registerOtpModel.Code + "}") : (registerOtpModel.Code ?? ""));
				break;
			case "{تاریخ و ساعت}":
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + registerOtpModel.PersianDate.Date + "} {" + registerOtpModel.PersianDate.Time + "}") : (registerOtpModel.PersianDate.Date + " " + registerOtpModel.PersianDate.Time));
				break;
			case "{تاریخ}":
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + registerOtpModel.PersianDate.Date + "}") : registerOtpModel.PersianDate.Date);
				break;
			case "{آی پی}":
			{
				string text = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + text + "}") : text);
				break;
			}
			case "{مرورگر}":
			{
				StringValues stringValues = _httpContextAccessor.HttpContext.Request.Headers["User-Agent"];
				ClientInfo clientInfo = Parser.GetDefault().Parse(stringValues);
				string text = clientInfo.UA.Family + " " + clientInfo.OS.Family;
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + text + "}") : text);
				break;
			}
			case "{نام فروشگاه}":
			{
				Customer customer = await _customerService.GetById(_currentUserService.CustomerId.Value);
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + customer.Name + "}") : customer.Name);
				break;
			}
			}
		}
		return pattern;
	}

	private async Task<string> ExtractTextFromPattern(WalletChargeModel walletModel, string pattern, bool isPatternBase)
	{
		List<string> list = new List<string> { "{نام مشتری}", "{نام فروشگاه}", "{لینک پرداخت}", "{تاریخ}" };
		foreach (string tag in list)
		{
			switch (tag)
			{
			case "{نام مشتری}":
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + walletModel.Member.FullName + "}") : walletModel.Member.FullName);
				break;
			case "{نام فروشگاه}":
			{
				Customer customer = await _customerService.GetById(_currentUserService.CustomerId.Value);
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + customer.Name + "}") : customer.Name);
				break;
			}
			case "{تاریخ}":
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + walletModel.PersianDate.Date + "}") : walletModel.PersianDate.Date);
				break;
			case "{لینک پرداخت}":
			{
				Customer customer = await _customerService.GetById(_currentUserService.CustomerId.Value);
				string text = _appConfig.BaseUrl.TrimEnd('/') + $"/walletcharge/?ccode={customer.Code}&mid={walletModel.Member.Id}";
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + text + "}") : text);
				break;
			}
			}
		}
		return pattern;
	}

	private async Task<string> ExtractTextFromPattern(AgentFactorModel agentFactorModel, string pattern, bool isPatternBase)
	{
		List<string> list = new List<string> { "{نام مشتری}", "{نام تجاری نماینده}", "{لینک پرداخت}", "{تاریخ}", "{مبلغ صورتحساب}", "{شماره سیستمی صورتحساب}" };
		foreach (string tag in list)
		{
			switch (tag)
			{
			case "{نام مشتری}":
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + agentFactorModel.Member.FullName + "}") : agentFactorModel.Member.FullName);
				break;
			case "{نام تجاری نماینده}":
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + agentFactorModel.Agent.BusinessName + "}") : agentFactorModel.Agent.BusinessName);
				break;
			case "{تاریخ}":
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + agentFactorModel.CreateAt_Persian.Date + "}") : agentFactorModel.CreateAt_Persian.Date);
				break;
			case "{لینک پرداخت}":
			{
				Customer customer = await _customerService.GetById(_currentUserService.CustomerId.Value);
				string text = _appConfig.BaseUrl.TrimEnd('/') + $"/afpp/?customerCode={customer.Code}&agentFactorCode={agentFactorModel.Code}";
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + text + "}") : text);
				break;
			}
			case "{مبلغ صورتحساب}":
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + agentFactorModel.Amount.ToString("N0") + "}") : agentFactorModel.Amount.ToString("N0"));
				break;
			case "{شماره سیستمی صورتحساب}":
				pattern = pattern.Replace(tag, isPatternBase ? ("{" + agentFactorModel.Code + "}") : agentFactorModel.Code.ToString());
				break;
			}
		}
		return pattern;
	}

	private async Task<string> ExtractTextFromPattern(CustomerOrderModel model, string pattern, bool isPatternBase, Customer customer)
	{
		foreach (string item in new List<string> { "{نام مشتری}", "{نام فروشگاه}", "{کد ملی}", "{تاریخ}", "{کالای مورد درخواست}" })
		{
			switch (item)
			{
			case "{نام مشتری}":
				pattern = pattern.Replace(item, isPatternBase ? ("{" + model.Name + "}") : model.Name);
				break;
			case "{نام فروشگاه}":
				pattern = pattern.Replace(item, isPatternBase ? ("{" + customer.Name + "}") : customer.Name);
				break;
			case "{تاریخ}":
				pattern = pattern.Replace(item, isPatternBase ? ("{" + model.SaveDate_Persian.Date + "}") : model.SaveDate_Persian.Date);
				break;
			case "{کد ملی}":
				pattern = pattern.Replace(item, isPatternBase ? ("{" + model.NationalCode + "}") : model.NationalCode);
				break;
			case "{کالای مورد درخواست}":
				pattern = pattern.Replace(item, isPatternBase ? ("{" + model.ProductName + "}") : model.ProductName);
				break;
			}
		}
		return pattern;
	}

	private async Task<SmsPanel> SetPayamakService(AppSettings appSettings = null)
	{
		SmsPanel smsPanel = SmsPanel.Unknown;
		if (appSettings == null)
		{
			appSettings = await _appSettingsService.GetAppSettings();
		}
		if (appSettings != null)
		{
			AppSettingsModel appSettingsModel = new AppSettingsModel(appSettings);
			switch (appSettingsModel.DefaultSmsPanel)
			{
			case SmsPanel.JetAqsaat:
				if (_appConfig.DefaultSmsPanel == "MelliPayamak" && _melliPayamakConfig != null)
				{
					_payamakService = new MelliPayamakService(new MelliPayamakConfig
					{
						From = _melliPayamakConfig.From,
						Password = _melliPayamakConfig.Password,
						Username = _melliPayamakConfig.Username
					});
					smsPanel = SmsPanel.JetAqsaat_MelliPayamak;
				}
				else if (_appConfig.DefaultSmsPanel == "KaveNegar" && _kaveNegarConfig != null)
				{
					_payamakService = new KaveNegarService(new KaveNegarConfig
					{
						ApiKey = _kaveNegarConfig.ApiKey,
						Sender = _kaveNegarConfig.Sender,
						RegTemplate = _kaveNegarConfig.RegTemplate
					});
					smsPanel = SmsPanel.JetAqsaat_KaveNegar;
				}
				else if (_appConfig.DefaultSmsPanel == "IPPanel" && _ippanelConfig != null)
				{
					_payamakService = new IPPanelService(new IPPanelConfig
					{
						ApiKey = _ippanelConfig.ApiKey,
						Sender = _ippanelConfig.Sender,
						RegTemplate = _ippanelConfig.RegTemplate
					});
					smsPanel = SmsPanel.JetAqsaat_IPPanel;
				}
				break;
			case SmsPanel.JetAqsaat_MelliPayamak:
				if (_melliPayamakConfig != null)
				{
					_payamakService = new MelliPayamakService(new MelliPayamakConfig
					{
						From = _melliPayamakConfig.From,
						Password = _melliPayamakConfig.Password,
						Username = _melliPayamakConfig.Username
					});
				}
				smsPanel = SmsPanel.JetAqsaat_MelliPayamak;
				break;
			case SmsPanel.JetAqsaat_IPPanel:
				if (_ippanelConfig != null)
				{
					_payamakService = new IPPanelService(new IPPanelConfig
					{
						Sender = _ippanelConfig.Sender,
						ApiKey = _ippanelConfig.ApiKey
					});
				}
				smsPanel = SmsPanel.JetAqsaat_IPPanel;
				break;
			case SmsPanel.MelliPayamak:
				if (appSettingsModel.MelliPayamakConfig != null)
				{
					_payamakService = new MelliPayamakService(new MelliPayamakConfig
					{
						From = appSettingsModel.MelliPayamakConfig.From,
						Password = appSettingsModel.MelliPayamakConfig.Password,
						Username = appSettingsModel.MelliPayamakConfig.Username
					});
				}
				smsPanel = SmsPanel.MelliPayamak;
				break;
			case SmsPanel.IPPanel:
				if (appSettingsModel.IPPanelConfig != null)
				{
					_payamakService = new IPPanelService(new IPPanelConfig
					{
						ApiKey = appSettingsModel.IPPanelConfig.ApiKey,
						Sender = appSettingsModel.IPPanelConfig.Sender
					});
				}
				smsPanel = SmsPanel.IPPanel;
				break;
			case SmsPanel.JetAqsaat_KaveNegar:
				if (_kaveNegarConfig != null)
				{
					_payamakService = new KaveNegarService(new KaveNegarConfig
					{
						ApiKey = _kaveNegarConfig.ApiKey,
						Sender = _kaveNegarConfig.Sender,
						RegTemplate = _kaveNegarConfig.RegTemplate
					});
				}
				smsPanel = SmsPanel.JetAqsaat_KaveNegar;
				break;
			case SmsPanel.KaveNegar:
				if (appSettingsModel.KaveNegarConfig != null)
				{
					_payamakService = new KaveNegarService(new KaveNegarConfig
					{
						ApiKey = appSettingsModel.KaveNegarConfig.ApiKey,
						Sender = appSettingsModel.KaveNegarConfig.Sender,
						RegTemplate = appSettingsModel.KaveNegarConfig.RegTemplate
					});
				}
				smsPanel = SmsPanel.KaveNegar;
				break;
			case SmsPanel.WebOne:
				if (appSettingsModel.WebOneConfig != null)
				{
					_payamakService = new WebOneService(new WebOneConfig
					{
						ApiKey = appSettingsModel.WebOneConfig.ApiKey,
						Sender = appSettingsModel.WebOneConfig.Sender,
						RegTemplate = appSettingsModel.WebOneConfig.RegTemplate
					});
				}
				smsPanel = SmsPanel.WebOne;
				break;
			case SmsPanel.Bale:
				if (appSettingsModel.BaleConfig != null)
				{
					_payamakService = new BaleApiService(new BaleConfig
					{
						ApiKey = appSettingsModel.BaleConfig.ApiKey,
						BotId = appSettingsModel.BaleConfig.BotId
					});
				}
				smsPanel = SmsPanel.Bale;
				break;
			}
		}
		return smsPanel;
	}

	private void PrepareIPPanelKeys(SendIPPanelPattern sendPayamak, string input, string pattern, string json)
	{
		Regex regex = new Regex("{(.*?)}");
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		MatchCollection matchCollection = regex.Matches(pattern);
		MatchCollection matchCollection2 = regex.Matches(input);
		for (int i = 0; i < matchCollection.Count; i++)
		{
			dictionary.TryAdd(matchCollection[i].Groups[1].Value, (matchCollection2.Count > i) ? matchCollection2[i].Groups[1].Value : "");
		}
		List<object>? list = JsonConvert.DeserializeObject<List<object>>(json);
		sendPayamak.Variable = new ExpandoObject();
		foreach (dynamic item in list)
		{
			string key = ((string)item.key).Replace("{", "").Replace("}", "");
			string key2 = ((string)item.tag).Replace("{", "").Replace("}", "");
			key2 = (dictionary.ContainsKey(key2) ? dictionary[key2] : "");
			((IDictionary<string, object>)sendPayamak.Variable).Add(key, (object)key2);
		}
	}

	private void PrepareKavehTokens(SendKaveNegar sendPayamak, string input)
	{
		MatchCollection matchCollection = new Regex("{(.*?)}").Matches(input);
		int num = 0;
		foreach (Match item in matchCollection)
		{
			switch (num++)
			{
			case 0:
				sendPayamak.Token1 = item.Groups[1].Value.Replace(" ", "_");
				break;
			case 1:
				sendPayamak.Token2 = item.Groups[1].Value.Replace(" ", "_");
				break;
			case 2:
				sendPayamak.Token3 = item.Groups[1].Value.Replace(" ", "_");
				break;
			case 3:
				sendPayamak.Token10 = item.Groups[1].Value.Replace(" ", "_");
				break;
			case 4:
				sendPayamak.Token20 = item.Groups[1].Value.Replace(" ", "_");
				break;
			}
		}
	}

	private void PrepareMelliPayamakTextPattern(SendMelliPayamak sendMelliPayamak, string input)
	{
		MatchCollection source = new Regex("{(.*?)}").Matches(input);
		sendMelliPayamak.Text = string.Join(';', source.Select((Match x) => x.Groups[1].Value));
	}
}
