using System.Collections.Generic;
using LoanSystem.Services.Models;

namespace LoanSystem.Site.Areas.Admin.Models;

public class ThirdPartyTransferListResultModel
{
	public List<ThirdPartyTransferModel> Items { get; set; } = new List<ThirdPartyTransferModel>();

	public int CountAll { get; set; }
}
