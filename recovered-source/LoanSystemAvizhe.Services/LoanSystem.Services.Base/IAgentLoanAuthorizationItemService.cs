using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Base;

public interface IAgentLoanAuthorizationItemService
{
	Task<List<AgentLoanAuthorizationItem>> GetByCondition(Expression<Func<AgentLoanAuthorizationItem, bool>> condition, params string[] includs);

	Task Insert(AgentLoanAuthorizationItem entity);

	Task Update(AgentLoanAuthorizationItem entity);

	Task Delete(AgentLoanAuthorizationItem entity);
}
