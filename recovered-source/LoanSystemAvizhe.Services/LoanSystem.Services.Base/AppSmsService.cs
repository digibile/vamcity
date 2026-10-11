using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoanSystem.Data;
using LoanSystem.Data.Entities;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Spm.Services.Infrastructure;

namespace LoanSystem.Services.Base;

public class AppSmsService : BaseService, IAppSmsService
{
	public AppSmsService(AppDbContext context, ICurrentUserService currentUserService)
		: base(context, currentUserService)
	{
	}

	public Task<int> GetCountByCondition(Expression<Func<AppSms, bool>> condition, bool ignoreQueryFilter = false, params string[] includs)
	{
		IQueryable<AppSms> source = Table<AppSms>();
		if (ignoreQueryFilter)
		{
			source = source.IgnoreQueryFilters();
		}
		foreach (string navigationPropertyPath in includs)
		{
			source = source.Include(navigationPropertyPath);
		}
		source = source.Where(condition);
		return source.CountAsync();
	}

	public Task<List<AppSms>> GetByCondition(Expression<Func<AppSms, bool>> condition, bool ignoreQueryFilter = false, params string[] includs)
	{
		IQueryable<AppSms> source = Table<AppSms>();
		if (ignoreQueryFilter)
		{
			source = source.IgnoreQueryFilters();
		}
		foreach (string navigationPropertyPath in includs)
		{
			source = source.Include(navigationPropertyPath);
		}
		source = source.Where(condition);
		return source.ToListAsync();
	}

	public Task<List<AppSms>> GetAll(Guid? customerId)
	{
		IQueryable<AppSms> source = Table<AppSms>();
		if (customerId.HasValue)
		{
			source = from x in source.IgnoreQueryFilters()
				where x.CustomerId == customerId
				select x;
		}
		return source.ToListAsync();
	}

	public Task<AppSms> GetById(Guid entityId, params string[] includs)
	{
		return GetById<AppSms>(entityId, includs);
	}

	public async Task Insert(AppSms entity)
	{
		entity.CustomerId = _currentUserService.CustomerId.Value;
		Insert<AppSms>(entity);
		await Save();
	}

	public Task Update(AppSms entity)
	{
		entity.CustomerId = _currentUserService.CustomerId.Value;
		Update(entity, true);
		return Save();
	}

	public async Task UpdateStatus(Guid entityId, AppSmsStatus newStatus)
	{
		AppSms appSms = await GetById(entityId);
		if (appSms == null)
		{
			appSms.Status = newStatus;
			Update(appSms, true);
			await Save();
		}
	}

	public Task Delete(AppSms entity)
	{
		Delete<AppSms>(entity);
		return Save();
	}
}
