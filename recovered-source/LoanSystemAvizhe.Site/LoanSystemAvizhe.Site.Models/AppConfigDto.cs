using LoanSystem.Core.Configuration;

namespace LoanSystemAvizhe.Site.Models;

public class AppConfigDto
{
	public int CustomerCode { get; set; }

	public bool IsCloud { get; set; }

	public int PermanentPrice { get; set; }

	public int CloudPrice { get; set; }

	public string BaseUrl { get; set; }

	public string TitlePart1 { get; set; }

	public string TitlePart2 { get; set; }

	public string FullTitle { get; set; }

	public string LongTitle { get; set; }

	public bool RouteToPlansAsDefault { get; set; }

	public bool ShowSponsoreCheck { get; set; }

	public ActiveParts ActiveParts { get; set; }
}
