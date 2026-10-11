using System.Text.Json;

namespace LoanSystem.Services.Models.Sep;

public class SepPurchase
{
	public int TokenStatus { get; set; }

	public dynamic TokenErrorCode { get; set; }

	public string TokenErrorDesc { get; set; }

	public string Token { get; set; }

	public string MID { get; set; }

	public string TerminalId { get; set; }

	public string State { get; set; }

	public int Status { get; set; }

	public string RRN { get; set; }

	public string RefNum { get; set; }

	public string ResNum { get; set; }

	public string TraceNo { get; set; }

	public string SecurePan { get; set; }

	public long VerifyOrginalAmount { get; set; }

	public string VerifyResultDescription { get; set; }

	public int VerifyResultCode { get; set; }

	public override string ToString()
	{
		return JsonSerializer.Serialize(this);
	}
}
