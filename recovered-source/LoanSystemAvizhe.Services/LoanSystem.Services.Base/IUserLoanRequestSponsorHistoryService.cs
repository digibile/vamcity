using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Base;

public interface IUserLoanRequestSponsorHistoryService
{
	Task<int> GetHistoryCount(Guid userLoanRequestId);

	Task<List<UserLoanRequestSponsorHistory>> GetByUserLoanRequestId(Guid id, params string[] includs);

	Task<List<UserLoanRequestSponsorHistory>> GetByCondition(Expression<Func<UserLoanRequestSponsorHistory, bool>> condition, params string[] includs);

	Task Insert(UserLoanRequestSponsorHistory entity);

	Task Update(UserLoanRequestSponsorHistory entity);

	Task Delete(UserLoanRequestSponsorHistory entity);
}
