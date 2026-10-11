using System.Collections.Generic;

namespace LoanSystem.Services.Models;

public class CustomerMarketerDashboardModel
{
	public List<dynamic> ReffererChart { get; set; }

	public int reffererCount { get; set; }

	public int registerCount { get; set; }

	public int loanRequstCounts { get; set; }

	public int userLoanCounts { get; set; }

	public decimal totalCommission { get; set; }

	public decimal totalReceives { get; set; }
}
