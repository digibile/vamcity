using LoanSystemAvizhe.Services.Sms.Models.MelliPayamak;

namespace LoanSystemAvizhe.Services.Sms.Models.WebOne;

public class SendWebOne : BaseSendPayamak
{
    public string ToNumber { get; set; }

    public string From { get; set; } = "";

    public string Content { get; set; } = "";
}
