using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Models;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Services.Base;

public interface ITransactionService
{
	Task<List<AgentFactorTransactionModel>> GetAgentFactorTransactionsListAsync(Guid? agentFactorId, long? agentId, DateTime? start, DateTime? end);

	Task<List<Transaction>> GetByCondition(Expression<Func<Transaction, bool>> condition, params string[] includs);

	Task<List<Transaction>> GetAllWithdraws(PaymentMethods? PaymentMethod);

	Task<GroupedTransactionReportModel> GetCountAndSumAgentReportByDateAsync(DateTime startDate, DateTime endDate);

	Task<Transaction> GetById(Guid entityId, params string[] includs);

	Task Insert(Transaction entity);

	Task Update(Transaction entity);

	Task Delete(Transaction entity);
}
