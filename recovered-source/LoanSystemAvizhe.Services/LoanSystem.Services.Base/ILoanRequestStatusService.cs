using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Base;

public interface ILoanRequestStatusService
{
	Task<List<LoanRequestStatus>> GetAllLoanRequestStatuses(params string[] includs);

	Task<LoanRequestStatus> GetById(Guid entityId, params string[] includs);

	Task Insert(LoanRequestStatus entity);

	Task InsertBatch(List<LoanRequestStatus> entities);

	Task Update(LoanRequestStatus entity);

	Task Delete(LoanRequestStatus entity);

	Task<List<LoanRequestStatus>> GetByCondition(Expression<Func<LoanRequestStatus, bool>> condition, params string[] includs);
}
