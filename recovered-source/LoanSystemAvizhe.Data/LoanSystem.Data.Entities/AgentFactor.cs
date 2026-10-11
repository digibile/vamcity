using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Data.Entities;

public class AgentFactor : BaseEntity
{
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public int Code { get; set; }

	public string UserCode { get; set; }

	public long MemberId { get; set; }

	public virtual User Member { get; set; }

	public long? AgentId { get; set; }

	public virtual User Agent { get; set; }

	public DateTime CreateAt { get; set; }

	public AgentFactorCheckoutTypes CheckoutType { get; set; }

	public decimal Amount { get; set; }

	public string Description { get; set; }

	public string AdminDescription { get; set; }

	public AdminAcceptStatus AdminAcceptStatus { get; set; }

	public decimal Commission { get; set; }

	public DateTime CheckoutRequestDateTime { get; set; }

	public AgentFactorOnlinePaymentMethod AgentFactorOnlinePaymentMethod { get; set; }

	public Guid? MemberPurchaseId { get; set; }

	public virtual MemberPurchase MemberPurchase { get; set; }

	public Guid CustomerId { get; set; }

	public virtual Customer Customer { get; set; }

	public virtual ICollection<WithdrawAgentFactor> WithdrawAgentFactors { get; set; } = new HashSet<WithdrawAgentFactor>();

	public virtual ICollection<AgentFactorItem> AgentFactorItems { get; set; } = new HashSet<AgentFactorItem>();
}
