using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Data;
using LoanSystem.Data.Entities;
using LoanSystemAvizhe.Core;
using Microsoft.EntityFrameworkCore;
using Spm.Services.Infrastructure;

namespace LoanSystem.Services.Base;

public class CustomerOrderService : BaseService, ICustomerOrderService
{
	public CustomerOrderService(AppDbContext context, ICurrentUserService currentUserService)
		: base(context, currentUserService)
	{
	}

	public Task<List<CustomerOrder>> GetAll(bool? isSeen)
	{
		IQueryable<CustomerOrder> source = Table<CustomerOrder>();
		if (isSeen.HasValue)
		{
			source = source.Where((CustomerOrder x) => (bool?)x.IsSeen == isSeen);
		}
		return source.ToListAsync();
	}

	public Task<int> GetCount(bool? isSeen)
	{
		IQueryable<CustomerOrder> source = Table<CustomerOrder>();
		if (isSeen.HasValue)
		{
			source = source.Where((CustomerOrder x) => (bool?)x.IsSeen == isSeen);
		}
		return source.CountAsync();
	}

	public Task<CustomerOrder> GetById(Guid entityId)
	{
		return GetById<CustomerOrder>(entityId);
	}

	public Task Insert(CustomerOrder entity)
	{
		entity.CustomerId = _currentUserService.CustomerId.Value;
		Insert<CustomerOrder>(entity);
		return Save();
	}

	public Task Update(CustomerOrder entity)
	{
		entity.CustomerId = _currentUserService.CustomerId.Value;
		Update(entity, true);
		return Save();
	}

	public Task Delete(CustomerOrder entity)
	{
		Delete<CustomerOrder>(entity);
		return Save();
	}
}
