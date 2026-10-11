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

public class UserLoanRequestStatusHistoryService : BaseService, IUserLoanRequestStatusHistoryService
{
	private readonly IUserPaymentService _userPaymentService;

	public UserLoanRequestStatusHistoryService(AppDbContext context, ICurrentUserService currentUserService, IUserPaymentService userPaymentService)
		: base(context, currentUserService)
	{
		_userPaymentService = userPaymentService;
	}

	public Task<int> GetCountByCondition(Expression<Func<UserLoanRequestStatusHistory, bool>> condition)
	{
		return Table<UserLoanRequestStatusHistory>().Where(condition).CountAsync();
	}

	public Task<List<UserLoanRequestStatusHistory>> GetAll(bool include = true)
	{
		IQueryable<UserLoanRequestStatusHistory> source = Table<UserLoanRequestStatusHistory>();
		if (include)
		{
			source = source.Include((UserLoanRequestStatusHistory x) => x.FromStatus);
			source = source.Include((UserLoanRequestStatusHistory x) => x.FromAgentStatus);
			source = source.Include((UserLoanRequestStatusHistory x) => x.ToStatus);
			source = source.Include((UserLoanRequestStatusHistory x) => x.ToAgentStatus);
		}
		return source.ToListAsync();
	}

	public Task<UserLoanRequestStatusHistory> GetById(Guid entityId, bool include = true)
	{
		IQueryable<UserLoanRequestStatusHistory> source = Table<UserLoanRequestStatusHistory>();
		if (include)
		{
			source = source.Include((UserLoanRequestStatusHistory x) => x.FromStatus);
			source = source.Include((UserLoanRequestStatusHistory x) => x.FromAgentStatus);
			source = source.Include((UserLoanRequestStatusHistory x) => x.ToStatus);
			source = source.Include((UserLoanRequestStatusHistory x) => x.ToAgentStatus);
			source = source.Include((UserLoanRequestStatusHistory x) => x.User);
		}
		return source.SingleOrDefaultAsync((UserLoanRequestStatusHistory x) => x.Id == entityId);
	}

	public Task<List<UserLoanRequestStatusHistory>> GetByUserId(long userId)
	{
		return (from x in Table<UserLoanRequestStatusHistory>().Include((UserLoanRequestStatusHistory x) => x.FromStatus).Include((UserLoanRequestStatusHistory x) => x.FromAgentStatus).Include((UserLoanRequestStatusHistory x) => x.ToStatus)
				.Include((UserLoanRequestStatusHistory x) => x.User)
			where x.UserId == userId
			select x).ToListAsync();
	}

	public Task<List<UserLoanRequestStatusHistory>> GetByUserLoanRequestId(Guid userLoanRequestId)
	{
		return (from x in Table<UserLoanRequestStatusHistory>().Include((UserLoanRequestStatusHistory x) => x.FromStatus).Include((UserLoanRequestStatusHistory x) => x.FromAgentStatus).Include((UserLoanRequestStatusHistory x) => x.ToStatus)
				.Include((UserLoanRequestStatusHistory x) => x.ToAgentStatus)
				.Include((UserLoanRequestStatusHistory x) => x.User)
			where x.UserLoanRequestId == userLoanRequestId
			select x).ToListAsync();
	}

	public Task<int> CountByUserLoanRequestId(Guid userLoanRequestId)
	{
		return Table<UserLoanRequestStatusHistory>().CountAsync((UserLoanRequestStatusHistory x) => x.UserLoanRequestId == userLoanRequestId);
	}

	public Task Insert(UserLoanRequestStatusHistory entity)
	{
		Insert<UserLoanRequestStatusHistory>(entity);
		return Save();
	}

	public Task Update(UserLoanRequestStatusHistory entity)
	{
		Update(entity, true);
		return Save();
	}

	public Task Delete(UserLoanRequestStatusHistory entity)
	{
		Delete<UserLoanRequestStatusHistory>(entity);
		return Save();
	}
}
