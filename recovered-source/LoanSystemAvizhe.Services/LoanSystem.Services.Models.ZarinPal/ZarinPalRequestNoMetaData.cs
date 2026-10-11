namespace LoanSystem.Services.Models.ZarinPal;

public class ZarinPalRequestNoMetaData
{
	public string merchant_id { get; set; }

	public string amount { get; set; }

	public string callback_url { get; set; }

	public string description { get; set; }
}
