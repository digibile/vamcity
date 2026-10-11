using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoanSystem.Data;
using LoanSystem.Data.Entities;
using LoanSystemAvizhe.Core;
using Microsoft.EntityFrameworkCore;
using Spm.Services.Infrastructure;

namespace LoanSystem.Services.Base;

public class AgentLoanAuthorizationItemService : BaseService, IAgentLoanAuthorizationItemService
{
	public AgentLoanAuthorizationItemService(AppDbContext context, ICurrentUserService currentUserService)
		: base(context, currentUserService)
	{
	}

	public Task<List<AgentLoanAuthorizationItem>> GetByCondition(Expression<Func<AgentLoanAuthorizationItem, bool>> condition, params string[] includs)
	{
		IQueryable<AgentLoanAuthorizationItem> source = Table<AgentLoanAuthorizationItem>();
		foreach (string navigationPropertyPath in includs)
		{
			source = source.Include(navigationPropertyPath);
		}
		source = source.Where(condition);
		return source.ToListAsync();
	}

	public async Task Insert(AgentLoanAuthorizationItem entity)
	{
		entity.CustomerId = _currentUserService.CustomerId.Value;
		Insert<AgentLoanAuthorizationItem>(entity);
		await Save();
	}

	public Task Update(AgentLoanAuthorizationItem entity)
	{
		entity.CustomerId = _currentUserService.CustomerId.Value;
		Update(entity, true);
		return Save();
	}

	public Task Delete(AgentLoanAuthorizationItem entity)
	{
		Delete<AgentLoanAuthorizationItem>(entity);
		return Save();
	}
}
