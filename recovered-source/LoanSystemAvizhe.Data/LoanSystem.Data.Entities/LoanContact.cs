using System;

namespace LoanSystem.Data.Entities;

public class LoanContact : BaseEntity
{
	public Guid ContactId { get; set; }

	public virtual Contact Contact { get; set; }

	public Guid LoanId { get; set; }

	public virtual Loan Loan { get; set; }

	public decimal Share { get; set; }

	public Guid CustomerId { get; set; }

	public virtual Customer Customer { get; set; }
}
