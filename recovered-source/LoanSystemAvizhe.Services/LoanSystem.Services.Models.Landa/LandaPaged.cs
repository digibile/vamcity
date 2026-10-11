using System.Collections.Generic;

namespace LoanSystem.Services.Models.Landa;

public class LandaPaged
{
	public int currentPage { get; set; }

	public int totalItems { get; set; }

	public int pageSize { get; set; }

	public string columns { get; set; }

	public string exportColumns { get; set; }

	public List<object> filterExpression { get; set; } = new List<object>();

	public int totalPages { get; set; }
}
