using System;

namespace LoanSystem.Data.Entities;

public class UserLoanContact : BaseEntity
{
	public Guid UserLoanId { get; set; }

	public virtual UserLoan UserLoan { get; set; }

	public Guid ContactId { get; set; }

	public virtual Contact Contact { get; set; }

	public decimal Share { get; set; }

	public DateTime DueDate { get; set; }

	public Guid CustomerId { get; set; }

	public virtual Customer Customer { get; set; }
}
