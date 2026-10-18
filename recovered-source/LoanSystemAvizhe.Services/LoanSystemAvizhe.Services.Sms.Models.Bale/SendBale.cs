using LoanSystemAvizhe.Services.Sms.Models.MelliPayamak;

namespace LoanSystemAvizhe.Services.Sms.Models.Bale;

internal class SendBale : BaseSendPayamak
{
    public string RequestId { get; set; }

    public int Otp { get; set; }
}
