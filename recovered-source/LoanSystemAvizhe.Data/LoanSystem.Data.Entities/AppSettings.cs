using System;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Data.Entities;

public class AppSettings : BaseEntity
{
    public SiteThemes SiteTheme { get; set; }

    public byte RoundCountNumber { get; set; }

    public AppendToWhichInstallment AppendToWhichInstallment { get; set; }

    public bool IsOnlinePaymentActive { get; set; }

    public OnlinePaymentGateways OnlinePaymentGateway { get; set; }

    public OnlinePaymentGateways AgentFactorOnlinePaymentGateway { get; set; }

    public decimal AgentFactorCommission { get; set; }

    public decimal MaxAgentFactorCommission { get; set; }

    public string ZarinpalMerchantCode { get; set; }

    public string ApiKey { get; set; }

    public bool VerifyMobileByNationalCodeInRegister { get; set; }

    public bool InquiryPersonInfoInRegister { get; set; }

    public bool InquiryPostalAddressInRegister { get; set; }

    public bool InquiryBankInRegister { get; set; }

    public string ApiPassword { get; set; }

    public string Shaba { get; set; }

    public string NextPayApiKey { get; set; }

    public string IPGParsianLoginAccount { get; set; }

    public string SepTerminalId { get; set; }

    public long SepehrTerminalId { get; set; }

    public string SadadMerchantId { get; set; }

    public string SadadMerchantKey { get; set; }

    public string SadadTerminalId { get; set; }

    public bool IsRememberSmsActive { get; set; }

    public TimeSpan? DueDatedRememberSmsTime { get; set; }

    public bool IsDueDateSmsActive { get; set; }

    public int BeforeDueDaysCount { get; set; }

    public int OverDueDaysCount { get; set; }

    public bool OverDueDayRemeberOnce { get; set; }

    public int OverDueDayGapPeriod { get; set; }

    public bool IsRememberEmailActive { get; set; }

    public TimeSpan? DueDatedRememberEmailTime { get; set; }

    public bool IsDailyReportEmailActive { get; set; }

    public TimeSpan? DailyReportEmailTime { get; set; }

    public bool IsDueDateEmailActive { get; set; }

    public int BeforeDueDaysCount_Email { get; set; }

    public int OverDueDaysCount_Email { get; set; }

    public bool OverDueDayRemeberOnce_Email { get; set; }

    public string DashboardSettingsJson { get; set; }

    public EmailPanel DefaultEmailPanel { get; set; }

    public string EmailConfigJson { get; set; }

    public SmsPanel DefaultSmsPanel { get; set; }

    public InquiryPanel DefaultInquiryPanel { get; set; }

    public string MelliPayamakConfigJson { get; set; }

    public string KaveNegarConfigJson { get; set; }

    public string BaleConfigJson { get; set; }

    public string IPPanelConfigJson { get; set; }

    public string WebOneConfigJson { get; set; }

    public string BankAccountsJson { get; set; }

    public string ThemeSettingsJson { get; set; }

    public string FinnoTechConfigJson { get; set; }

    public string ApiIRConfigJson { get; set; }

    public string RefahBetaConfigJson { get; set; }

    public string EmzameConfigJson { get; set; }

    public int SmsCredit { get; set; }

    public int InquiryCredit { get; set; }

    public string BankInfo { get; set; }

    public Guid CustomerId { get; set; }

    public virtual Customer Customer { get; set; }

    public Guid? NewUserLoanSmsPatternId { get; set; }

    public virtual SmsPattern NewUserLoanSmsPattern { get; set; }

    public Guid? DueDateRememberSmsPatternId { get; set; }

    public virtual SmsPattern DueDateRememberSmsPattern { get; set; }

    public Guid? ThanksPaymentSmsPatternId { get; set; }

    public virtual SmsPattern ThanksPaymentSmsPattern { get; set; }

    public Guid? OverDueDateSmsPatternId { get; set; }

    public virtual SmsPattern OverDueDateSmsPattern { get; set; }

    public Guid? LoginSmsPatternId { get; set; }

    public virtual SmsPattern LoginSmsPattern { get; set; }

    public Guid? NewOrderSmsPatternId { get; set; }

    public virtual SmsPattern NewOrderSmsPattern { get; set; }

    public Guid? NewOrderEmailPatternId { get; set; }

