namespace LoanSystem.Services.Models.Sep;

public class SepRequestResult
{
    public int Status { get; set; }

    public string Token { get; set; }

    public string ErrorCode { get; set; }

    public string ErrorDesc { get; set; }
}
