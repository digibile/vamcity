using System.Text.Json;

namespace LoanSystem.Services.Models.ZarinPal;

public class ZarinPalPurchase
{
    public string Authority { get; set; }

    public string MerchantId { get; set; }

    public string RefId { get; set; }

    public string Status { get; set; }

    public int AuthorityRequestStatus { get; set; }

    public int VerifyStatus { get; set; }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}
