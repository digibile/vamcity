namespace LoanSystem.Services.Models.IPGParsian;

public class IPGParsianVerifyResult
{
    public string CardNumberMasked { get; set; }

    public long RRN { get; set; }

    public long Token { get; set; }

    public int Status { get; set; }
}
