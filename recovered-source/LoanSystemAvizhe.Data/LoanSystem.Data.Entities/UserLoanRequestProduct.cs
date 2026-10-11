using System;

namespace LoanSystem.Data.Entities;

public class UserLoanRequestProduct : BaseEntity
{
	public decimal Price { get; set; }

	public decimal Count { get; set; }

	public Guid ProductId { get; set; }

	public virtual Product Product { get; set; }

	public Guid CustomerId { get; set; }

	public virtual Customer Customer { get; set; }
}
