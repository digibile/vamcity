using System.Text.Json;

namespace LoanSystem.Services.Models.EFarda;

public class EFardaResult
{
	public string result { get; set; }

	public string description { get; set; }

	public string doTime { get; set; }

	public string traceNumber { get; set; }

	public string amount { get; set; }

	public string refNumber { get; set; }

	public override string ToString()
	{
		return JsonSerializer.Serialize(this);
	}
}
