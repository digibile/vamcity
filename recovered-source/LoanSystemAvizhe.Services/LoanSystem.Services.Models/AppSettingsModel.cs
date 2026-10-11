using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Models.OnlineMoneyTransferPanel;
using LoanSystemAvizhe.Data.Enums;
using LoanSystemAvizhe.Services.Email;
using LoanSystemAvizhe.Services.Inquiry.Models.ApiIR;
using LoanSystemAvizhe.Services.Inquiry.Models.FinnoTech;
using LoanSystemAvizhe.Services.Models.IranValidity;
using LoanSystemAvizhe.Services.Models.RefahBeta;
using LoanSystemAvizhe.Services.Sms.Models.IPPanel;
using LoanSystemAvizhe.Services.Sms.Models.KaveNegar;
using LoanSystemAvizhe.Services.Sms.Models.MelliPayamak;
using LoanSystemAvizhe.Services.Sms.Models.WebOne;

namespace LoanSystem.Services.Models;

public class AppSettingsModel : BaseModel
{
	public SiteThemes SiteTheme { get; set; }

	public EmailPanel DefaultEmailPanel { get; set; }

	public string DefaultEmailPanelText => DefaultEmailPanel.ToString();

	public SmsPanel DefaultSmsPanel { get; set; }

	public string DefaultSmsPanelText => DefaultSmsPanel.ToString();

	public InquiryPanel DefaultInquiryPanel { get; set; }

	public string DefaultInquiryPanelText => DefaultInquiryPanel.ToString();

	public bool VerifyMobileByNationalCodeInRegister { get; set; }

	public bool InquiryPersonInfoInRegister { get; set; }

	public bool InquiryPostalAddressInRegister { get; set; }

	public bool InquiryBankInRegister { get; set; }

	public MelliPayamakConfig MelliPayamakConfig { get; set; } = new MelliPayamakConfig();

	public KaveNegarConfig KaveNegarConfig { get; set; } = new KaveNegarConfig();

	public WebOneConfig WebOneConfig { get; set; } = new WebOneConfig();

	public BaleConfig BaleConfig { get; set; } = new BaleConfig();

	public IPPanelConfig IPPanelConfig { get; set; } = new IPPanelConfig();

	public EmailConfig EmailConfig { get; set; } = new EmailConfig();

	public FinnoTechConfig FinnoTechConfig { get; set; } = new FinnoTechConfig();

	public EmzameConfig EmzameConfig { get; set; } = new EmzameConfig();

	public ApiIRConfig ApiIRConfig { get; set; } = new ApiIRConfig();

	public RefahBetaConfig RefahBetaConfig { get; set; } = new RefahBetaConfig();

	public string ApiKey { get; set; }

	public string ApiPassword { get; set; }

	public byte RoundCountNumber { get; set; }

	public AppendToWhichInstallment AppendToWhichInstallment { get; set; }

	public bool IsOnlinePaymentActive { get; set; }

	public OnlinePaymentGateways OnlinePaymentGateway { get; set; }

	public OnlinePaymentGateways AgentFactorOnlinePaymentGateway { get; set; }

	public decimal AgentFactorCommission { get; set; }

	public decimal MaxAgentFactorCommission { get; set; }

	public string ZarinpalMerchantCode { get; set; }

	public string IPGParsianLoginAccount { get; set; }

	public string NextPayApiKey { get; set; }

	public string SepTerminalId { get; set; }

	public string SadadTerminalId { get; set; }

	public long SepehrTerminalId { get; set; }

	public string SadadMerchantId { get; set; }

	public string SadadMerchantKey { get; set; }

	public string Shaba { get; set; }

	public string BankInfo { get; set; }

	public bool IsRememberSmsActive { get; set; }

	public string DueDatedRememberSmsTime { get; set; }

	public bool IsDueDateSmsActive { get; set; }

	[Range(0.0, double.MaxValue, ErrorMessage = "تعداد روز پیامک یادآوری پیش از موعد نمی تواند کمتر از صفر باشد")]
	public int BeforeDueDaysCount { get; set; }

