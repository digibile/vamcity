using LoanSystemAvizhe.Services.Sms.Models.MelliPayamak;

namespace LoanSystemAvizhe.Services.Inquiry.Models.FinnoTech;

public class SendFinnoTech_Sana : BaseSendInquiry
{
	public string NationalCode { get; set; } = "";

	public bool IsCompany { get; set; }
}
