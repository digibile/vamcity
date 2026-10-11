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

public class UserLoanContactService : BaseService, IUserLoanContactService
{
	public UserLoanContactService(AppDbContext context, ICurrentUserService currentUserService)
		: base(context, currentUserService)
	{
	}

	public Task<List<UserLoanContact>> GetByCondition(Expression<Func<UserLoanContact, bool>> condition, params string[] includs)
	{
		IQueryable<UserLoanContact> source = Table<UserLoanContact>();
		foreach (string navigationPropertyPath in includs)
		{
			source = source.Include(navigationPropertyPath);
		}
		source = source.Where(condition);
		return source.ToListAsync();
	}

	public async Task Insert(UserLoanContact entity)
	{
		entity.CustomerId = _currentUserService.CustomerId.Value;
		Insert<UserLoanContact>(entity);
		await Save();
	}

	public Task Update(UserLoanContact entity)
	{
		entity.CustomerId = _currentUserService.CustomerId.Value;
		Update(entity, true);
		return Save();
	}

	public Task Delete(UserLoanContact entity)
	{
		Delete<UserLoanContact>(entity);
		return Save();
	}
}
