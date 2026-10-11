using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Models;

namespace LoanSystem.Services.Base;

public interface IUserLoanService
{
	Task<List<UserLoan>> GetByCondition(Expression<Func<UserLoan, bool>> condition, params string[] includs);

	IQueryable<UserLoan> GetAllAsQueryable();

	Task<List<UserLoan>> GetAllUserLoans(params string[] includs);

	Task<List<UserLoan>> GetUserLoansHasAgent(params string[] includs);

	Task<List<UserLoan>> Search(string search);

	Task<List<UserLoan>> GetNotCheckoutUserLoans();

	Task<UserLoan> GetById(Guid entityId, params string[] includs);

	Task<UserLoan> GetByIdNoTrack(Guid entityId, params string[] includs);

	Task<List<UserLoan>> GetByUserId(long userId, params string[] includs);

	Task<List<UserLoan>> GetByAgentId(long agentId, params string[] includs);

	Task<List<UserLoan>> GetByUserLoanRequestId(Guid userLoanRequestId, params string[] includs);

	Task<int> GetLoanCountByDateAsync(DateTime startDate, DateTime endDate);

	Task SetAppSms(List<UserLoanModel> userLoans, bool ignoreText);

	Task<(decimal, decimal)> GetCheckoutAmount(Guid id);

	Task Insert(UserLoan entity);

	Task Update(UserLoan entity);

	Task Delete(UserLoan entity);
}
