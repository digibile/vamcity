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

public class LoanRequestStatusService : BaseService, ILoanRequestStatusService
{
	public LoanRequestStatusService(AppDbContext context, ICurrentUserService currentUserService)
		: base(context, currentUserService)
	{
	}

	public Task<List<LoanRequestStatus>> GetByCondition(Expression<Func<LoanRequestStatus, bool>> condition, params string[] includs)
	{
		IQueryable<LoanRequestStatus> source = Table<LoanRequestStatus>();
		foreach (string navigationPropertyPath in includs)
		{
			source = source.Include(navigationPropertyPath);
		}
		source = source.Where(condition);
		return source.ToListAsync();
	}

	public Task<List<LoanRequestStatus>> GetAllLoanRequestStatuses(params string[] includs)
	{
		IQueryable<LoanRequestStatus> source = Table<LoanRequestStatus>();
		foreach (string navigationPropertyPath in includs)
		{
			source = source.Include(navigationPropertyPath);
		}
		return source.OrderBy((LoanRequestStatus x) => x.Order).ToListAsync();
	}

	public Task<LoanRequestStatus> GetById(Guid entityId, params string[] includs)
	{
		return GetById<LoanRequestStatus>(entityId, includs);
	}

	public Task Insert(LoanRequestStatus entity)
	{
		entity.CustomerId = _currentUserService.CustomerId.Value;
		Insert<LoanRequestStatus>(entity);
		return Save();
	}

	public Task InsertBatch(List<LoanRequestStatus> entities)
	{
		foreach (LoanRequestStatus entity in entities)
		{
			Insert<LoanRequestStatus>(entity);
		}
		return Save();
	}

	public Task Update(LoanRequestStatus entity)
	{
		Update(entity, true);
		return Save();
	}

	public Task Delete(LoanRequestStatus entity)
	{
		Delete<LoanRequestStatus>(entity);
		return Save();
	}
}
