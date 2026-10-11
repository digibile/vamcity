namespace LoanSystem.Services.Models.ZarinPal;

public class ZarinPalConfig
{
	public string MerchantID { get; set; }

	public string CallbackUrl { get; set; }

	public string MemberCallbackUrl { get; set; }

	public int Mode { get; set; }
}
