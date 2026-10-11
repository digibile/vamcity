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

public class UserLoanRequestAuthorizationItemAttachmentService : BaseService, IUserLoanRequestAuthorizationItemAttachmentService
{
	public UserLoanRequestAuthorizationItemAttachmentService(AppDbContext context, ICurrentUserService currentUserService)
		: base(context, currentUserService)
	{
	}

	public Task<UserLoanRequestAuthorizationItemAttachment> GetById(Guid id, params string[] includs)
	{
		return GetById<UserLoanRequestAuthorizationItemAttachment>(id, includs);
	}

	public Task<List<UserLoanRequestAuthorizationItemAttachment>> GetByCondition(Expression<Func<UserLoanRequestAuthorizationItemAttachment, bool>> condition, params string[] includs)
	{
		IQueryable<UserLoanRequestAuthorizationItemAttachment> source = Table<UserLoanRequestAuthorizationItemAttachment>();
		foreach (string navigationPropertyPath in includs)
		{
			source = source.Include(navigationPropertyPath);
		}
		source = source.Where(condition);
		return source.ToListAsync();
	}

	public async Task Insert(UserLoanRequestAuthorizationItemAttachment entity)
	{
		entity.CustomerId = _currentUserService.CustomerId.Value;
		Insert<UserLoanRequestAuthorizationItemAttachment>(entity);
		await Save();
	}

	public async Task InsertBatch(List<UserLoanRequestAuthorizationItemAttachment> entities)
	{
		entities.ForEach((UserLoanRequestAuthorizationItemAttachment x) =>
		{
			x.CustomerId = _currentUserService.CustomerId.Value;
		});
		_context.UserLoanRequestAuthorizationItemAttachments.AddRange(entities);
		await Save();
	}

	public Task Update(UserLoanRequestAuthorizationItemAttachment entity)
	{
		entity.CustomerId = _currentUserService.CustomerId.Value;
		Update(entity, true);
		return Save();
	}

	public Task Delete(UserLoanRequestAuthorizationItemAttachment entity)
	{
		Delete<UserLoanRequestAuthorizationItemAttachment>(entity);
		return Save();
	}
}
