using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoanSystem.Data;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Models;
using LoanSystemAvizhe.Core;
using Microsoft.EntityFrameworkCore;
using Spm.Services.Infrastructure;

namespace LoanSystem.Services.Base;

public class LoanAssuranceRequirementService : BaseService, ILoanAssuranceRequirementService
{
	public LoanAssuranceRequirementService(AppDbContext context, ICurrentUserService currentUserService)
		: base(context, currentUserService)
	{
	}

	public Task<List<LoanAssuranceRequirement>> GetByCondition(Expression<Func<LoanAssuranceRequirement, bool>> condition, params string[] includs)
	{
		IQueryable<LoanAssuranceRequirement> source = Table<LoanAssuranceRequirement>();
		foreach (string navigationPropertyPath in includs)
		{
			source = source.Include(navigationPropertyPath);
		}
		source = source.Where(condition);
		return source.ToListAsync();
	}

	public async Task Insert(LoanAssuranceRequirement entity)
	{
		entity.CustomerId = _currentUserService.CustomerId.Value;
		Insert<LoanAssuranceRequirement>(entity);
		await Save();
	}

	public Task Update(LoanAssuranceRequirement entity)
	{
		entity.CustomerId = _currentUserService.CustomerId.Value;
		Update(entity, true);
		return Save();
	}

	public Task Delete(LoanAssuranceRequirement entity)
	{
		Delete<LoanAssuranceRequirement>(entity);
		return Save();
	}

	public async Task<List<Guid>> GetMatchLoanIdsByAssuranceRequirement(Dictionary<Guid, Guid?> dic)
	{
		List<LoanAssuranceRequirementModel> list = (await Table<LoanAssuranceRequirement>().ToListAsync()).Select((LoanAssuranceRequirement x) => new LoanAssuranceRequirementModel(x)).ToList();
		List<Guid> list2 = new List<Guid>();
		foreach (LoanAssuranceRequirementModel item in list)
		{
			bool flag = false;
			foreach (KeyValuePair<Guid, List<Guid?>> assuranceRequirementItemId in item.AssuranceRequirementItemIds)
			{
				List<Guid?> value = assuranceRequirementItemId.Value;
				if (value != null && value.Any() && (!dic.TryGetValue(assuranceRequirementItemId.Key, out var value2) || !value2.HasValue || !assuranceRequirementItemId.Value.Contains(value2.Value)))
				{
					flag = true;
				}
			}
			if (!flag)
			{
				list2.Add(item.LoanId);
			}
		}
		return list2.Distinct().ToList();
	}
}
