using System;

namespace LoanSystem.Data.Entities;

public class LoanAssuranceRequirement : BaseEntity
{
	public string AssuranceRequirementsJson { get; set; }

	public Guid LoanId { get; set; }

	public virtual Loan Loan { get; set; }

	public Guid CustomerId { get; set; }

	public virtual Customer Customer { get; set; }
}
