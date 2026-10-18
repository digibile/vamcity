namespace LoanSystemAvizhe.Services.Sms.Models.Bale;

public class BaleBaseSend
{
    public string AccessToken { get; set; }

    public string RequestId { get; set; }

    public long BotId { get; set; }

    public string PhoneNumber { get; set; }
}
