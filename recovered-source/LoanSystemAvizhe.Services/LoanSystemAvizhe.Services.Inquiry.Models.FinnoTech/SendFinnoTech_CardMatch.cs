using LoanSystemAvizhe.Services.Sms.Models.MelliPayamak;

namespace LoanSystemAvizhe.Services.Inquiry.Models.FinnoTech;

public class SendFinnoTech_CardMatch : BaseSendInquiry
{
	public string NationalCode { get; set; } = "";

	public string CardNumber { get; set; } = "";
}
