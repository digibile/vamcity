using System.Text.Json;

namespace LoanSystem.Services.Models.Sep;

public class SadadPurchase
{
    public string MerchantId { get; set; }

    public string TerminalId { get; set; }

    public string Token { get; set; }

    public int OrderId { get; set; }

    public int ResCode { get; set; }

    public string Description { get; set; }

    public string RetrivalRefNo { get; set; }

    public string SystemTraceNo { get; set; }

    public string CardHolderFullName { get; set; }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}
