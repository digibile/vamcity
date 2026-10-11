using System;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Data.Entities;

public class SubAgentTransaction : BaseEntity
{
	public decimal Amount { get; set; }

	public DateTime SaveDate { get; set; }

	public PaymentMethods PaymentMethod { get; set; }

	public bool IsWithdraw { get; set; }

	public string Description { get; set; }

	public string SavedBy { get; set; }

	public string FileName { get; set; }

	public Guid? UserLoanId { get; set; }

	public virtual UserLoan UserLoan { get; set; }

	public Guid CustomerId { get; set; }

	public virtual Customer Customer { get; set; }
}
