using Newtonsoft.Json;

namespace LoanSystemAvizhe.Services.Models.RefahBeta;

public class RefahBetaResult<T>
{
	[JsonProperty]
	public T Data { get; set; }

	[JsonProperty]
	public int Status { get; set; }

	[JsonProperty]
	public string Message { get; set; }
}
