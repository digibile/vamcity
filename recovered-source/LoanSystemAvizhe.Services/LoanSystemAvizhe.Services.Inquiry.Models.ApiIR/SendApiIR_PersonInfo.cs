using LoanSystemAvizhe.Services.Sms.Models.MelliPayamak;

namespace LoanSystemAvizhe.Services.Inquiry.Models.ApiIR;

public class SendApiIR_PersonInfo : BaseSendInquiry
{
	public string NationalCode { get; set; } = "";

	public string BirthDate { get; set; } = "";
}
