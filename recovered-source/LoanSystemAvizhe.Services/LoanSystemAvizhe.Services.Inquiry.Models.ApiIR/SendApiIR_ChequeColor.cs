using LoanSystemAvizhe.Services.Sms.Models.MelliPayamak;

namespace LoanSystemAvizhe.Services.Inquiry.Models.ApiIR;

public class SendApiIR_ChequeColor : BaseSendInquiry
{
	public string NationalCode { get; set; } = "";

	public bool IsCompany { get; set; }
}
