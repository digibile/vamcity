namespace LoanSystem.Services.Models.NextPay;

public class NextPayRequest
{
	public string api_key { get; set; }

	public string amount { get; set; }

	public string callback_url { get; set; }

	public string customer_phone { get; set; }

	public string payer_name { get; set; }

	public string payer_desc { get; set; }

	public string order_id { get; set; }
}
