using System;

namespace LoanSystem.Services.Models.Landa;

public class GetCustomerResponse
{
	public Guid id { get; set; }

	public string code { get; set; }

	public string name { get; set; }
}
