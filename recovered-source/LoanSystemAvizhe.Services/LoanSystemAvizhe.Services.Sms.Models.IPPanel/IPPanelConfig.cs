namespace LoanSystemAvizhe.Services.Sms.Models.IPPanel;

public class IPPanelConfig
{
    public string ApiKey { get; set; }

    public string Sender { get; set; }

    public string RegTemplate { get; set; }

    public string VerifyTemplate { get; set; }

    public string OnlineMoneyTransferOTP { get; set; }
}
