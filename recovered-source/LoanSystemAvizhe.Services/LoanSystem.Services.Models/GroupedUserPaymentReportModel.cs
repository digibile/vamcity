using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Services.Models;

public class GroupedUserPaymentReportModel
{
	public PaymentMethods PaymentMethod { get; set; }

	public int Count { get; set; }

	public decimal Sum { get; set; }
}
