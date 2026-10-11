namespace LoanSystemAvizhe.Services.Models.Shahin;

public class ShahinRespObject
{
	public string sourceAccountNumber { get; set; }

	public string destinationAccountNumber { get; set; }

	public long amount { get; set; }

	public string sourceBank { get; set; }

	public string destinationBank { get; set; }

	public string transferType { get; set; }

	public string traceNumber { get; set; }
}
