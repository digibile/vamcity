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

namespace LoanSystemAvizhe.Services.ThirdParty;

public class IceReportService : BaseService, IIceReportService
{
	public IceReportService(AppDbContext context, ICurrentUserService currentUserService)
		: base(context, currentUserService)
	{
	}

	public IQueryable<IceReport> AsQueryable()
	{
		return Table<IceReport>();
	}

	public Task<int> GetCountByCondition(Expression<Func<IceReport, bool>> condition)
	{
		return Table<IceReport>().Where(condition).CountAsync();
	}

	public Task<List<IceReport>> GetByCondition(Expression<Func<IceReport, bool>> condition)
	{
		return Table<IceReport>().Where(condition).ToListAsync();
	}

	public Task<List<IceReport>> GetAll(Guid? customerId)
	{
		IQueryable<IceReport> source = Table<IceReport>();
		if (customerId.HasValue)
		{
			source = from x in source.IgnoreQueryFilters()
				where x.CustomerId == customerId
				select x;
		}
		return source.ToListAsync();
	}

	public Task<IceReport> GetById(Guid entityId, params string[] includs)
	{
		return GetById<IceReport>(entityId, includs);
	}

	public async Task Insert(IceReport entity)
	{
		entity.CustomerId = _currentUserService.CustomerId.Value;
		Insert<IceReport>(entity);
		await Save();
	}

	public Task Update(IceReport entity)
	{
		Update(entity, true);
		return Save();
	}

	public Task Delete(IceReport entity)
	{
		Delete<IceReport>(entity);
		return Save();
	}
}
