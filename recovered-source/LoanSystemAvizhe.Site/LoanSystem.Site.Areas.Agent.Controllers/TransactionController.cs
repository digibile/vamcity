using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoanSystem.Site.Areas.Agent.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Agent,Admin")]
public class TransactionController : BaseAgentController
{
	private readonly ICurrentUserService _currentUserService;

	private readonly ITransactionService _transactionService;

	public TransactionController(ILogger<UserLoanController> logger, ICurrentUserService currentUserService, ITransactionService transactionService)
		: base(logger)
	{
		_currentUserService = currentUserService;
		_transactionService = transactionService;
	}

	[HttpGet("[action]/{agentId?}/{userLoanId?}")]
	public Task<ResponseState<List<TransactionModel>>> SponsorePayments(long? agentId, Guid? userLoanId, DateTime? start, DateTime? end)
	{
		return TryCatch(async () =>
		{
			if (!agentId.HasValue)
			{
				agentId = _currentUserService.UserId;
			}
			List<Transaction> source = await _transactionService.GetByCondition((Transaction x) => (((DateTime?)start).HasValue ? (x.SaveDate.Date >= ((DateTime?)start).Value.Date) : true) && (((DateTime?)end).HasValue ? (x.SaveDate.Date <= ((DateTime?)end).Value.Date) : true) && x.AgentId == agentId && x.IsWithdraw, "Agent", "UserLoan.Loan", "UserLoan.User", "UserLoan.UserLoanRequest");
			if (userLoanId.HasValue)
			{
				source = source.Where((Transaction x) =>
				{
					Guid? userLoanId2 = x.UserLoanId;
					Guid? guid = userLoanId;
					if (userLoanId2.HasValue != guid.HasValue)
					{
						return false;
					}
					return !userLoanId2.HasValue || userLoanId2.GetValueOrDefault() == guid.GetValueOrDefault();
				}).ToList();
			}
			List<TransactionModel> data = (from x in source
				select new TransactionModel(x) into x
				orderby x.SaveDate descending
				select x).ToList();
			return Success(data);
		});
	}

	[HttpGet("[action]")]
	public Task<ResponseState<List<AgentFactorTransactionModel>>> GetAgentFactorTransactionsList(Guid? agentFactorId, long? agentId, DateTime? start, DateTime? end)
	{
		return TryCatch(async () =>
		{
			if (agentId.HasValue && agentId != _currentUserService.UserId)
			{
				return Error<List<AgentFactorTransactionModel>>(null, "دسترسی غیر مجاز");
			}
			if (!agentId.HasValue)
			{
				agentId = _currentUserService.UserId;
			}
			List<AgentFactorTransactionModel> data = await _transactionService.GetAgentFactorTransactionsListAsync(agentFactorId, agentId, start, end);
			return Success(data);
		});
	}
}
