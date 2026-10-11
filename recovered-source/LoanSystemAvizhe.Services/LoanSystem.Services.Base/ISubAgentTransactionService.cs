using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Services.Base;

public interface ISubAgentTransactionService
{
	Task<List<SubAgentTransaction>> GetByCondition(Expression<Func<SubAgentTransaction, bool>> condition, params string[] includs);

	Task<List<SubAgentTransaction>> GetAllWithdraws(PaymentMethods? PaymentMethod);

	Task<SubAgentTransaction> GetById(Guid entityId, params string[] includs);

	Task Insert(SubAgentTransaction entity);

	Task Update(SubAgentTransaction entity);

	Task Delete(SubAgentTransaction entity);
}
