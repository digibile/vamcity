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

public class UserLoanRequestService : BaseService, IUserLoanRequestService
{
	public UserLoanRequestService(AppDbContext context, ICurrentUserService currentUserService)
		: base(context, currentUserService)
	{
	}

	public Task<int> GetRequestCountByDateAsync(DateTime startDate, DateTime endDate)
	{
		return _context.UserLoanRequests.CountAsync((UserLoanRequest x) => x.RequestDate.Date >= startDate && x.RequestDate.Date <= endDate && x.CustomerId == _currentUserService.CustomerId);
	}

	public Task<List<UserLoanRequest>> GetByCondition(Expression<Func<UserLoanRequest, bool>> condition, bool asTracking = false, params string[] includs)
	{
		IQueryable<UserLoanRequest> source = ((!asTracking) ? Table<UserLoanRequest>() : TableTracking<UserLoanRequest>());
		foreach (string navigationPropertyPath in includs)
		{
			source = source.Include(navigationPropertyPath);
		}
		source = source.Where(condition);
		return source.ToListAsync();
	}

	public Task<List<UserLoanRequest>> GetAllUserLoanRequests(params string[] includs)
	{
		IQueryable<UserLoanRequest> source = Table<UserLoanRequest>();
		foreach (string navigationPropertyPath in includs)
		{
			source = source.Include(navigationPropertyPath);
		}
		return source.Where((UserLoanRequest x) => x.User.CustomerId == _currentUserService.CustomerId).ToListAsync();
	}

	public Task<UserLoanRequest> GetById(Guid entityId, params string[] includs)
	{
		return GetById<UserLoanRequest>(entityId, includs);
	}

	public Task<List<UserLoanRequest>> GetByUserId(long userId, params string[] includs)
	{
		IQueryable<UserLoanRequest> source = Table<UserLoanRequest>();
		foreach (string navigationPropertyPath in includs)
		{
			source = source.Include(navigationPropertyPath);
		}
		return source.Where((UserLoanRequest x) => x.UserId == userId).ToListAsync();
	}

	public Task<List<UserLoanRequest>> GetByAgentId(long agentId, params string[] includs)
	{
		IQueryable<UserLoanRequest> source = Table<UserLoanRequest>();
		foreach (string navigationPropertyPath in includs)
		{
			source = source.Include(navigationPropertyPath);
		}
		return source.Where((UserLoanRequest x) => x.AgentId == (long?)agentId).ToListAsync();
	}

	public new Task RunInTransaction(Func<DbTransaction, Task> action)
	{
		return base.RunInTransaction(action);
	}

	public Task Insert(UserLoanRequest entity)
	{
		entity.CustomerId = _currentUserService.CustomerId.Value;
		Insert<UserLoanRequest>(entity);
		return Save();
	}

	public Task Update(UserLoanRequest entity)
	{
		Update(entity, true);
		return Save();
	}

	public Task Delete(UserLoanRequest entity)
	{
		IQueryable<UserLoanRequestStatusHistory> entities = _context.UserLoanRequestStatusHistories.Where((UserLoanRequestStatusHistory x) => x.UserLoanRequestId == entity.Id);
		_context.UserLoanRequestStatusHistories.RemoveRange(entities);
		IQueryable<UserLoanRequestAuthorizationItem> entities2 = _context.UserLoanRequestAuthorizationItems.Where((UserLoanRequestAuthorizationItem x) => x.UserLoanRequestId == entity.Id);
		_context.UserLoanRequestAuthorizationItems.RemoveRange(entities2);
		Delete<UserLoanRequest>(entity);
		return Save();
	}
}
