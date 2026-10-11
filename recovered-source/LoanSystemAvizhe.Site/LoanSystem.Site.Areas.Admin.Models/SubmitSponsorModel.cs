using System;

namespace LoanSystem.Site.Areas.Admin.Models;

public class SubmitSponsorModel
{
	public Guid UserLoanRequestId { get; set; }

	public long? SponsorId { get; set; }
}
