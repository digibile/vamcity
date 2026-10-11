using System;

namespace LoanSystem.Data.Entities;

public class WithdrawAgentFactor : BaseEntity
{
	public Guid WithdrawId { get; set; }

	public virtual Transaction Withdraw { get; set; }

	public Guid AgentFactorId { get; set; }

	public virtual AgentFactor AgentFactor { get; set; }
}
