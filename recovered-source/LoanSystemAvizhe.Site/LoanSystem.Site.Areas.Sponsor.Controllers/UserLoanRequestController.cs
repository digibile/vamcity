using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Services.Users;
using LoanSystem.Site.Areas.Admin.Models;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Data.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LoanSystem.Site.Areas.Sponsor.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Sponsor")]
public class UserLoanRequestController : BaseSponsorController
{
	private readonly IAgentLoanService _agentLoanService;

	private readonly ILoanService _loanService;

	private readonly IUserLoanRequestService _userLoanRequestService;

	private readonly IUserLoanService _userLoanService;

	private readonly ITransactionService _transactionService;

	private readonly ISubAgentTransactionService _subAgentTransactionService;

	private readonly IAppContext _appContext;

	private readonly ICurrentUserService _currentUserService;

	private readonly IUserLoanRequestAuthorizationItemService _userLoanRequestAuthorizationItemService;

	private readonly ILoanRequestStatusService _loanRequestStatusService;

	private readonly IUserLoanRequestSponsorHistoryService _userLoanRequestSponsorHistoryService;

	private readonly UserManager _userManager;

	private readonly IntegratedSmsSenderService _integratedSmsSenderService;

	private readonly IntegratedEmailSenderService _integratedEmailSenderService;

	public UserLoanRequestController(ILogger<UserLoanRequestController> logger, IUserLoanRequestService userLoanRequestService, IUserLoanService userLoanService, ILoanService loanService, IAppContext appContext, ICurrentUserService currentUserService, IAgentLoanService agentLoanService, ITransactionService transactionService, IUserLoanRequestAuthorizationItemService userLoanRequestAuthorizationItemService, ISubAgentTransactionService subAgentTransactionService, ILoanRequestStatusService loanRequestStatusService, IUserLoanRequestSponsorHistoryService userLoanRequestSponsorHistoryService, IntegratedSmsSenderService integratedSmsSenderService, IntegratedEmailSenderService integratedEmailSenderService, UserManager userManager)
		: base(logger)
	{
		_userLoanRequestService = userLoanRequestService;
		_appContext = appContext;
		_currentUserService = currentUserService;
		_loanService = loanService;
		_userLoanRequestAuthorizationItemService = userLoanRequestAuthorizationItemService;
		_agentLoanService = agentLoanService;
		_userLoanService = userLoanService;
		_transactionService = transactionService;
		_subAgentTransactionService = subAgentTransactionService;
		_loanRequestStatusService = loanRequestStatusService;
		_userLoanRequestSponsorHistoryService = userLoanRequestSponsorHistoryService;
		_integratedSmsSenderService = integratedSmsSenderService;
		_integratedEmailSenderService = integratedEmailSenderService;
		_userManager = userManager;
	}

	[HttpGet("[action]/{id}")]
	public Task<ResponseState<UserLoanRequestModel>> Get(Guid id)
	{
		return TryCatch(async () =>
		{
			if (_appContext.User == null)
			{
				return Error<UserLoanRequestModel>(null, "کاربر یافت نشد");
			}
			if (id == Guid.Empty)
			{
				return Success(new UserLoanRequestModel());
			}
			UserLoanRequest userLoanRequest = await _userLoanRequestService.GetById(id, "Loan", "User", "Status", "AgentStatus");
			if (userLoanRequest == null)
			{
				return Error<UserLoanRequestModel>(null, "اطلاعات مورد نظر یافت نشد");
			}
			if (userLoanRequest.SponsorId != _appContext.User.Id)
			{
				return Error<UserLoanRequestModel>(null, "دسترسی به این درخواست برای شما مجاز نمی باشد.");
			}
			UserLoanRequestModel model = new UserLoanRequestModel(userLoanRequest);
			List<UserLoan> list = await _userLoanService.GetByUserLoanRequestId(id);
			if (list != null && list.Any())
			{
				model.UserLoan = new UserLoanModel(list.First());
			}
			return Success(model);
		});
	}

