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

public class LoanService : BaseService, ILoanService
{
	public LoanService(AppDbContext context, ICurrentUserService currentUserService)
		: base(context, currentUserService)
	{
	}

	public IQueryable<Loan> GetAllLoansAsQueryable()
	{
		return Table<Loan>();
	}

	public Task<List<Loan>> GetAllLoans(params string[] includs)
	{
		IQueryable<Loan> source = Table<Loan>();
		foreach (string navigationPropertyPath in includs)
		{
			source = source.Include(navigationPropertyPath);
		}
		return source.ToListAsync();
	}

	public Task<List<Loan>> GetByCustomerId(Guid customerId, params string[] includs)
	{
		IQueryable<Loan> source = Table<Loan>();
		foreach (string navigationPropertyPath in includs)
		{
			source = source.Include(navigationPropertyPath);
		}
		return source.IgnoreAutoIncludes().ToListAsync();
	}

	public Task<Loan> GetById(Guid entityId, params string[] includs)
	{
		return GetById<Loan>(entityId, includs);
	}

	public Task<Loan> GetByTitle(string title)
	{
		return Table<Loan>().FirstOrDefaultAsync((Loan x) => x.Title == title);
	}

	public Task<Loan> GetByCode(int code)
	{
		return Table<Loan>().FirstOrDefaultAsync((Loan x) => x.Code == code);
	}

	public Task Insert(Loan entity)
	{
		entity.CustomerId = _currentUserService.CustomerId.Value;
		Insert<Loan>(entity);
		return Save();
	}

	public async Task Update(Loan entity)
	{
		entity.CustomerId = _currentUserService.CustomerId.Value;
		List<LoanTag> currentItems = await (from x in Table<LoanTag>()
			where x.LoanId == entity.Id
			select x).ToListAsync();
		List<LoanTag> list = (from loan_tag in entity.LoanTags
			where !currentItems.Select((LoanTag t) => t.TagId).ToList().Contains(loan_tag.TagId)
			select new LoanTag
			{
				LoanId = entity.Id,
				TagId = loan_tag.TagId
			}).ToList();
		List<LoanTag> list2 = currentItems.Where((LoanTag x) => !entity.LoanTags.Select((LoanTag lt) => lt.TagId).ToList().Contains(x.TagId)).ToList();
		if (list.Any())
		{
			_context.Set<LoanTag>().AddRange(list);
		}
		if (list2.Any())
		{
			_context.Set<LoanTag>().RemoveRange(list2);
		}
		entity.LoanTags.Clear();
		Update(entity, true);
		await Save();
	}

	public Task Delete(Loan entity)
	{
		Delete<Loan>(entity);
		return Save();
	}
}
