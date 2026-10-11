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

public class AssuranceService : BaseService, IAssuranceService
{
	public AssuranceService(AppDbContext context, ICurrentUserService currentUserService)
		: base(context, currentUserService)
	{
	}

	public Task<List<Assurance>> GetAllAssurances()
	{
		return Table<Assurance>().ToListAsync();
	}

	public Task<List<Assurance>> GetByUserLoanId(Guid userLoanId)
	{
		return (from x in Table<Assurance>().Include((Assurance x) => x.UserLoan).Include((Assurance x) => x.Organization).Include((Assurance x) => x.AssuranceChequeItems)
			where x.UserLoanId == userLoanId
			select x).ToListAsync();
	}

	public Task<Assurance> GetById(Guid entityId, params string[] includs)
	{
		return GetById<Assurance>(entityId, includs);
	}

	public new Task RunInTransaction(Func<DbTransaction, Task> action)
	{
		return base.RunInTransaction(action);
	}

	public Task Insert(Assurance entity)
	{
		Insert<Assurance>(entity);
		return Save();
	}

	public Task Update(Assurance entity)
	{
		Update(entity, true);
		return Save();
	}

	public Task Delete(Assurance entity)
	{
		Delete<Assurance>(entity);
		return Save();
	}
}