	[Range(0.0, double.MaxValue, ErrorMessage = "تعداد روز پیامک اقساط معوق نمی تواند کمتر از صفر باشد")]
	public int OverDueDaysCount { get; set; }

	public bool OverDueDayRemeberOnce { get; set; }

	public int OverDueDayGapPeriod { get; set; }

	public DashboardSettingsModel DashboardSettings { get; set; }

	public int SmsCredit { get; set; }

	public SmsPatternModel NewUserLoanSmsPattern { get; set; }

	public SmsPatternModel DueDateRememberSmsPattern { get; set; }

	public SmsPatternModel ThanksPaymentSmsPattern { get; set; }

	public SmsPatternModel OverDueDateSmsPattern { get; set; }

	public SmsPatternModel BeforeDueDateSmsPattern { get; set; }

	public SmsPatternModel LoginSmsPattern { get; set; }

	public SmsPatternModel RegisterSmsPattern { get; set; }

	public SmsPatternModel RegisterOtpSmsPattern { get; set; }

	public SmsPatternModel WalletChargeSmsPattern { get; set; }

	public SmsPatternModel AgentFactorPaymentLinkSmsPattern { get; set; }

	public SmsPatternModel NewOrderSmsPattern { get; set; }

	public SmsPatternModel WalletWithdrawNotificationSmsPattern { get; set; }

	public SmsPatternModel WalletWithdrawOtpSmsPattern { get; set; }

	public SmsPatternModel DigitalSignLinkSmsPattern { get; set; }

	public SmsPatternModel DigitalSignCompleteSmsPattern { get; set; }

	public SmsPatternModel DigitalSignOtpSmsPattern { get; set; }

	public SmsPatternModel ThirdPersonDigitalSignOtpSmsPattern { get; set; }

	public SmsPatternModel ThirdPersonDigitalSignLinkSmsPattern { get; set; }

	public SmsPatternModel ThirdPersonDigitalSignCompleteSmsPattern { get; set; }

	public bool IsRememberEmailActive { get; set; }

	public string DueDatedRememberEmailTime { get; set; }

	public bool IsDailyReportEmailActive { get; set; }

	public string DailyReportEmailTime { get; set; }

	public bool IsDueDateEmailActive { get; set; }

	[Range(0.0, double.MaxValue, ErrorMessage = "تعداد روز ایمیل یادآوری پیش از موعد نمی تواند کمتر از صفر باشد")]
	public int BeforeDueDaysCount_Email { get; set; }

	[Range(0.0, double.MaxValue, ErrorMessage = "تعداد روز ایمیل یادآوری قسط معوق نمی تواند کمتر از صفر باشد")]
	public int OverDueDaysCount_Email { get; set; }

	public bool OverDueDayRemeberOnce_Email { get; set; }

	public string EmailConfigJson { get; set; }

	public SmsPatternModel DueDateRememberEmailPattern { get; set; }

	public SmsPatternModel ThanksPaymentEmailPattern { get; set; }

	public SmsPatternModel OverDueDateEmailPattern { get; set; }

	public SmsPatternModel LoginEmailPattern { get; set; }

	public SmsPatternModel RegisterOtpEmailPattern { get; set; }

	public SmsPatternModel RegisterEmailPattern { get; set; }

	public SmsPatternModel BeforeDueDateEmailPattern { get; set; }

	public SmsPatternModel WalletChargeEmailPattern { get; set; }

	public SmsPatternModel NewOrderEmailPattern { get; set; }

	public IceConfig IceConfig { get; set; } = new IceConfig();

	public IranValidityConfig IranValidityConfig { get; set; } = new IranValidityConfig();

	public string BankAccountsJson { get; set; }

	public ThemeSettingsModel ThemeSettings { get; set; }

	public string BusinessTitle { get; set; }

	public string Address { get; set; }

	public string ShopLink { get; set; }

