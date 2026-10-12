using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using LoanSystem.Core.Configuration;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Services.Users;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Data.Enums;
using LoanSystemAvizhe.Services.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LoanSystem.Site.Areas.Admin.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class AppSettingsController : BaseAdminController
{
	private readonly UserManager _userManager;

	private readonly IAppSettingsService _appSettingsService;

	private readonly ScheduleJobManager _scheduleJobManager;

	private readonly ICurrentUserService _currentUserService;

	private readonly AppConfig _appConfig;

	private readonly ICustomerService _customerService;

	public AppSettingsController(ILogger<AppSettingsController> logger, UserManager userManager, IAppSettingsService appSettingsService, ScheduleJobManager scheduleJobManager, ICurrentUserService currentUserService, IOptions<AppConfig> appConfig, ICustomerService customerService)
		: base(logger)
	{
		_appSettingsService = appSettingsService;
		_scheduleJobManager = scheduleJobManager;
		_currentUserService = currentUserService;
		_appConfig = appConfig.Value;
		_customerService = customerService;
		_userManager = userManager;
	}

	[HttpGet("[action]")]
	public Task<ResponseState<AppSettingsModel>> Get()
	{
		return TryCatch<AppSettingsModel>(async () =>
		{
			AppSettings appSettings = await GetCurrentAppSettingsAsync(true);
			if (appSettings == null)
			{
				return Error<AppSettingsModel>(default, "تنظیمات برای مشتری انتخاب‌شده پیدا نشد.");
			}
			AppSettingsModel appSettingsModel = new AppSettingsModel(appSettings);
			List<DashboardSettingsModel> source = JsonSerializer.Deserialize<List<DashboardSettingsModel>>((!string.IsNullOrWhiteSpace(appSettings?.DashboardSettingsJson)) ? appSettings.DashboardSettingsJson : "[]");
			appSettingsModel.DashboardSettings = source.FirstOrDefault((DashboardSettingsModel x) => x.UserId == _currentUserService.UserId) ?? new DashboardSettingsModel();
			return Success(appSettingsModel);
		});
	}

	[HttpPost("[action]")]
	public Task<ResponseState<Guid>> Edit([FromBody] AppSettingsModel model)
	{
		return TryCatch(async () =>
		{
			if (model.IsOnlinePaymentActive && model.OnlinePaymentGateway == OnlinePaymentGateways.None)
			{
				return Error("نوع درگاه مورد نظر را انتخاب کنید.");
			}
			if (model.OnlinePaymentGateway == OnlinePaymentGateways.ZarinPal && string.IsNullOrWhiteSpace(model.ZarinpalMerchantCode))
			{
				return Error("مرجنت کد زرین پال را وارد کنید.");
			}
			if (model.OnlinePaymentGateway == OnlinePaymentGateways.NextPay && string.IsNullOrWhiteSpace(model.NextPayApiKey))
			{
				return Error("کلید رابط نکست پی را وارد کنید.");
			}
			if (model.OnlinePaymentGateway == OnlinePaymentGateways.JetAqsaat && string.IsNullOrWhiteSpace(model.Shaba))
			{
				return Error("شماره شبا را وارد کنید.");
			}
			if (model.OnlinePaymentGateway == OnlinePaymentGateways.Sep && string.IsNullOrWhiteSpace(model.SepTerminalId))
			{
				return Error("شماره ترمینال سامان را وارد کنید.");
			}
			if (model.OnlinePaymentGateway == OnlinePaymentGateways.Sadad && string.IsNullOrWhiteSpace(model.SadadMerchantId))
			{
				return Error("مرچنت کد سداد را وارد کنید.");
			}
			if (model.OnlinePaymentGateway == OnlinePaymentGateways.Sadad && string.IsNullOrWhiteSpace(model.SadadTerminalId))
			{
				return Error("شماره ترمینال سداد را وارد کنید.");
			}
			if (model.DefaultSmsPanel == SmsPanel.JetAqsaat && !_appConfig.IsCloud)
			{
				return Error("در نسخه اختصاصی امکان استفاده از پنل پیامک جت اقساط وجود ندارد.");
			}
			if (model.DefaultSmsPanel == SmsPanel.MelliPayamak && (string.IsNullOrWhiteSpace(model.MelliPayamakConfig?.Username) || string.IsNullOrWhiteSpace(model.MelliPayamakConfig?.Password) || string.IsNullOrWhiteSpace(model.MelliPayamakConfig?.From)))
			{
				return Error("پر کردن تمام فیلدهای تنظیماتی ملی پیامک الزامی است.");
			}
			if (model.DefaultSmsPanel == SmsPanel.IPPanel && (string.IsNullOrWhiteSpace(model.IPPanelConfig?.ApiKey) || string.IsNullOrWhiteSpace(model.IPPanelConfig?.Sender)))
			{
				return Error("پر کردن تمام فیلدهای تنظیماتی IPPanel الزامی است.");
			}
			if (model.DefaultEmailPanel == EmailPanel.JetAqsaat && !_appConfig.IsCloud)
			{
				return Error("در نسخه اختصاصی امکان استفاده از سرویس پیامک جت اقساط وجود ندارد.");
			}
			if (model.DefaultEmailPanel == EmailPanel.Custom)
			{
				if (!string.IsNullOrWhiteSpace(model.EmailConfig?.Name) && !string.IsNullOrWhiteSpace(model.EmailConfig?.SmtpUsername) && !string.IsNullOrWhiteSpace(model.EmailConfig?.SmtpHost) && !string.IsNullOrWhiteSpace(model.EmailConfig?.SmtpPassword))
				{
					EmailConfig emailConfig = model.EmailConfig;
					if (emailConfig == null || emailConfig.SmtpPort != 0)
					{
						goto IL_03c8;
					}
				}
				return Error("پر کردن تمام فیلدهای تنظیماتی سرویس ایمیل الزامی است.");
			}
			goto IL_03c8;
			IL_03c8:
			if (!string.IsNullOrWhiteSpace(model.ApiPassword) && model.ApiPassword.Length < 8)
			{
				return Error("طول کلمه عبور نباید کمتر از 8 کاراکتر باشد");
			}
			if (model.DailyReportEmailTime == "00:00")
			{
				return Error("تنظیم ساعت 12 برای گزارش روزانه مجاز نمی باشد.");
			}
			if (!string.IsNullOrWhiteSpace(model.IranValidityConfig?.BaseUrl) && !model.IranValidityConfig.BaseUrl.StartsWith("https://"))
			{
				return Error("آدرس پایه سرویس اعتبارسنجی ایران باید با https:// شروع شود.");
			}
			AppSettings entity = await GetCurrentAppSettingsAsync(true);
			if (entity == null)
			{
				entity = new AppSettings { CustomerId = await ResolveSettingsCustomerIdAsync() };
			}
			bool mustInsertApiUser = false;
			if (!string.IsNullOrWhiteSpace(model.ApiKey) && entity.ApiKey != model.ApiKey)
			{
				mustInsertApiUser = true;
				List<AppSettings> list = await _appSettingsService.GetByCondition((AppSettings x) => x.Id != entity.Id && x.ApiKey == model.ApiKey, false);
				if (list != null && list.Any())
				{
					return Error("کلید رابط مجاز نمی باشد.");
				}
			}
			entity.RoundCountNumber = model.RoundCountNumber;
			entity.AppendToWhichInstallment = model.AppendToWhichInstallment;
			entity.ZarinpalMerchantCode = model.ZarinpalMerchantCode;
			entity.IPGParsianLoginAccount = model.IPGParsianLoginAccount;
			entity.NextPayApiKey = model.NextPayApiKey;
			entity.IsOnlinePaymentActive = model.IsOnlinePaymentActive;
			entity.SepTerminalId = model.SepTerminalId;
			entity.Shaba = model.Shaba;
			entity.BankInfo = model.BankInfo;
			entity.OnlinePaymentGateway = model.OnlinePaymentGateway;
			entity.WalletWithdrawNotificationSmsPatternId = model.WalletWithdrawNotificationSmsPattern?.Id;
			entity.WalletWithdrawOtpSmsPatternId = model.WalletWithdrawOtpSmsPattern?.Id;
			entity.NewUserLoanSmsPatternId = model.NewUserLoanSmsPattern?.Id;
			entity.DueDateRememberSmsPatternId = model.DueDateRememberSmsPattern?.Id;
			entity.OverDueDateSmsPatternId = model.OverDueDateSmsPattern?.Id;
			entity.ThanksPaymentSmsPatternId = model.ThanksPaymentSmsPattern?.Id;
			entity.BeforeDueDateSmsPatternId = model.BeforeDueDateSmsPattern?.Id;
			entity.LoginSmsPatternId = model.LoginSmsPattern?.Id;
			entity.DigitalSignLinkSmsPatternId = model.DigitalSignLinkSmsPattern?.Id;
			entity.DigitalSignCompleteSmsPatternId = model.DigitalSignCompleteSmsPattern?.Id;
			entity.DigitalSignOtpSmsPatternId = model.DigitalSignOtpSmsPattern?.Id;
			entity.ThirdPersonDigitalSignOtpSmsPatternId = model.ThirdPersonDigitalSignOtpSmsPattern?.Id;
			entity.ThirdPersonDigitalSignCompleteSmsPatternId = model.ThirdPersonDigitalSignCompleteSmsPattern?.Id;
			entity.ThirdPersonDigitalSignLinkSmsPatternId = model.ThirdPersonDigitalSignLinkSmsPattern?.Id;
			entity.LoginEmailPatternId = model.LoginEmailPattern?.Id;
			entity.NewOrderEmailPatternId = model.NewOrderEmailPattern?.Id;
			entity.OverDueDateEmailPatternId = model.OverDueDateEmailPattern?.Id;
			entity.RegisterEmailPatternId = model.RegisterEmailPattern?.Id;
			entity.RegisterOtpEmailPatternId = model.RegisterOtpEmailPattern?.Id;
			entity.BeforeDueDateEmailPatternId = model.BeforeDueDateEmailPattern?.Id;
			entity.WalletChargeEmailPatternId = model.WalletChargeEmailPattern?.Id;
			entity.ThanksPaymentEmailPatternId = model.ThanksPaymentEmailPattern?.Id;
			entity.DueDateRememberEmailPatternId = model.DueDateRememberEmailPattern?.Id;
			entity.RegisterSmsPatternId = model.RegisterSmsPattern?.Id;
			entity.RegisterOtpSmsPatternId = model.RegisterOtpSmsPattern?.Id;
			entity.WalletChargeSmsPatternId = model.WalletChargeSmsPattern?.Id;
			entity.AgentFactorPaymentLinkSmsPatternId = model.AgentFactorPaymentLinkSmsPattern?.Id;
			entity.NewOrderSmsPatternId = model.NewOrderSmsPattern?.Id;
			entity.DueDatedRememberSmsTime = (string.IsNullOrWhiteSpace(model.DueDatedRememberSmsTime) ? ((TimeSpan?)null) : new TimeSpan?(TimeSpan.Parse(model.DueDatedRememberSmsTime)));
			entity.DueDatedRememberEmailTime = (string.IsNullOrWhiteSpace(model.DueDatedRememberEmailTime) ? ((TimeSpan?)null) : new TimeSpan?(TimeSpan.Parse(model.DueDatedRememberEmailTime)));
			entity.DailyReportEmailTime = (string.IsNullOrWhiteSpace(model.DailyReportEmailTime) ? ((TimeSpan?)null) : new TimeSpan?(TimeSpan.Parse(model.DailyReportEmailTime)));
			entity.IsRememberEmailActive = model.IsRememberEmailActive;
			entity.IsRememberSmsActive = model.IsRememberSmsActive;
			entity.IsDailyReportEmailActive = model.IsDailyReportEmailActive;
			entity.OverDueDaysCount = model.OverDueDaysCount;
			entity.IsDueDateSmsActive = model.IsDueDateSmsActive;
			entity.BeforeDueDaysCount = model.BeforeDueDaysCount;
			entity.BeforeDueDaysCount_Email = model.BeforeDueDaysCount_Email;
			entity.OverDueDaysCount_Email = model.OverDueDaysCount_Email;
			entity.OverDueDayRemeberOnce_Email = model.OverDueDayRemeberOnce_Email;
			entity.OverDueDayRemeberOnce = model.OverDueDayRemeberOnce;
			entity.OverDueDayGapPeriod = model.OverDueDayGapPeriod;
			entity.BankAccountsJson = model.BankAccountsJson;
			entity.Address = model.Address;
			entity.ShopLink = model.ShopLink;
			entity.ShopTitle = model.ShopTitle;
			entity.Phones = model.Phones;
			entity.Email = model.Email;
			entity.EnamadLink = model.EnamadLink;
			entity.SamandehiLink = model.SamandehiLink;
			entity.EtehadieLink = model.EtehadieLink;
			entity.AnjomanSenfiLink = model.AnjomanSenfiLink;
			entity.KasboKarhayeInternetiTehranLink = model.KasboKarhayeInternetiTehranLink;
			entity.RubikaLink = model.RubikaLink;
			entity.WhatsappLink = model.WhatsappLink;
			entity.TelegramLink = model.TelegramLink;
			entity.InstagramLink = model.InstagramLink;
			entity.AparatLink = model.AparatLink;
			entity.ApplicationLink = model.ApplicationLink;
			entity.BusinessTitle = model.BusinessTitle;
			entity.DepositChargeDueDateStatuses = string.Join(',', model.DepositChargeDueDateStatuses);
			entity.SponsoreDueDateStatuses = string.Join(',', model.SponsoreDueDateStatuses);
			entity.BankGatewayRouterBaseUrl = model.BankGatewayRouterBaseUrl;
			entity.ApiKey = model.ApiKey;
			entity.ApiPassword = model.ApiPassword;
			entity.SadadTerminalId = model.SadadTerminalId;
			entity.SepehrTerminalId = model.SepehrTerminalId;
			entity.SadadMerchantId = model.SadadMerchantId;
			entity.SadadMerchantKey = model.SadadMerchantKey;
			entity.AgentOfficePhone = model.AgentOfficePhone;
			entity.SiteTheme = model.SiteTheme;
			entity.OtpRequiredInManualMemberRegistration = model.OtpRequiredInManualMemberRegistration;
			entity.ShowSubmitFirstLoanRequestNotifInMemberPanel = model.ShowSubmitFirstLoanRequestNotifInMemberPanel;
			entity.PercentOfConsideringAsPaid = model.PercentOfConsideringAsPaid;
			entity.PreventCreateLoanNonCurrentDate = model.PreventCreateLoanNonCurrentDate;
			entity.CalculatorType = model.CalculatorType;
			entity.DefaultInquiryPanel = model.DefaultInquiryPanel;
			entity.VerifyMobileByNationalCodeInRegister = model.VerifyMobileByNationalCodeInRegister;
			entity.InquiryPersonInfoInRegister = model.InquiryPersonInfoInRegister;
			entity.InquiryPostalAddressInRegister = model.InquiryPostalAddressInRegister;
			entity.InquiryBankInRegister = model.InquiryBankInRegister;
			entity.AgentFactorCommission = model.AgentFactorCommission;
			entity.AgentFactorOnlinePaymentGateway = model.AgentFactorOnlinePaymentGateway;
			entity.MaxAgentFactorCommission = model.MaxAgentFactorCommission;
			entity.MemberCanChangeProfile = model.MemberCanChangeProfile;
			entity.IsMemberRegistrationDisabled = model.IsMemberRegistrationDisabled;
			if (model.IceConfig != null)
			{
				entity.IceConfigJson = JsonSerializer.Serialize(model.IceConfig);
			}
			else
			{
				entity.IceConfigJson = "";
			}
			if (model.IranValidityConfig != null)
			{
				entity.IranValidityConfigJson = JsonSerializer.Serialize(model.IranValidityConfig);
			}
			else
			{
				entity.IranValidityConfigJson = "";
			}
			if (model.FinnoTechConfig != null)
			{
				entity.FinnoTechConfigJson = JsonSerializer.Serialize(model.FinnoTechConfig);
			}
			else
			{
				entity.FinnoTechConfigJson = "";
			}
			if (model.EmzameConfig != null)
			{
				entity.EmzameConfigJson = JsonSerializer.Serialize(model.EmzameConfig);
			}
			else
			{
				entity.EmzameConfigJson = "";
			}
			if (model.ApiIRConfig != null)
			{
				entity.ApiIRConfigJson = JsonSerializer.Serialize(model.ApiIRConfig);
			}
			else
			{
				entity.ApiIRConfigJson = "";
			}
			if (model.RefahBetaConfig != null)
			{
				entity.RefahBetaConfigJson = JsonSerializer.Serialize(model.RefahBetaConfig);
			}
			else
			{
				entity.RefahBetaConfigJson = "";
			}
			List<DashboardSettingsModel> list2 = JsonSerializer.Deserialize<List<DashboardSettingsModel>>((!string.IsNullOrWhiteSpace(entity.DashboardSettingsJson)) ? entity.DashboardSettingsJson : "[]");
			int num = list2.FindIndex((DashboardSettingsModel x) => x.UserId == _currentUserService.UserId);
			model.DashboardSettings.UserId = _currentUserService.UserId.Value;
			if (num != -1)
			{
				list2.RemoveAt(num);
			}
			list2.Add(model.DashboardSettings);
			entity.DashboardSettingsJson = JsonSerializer.Serialize(list2);
			entity.DefaultSmsPanel = model.DefaultSmsPanel;
			if (model.MelliPayamakConfig != null)
			{
				entity.MelliPayamakConfigJson = JsonSerializer.Serialize(model.MelliPayamakConfig);
			}
			else
			{
				entity.MelliPayamakConfigJson = "";
			}
			if (model.KaveNegarConfig != null)
			{
				entity.KaveNegarConfigJson = JsonSerializer.Serialize(model.KaveNegarConfig);
			}
			else
			{
				entity.KaveNegarConfigJson = "";
			}
			if (model.IPPanelConfig != null)
			{
				entity.IPPanelConfigJson = JsonSerializer.Serialize(model.IPPanelConfig);
			}
			else
			{
				entity.IPPanelConfigJson = "";
			}
			if (model.WebOneConfig != null)
			{
				entity.WebOneConfigJson = JsonSerializer.Serialize(model.WebOneConfig);
			}
			else
			{
				entity.WebOneConfigJson = "";
			}
			if (model.BaleConfig != null)
			{
				entity.BaleConfigJson = JsonSerializer.Serialize(model.BaleConfig);
			}
			else
			{
				entity.BaleConfigJson = "";
			}
			entity.DefaultEmailPanel = model.DefaultEmailPanel;
			if (model.EmailConfig != null)
			{
				entity.EmailConfigJson = JsonSerializer.Serialize(model.EmailConfig);
			}
			else
			{
				entity.EmailConfigJson = "";
			}
			if (mustInsertApiUser)
			{
				User user = (await _userManager.GetUsersInRoleAsync("AppApiUser"))?.FirstOrDefault();
				if (user == null)
				{
					Customer customer = await _customerService.GetById(entity.CustomerId);
					user = new User
					{
						FirstName = "کاربر",
						LastName = "رابط برنامه نویس",
						Email = $"{customer.Code}-apiuser@mail.com",
						UserName = $"{customer.Code}-apiuser",
						IsActive = true,
						CustomerId = entity.CustomerId,
						RegDate2 = DateTime.Now
					};
					user.UserRoleNames = new List<string> { "AppApiUser" };
					PasswordHasher<User> passwordHasher = new PasswordHasher<User>();
					string passwordHash = passwordHasher.HashPassword(user, "aw#$#$$%^$CTG34ERcgDF");
					user.PasswordHash = passwordHash;
					List<Claim> claims = new List<Claim>();
					await _userManager.Insert(user, "aw#$#$$%^$CTG34ERcgDF", user.UserRoleNames, claims);
				}
			}
			await _appSettingsService.Update(entity);
			await _scheduleJobManager.SetOrDeleteDueDateSmsJob();
			await _scheduleJobManager.SetOrDeleteDueDateEmailJob();
			await _scheduleJobManager.SetOrDeleteDailyReportEmailJob();
			return Success(entity.Id, "عملیات با موفقیت انجام شد.");
		});
	}

	[HttpPost("[action]")]
	public Task<ResponseState<Guid>> SetThemeSettings([FromBody] ThemeSettingsModel themeSettings)
	{
		return TryCatch(async () =>
		{
			AppSettings entity = await GetCurrentAppSettingsAsync(true);
			if (entity == null)
			{
				entity = new AppSettings();
			}
			if (themeSettings != null)
			{
				entity.ThemeSettingsJson = JsonSerializer.Serialize(themeSettings);
			}
			else
			{
				entity.ThemeSettingsJson = "";
			}
			await _appSettingsService.Update(entity);
			return Success(entity.Id, "عملیات با موفقیت انجام شد.");
		});
	}

	private async Task<Guid> ResolveSettingsCustomerIdAsync()
	{
		if (_currentUserService.CustomerId.HasValue)
		{
			return _currentUserService.CustomerId.Value;
		}
		if (!_currentUserService.IsInRole("SuperAdmin"))
		{
			throw new UnauthorizedAccessException("A customer context is required to access application settings.");
		}
		List<Customer> customers = await _customerService.GetAllCustomers();
		if (customers.Count != 1)
		{
			throw new InvalidOperationException("Select a customer before editing application settings.");
		}
		return customers[0].Id;
	}

	private async Task<AppSettings> GetCurrentAppSettingsAsync(bool createIfMissing)
	{
		Guid customerId = await ResolveSettingsCustomerIdAsync();
		AppSettings entity = _currentUserService.CustomerId.HasValue
			? await _appSettingsService.GetAppSettings()
			: await _appSettingsService.GetAppSettings(customerId);
		if (entity == null && createIfMissing)
		{
			entity = new AppSettings { CustomerId = customerId };
		}
		return entity;
	}
}
