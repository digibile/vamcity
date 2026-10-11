using System;

namespace LoanSystemAvizhe.Site.Areas.AppApi.Models;

public class AppApiUserLoanBulkWithPaymentsPerMemberResult
{
	public long MemberId { get; set; }

	public string ErrorMessage { get; set; }

	public Guid UserLoanId { get; set; }

	public int UserLoanCode { get; set; }
}
