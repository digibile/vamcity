namespace LoanSystemAvizhe.Services.Inquiry.Models.ApiIR;

public class ApiIR_ActiveLoansResult
{
	public int Count { get; set; }

	public string Name { get; set; }

	public long TotalAmount { get; set; }

	public long DebtTotalAmount { get; set; }

	public long PastExpiredTotalAmount { get; set; }

	public long DeferredTotalAmount { get; set; }

	public long SuspiciousTotalAmount { get; set; }

	public long Dishonored { get; set; }
}
