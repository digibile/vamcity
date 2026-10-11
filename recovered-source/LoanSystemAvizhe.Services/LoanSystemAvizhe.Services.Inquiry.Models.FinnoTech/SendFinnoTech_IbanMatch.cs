using LoanSystemAvizhe.Services.Sms.Models.MelliPayamak;

namespace LoanSystemAvizhe.Services.Inquiry.Models.FinnoTech;

public class SendFinnoTech_IbanMatch : BaseSendInquiry
{
	public string NationalCode { get; set; } = "";

	public string Iban { get; set; } = "";
}
