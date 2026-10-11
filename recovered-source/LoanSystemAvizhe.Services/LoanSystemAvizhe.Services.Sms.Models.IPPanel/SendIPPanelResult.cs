namespace LoanSystemAvizhe.Services.Sms.Models.IPPanel;

public class SendIPPanelResult
{
	public string Status { get; set; }

	public string ErrorMessage { get; set; }

	public int Code { get; set; }

	public IPPanelResultData Data { get; set; }
}