    public virtual SmsPattern NewOrderEmailPattern { get; set; }

    public Guid? RegisterSmsPatternId { get; set; }

    public virtual SmsPattern RegisterSmsPattern { get; set; }

    public Guid? RegisterOtpSmsPatternId { get; set; }

    public virtual SmsPattern RegisterOtpSmsPattern { get; set; }

    public Guid? DigitalSignOtpSmsPatternId { get; set; }

    public virtual SmsPattern DigitalSignOtpSmsPattern { get; set; }

    public Guid? ThirdPersonDigitalSignOtpSmsPatternId { get; set; }

    public virtual SmsPattern ThirdPersonDigitalSignOtpSmsPattern { get; set; }

    public Guid? WalletWithdrawOtpSmsPatternId { get; set; }

    public virtual SmsPattern WalletWithdrawOtpSmsPattern { get; set; }

    public Guid? WalletWithdrawNotificationSmsPatternId { get; set; }

    public virtual SmsPattern WalletWithdrawNotificationSmsPattern { get; set; }

    public Guid? BeforeDueDateSmsPatternId { get; set; }

    public virtual SmsPattern BeforeDueDateSmsPattern { get; set; }

    public Guid? WalletChargeSmsPatternId { get; set; }

    public virtual SmsPattern WalletChargeSmsPattern { get; set; }

    public Guid? DigitalSignLinkSmsPatternId { get; set; }

    public virtual SmsPattern DigitalSignLinkSmsPattern { get; set; }

    public Guid? DigitalSignCompleteSmsPatternId { get; set; }

    public virtual SmsPattern DigitalSignCompleteSmsPattern { get; set; }

    public Guid? ThirdPersonDigitalSignLinkSmsPatternId { get; set; }

    public virtual SmsPattern ThirdPersonDigitalSignLinkSmsPattern { get; set; }

    public Guid? ThirdPersonDigitalSignCompleteSmsPatternId { get; set; }

    public virtual SmsPattern ThirdPersonDigitalSignCompleteSmsPattern { get; set; }

    public Guid? AgentFactorPaymentLinkSmsPatternId { get; set; }

    public virtual SmsPattern AgentFactorPaymentLinkSmsPattern { get; set; }

    public Guid? DueDateRememberEmailPatternId { get; set; }

    public virtual SmsPattern DueDateRememberEmailPattern { get; set; }

    public Guid? ThanksPaymentEmailPatternId { get; set; }

    public virtual SmsPattern ThanksPaymentEmailPattern { get; set; }

    public Guid? OverDueDateEmailPatternId { get; set; }

    public virtual SmsPattern OverDueDateEmailPattern { get; set; }

    public Guid? LoginEmailPatternId { get; set; }

    public virtual SmsPattern LoginEmailPattern { get; set; }

    public Guid? RegisterEmailPatternId { get; set; }

    public virtual SmsPattern RegisterOtpEmailPattern { get; set; }

    public Guid? RegisterOtpEmailPatternId { get; set; }

    public virtual SmsPattern RegisterEmailPattern { get; set; }

    public Guid? BeforeDueDateEmailPatternId { get; set; }

    public virtual SmsPattern BeforeDueDateEmailPattern { get; set; }

    public Guid? WalletChargeEmailPatternId { get; set; }

    public virtual SmsPattern WalletChargeEmailPattern { get; set; }

    public Guid? WalletWithdrawOtpEmailPatternId { get; set; }

    public virtual SmsPattern WalletWithdrawOtpEmailPattern { get; set; }

    public string IceConfigJson { get; set; }

    public string IranValidityConfigJson { get; set; }

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

    public string DepositChargeDueDateStatuses { get; set; }

    public string SponsoreDueDateStatuses { get; set; }

    public string BankGatewayRouterBaseUrl { get; set; }

    public string AgentOfficePhone { get; set; }

    public bool OtpRequiredInManualMemberRegistration { get; set; }

    public bool ShowSubmitFirstLoanRequestNotifInMemberPanel { get; set; } = true;

    public byte PercentOfConsideringAsPaid { get; set; }

    public bool PreventCreateLoanNonCurrentDate { get; set; }

    public int CalculatorType { get; set; }

    public bool IsMemberRegistrationDisabled { get; set; }

    public bool MemberCanChangeProfile { get; set; }
}
