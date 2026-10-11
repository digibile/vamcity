using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Base;

public interface IUserLoanRequestStatusHistoryService
{
	Task<int> GetCountByCondition(Expression<Func<UserLoanRequestStatusHistory, bool>> condition);

	Task<List<UserLoanRequestStatusHistory>> GetAll(bool include);

	Task<UserLoanRequestStatusHistory> GetById(Guid entityId, bool include);

	Task<List<UserLoanRequestStatusHistory>> GetByUserId(long userId);

	Task<List<UserLoanRequestStatusHistory>> GetByUserLoanRequestId(Guid userLoanRequestId);

	Task<int> CountByUserLoanRequestId(Guid userLoanRequestId);

	Task Insert(UserLoanRequestStatusHistory entity);

	Task Update(UserLoanRequestStatusHistory entity);

	Task Delete(UserLoanRequestStatusHistory entity);
}
