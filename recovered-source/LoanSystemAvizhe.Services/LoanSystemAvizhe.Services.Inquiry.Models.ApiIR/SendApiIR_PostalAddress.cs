using LoanSystemAvizhe.Services.Sms.Models.MelliPayamak;

namespace LoanSystemAvizhe.Services.Inquiry.Models.ApiIR;

public class SendApiIR_PostalAddress : BaseSendInquiry
{
    public string PostalCode { get; set; } = "";
}
