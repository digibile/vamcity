namespace LoanSystem.Services.Models.EFarda;

public class EFardaRequest
{
	public string Account { get; set; }

	public string Iban { get; set; }

	public decimal Amount { get; set; }

	public string Cif { get; set; }

	public string TraceNumber { get; set; }
}
