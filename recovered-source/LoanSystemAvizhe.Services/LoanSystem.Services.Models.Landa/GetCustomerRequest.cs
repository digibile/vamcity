using System.Collections.Generic;

namespace LoanSystem.Services.Models.Landa;

public class GetCustomerRequest
{
	public int currentPage { get; set; }

	public int pageSize { get; set; }

	public List<FilterExpression> filterExpression { get; set; } = new List<FilterExpression>();
}
