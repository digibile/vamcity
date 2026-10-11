namespace LoanSystem.Services.Models.OnlineMoneyTransferPanel;

public class OnlineMoneyTransferPanelInitAndSendOTP
{
	public decimal Amount { get; set; }

	public string From { get; set; }

	public string To { get; set; }

	public string ToName { get; set; }

	public string ClientTraceNumber { get; set; }
}
