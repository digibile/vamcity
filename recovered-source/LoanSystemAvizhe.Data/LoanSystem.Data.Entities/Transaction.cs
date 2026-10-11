using System;
using System.Collections.Generic;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Data.Entities;

public class Transaction : BaseEntity
{
	public decimal Amount { get; set; }

	public DateTime SaveDate { get; set; }

	public PaymentMethods PaymentMethod { get; set; }

	public bool IsWithdraw { get; set; }

	public bool IsUnknown { get; set; }

	public double GoldWeight { get; set; }

	public double GoldRate { get; set; }

	public string Description { get; set; }

	public string SavedBy { get; set; }

	public string FileName { get; set; }

	public string FileName2 { get; set; }

	public string FileName3 { get; set; }

	public double WeightBalance { get; set; }

	public Guid? SponsoredLoanId { get; set; }

	public virtual Loan SponsoredLoan { get; set; }

	public Guid? UserLoanId { get; set; }

	public virtual UserLoan UserLoan { get; set; }

	public long? AgentId { get; set; }

	public virtual User Agent { get; set; }

	public Guid? CustomerMarketerCommissionId { get; set; }

	public virtual CustomerMarketerCommission CustomerMarketerCommission { get; set; }

	public Guid? UserLoanContactId { get; set; }

	public virtual UserLoanContact UserLoanContact { get; set; }

	public Guid? ContactId { get; set; }

	public virtual Contact Contact { get; set; }

	public Guid CustomerId { get; set; }

	public virtual Customer Customer { get; set; }

	public Guid? SuccessfullOnlineMoneyTransferId { get; set; }

	public virtual ICollection<OnlineMoneyTransfer> OnlineMoneyTransfers { get; set; } = new HashSet<OnlineMoneyTransfer>();
}
