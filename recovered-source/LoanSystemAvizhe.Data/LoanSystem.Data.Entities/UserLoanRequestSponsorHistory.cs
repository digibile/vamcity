using System;

namespace LoanSystem.Data.Entities;

public class UserLoanRequestSponsorHistory : BaseEntity
{
	public long UserId { get; set; }

	public virtual User User { get; set; }

	public Guid UserLoanRequestId { get; set; }

	public virtual UserLoanRequest UserLoanRequest { get; set; }

	public DateTime DateTime { get; set; }

	public string Subject { get; set; }

	public string Description { get; set; }

	public Guid CustomerId { get; set; }
}
