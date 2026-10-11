using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Services.Users;
using LoanSystem.Site.Areas.Sponsor.Models;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Data.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoanSystem.Site.Areas.Sponsor.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Sponsor")]
public class UserLoanRequestSponsorHistoryController : BaseSponsorController
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

	public UserLoanRequestSponsorHistoryController(ILogger<UserLoanRequestSponsorHistoryController> logger, IUserLoanRequestService userLoanRequestService, IUserLoanService userLoanService, ILoanService loanService, IAppContext appContext, ICurrentUserService currentUserService, IAgentLoanService agentLoanService, ITransactionService transactionService, IUserLoanRequestAuthorizationItemService userLoanRequestAuthorizationItemService, ISubAgentTransactionService subAgentTransactionService, ILoanRequestStatusService loanRequestStatusService, IUserLoanRequestSponsorHistoryService userLoanRequestSponsorHistoryService, IntegratedSmsSenderService integratedSmsSenderService, IntegratedEmailSenderService integratedEmailSenderService, UserManager userManager)
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

	[HttpGet("[action]")]
	public Task<ResponseState<List<UserLoanRequestSponsorHistoryModel>>> GetSponsorHistory(Guid userLoanRequestId)
	{
		return TryCatch(async () =>
		{
			List<UserLoanRequestSponsorHistoryModel> data = (from x in await _userLoanRequestSponsorHistoryService.GetByUserLoanRequestId(userLoanRequestId, "User")
				select new UserLoanRequestSponsorHistoryModel(x) into x
				orderby x.DateTime descending
				select x).ToList();
			return Success(data);
		});
	}

	[HttpPost("[action]")]
	public Task<ResponseState<Guid>> Create([FromBody] CreateSponsorHistoryModel model)
	{
		return TryCatch(async () =>
		{
			if (model.type == CreateSponsorHistoryType.Comment && string.IsNullOrWhiteSpace(model.Comment))
			{
				return Error(Guid.Empty, "توضیحات وارد نشده است.");
			}
			UserLoanRequest entity = await _userLoanRequestService.GetById(model.UserLoanRequestId);
			if (entity == null)
			{
				return Error(Guid.Empty, "اطلاعات مورد نظر یافت نشد.");
			}
			if (entity.SponsorId != _appContext.User.Id)
			{
				return Error(Guid.Empty, "دسترسی به این درخواست وام برای شما مجاز نمی باشد.");
			}
			if (model.type == CreateSponsorHistoryType.ChangeStatus && entity.UserLoanSponsorStatus != UserLoanSponsorStatuses.در_انتظار_بررسی && model.Status == UserLoanSponsorStatuses.در_انتظار_بررسی)
			{
				return Error(Guid.Empty, "تغییر وضعیت به در انتظار بررسی مجاز نمی باشد.");
			}
			UserLoanRequestSponsorHistory entity2 = new UserLoanRequestSponsorHistory
			{
				DateTime = DateTime.Now,
				UserId = _currentUserService.UserId.Value,
				UserLoanRequestId = model.UserLoanRequestId,
				Subject = ((model.type == CreateSponsorHistoryType.ChangeStatus) ? "تغییر وضعیت" : "ثبت توضیحات"),
				Description = ((model.type == CreateSponsorHistoryType.ChangeStatus) ? ("تغییر وضعیت به " + model.Status.ToString().Replace("_", "")) : model.Comment)
			};
			await _userLoanRequestSponsorHistoryService.Insert(entity2);
			if (model.type == CreateSponsorHistoryType.ChangeStatus)
			{
				entity.UserLoanSponsorStatus = model.Status;
			}
			if (model.type == CreateSponsorHistoryType.Comment)
			{
				entity.LastSponsorComment = model.Comment;
			}
			await _userLoanRequestService.Update(entity);
			return Success(entity.Id, "عملیات با موفقیت انجام شد.");
		});
	}
}
