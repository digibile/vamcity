using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Base;

public interface IAgentLoanService
{
	Task<List<AgentLoan>> GetByCondition(Expression<Func<AgentLoan, bool>> condition, params string[] includs);

	Task Modify(List<AgentLoan> agentLoans, long agentId);

	Task<bool> CheckLoanAssignedToSponsor(Guid loanId);
}
