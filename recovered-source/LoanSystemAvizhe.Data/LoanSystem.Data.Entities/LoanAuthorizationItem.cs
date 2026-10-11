using System;

namespace LoanSystem.Data.Entities;

public class LoanAuthorizationItem : BaseEntity
{
	public Guid AuthorizationItemId { get; set; }

	public virtual AuthorizationItem AuthorizationItem { get; set; }

	public bool IsPrimary { get; set; }

	public Guid LoanId { get; set; }

	public virtual Loan Loan { get; set; }

	public Guid CustomerId { get; set; }

	public virtual Customer Customer { get; set; }
}
