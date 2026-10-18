using System.Collections.Generic;

namespace LoanSystem.Services.Models.ZarinPal;

public class ZarinPalRequestWage
{
    public string merchant_id { get; set; }

    public string amount { get; set; }

    public string callback_url { get; set; }

    public string description { get; set; }

    public ZarinPalRequestMetadata metadata { get; set; }

    public List<ZarinPalWage> wages { get; set; }
}
