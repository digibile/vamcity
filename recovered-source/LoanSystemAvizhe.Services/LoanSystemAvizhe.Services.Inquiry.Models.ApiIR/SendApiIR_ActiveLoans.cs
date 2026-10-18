using LoanSystemAvizhe.Services.Sms.Models.MelliPayamak;

namespace LoanSystemAvizhe.Services.Inquiry.Models.ApiIR;

public class SendApiIR_ActiveLoans : BaseSendInquiry
{
    public string NationalCode { get; set; } = "";
}
