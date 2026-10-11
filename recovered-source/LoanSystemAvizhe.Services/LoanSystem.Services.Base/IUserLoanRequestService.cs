using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using Spm.Services.Infrastructure;

namespace LoanSystem.Services.Base;

public interface IUserLoanRequestService
{
	Task<List<UserLoanRequest>> GetAllUserLoanRequests(params string[] includs);

	Task<UserLoanRequest> GetById(Guid entityId, params string[] includs);

	Task<List<UserLoanRequest>> GetByUserId(long userId, params string[] includs);

	Task<List<UserLoanRequest>> GetByAgentId(long agentId, params string[] includs);

	Task RunInTransaction(Func<DbTransaction, Task> action);

	Task Insert(UserLoanRequest entity);

	Task Update(UserLoanRequest entity);

	Task Delete(UserLoanRequest entity);

	Task<List<UserLoanRequest>> GetByCondition(Expression<Func<UserLoanRequest, bool>> condition, bool asTracking = false, params string[] includs);

	Task<int> GetRequestCountByDateAsync(DateTime startDate, DateTime endDate);
}
