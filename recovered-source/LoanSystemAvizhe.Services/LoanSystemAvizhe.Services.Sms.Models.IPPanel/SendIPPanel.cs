using LoanSystemAvizhe.Services.Sms.Models.MelliPayamak;

namespace LoanSystemAvizhe.Services.Sms.Models.IPPanel;

public class SendIPPanel : BaseSendPayamak
{
	public string[] Recipient { get; set; }

	public string Sender { get; set; } = "";

	public string Message { get; set; } = "";

	public string Time { get; set; } = "";

	public string Code { get; set; } = "";
}
