using System.Dynamic;
using LoanSystemAvizhe.Services.Sms.Models.MelliPayamak;

namespace LoanSystemAvizhe.Services.Sms.Models.IPPanel;

public class SendIPPanelPattern : BaseSendPayamak
{
	public string Code { get; set; } = "";

	public string Sender { get; set; } = "";

	public string Recipient { get; set; }

	public ExpandoObject Variable { get; set; }
}
