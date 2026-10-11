namespace LoanSystem.Services.Models.Sepehr;

public class SepehrRequest
{
	public decimal Amount { get; set; }

	public string CallbackUrl { get; set; }

	public long TerminalId { get; set; }

	public string InvoiceId { get; set; }

	public string Email { get; set; }

	public string Payload { get; set; }
}
