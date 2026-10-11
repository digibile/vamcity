using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Base;

public interface ILoanAuthorizationItemService
{
	Task<List<LoanAuthorizationItem>> GetByCondition(Expression<Func<LoanAuthorizationItem, bool>> condition, params string[] includs);

	Task Insert(LoanAuthorizationItem entity);

	Task Update(LoanAuthorizationItem entity);

	Task Delete(LoanAuthorizationItem entity);
}
