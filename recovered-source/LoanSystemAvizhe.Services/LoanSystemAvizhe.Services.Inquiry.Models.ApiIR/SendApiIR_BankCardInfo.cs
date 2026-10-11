using LoanSystemAvizhe.Services.Sms.Models.MelliPayamak;

namespace LoanSystemAvizhe.Services.Inquiry.Models.ApiIR;

public class SendApiIR_BankCardInfo : BaseSendInquiry
{
	public string CardNumber { get; set; } = "";
}
