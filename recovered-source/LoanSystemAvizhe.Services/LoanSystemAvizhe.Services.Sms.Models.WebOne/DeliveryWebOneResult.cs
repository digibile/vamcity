using System.Collections.Generic;

namespace LoanSystemAvizhe.Services.Sms.Models.WebOne;

public class DeliveryWebOneResult
{
	public bool Status { get; set; }

	public string Message { get; set; }

	public List<DeliveryWebOneResultData> Deliveries { get; set; }
}
