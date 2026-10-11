using LoanSystemAvizhe.Services.Sms.Models.MelliPayamak;

namespace LoanSystemAvizhe.Services.Inquiry.Models.ApiIR;

public class SendApiIR_UnpaidCheque : BaseSendInquiry
{
	public string NationalCode { get; set; } = "";
}
