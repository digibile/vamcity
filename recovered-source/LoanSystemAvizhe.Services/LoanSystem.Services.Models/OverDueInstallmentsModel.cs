using System.Collections.Generic;

namespace LoanSystem.Services.Models;

public class OverDueInstallmentsModel
{
    public decimal Sum { get; set; }

    public decimal SumByDelays { get; set; }

    public int Count { get; set; }

    public List<LoanPaymentModel> LoanPayments { get; set; }
}
