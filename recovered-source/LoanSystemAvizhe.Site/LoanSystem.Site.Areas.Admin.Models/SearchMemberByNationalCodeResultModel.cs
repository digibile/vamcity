using System.Collections.Generic;
using LoanSystem.Services.Models;

namespace LoanSystem.Site.Areas.Admin.Models;

public class SearchMemberByNationalCodeResultModel
{
	public Member2Model Member { get; set; }

	public List<UserLoanModel> UserLoans { get; set; } = new List<UserLoanModel>();
}
