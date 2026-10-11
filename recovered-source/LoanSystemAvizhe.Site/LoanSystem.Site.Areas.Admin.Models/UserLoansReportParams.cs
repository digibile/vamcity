using System;

namespace LoanSystem.Site.Areas.Admin.Models;

public class UserLoansReportParams
{
	public DateTime? Start { get; set; }

	public DateTime? End { get; set; }
}
