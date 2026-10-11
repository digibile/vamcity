using LoanSystemAvizhe.Services.Sms.Models.MelliPayamak;

namespace LoanSystemAvizhe.Services.Inquiry.Models.FinnoTech;

public class SendFinnoTech_Shahkar : BaseSendInquiry
{
	public string Mobile { get; set; }

	public string NationalCode { get; set; }

	public string TrackId { get; set; }
}