	public string ShopTitle { get; set; }

	public string Phones { get; set; }

	public string Email { get; set; }

	public string EnamadLink { get; set; }

	public string EtehadieLink { get; set; }

	public string SamandehiLink { get; set; }

	public string AnjomanSenfiLink { get; set; }

	public string KasboKarhayeInternetiTehranLink { get; set; }

	public string ApplicationLink { get; set; }

	public string WhatsappLink { get; set; }

	public string TelegramLink { get; set; }

	public string InstagramLink { get; set; }

	public string RubikaLink { get; set; }

	public string AparatLink { get; set; }

	public string[] DepositChargeDueDateStatuses { get; set; } = new string[0];

	public string[] SponsoreDueDateStatuses { get; set; } = new string[0];

	public string BankGatewayRouterBaseUrl { get; set; }

	public string AgentOfficePhone { get; set; } = "";

	public bool OtpRequiredInManualMemberRegistration { get; set; }

	public bool ShowSubmitFirstLoanRequestNotifInMemberPanel { get; set; }

	[Range(0, 100, ErrorMessage = "درصد در نظر گرفته شدن قسط به عنوان پرداخت شده، نمی تواند کمتر از صفر و بیشتر از 100 باشد")]
	public byte PercentOfConsideringAsPaid { get; set; }

	public bool PreventCreateLoanNonCurrentDate { get; set; }

	public int CalculatorType { get; set; }

	public bool IsMemberRegistrationDisabled { get; set; }

	public bool MemberCanChangeProfile { get; set; }

	public AppSettingsModel()
	{
	}

