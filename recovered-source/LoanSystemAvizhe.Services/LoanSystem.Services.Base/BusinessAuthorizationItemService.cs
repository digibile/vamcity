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

public class BusinessAuthorizationItemService : BaseService, IBusinessAuthorizationItemService
{
	public BusinessAuthorizationItemService(AppDbContext context, ICurrentUserService currentUserService)
		: base(context, currentUserService)
	{
	}

	public Task<List<BusinessAuthorizationItem>> GetByCondition(Expression<Func<BusinessAuthorizationItem, bool>> condition, params string[] includs)
	{
		IQueryable<BusinessAuthorizationItem> source = Table<BusinessAuthorizationItem>();
		foreach (string navigationPropertyPath in includs)
		{
			source = source.Include(navigationPropertyPath);
		}
		source = source.Where(condition);
		return source.ToListAsync();
	}

	public async Task Insert(BusinessAuthorizationItem entity)
	{
		entity.CustomerId = _currentUserService.CustomerId.Value;
		Insert<BusinessAuthorizationItem>(entity);
		await Save();
	}

	public Task Update(BusinessAuthorizationItem entity)
	{
		entity.CustomerId = _currentUserService.CustomerId.Value;
		Update(entity, true);
		return Save();
	}

	public Task Delete(BusinessAuthorizationItem entity)
	{
		Delete<BusinessAuthorizationItem>(entity);
		return Save();
	}
}
