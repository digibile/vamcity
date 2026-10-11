using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Data.Enums;
using LoanSystemAvizhe.Services.Facades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoanSystem.Site.Areas.Agent.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Agent,Admin")]
public class UserLoanController : BaseAgentController
{
	private readonly IUserLoanService _userLoanService;

	private readonly IUserLoanRequestService _userLoanRequestService;

	private readonly IUserPaymentService _userPaymentService;

	private readonly ICurrentUserService _currentUserService;

	private readonly ITransactionService _transactionService;

	private readonly IUserLoanFacade _userLoanFacade;

	private readonly ISubAgentTransactionService _subAgentTransactionService;

	public UserLoanController(ILogger<UserLoanController> logger, IUserLoanService userLoanService, IUserPaymentService userPaymentService, ICurrentUserService currentUserService, IUserLoanRequestService userLoanRequestService, ISubAgentTransactionService subAgentTransactionService, ITransactionService transactionService, IUserLoanFacade userLoanFacade)
		: base(logger)
	{
		_userLoanService = userLoanService;
		_userPaymentService = userPaymentService;
		_currentUserService = currentUserService;
		_userLoanRequestService = userLoanRequestService;
		_transactionService = transactionService;
		_userLoanFacade = userLoanFacade;
		_subAgentTransactionService = subAgentTransactionService;
	}

	[HttpGet("[action]/{id}")]
	public Task<ResponseState<UserLoanModel>> Get(Guid id)
	{
		return TryCatch(async () =>
		{
			FacadeResponse<UserLoanModel> facadeResponse = await _userLoanFacade.Get(id);
			return (!facadeResponse.Success) ? Error<UserLoanModel>(null, facadeResponse.Message) : Success(facadeResponse.Data);
		});
	}

	[HttpGet("[action]/{id}")]
	public Task<ResponseState<dynamic>> GetCheckoutAmount(Guid id)
	{
		return TryCatch(async () =>
		{
			if (id == Guid.Empty)
			{
				return Error<object>(null, "شناسه وام معتبر نمی باشد.");
			}
			FacadeResponse<(decimal, decimal)> facadeResponse = await _userLoanFacade.GetCheckoutAmount(id);
			return (!facadeResponse.Success) ? Error<object>(null, facadeResponse.Message) : Success((object)new
			{
				item1 = facadeResponse.Data.Item1,
				item2 = facadeResponse.Data.Item2
			}, "");
		});
	}

