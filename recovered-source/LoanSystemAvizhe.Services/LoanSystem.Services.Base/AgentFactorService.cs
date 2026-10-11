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

public class AgentFactorService : BaseService, IAgentFactorService
{
	public AgentFactorService(AppDbContext context, ICurrentUserService currentUserService)
		: base(context, currentUserService)
	{
	}

	public Task<List<AgentFactor>> GetByCondition(Expression<Func<AgentFactor, bool>> condition, bool asTracking = false, params string[] includs)
	{
		IQueryable<AgentFactor> source = ((!asTracking) ? Table<AgentFactor>() : TableTracking<AgentFactor>());
		foreach (string navigationPropertyPath in includs)
		{
			source = source.Include(navigationPropertyPath);
		}
		source = source.Where(condition);
		return source.ToListAsync();
	}

	public Task<List<AgentFactor>> GetAllAgentFactors(params string[] includs)
	{
		IQueryable<AgentFactor> source = Table<AgentFactor>();
		foreach (string navigationPropertyPath in includs)
		{
			source = source.Include(navigationPropertyPath);
		}
		return source.ToListAsync();
	}

	public Task<AgentFactor> GetById(Guid entityId, params string[] includs)
	{
		return GetById<AgentFactor>(entityId, includs);
	}

	public Task<List<AgentFactor>> GetByUserId(long userId, params string[] includs)
	{
		IQueryable<AgentFactor> source = Table<AgentFactor>();
		foreach (string navigationPropertyPath in includs)
		{
			source = source.Include(navigationPropertyPath);
		}
		return source.Where((AgentFactor x) => x.MemberId == userId).ToListAsync();
	}

	public Task<List<AgentFactor>> GetByAgentId(long agentId, params string[] includs)
	{
		IQueryable<AgentFactor> source = Table<AgentFactor>();
		foreach (string navigationPropertyPath in includs)
		{
			source = source.Include(navigationPropertyPath);
		}
		return source.Where((AgentFactor x) => x.AgentId == (long?)agentId).ToListAsync();
	}

	public Task Insert(AgentFactor entity)
	{
		entity.CustomerId = _currentUserService.CustomerId.Value;
		Insert<AgentFactor>(entity);
		return Save();
	}

	public Task Update(AgentFactor entity)
	{
		Update(entity, true);
		return Save();
	}

	public Task Delete(AgentFactor entity)
	{
		Delete<AgentFactor>(entity);
		return Save();
	}
}
