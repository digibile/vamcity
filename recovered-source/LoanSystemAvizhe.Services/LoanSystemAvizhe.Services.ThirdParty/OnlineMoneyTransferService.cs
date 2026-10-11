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

public class OnlineMoneyTransferService : BaseService, IOnlineMoneyTransferService
{
	public OnlineMoneyTransferService(AppDbContext context, ICurrentUserService currentUserService)
		: base(context, currentUserService)
	{
	}

	public Task<List<OnlineMoneyTransfer>> GetByCondition(Expression<Func<OnlineMoneyTransfer, bool>> condition, params string[] includs)
	{
		IQueryable<OnlineMoneyTransfer> source = Table<OnlineMoneyTransfer>();
		foreach (string navigationPropertyPath in includs)
		{
			source = source.Include(navigationPropertyPath);
		}
		source = source.Where(condition);
		return source.ToListAsync();
	}

	public Task<OnlineMoneyTransfer> GetById(Guid entityId, params string[] includs)
	{
		return GetById<OnlineMoneyTransfer>(entityId, includs);
	}

	public Task Insert(OnlineMoneyTransfer entity)
	{
		entity.CustomerId = _currentUserService.CustomerId.Value;
		Insert<OnlineMoneyTransfer>(entity);
		return Save();
	}

	public Task Update(OnlineMoneyTransfer entity)
	{
		entity.CustomerId = _currentUserService.CustomerId.Value;
		Update(entity, true);
		return Save();
	}

	public Task Delete(OnlineMoneyTransfer entity)
	{
		Delete<OnlineMoneyTransfer>(entity);
		return Save();
	}
}
