using System.Collections.Generic;

namespace LoanSystem.Services.Models;

public class InstallmentImportResultModel : BaseModel
{
    public int UserLoanNew { get; set; }

    public int UserLoanUpdate { get; set; }

    public int PaymentNew { get; set; }

    public int PaymentUpdate { get; set; }

    public int Failed { get; set; }

    public List<string> Errors { get; set; } = new List<string>();
}
