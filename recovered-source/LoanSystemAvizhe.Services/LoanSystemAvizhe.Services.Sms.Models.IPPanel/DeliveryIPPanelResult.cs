namespace LoanSystemAvizhe.Services.Sms.Models.IPPanel;

public class DeliveryIPPanelResult
{
	public string Status { get; set; }

	public string ErrorMessage { get; set; }

	public int Code { get; set; }

	public DeliveryIPPanelResultData Data { get; set; }
}
