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

public class CustomerMarketerCommissionService : BaseService, ICustomerMarketerCommissionService
{
	public CustomerMarketerCommissionService(AppDbContext context, ICurrentUserService currentUserService)
		: base(context, currentUserService)
	{
	}

	public Task<List<CustomerMarketerCommission>> GetByCondition(Expression<Func<CustomerMarketerCommission, bool>> condition, params string[] includs)
	{
		IQueryable<CustomerMarketerCommission> source = Table<CustomerMarketerCommission>();
		foreach (string navigationPropertyPath in includs)
		{
			source = source.Include(navigationPropertyPath);
		}
		source = source.Where(condition);
		return source.ToListAsync();
	}

	public Task<List<CustomerMarketerCommission>> GetAll(params string[] includs)
	{
		IQueryable<CustomerMarketerCommission> source = Table<CustomerMarketerCommission>();
		foreach (string navigationPropertyPath in includs)
		{
			source = source.Include(navigationPropertyPath);
		}
		return source.ToListAsync();
	}

	public Task<CustomerMarketerCommission> GetById(Guid entityId, params string[] includs)
	{
		IQueryable<CustomerMarketerCommission> source = Table<CustomerMarketerCommission>();
		foreach (string navigationPropertyPath in includs)
		{
			source = source.Include(navigationPropertyPath);
		}
		return source.SingleOrDefaultAsync((CustomerMarketerCommission x) => x.Id == entityId);
	}

	public Task<List<CustomerMarketerCommission>> GetByUserId(long marketerId, params string[] includs)
	{
		IQueryable<CustomerMarketerCommission> source = Table<CustomerMarketerCommission>();
		foreach (string navigationPropertyPath in includs)
		{
			source = source.Include(navigationPropertyPath);
		}
		return source.Where((CustomerMarketerCommission x) => x.MarketerId == marketerId).ToListAsync();
	}

	public Task Insert(CustomerMarketerCommission entity)
	{
		if (entity == null)
		{
			throw new ArgumentNullException("entity");
		}
		_context.CustomerMarketerCommissions.Add(entity);
		return Save();
	}

	public Task Update(CustomerMarketerCommission entity)
	{
		if (entity == null)
		{
			throw new ArgumentNullException("entity");
		}
		_context.CustomerMarketerCommissions.Update(entity);
		return Save();
	}

	public Task Delete(CustomerMarketerCommission entity)
	{
		_context.CustomerMarketerCommissions.Remove(entity);
		return Save();
	}
}
