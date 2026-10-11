using System.Text.Json;

namespace LoanSystem.Services.Models.OnlineMoneyTransferPanel;

public class OnlineMoneyTransferPanelInitAndSendOTPResult
{
	public string result { get; set; }

	public bool success { get; set; }

	public string message { get; set; }

	public override string ToString()
	{
		return JsonSerializer.Serialize(this);
	}
}
