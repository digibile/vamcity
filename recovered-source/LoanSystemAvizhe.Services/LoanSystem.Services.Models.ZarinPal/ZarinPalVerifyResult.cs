using System.Collections.Generic;

namespace LoanSystem.Services.Models.ZarinPal;

public class ZarinPalVerifyResult
{
    public ZarinPalVerifyResultData data { get; set; }

    public List<dynamic> errors { get; set; }
}
