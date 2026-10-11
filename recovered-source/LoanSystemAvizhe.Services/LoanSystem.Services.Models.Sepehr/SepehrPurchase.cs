using System.Text.Json;

namespace LoanSystem.Services.Models.Sepehr;

public class SepehrPurchase
{
	public int TokenStatus { get; set; }

	public string AccessToken { get; set; }

	public long Amount { get; set; }

	public long TerminalId { get; set; }

	public string InvoiceId { get; set; }

	public string Hash { get; set; }

	public long Rrn { get; set; }

	public long Tracenumber { get; set; }

	public string Digitalreceipt { get; set; }

	public int RespCode { get; set; }

	public string Respmsg { get; set; }

	public string VerifyReturnId { get; set; }

	public string VerifyStatus { get; set; }

	public string VerifyMessage { get; set; }

	public override string ToString()
	{
		return JsonSerializer.Serialize(this);
	}
}