	public AppSettingsModel(AppSettings appSettings)
	{
		if (appSettings != null)
		{
			Id = appSettings.Id;
			SiteTheme = appSettings.SiteTheme;
			DefaultSmsPanel = appSettings.DefaultSmsPanel;
			DefaultEmailPanel = appSettings.DefaultEmailPanel;
			RoundCountNumber = appSettings.RoundCountNumber;
			AppendToWhichInstallment = appSettings.AppendToWhichInstallment;
			IsOnlinePaymentActive = appSettings.IsOnlinePaymentActive;
			OnlinePaymentGateway = appSettings.OnlinePaymentGateway;
			AgentFactorOnlinePaymentGateway = appSettings.AgentFactorOnlinePaymentGateway;
			MaxAgentFactorCommission = appSettings.MaxAgentFactorCommission;
			AgentFactorCommission = appSettings.AgentFactorCommission;
			SepTerminalId = appSettings.SepTerminalId;
			Shaba = appSettings.Shaba;
			BankInfo = appSettings.BankInfo;
			SmsCredit = appSettings.SmsCredit;
			ZarinpalMerchantCode = appSettings.ZarinpalMerchantCode;
			NextPayApiKey = appSettings.NextPayApiKey;
			IPGParsianLoginAccount = appSettings.IPGParsianLoginAccount;
			DueDatedRememberSmsTime = appSettings.DueDatedRememberSmsTime.ToString();
			IsRememberSmsActive = appSettings.IsRememberSmsActive;
			IsDueDateSmsActive = appSettings.IsDueDateSmsActive;
			BeforeDueDaysCount = appSettings.BeforeDueDaysCount;
			OverDueDayRemeberOnce = appSettings.OverDueDayRemeberOnce;
			OverDueDayGapPeriod = appSettings.OverDueDayGapPeriod;
			OverDueDaysCount = appSettings.OverDueDaysCount;
			BankAccountsJson = appSettings.BankAccountsJson;
			Phones = appSettings.Phones;
			Address = appSettings.Address;
			Email = appSettings.Email;
			ShopLink = appSettings.ShopLink;
			ShopTitle = appSettings.ShopTitle;
			EnamadLink = appSettings.EnamadLink;
			SamandehiLink = appSettings.SamandehiLink;
			EtehadieLink = appSettings.EtehadieLink;
			AnjomanSenfiLink = appSettings.AnjomanSenfiLink;
			KasboKarhayeInternetiTehranLink = appSettings.KasboKarhayeInternetiTehranLink;
			ApplicationLink = appSettings.ApplicationLink;
			WhatsappLink = appSettings.WhatsappLink;
			TelegramLink = appSettings.TelegramLink;
			InstagramLink = appSettings.InstagramLink;
			RubikaLink = appSettings.RubikaLink;
			AparatLink = appSettings.AparatLink;
			BusinessTitle = appSettings.BusinessTitle;
			BankGatewayRouterBaseUrl = appSettings.BankGatewayRouterBaseUrl;
			ApiKey = appSettings.ApiKey;
			ApiPassword = appSettings.ApiPassword;
			SadadTerminalId = appSettings.SadadTerminalId;
			SepehrTerminalId = appSettings.SepehrTerminalId;
			SadadMerchantId = appSettings.SadadMerchantId;
			SadadMerchantKey = appSettings.SadadMerchantKey;
			AgentOfficePhone = appSettings.AgentOfficePhone;
			IsRememberEmailActive = appSettings.IsRememberEmailActive;
			DueDatedRememberEmailTime = appSettings.DueDatedRememberEmailTime.ToString();
			IsDailyReportEmailActive = appSettings.IsDailyReportEmailActive;
			DailyReportEmailTime = appSettings.DailyReportEmailTime.ToString();
			IsDueDateEmailActive = appSettings.IsDueDateEmailActive;
			BeforeDueDaysCount_Email = appSettings.BeforeDueDaysCount_Email;
			OverDueDaysCount_Email = appSettings.OverDueDaysCount_Email;
			OverDueDayRemeberOnce_Email = appSettings.OverDueDayRemeberOnce_Email;
			OtpRequiredInManualMemberRegistration = appSettings.OtpRequiredInManualMemberRegistration;
			ShowSubmitFirstLoanRequestNotifInMemberPanel = appSettings.ShowSubmitFirstLoanRequestNotifInMemberPanel;
			PercentOfConsideringAsPaid = appSettings.PercentOfConsideringAsPaid;
			PreventCreateLoanNonCurrentDate = appSettings.PreventCreateLoanNonCurrentDate;
			CalculatorType = appSettings.CalculatorType;
			VerifyMobileByNationalCodeInRegister = appSettings.VerifyMobileByNationalCodeInRegister;
			InquiryBankInRegister = appSettings.InquiryBankInRegister;
			InquiryPersonInfoInRegister = appSettings.InquiryPersonInfoInRegister;
			InquiryPostalAddressInRegister = appSettings.InquiryPostalAddressInRegister;
			DefaultInquiryPanel = appSettings.DefaultInquiryPanel;
			MemberCanChangeProfile = appSettings.MemberCanChangeProfile;
			IsMemberRegistrationDisabled = appSettings.IsMemberRegistrationDisabled;
			if (appSettings.WalletWithdrawOtpSmsPattern != null)
			{
				WalletWithdrawOtpSmsPattern = new SmsPatternModel(appSettings.WalletWithdrawOtpSmsPattern);
			}
			if (appSettings.WalletWithdrawNotificationSmsPattern != null)
			{
				WalletWithdrawNotificationSmsPattern = new SmsPatternModel(appSettings.WalletWithdrawNotificationSmsPattern);
			}
			if (appSettings.NewUserLoanSmsPattern != null)
			{
				NewUserLoanSmsPattern = new SmsPatternModel(appSettings.NewUserLoanSmsPattern);
			}
			if (appSettings.DueDateRememberSmsPattern != null)
			{
				DueDateRememberSmsPattern = new SmsPatternModel(appSettings.DueDateRememberSmsPattern);
			}
			if (appSettings.ThanksPaymentSmsPattern != null)
			{
				ThanksPaymentSmsPattern = new SmsPatternModel(appSettings.ThanksPaymentSmsPattern);
			}
			if (appSettings.OverDueDateSmsPattern != null)
			{
				OverDueDateSmsPattern = new SmsPatternModel(appSettings.OverDueDateSmsPattern);
			}
			if (appSettings.BeforeDueDateSmsPattern != null)
			{
				BeforeDueDateSmsPattern = new SmsPatternModel(appSettings.BeforeDueDateSmsPattern);
			}
			if (appSettings.LoginSmsPattern != null)
			{
				LoginSmsPattern = new SmsPatternModel(appSettings.LoginSmsPattern);
			}
			if (appSettings.RegisterSmsPattern != null)
			{
				RegisterSmsPattern = new SmsPatternModel(appSettings.RegisterSmsPattern);
			}
			if (appSettings.RegisterOtpSmsPattern != null)
			{
				RegisterOtpSmsPattern = new SmsPatternModel(appSettings.RegisterOtpSmsPattern);
			}
			if (appSettings.WalletChargeSmsPattern != null)
			{
				WalletChargeSmsPattern = new SmsPatternModel(appSettings.WalletChargeSmsPattern);
			}
			if (appSettings.AgentFactorPaymentLinkSmsPattern != null)
			{
				AgentFactorPaymentLinkSmsPattern = new SmsPatternModel(appSettings.AgentFactorPaymentLinkSmsPattern);
			}
			if (appSettings.DueDateRememberEmailPattern != null)
			{
				DueDateRememberEmailPattern = new SmsPatternModel(appSettings.DueDateRememberEmailPattern);
			}
			if (appSettings.ThanksPaymentEmailPattern != null)
			{
				ThanksPaymentEmailPattern = new SmsPatternModel(appSettings.ThanksPaymentEmailPattern);
			}
			if (appSettings.OverDueDateEmailPattern != null)
			{
				OverDueDateEmailPattern = new SmsPatternModel(appSettings.OverDueDateEmailPattern);
			}
			if (appSettings.LoginEmailPattern != null)
			{
				LoginEmailPattern = new SmsPatternModel(appSettings.LoginEmailPattern);
			}
			if (appSettings.RegisterOtpEmailPattern != null)
			{
				RegisterOtpEmailPattern = new SmsPatternModel(appSettings.RegisterOtpEmailPattern);
			}
			if (appSettings.RegisterEmailPattern != null)
			{
				RegisterEmailPattern = new SmsPatternModel(appSettings.RegisterEmailPattern);
			}
			if (appSettings.BeforeDueDateEmailPattern != null)
			{
				BeforeDueDateEmailPattern = new SmsPatternModel(appSettings.BeforeDueDateEmailPattern);
			}
			if (appSettings.WalletChargeEmailPattern != null)
			{
				WalletChargeEmailPattern = new SmsPatternModel(appSettings.WalletChargeEmailPattern);
			}
			if (appSettings.NewOrderSmsPattern != null)
			{
				NewOrderSmsPattern = new SmsPatternModel(appSettings.NewOrderSmsPattern);
			}
			if (appSettings.NewOrderEmailPattern != null)
			{
				NewOrderEmailPattern = new SmsPatternModel(appSettings.NewOrderEmailPattern);
			}
			if (appSettings.DigitalSignLinkSmsPattern != null)
			{
				DigitalSignLinkSmsPattern = new SmsPatternModel(appSettings.DigitalSignLinkSmsPattern);
			}
			if (appSettings.DigitalSignCompleteSmsPattern != null)
			{
				DigitalSignCompleteSmsPattern = new SmsPatternModel(appSettings.DigitalSignCompleteSmsPattern);
			}
			if (appSettings.DigitalSignOtpSmsPattern != null)
			{
				DigitalSignOtpSmsPattern = new SmsPatternModel(appSettings.DigitalSignOtpSmsPattern);
			}
			if (appSettings.ThirdPersonDigitalSignLinkSmsPattern != null)
			{
				ThirdPersonDigitalSignLinkSmsPattern = new SmsPatternModel(appSettings.ThirdPersonDigitalSignLinkSmsPattern);
			}
			if (appSettings.ThirdPersonDigitalSignCompleteSmsPattern != null)
			{
				ThirdPersonDigitalSignCompleteSmsPattern = new SmsPatternModel(appSettings.ThirdPersonDigitalSignCompleteSmsPattern);
			}
			if (appSettings.ThirdPersonDigitalSignOtpSmsPattern != null)
			{
				ThirdPersonDigitalSignOtpSmsPattern = new SmsPatternModel(appSettings.ThirdPersonDigitalSignOtpSmsPattern);
			}
			if (!string.IsNullOrEmpty(appSettings.EmailConfigJson))
			{
				EmailConfig = JsonSerializer.Deserialize<EmailConfig>(appSettings.EmailConfigJson);
			}
			if (!string.IsNullOrEmpty(appSettings.MelliPayamakConfigJson))
			{
				MelliPayamakConfig = JsonSerializer.Deserialize<MelliPayamakConfig>(appSettings.MelliPayamakConfigJson);
			}
			if (!string.IsNullOrEmpty(appSettings.KaveNegarConfigJson))
			{
				KaveNegarConfig = JsonSerializer.Deserialize<KaveNegarConfig>(appSettings.KaveNegarConfigJson);
			}
			if (!string.IsNullOrEmpty(appSettings.BaleConfigJson))
			{
				BaleConfig = JsonSerializer.Deserialize<BaleConfig>(appSettings.BaleConfigJson);
			}
			if (!string.IsNullOrEmpty(appSettings.IPPanelConfigJson))
			{
				IPPanelConfig = JsonSerializer.Deserialize<IPPanelConfig>(appSettings.IPPanelConfigJson);
			}
			if (!string.IsNullOrEmpty(appSettings.WebOneConfigJson))
			{
				WebOneConfig = JsonSerializer.Deserialize<WebOneConfig>(appSettings.WebOneConfigJson);
			}
			if (!string.IsNullOrEmpty(appSettings.IceConfigJson))
			{
				IceConfig = JsonSerializer.Deserialize<IceConfig>(appSettings.IceConfigJson);
			}
			if (!string.IsNullOrEmpty(appSettings.FinnoTechConfigJson))
			{
				FinnoTechConfig = JsonSerializer.Deserialize<FinnoTechConfig>(appSettings.FinnoTechConfigJson);
			}
			if (!string.IsNullOrEmpty(appSettings.EmzameConfigJson))
			{
				EmzameConfig = JsonSerializer.Deserialize<EmzameConfig>(appSettings.EmzameConfigJson);
			}
			if (!string.IsNullOrEmpty(appSettings.ApiIRConfigJson))
			{
				ApiIRConfig = JsonSerializer.Deserialize<ApiIRConfig>(appSettings.ApiIRConfigJson);
			}
			if (!string.IsNullOrEmpty(appSettings.RefahBetaConfigJson))
			{
				RefahBetaConfig = JsonSerializer.Deserialize<RefahBetaConfig>(appSettings.RefahBetaConfigJson);
			}
			if (!string.IsNullOrEmpty(appSettings.IranValidityConfigJson))
			{
				IranValidityConfig = JsonSerializer.Deserialize<IranValidityConfig>(appSettings.IranValidityConfigJson);
			}
			if (!string.IsNullOrEmpty(appSettings.ThemeSettingsJson))
			{
				ThemeSettings = JsonSerializer.Deserialize<ThemeSettingsModel>(appSettings.ThemeSettingsJson);
			}
			if (!string.IsNullOrEmpty(appSettings.DepositChargeDueDateStatuses))
			{
				DepositChargeDueDateStatuses = appSettings.DepositChargeDueDateStatuses.Split(',');
			}
			if (!string.IsNullOrEmpty(appSettings.SponsoreDueDateStatuses))
			{
				SponsoreDueDateStatuses = appSettings.SponsoreDueDateStatuses.Split(',');
			}
		}
	}
}
