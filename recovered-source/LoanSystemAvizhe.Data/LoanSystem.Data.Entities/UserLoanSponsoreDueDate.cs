using System;

namespace LoanSystem.Data.Entities;

public class UserLoanSponsoreDueDate : BaseEntity
{
	public Guid UserLoanId { get; set; }

	public virtual UserLoan UserLoan { get; set; }

	public decimal Amount { get; set; }

	public DateTime DueDate { get; set; }

	public string Description { get; set; }

	public bool IsCheckouted { get; set; }

	public Guid CustomerId { get; set; }

	public virtual Customer Customer { get; set; }
}
