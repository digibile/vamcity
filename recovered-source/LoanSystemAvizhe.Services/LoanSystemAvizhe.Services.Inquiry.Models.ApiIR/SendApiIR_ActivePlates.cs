using LoanSystemAvizhe.Services.Sms.Models.MelliPayamak;

namespace LoanSystemAvizhe.Services.Inquiry.Models.ApiIR;

public class SendApiIR_ActivePlates : BaseSendInquiry
{
    public string NationalCode { get; set; } = "";

    public string Mobile { get; set; } = "";
}
