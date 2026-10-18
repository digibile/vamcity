namespace LoanSystemAvizhe.Services.Sms.Models.Bale;

public class OptMessageModel
{
    public string otp { get; private set; }

    public OptMessageModel(string otp)
    {
        this.otp = otp;
    }
}
