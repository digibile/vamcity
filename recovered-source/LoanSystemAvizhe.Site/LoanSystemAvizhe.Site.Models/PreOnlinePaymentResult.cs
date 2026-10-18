using LoanSystem.Services.Models;

namespace LoanSystemAvizhe.Site.Models;

public class PreOnlinePaymentResult
{
    public LoanPaymentModel UserLoanPayment { get; set; }

    public string Customer { get; set; }

    public string Member { get; set; }
}
