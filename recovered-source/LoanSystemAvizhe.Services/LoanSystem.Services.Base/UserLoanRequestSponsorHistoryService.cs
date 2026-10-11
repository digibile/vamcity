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

public class UserLoanRequestSponsorHistoryService : BaseService, IUserLoanRequestSponsorHistoryService
{
	public UserLoanRequestSponsorHistoryService(AppDbContext context, ICurrentUserService currentUserService)
		: base(context, currentUserService)
	{
	}

	public Task<List<UserLoanRequestSponsorHistory>> GetByUserLoanRequestId(Guid id, params string[] includs)
	{
		return GetByCondition((UserLoanRequestSponsorHistory x) => x.UserLoanRequestId == id, includs);
	}

	public Task<List<UserLoanRequestSponsorHistory>> GetByCondition(Expression<Func<UserLoanRequestSponsorHistory, bool>> condition, params string[] includs)
	{
		IQueryable<UserLoanRequestSponsorHistory> source = Table<UserLoanRequestSponsorHistory>();
		foreach (string navigationPropertyPath in includs)
		{
			source = source.Include(navigationPropertyPath);
		}
		source = source.Where(condition);
		return source.ToListAsync();
	}

	public Task Insert(UserLoanRequestSponsorHistory entity)
	{
		entity.CustomerId = _currentUserService.CustomerId.Value;
		Insert<UserLoanRequestSponsorHistory>(entity);
		return Save();
	}

	public Task Update(UserLoanRequestSponsorHistory entity)
	{
		entity.CustomerId = _currentUserService.CustomerId.Value;
		Update(entity, true);
		return Save();
	}

	public Task Delete(UserLoanRequestSponsorHistory entity)
	{
		Delete<UserLoanRequestSponsorHistory>(entity);
		return Save();
	}

	public Task<int> GetHistoryCount(Guid userLoanRequestId)
	{
		return Table<UserLoanRequestSponsorHistory>().CountAsync((UserLoanRequestSponsorHistory x) => x.UserLoanRequestId == userLoanRequestId);
	}
}
