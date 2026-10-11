using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoanSystem.Data;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Models;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Spm.Services.Infrastructure;

namespace LoanSystem.Services.Base;

public class TransactionService : BaseService, ITransactionService
{
	public TransactionService(AppDbContext context, ICurrentUserService currentUserService)
		: base(context, currentUserService)
	{
	}

	public async Task<List<AgentFactorTransactionModel>> GetAgentFactorTransactionsListAsync(Guid? agentFactorId, long? agentId, DateTime? start, DateTime? end)
	{
		List<Guid> withdrawIds = await (from x in Table<WithdrawAgentFactor>().Include((WithdrawAgentFactor x) => x.AgentFactor).Include((WithdrawAgentFactor x) => x.Withdraw).ThenInclude((Transaction x) => x.Agent)
			where (((DateTime?)start).HasValue ? (x.Withdraw.SaveDate.Date >= ((DateTime?)start).Value.Date) : true) && (((DateTime?)end).HasValue ? (x.Withdraw.SaveDate.Date <= ((DateTime?)end).Value.Date) : true) && (((long?)agentId).HasValue ? (x.Withdraw.AgentId == (long?)((long?)agentId).Value) : true) && (((Guid?)agentFactorId).HasValue ? (x.AgentFactorId == ((Guid?)agentFactorId).Value) : true)
			select x.WithdrawId).Distinct().ToListAsync();
		List<WithdrawAgentFactor> source = await (from x in Table<WithdrawAgentFactor>().Include((WithdrawAgentFactor x) => x.AgentFactor).Include((WithdrawAgentFactor x) => x.Withdraw).ThenInclude((Transaction x) => x.Agent)
			where withdrawIds.Contains(x.WithdrawId)
			select x).ToListAsync();
		List<AgentFactorTransactionModel> list = new List<AgentFactorTransactionModel>();
		foreach (Guid withdrawId in withdrawIds)
		{
			if (!list.Any((AgentFactorTransactionModel x) => x.Id == withdrawId))
			{
				WithdrawAgentFactor withdrawAgentFactor = source.FirstOrDefault((WithdrawAgentFactor x) => x.WithdrawId == withdrawId);
				AgentFactorTransactionModel agentFactorTransactionModel = new AgentFactorTransactionModel(withdrawAgentFactor.Withdraw);
				agentFactorTransactionModel.AgentFactors = (from x in source
					where x.WithdrawId.Equals(withdrawAgentFactor.WithdrawId)
					select new IdCode
					{
						Id = x.AgentFactor.Id,
						Code = x.AgentFactor.Code,
						Amount = x.AgentFactor.Amount
					}).ToList();
				list.Add(agentFactorTransactionModel);
			}
		}
		return list;
	}

	public async Task<GroupedTransactionReportModel> GetCountAndSumAgentReportByDateAsync(DateTime startDate, DateTime endDate)
	{
		int count = await (from x in Table<Transaction>()
			where x.SaveDate.Date >= startDate && x.SaveDate.Date <= endDate && x.CustomerId == _currentUserService.CustomerId && x.Amount > 0m && x.AgentId != (long?)null
			select x).CountAsync();
		decimal sum = await (from x in Table<Transaction>()
			where x.SaveDate.Date >= startDate && x.SaveDate.Date <= endDate && x.CustomerId == _currentUserService.CustomerId && x.Amount > 0m && x.AgentId != (long?)null
			select x).SumAsync((Transaction x) => x.Amount);
		return new GroupedTransactionReportModel
		{
			Count = count,
			Sum = sum
		};
	}

	public Task<List<Transaction>> GetByCondition(Expression<Func<Transaction, bool>> condition, params string[] includs)
	{
		IQueryable<Transaction> source = Table<Transaction>();
		foreach (string navigationPropertyPath in includs)
		{
			source = source.Include(navigationPropertyPath);
		}
		source = source.Where(condition);
		return source.ToListAsync();
	}

	public Task<List<Transaction>> GetAllWithdraws(PaymentMethods? PaymentMethod)
	{
		IQueryable<Transaction> source = Table<Transaction>();
		if (PaymentMethod.HasValue)
		{
			source = source.Where((Transaction x) => (int?)x.PaymentMethod == (int?)PaymentMethod);
		}
		return source.ToListAsync();
	}

	public Task<Transaction> GetById(Guid entityId, params string[] includs)
	{
		return GetById<Transaction>(entityId, includs);
	}

	public Task Insert(Transaction entity)
	{
		entity.CustomerId = _currentUserService.CustomerId.Value;
		Insert<Transaction>(entity);
		return Save();
	}

	public Task Update(Transaction entity)
	{
		entity.CustomerId = _currentUserService.CustomerId.Value;
		Update(entity, true);
		return Save();
	}

	public Task Delete(Transaction entity)
	{
		Delete<Transaction>(entity);
		return Save();
	}
}
