using System.Collections.Generic;
using Newtonsoft.Json;

namespace LoanSystem.Services.Models.IranValidity;

public class IranValidityResult<T> where T : class
{
	[JsonProperty]
	public T Data { get; set; }

	[JsonProperty]
	public bool HasError { get; set; }

	[JsonProperty]
	public List<ResponseMessage> Messages { get; set; }
}
