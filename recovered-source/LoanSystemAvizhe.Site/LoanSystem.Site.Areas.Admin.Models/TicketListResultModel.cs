using System.Collections.Generic;

namespace LoanSystem.Site.Areas.Admin.Models;

public class TicketListResultModel
{
	public List<dynamic> Tickets { get; set; }

	public int CountAll { get; set; }

	public dynamic Counts { get; set; }
}
