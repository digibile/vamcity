namespace LoanSystem.Services.Models.Sep;

public class SepRequest
{
	public string Action { get; set; }

	public string TerminalId { get; set; }

	public decimal Amount { get; set; }

	public string ResNum { get; set; }

	public string RedirectUrl { get; set; }

	public string CellNumber { get; set; }
}
