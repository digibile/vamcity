namespace LoanSystemAvizhe.Services.Sms.Models.MelliPayamak;

public class MelliPayamakConfig
{
    public string Username { get; set; }

    public string Password { get; set; }

    public string From { get; set; }

    public int RegBaseNumber { get; set; }

    public int VerifyBaseNumber { get; set; }

    public int OnlineMoneyTransferOTP { get; set; }

    public bool IsFlash { get; set; }
}
