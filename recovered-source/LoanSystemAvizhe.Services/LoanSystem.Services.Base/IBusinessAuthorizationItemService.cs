using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Base;

public interface IBusinessAuthorizationItemService
{
	Task<List<BusinessAuthorizationItem>> GetByCondition(Expression<Func<BusinessAuthorizationItem, bool>> condition, params string[] includs);

	Task Insert(BusinessAuthorizationItem entity);

	Task Update(BusinessAuthorizationItem entity);

	Task Delete(BusinessAuthorizationItem entity);
}
