using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace LoanSystemAvizhe.Services.Models.Shahin;

public class ShahinTransferRequestModel
{
	public string bank { get; set; }

	public string nationalCode { get; set; }

	[Required(ErrorMessage = "شماره حساب مبدا الزامی است.")]
	public string sourceAccount { get; set; }

	[Required(ErrorMessage = "شماره حساب مقصد الزامی است.")]
	public string destinationAccountNumber { get; set; }

	[Required(ErrorMessage = "نام بانک مقصد الزامی است.")]
	public string destinationBank { get; set; }

	public string destinationAccountName { get; set; }

	public decimal amount { get; set; }

	public string transferType { get; set; }

	public string babat { get; set; }

	public string paymentID { get; set; }

	public string documentID { get; set; }

	public string transferID { get; set; }

	public string depositDescription { get; set; }

	public string withdrawDescription { get; set; }

	public override string ToString()
	{
		return JsonSerializer.Serialize(this);
	}
}
