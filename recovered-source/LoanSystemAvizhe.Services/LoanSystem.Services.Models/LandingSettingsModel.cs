using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Services.Models;

public class LandingSettingsModel
{
	public SiteThemes SiteTheme { get; set; }

	public ThemeSettingsModel ThemeSettings { get; set; }

	public OnlinePaymentGateways AgentFactorOnlinePaymentGateway { get; set; }

	public string Phones { get; set; } = "";

	public string Email { get; set; } = "";

	public string Address { get; set; } = "";

	public string ShopLink { get; set; } = "";

	public string ShopTitle { get; set; } = "";

	public string BusinessTitle { get; set; } = "";

	public string EnamadLink { get; set; } = "";

	public string EtehadieLink { get; set; } = "";

	public string SamandehiLink { get; set; } = "";

	public string AnjomanSenfiLink { get; set; } = "";

	public string KasboKarhayeInternetiTehranLink { get; set; } = "";

	public string ApplicationLink { get; set; } = "";

	public string WhatsappLink { get; set; } = "";

	public string TelegramLink { get; set; } = "";

	public string InstagramLink { get; set; } = "";

	public string RubikaLink { get; set; } = "";

	public string AparatLink { get; set; } = "";

	public string AgentOfficePhone { get; set; } = "";

	public int CalculatorType { get; set; }

	public bool InquiryPersonInfoInRegister { get; set; }

	public bool InquiryPostalAddressInRegister { get; set; }

	public bool InquiryBankInRegister { get; set; }

	public bool HasRegisterOtpSms { get; set; }
}