	[HttpGet("[action]")]
	public Task<ResponseState<List<UserLoanRequestModel>>> List(DateTime? start, DateTime? end)
	{
		return TryCatch(async () =>
		{
			if (_appContext.User == null)
			{
				return Error<List<UserLoanRequestModel>>(null, "کاربر یافت نشد");
			}
			List<UserLoanRequest> entities = await _userLoanRequestService.GetByCondition((UserLoanRequest x) => (((DateTime?)start).HasValue ? (x.RequestDate.Date >= ((DateTime?)start).Value.Date) : true) && (((DateTime?)end).HasValue ? (x.RequestDate.Date <= ((DateTime?)end).Value.Date) : true) && x.SponsorId == (long?)_appContext.User.Id, false, "User", "Loan");
			List<UserLoan> userLoans = await _userLoanService.GetByCondition((UserLoan x) => x.UserLoanRequestId.HasValue && entities.Select((UserLoanRequest e) => e.Id).ToList().Contains((Guid)x.UserLoanRequestId), "Loan");
			List<UserLoanRequestModel> data = (from x in entities
				select new UserLoanRequestModel(x)
				{
					UserLoan = (userLoans.Any((UserLoan ul) => ul.UserLoanRequestId == x.Id) ? new UserLoanModel(userLoans.FirstOrDefault((UserLoan ul) => ul.UserLoanRequestId == x.Id)) : null)
				} into x
				orderby x.RequestDate descending
				select x).ToList();
			return Success(data);
		});
	}

	[HttpGet("[action]")]
	public Task<ResponseState<List<LoanModel>>> LoansList()
	{
		return TryCatch(async () =>
		{
			List<AgentLoan> agentLoans = await _agentLoanService.GetByCondition((AgentLoan x) => (long?)x.AgentId == _currentUserService.UserId);
			List<LoanModel> data = (await (from x in _loanService.GetAllLoansAsQueryable().Include((Loan x) => x.AgentLoanAuthorizationItems).ThenInclude((AgentLoanAuthorizationItem x) => x.AuthorizationItem)
				where agentLoans.Select((AgentLoan agentLoan) => agentLoan.LoanId).Contains(x.Id)
				select x).ToListAsync()).Select((Loan x) => new LoanModel(x)).ToList();
			return Success(data);
		});
	}

	[HttpPost("[action]")]
	public Task<ResponseState<Guid>> SubmitSponsor([FromBody] SubmitSponsorModel model)
	{
		return TryCatch(async () =>
		{
			UserLoanRequest entity = await _userLoanRequestService.GetById(model.UserLoanRequestId);
			if (entity == null)
			{
				return Error(Guid.Empty, "اطلاعات مورد نظر یافت نشد.");
			}
			if (entity.UserLoanSponsorStatus != UserLoanSponsorStatuses.در_انتظار_بررسی)
			{
				return Error(Guid.Empty, "به دلیل تغییر وضعیت توسط اسپانسر، امکان تغییر اسپانسر وجود ندارد.");
			}
			entity.SponsorId = model.SponsorId;
			await _userLoanRequestService.Update(entity);
			User user = null;
			if (model.SponsorId.HasValue)
			{
				user = await _userManager.GetById(model.SponsorId.Value);
			}
			UserLoanRequestSponsorHistory entity2 = new UserLoanRequestSponsorHistory
			{
				DateTime = DateTime.Now,
				UserId = _currentUserService.UserId.Value,
				UserLoanRequestId = model.UserLoanRequestId,
				Subject = ((!model.SponsorId.HasValue) ? "حذف اسپانسر" : "تعیین اسپانسر"),
				Description = ((!model.SponsorId.HasValue) ? "اسپانسر حذف شد." : ("اسپانسر تعیین شد: [" + user?.BusinessName + "]"))
			};
			await _userLoanRequestSponsorHistoryService.Insert(entity2);
			return Success(entity.Id, "عملیات با موفقیت انجام شد.");
		});
	}
}
