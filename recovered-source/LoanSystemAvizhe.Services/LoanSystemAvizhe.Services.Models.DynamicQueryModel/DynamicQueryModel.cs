using System.Collections.Generic;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystemAvizhe.Services.Models.DynamicQueryModel;

public class DynamicQueryModel
{
	public List<string> SortBy { get; set; }

	public List<bool> SortDesc { get; set; }

	public int Page { get; set; }

	public int ItemsPerPage { get; set; }

	public bool All { get; set; }

	public string Search { get; set; }

	public string UserLoanCode { get; set; }

	public ThirdPart ThirdParty { get; set; }
}
