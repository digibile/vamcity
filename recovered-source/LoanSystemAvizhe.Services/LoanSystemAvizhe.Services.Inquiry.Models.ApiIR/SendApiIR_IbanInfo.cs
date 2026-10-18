using LoanSystemAvizhe.Services.Sms.Models.MelliPayamak;

namespace LoanSystemAvizhe.Services.Inquiry.Models.ApiIR;

public class SendApiIR_IbanInfo : BaseSendInquiry
{
    public string Iban { get; set; } = "";
}
