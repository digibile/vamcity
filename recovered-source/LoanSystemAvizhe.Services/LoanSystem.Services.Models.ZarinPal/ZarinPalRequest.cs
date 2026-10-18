namespace LoanSystem.Services.Models.ZarinPal;

public class ZarinPalRequest
{
    public string merchant_id { get; set; }

    public string amount { get; set; }

    public string callback_url { get; set; }

    public string description { get; set; }

    public ZarinPalRequestMetadata metadata { get; set; }
}