	[HttpGet("[action]/{id}")]
	public Task<ResponseState<List<UserLoanModel>>> GetLoansByAgentId(long id)
	{
		return TryCatch(async () =>
		{
			List<UserLoanModel> models = new List<UserLoanModel>();
			if (id == 0L)
			{
				return Error<List<UserLoanModel>>(null, "کاربر یافت نشد");
			}
			List<UserLoan> userLoans = await _userLoanService.GetByCondition((UserLoan x) => x.SubAgentId == (long?)id || x.AgentId == (long?)id, "Loan", "User", "UserLoanPayments", "Agent");
			List<Transaction> payments = await _transactionService.GetByCondition((Transaction x) => x.UserLoanId.HasValue && x.AgentId.HasValue && userLoans.Select((UserLoan ul) => ul.Id).ToList().Contains((Guid)x.UserLoanId));
			List<SubAgentTransaction> subAgentPayments = await _subAgentTransactionService.GetByCondition((SubAgentTransaction x) => x.UserLoanId.HasValue && userLoans.Select((UserLoan ul) => ul.Id).ToList().Contains((Guid)x.UserLoanId));
			foreach (UserLoan userLoan in userLoans)
			{
				UserLoanModel model = new UserLoanModel
				{
					Id = userLoan.Id,
					Code = userLoan.Code,
					AgentModel = new AgentModel(userLoan.Agent),
					LoanModel = new Loan2Model
					{
						Id = userLoan.Loan.Id,
						Title = userLoan.Loan.Title,
						DelayPercent = userLoan.Loan.DelayPercent,
						EarlyPercent = userLoan.Loan.EarlyPercent,
						ProfitPercent = userLoan.Loan.ProfitPercent,
						AllowedInstallments = userLoan.Loan.AllowedInstallments,
						Disable = userLoan.Loan.Disable,
						DisableText = userLoan.Loan.DisableText,
						IsSponsored = userLoan.Loan.IsSponsored
					},
					MemberModel = new Member2Model(userLoan.User),
					PayableAmount = userLoan.PayableAmount,
					Amount = userLoan.Amount,
					Installment = userLoan.Installment,
					Month = userLoan.Month,
					MonthGap = userLoan.MonthGap,
					Paid = userLoan.Paid,
					StartDate = userLoan.StartDate,
					PaidCount = userLoan.UserLoanPayments.Count((UserLoanPayment x) => x.SumAmount > 0m),
					IsCheckouted = userLoan.IsCheckouted,
					CheckoutDate = userLoan.CheckoutDate,
					Description = userLoan.Description,
					LoanPayments = userLoan.UserLoanPayments.Select((UserLoanPayment p) => new LoanPaymentModel(p)).ToList(),
					AgentPrice = userLoan.AgentPrice,
					AgentDueDate = userLoan.AgentDueDate,
					SubAgentPrice = userLoan.SubAgentPrice,
					SubAgentDueDate = userLoan.SubAgentDueDate,
					SumSponsorPayments = payments.Where((Transaction pay) => pay.UserLoanId == userLoan.Id).Sum((Transaction p) => p.Amount),
					SumParentAgentPayments = subAgentPayments.Where((SubAgentTransaction pay) => pay.UserLoanId == userLoan.Id).Sum((SubAgentTransaction p) => p.Amount)
				};
				await _userPaymentService.SetPaymentsDelays(model.LoanPayments);
				models.Add(model);
			}
			return Success(models);
		});
	}

	[HttpGet("[action]/{id}")]
	public Task<ResponseState<List<UserLoanRequestModel>>> GetLoansByUserId(long id)
	{
		return TryCatch(async () =>
		{
			List<UserLoanRequestModel> models = new List<UserLoanRequestModel>();
			if (id == 0L)
			{
				return Error<List<UserLoanRequestModel>>(null, "کاربر یافت نشد");
			}
			List<UserLoanRequest> requests = (await _userLoanRequestService.GetByUserId(id, "Loan", "User", "Status")).Where((UserLoanRequest x) => x.AgentId == _currentUserService.UserId).ToList();
			if (!requests.Any())
			{
				return Success(models);
			}
			List<UserLoan> userLoans = await _userLoanService.GetByUserId(id, "Loan", "User", "UserLoanPayments");
			foreach (UserLoanRequest request in requests.OrderByDescending((UserLoanRequest x) => x.RequestDate))
			{
				UserLoanRequestModel model = new UserLoanRequestModel(request);
				UserLoan userLoan = userLoans.FirstOrDefault((UserLoan x) => x.UserLoanRequestId == request.Id);
				if (userLoan != null)
				{
					model.UserLoan = new UserLoanModel
					{
						Id = userLoan.Id,
						Code = userLoan.Code,
						LoanModel = new Loan2Model
						{
							Id = userLoan.Loan.Id,
							Title = userLoan.Loan.Title,
							DelayPercent = userLoan.Loan.DelayPercent,
							EarlyPercent = userLoan.Loan.EarlyPercent,
							ProfitPercent = userLoan.Loan.ProfitPercent,
							AllowedInstallments = userLoan.Loan.AllowedInstallments
						},
						MemberModel = new Member2Model(userLoan.User),
						PayableAmount = userLoan.PayableAmount,
						Amount = userLoan.Amount,
						Installment = userLoan.Installment,
						Month = userLoan.Month,
						MonthGap = userLoan.MonthGap,
						Paid = userLoan.Paid,
						StartDate = userLoan.StartDate,
						PaidCount = userLoan.UserLoanPayments.Count((UserLoanPayment x) => x.SumAmount > 0m),
						IsCheckouted = userLoan.IsCheckouted,
						CheckoutDate = userLoan.CheckoutDate,
						Description = userLoan.Description,
						LoanPayments = userLoan.UserLoanPayments.Select((UserLoanPayment p) => new LoanPaymentModel(p)).ToList(),
						AgentPrice = userLoan.AgentPrice
					};
					await _userPaymentService.SetPaymentsDelays(model.UserLoan.LoanPayments);
				}
				models.Add(model);
			}
			return Success(models);
		});
	}

