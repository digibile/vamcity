using System;

namespace LoanSystem.Data.Entities;

public class CustomerOrder : BaseEntity
{
	public string Name { get; set; }

	public string NationalCode { get; set; }

	public string Phone { get; set; }

	public string ProductName { get; set; }

	public string Description { get; set; }

	public bool IsSeen { get; set; }

	public string CallResult { get; set; }

	public DateTime SaveDate { get; set; }

	public Guid CustomerId { get; set; }

	public virtual Customer Customer { get; set; }
}
