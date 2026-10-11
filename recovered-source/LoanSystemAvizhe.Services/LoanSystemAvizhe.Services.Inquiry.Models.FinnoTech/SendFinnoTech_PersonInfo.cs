using LoanSystemAvizhe.Services.Sms.Models.MelliPayamak;

namespace LoanSystemAvizhe.Services.Inquiry.Models.FinnoTech;

public class SendFinnoTech_PersonInfo : BaseSendInquiry
{
	public string NationalCode { get; set; } = "";

	public string BirthDate { get; set; } = "";
}
