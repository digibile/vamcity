using System.Collections.Generic;

namespace LoanSystem.Services.Models.Landa;

public class LandaPagedResponse<T>
{
	public LandaPaged page { get; set; }

	public List<T> items { get; set; }
}
