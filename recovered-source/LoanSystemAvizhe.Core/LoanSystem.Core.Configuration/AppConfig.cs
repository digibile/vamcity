namespace LoanSystem.Core.Configuration;

public class AppConfig
{
	public string JwtSecretKey { get; set; }

	public bool BypassRecaptcha { get; set; }

	public string Currency { get; set; } = "ریال";

	public string FrontUrl { get; set; }

	public string BaseUrl { get; set; }

	public int BaseSmsPrice { get; set; }

	public string DefaultSmsPanel { get; set; } = "MelliPayamak";

	public string DefaultInquiryPanel { get; set; } = "ApiIR";

	public bool IsCloud { get; set; }

	public string AdminPhone { get; set; }

	public string AdminEmail { get; set; }

	public int PermanentPrice { get; set; }

	public int CloudPrice { get; set; }

	public int CustomerCode { get; set; }

	public string TitlePart1 { get; set; }

	public string TitlePart2 { get; set; }

	public string FullTitle { get; set; }

	public string LongTitle { get; set; }

	public bool RouteToPlansAsDefault { get; set; }

	public bool ShowSponsoreCheck { get; set; }

	public ActiveParts ActiveParts { get; set; }
}
