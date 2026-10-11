namespace LoanSystem.Services.Models;

public class AffiliateCommissionPaymentModel : BaseModel
{
	public decimal Amount { get; set; }

	public int PaidToUser { get; set; }
}