	[HttpGet("[action]")]
	public Task<ResponseState<List<UserLoanModel>>> SubAgentsOverDueList()
	{
		return TryCatch(async () =>
		{
			long? agentId = _currentUserService.UserId;
			List<UserLoan> entities = await _userLoanService.GetByCondition((UserLoan x) => x.AgentId == agentId && x.SubAgentId.HasValue, "Loan", "User", "SubAgent");
			List<SubAgentTransaction> withdrawUserLoans = await _subAgentTransactionService.GetByCondition((SubAgentTransaction x) => x.IsWithdraw && x.UserLoan.AgentId == _currentUserService.UserId && x.UserLoan.SubAgentId.HasValue, "UserLoan");
			withdrawUserLoans.Select((SubAgentTransaction x) => x.UserLoanId).ToList();
			entities = entities.Where((UserLoan userLoan) => userLoan.AgentDueDate <= DateTime.Now && withdrawUserLoans.Where((SubAgentTransaction w) => w.UserLoanId == userLoan.Id).Sum((SubAgentTransaction w) => w.Amount) < userLoan.SubAgentPrice).ToList();
			List<UserLoanModel> list = entities.Select((UserLoan x) => new UserLoanModel(x)).ToList();
			list.ForEach((UserLoanModel userLoan) =>
			{
				userLoan.SumParentAgentPayments = withdrawUserLoans.Where((SubAgentTransaction pay) => pay.IsWithdraw && pay.UserLoanId == userLoan.Id).Sum((SubAgentTransaction p) => p.Amount);
			});
			return Success(list);
		});
	}

	[HttpGet("[action]")]
	public Task<ResponseState<List<UserLoanModel>>> SponsoreOverDueList()
	{
		return TryCatch(async () =>
		{
			long? agentId = _currentUserService.UserId;
			List<UserLoan> entities = await _userLoanService.GetByCondition((UserLoan x) => x.AgentId == agentId, "Loan", "User", "SubAgent");
			List<Transaction> withdrawUserLoans = await _transactionService.GetByCondition((Transaction x) => x.IsWithdraw && x.AgentId.HasValue && x.UserLoanId.HasValue);
			withdrawUserLoans.Select((Transaction x) => x.UserLoanId).ToList();
			entities = entities.Where((UserLoan userLoan) =>
			{
				DateTime? agentDueDate = userLoan.AgentDueDate;
				DateTime dateTime = DateTime.Now.AddDays(1.0);
				return agentDueDate.HasValue && agentDueDate.GetValueOrDefault() <= dateTime && userLoan.AgentId.HasValue && withdrawUserLoans.Where((Transaction w) => w.UserLoanId == userLoan.Id).Sum((Transaction w) => w.Amount) < userLoan.AgentPrice;
			}).ToList();
			List<UserLoanModel> list = entities.Select((UserLoan x) => new UserLoanModel(x)).ToList();
			list.ForEach((UserLoanModel userLoan) =>
			{
				userLoan.SumSponsorPayments = withdrawUserLoans.Where((Transaction pay) => pay.IsWithdraw && pay.UserLoanId == userLoan.Id && pay.AgentId.HasValue).Sum((Transaction p) => p.Amount);
				userLoan.ChargeType = userLoan.UserLoanRequest?.ChargeType ?? AgentLoanRequestChargeTypes.پول_نقد;
			});
			return Success(list);
		});
	}
}
