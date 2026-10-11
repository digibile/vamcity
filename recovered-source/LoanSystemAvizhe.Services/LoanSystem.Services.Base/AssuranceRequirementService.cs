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

public class AssuranceRequirementService : BaseService, IAssuranceRequirementService
{
	public AssuranceRequirementService(AppDbContext context, ICurrentUserService currentUserService)
		: base(context, currentUserService)
	{
	}

	public Task<List<AssuranceRequirement>> GetByCondition(Expression<Func<AssuranceRequirement, bool>> condition, params string[] includs)
	{
		IQueryable<AssuranceRequirement> source = Table<AssuranceRequirement>();
		foreach (string navigationPropertyPath in includs)
		{
			source = source.Include(navigationPropertyPath);
		}
		source = source.Where(condition);
		return source.ToListAsync();
	}

	public async Task Insert(AssuranceRequirement entity)
	{
		entity.CustomerId = _currentUserService.CustomerId.Value;
		Insert<AssuranceRequirement>(entity);
		await Save();
	}

	public async Task Update(AssuranceRequirement entity)
	{
		entity.CustomerId = _currentUserService.CustomerId.Value;
		List<AssuranceRequirementItem> source = await (from x in _context.AssuranceRequirementItems.AsNoTracking()
			where x.AssuranceRequirementId == entity.Id
			select x).ToListAsync();
		List<Guid> currentItemIds = source.Select((AssuranceRequirementItem x) => x.Id).ToList();
		List<AssuranceRequirementItem> list = entity.AssuranceRequirementItems.Where((AssuranceRequirementItem x) => !currentItemIds.Contains(x.Id)).ToList();
		List<AssuranceRequirementItem> list2 = entity.AssuranceRequirementItems.Where((AssuranceRequirementItem x) => currentItemIds.Contains(x.Id)).ToList();
		List<AssuranceRequirementItem> list3 = source.Where((AssuranceRequirementItem x) => !entity.AssuranceRequirementItems.Select((AssuranceRequirementItem s) => s.Id).Contains(x.Id)).ToList();
		entity.AssuranceRequirementItems.Clear();
		Update(entity, true);
		if (list.Any())
		{
			_context.AssuranceRequirementItems.AddRange(list);
		}
		if (list2.Any())
		{
			_context.AssuranceRequirementItems.UpdateRange(list2);
		}
		if (list3.Any())
		{
			_context.AssuranceRequirementItems.RemoveRange(list3);
		}
		try
		{
			await Save();
		}
		catch (Exception)
		{
			throw;
		}
	}

	public Task Delete(AssuranceRequirement entity)
	{
		Delete<AssuranceRequirement>(entity);
		return Save();
	}
}
