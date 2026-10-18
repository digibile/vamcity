using System.Text.Json;

namespace LoanSystem.Services.Models.IPGParsian;

public class IPGParsianPurchase
{
    public string LoginAccount { get; set; }

    public int TokenStatus { get; set; }

    public int TokenErrorCode { get; set; }

    public string TokenErrorDesc { get; set; }

    public long Token { get; set; }

    public int OrderId { get; set; }

    public long RRN { get; set; }

    public string TerminalNo { get; set; }

    public string HashCardNumber { get; set; }

    public int Status { get; set; }

    public string Amount { get; set; }

    public string VerifyCardNumberMasked { get; set; }

    public long VerifyRRN { get; set; }

    public long VerifyToken { get; set; }

    public int VerifyStatus { get; set; }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}
