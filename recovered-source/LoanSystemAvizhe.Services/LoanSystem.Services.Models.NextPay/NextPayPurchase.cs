using System.Text.Json;

namespace LoanSystem.Services.Models.NextPay;

public class NextPayPurchase
{
    public string ApiKey { get; set; }

    public string TransId { get; set; }

    public string OrderId { get; set; }

    public string Shaparak_Ref_Id { get; set; }

    public int AuthorityRequestStatus { get; set; }

    public int VerifyCode { get; set; }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}
