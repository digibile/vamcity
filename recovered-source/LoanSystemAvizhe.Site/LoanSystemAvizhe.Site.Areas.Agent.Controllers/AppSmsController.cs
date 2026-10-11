using System;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Core.Exceptions;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Areas.Agent.Controllers;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoanSystemAvizhe.Site.Areas.Agent.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Agent")]
public class AppSmsController : BaseAgentController
{
	private readonly IAppSmsService _appSmsService;

	private readonly ICurrentUserService _currentUserService;

	private readonly IntegratedSmsSenderService _integratedSmsSenderService;

	private readonly IUserLoanRequestAuthorizationItemService _userLoanRequestAuthorizationItemService;

	private readonly IMemberService _memberService;

	public AppSmsController(ILogger<AppSmsController> logger, IAppSmsService appSmsService, ICurrentUserService currentUserService, IntegratedSmsSenderService integratedSmsSenderService, IUserLoanRequestAuthorizationItemService userLoanRequestAuthorizationItemService, IMemberService memberService)
		: base(logger)
	{
		_appSmsService = appSmsService;
		_currentUserService = currentUserService;
		_integratedSmsSenderService = integratedSmsSenderService;
		_userLoanRequestAuthorizationItemService = userLoanRequestAuthorizationItemService;
		_memberService = memberService;
	}

	[HttpPost("[action]")]
	public Task<ResponseState<AppSmsModel>> SendDigitalSignLink(Guid id)
	{
		return TryCatch(async () =>
		{
			UserLoanRequestAuthorizationItem item = (await _userLoanRequestAuthorizationItemService.GetByCondition((UserLoanRequestAuthorizationItem x) => x.Id == id, "UserLoanRequest.User", "UserLoanRequest.Agent", "UserLoanRequest.Loan", "AuthorizationItem")).FirstOrDefault();
			if (item == null)
			{
				return Error<AppSmsModel>(null, "اطلاعات مورد نظر یافت نشد.");
			}
			if (item.Status == LoanRequestAuthorizationStatus.تایید_شده_توسط_مدیر)
			{
				return Error<AppSmsModel>(null, "قراداد قبلا امضا شده است.");
			}
			if (item.UserLoanRequest.AgentId != _currentUserService.UserId)
			{
				return Error<AppSmsModel>(null, "مجوز ارسال لینک برای این قرارداد را ندارید.");
			}
			if ((await _appSmsService.GetByCondition((AppSms x) => x.To == item.UserLoanRequest.User.Mobile && (int)x.AppSmsType == 17 && x.Date >= DateTime.Now.AddMinutes(-2.0), false)).Any())
			{
				return Error<AppSmsModel>(null, "از آخرین ارسال باید حداقل 2 دقیقه گذشته باشد.");
			}
			try
			{
				AppSms appSms = await _integratedSmsSenderService.SendDigitalSignLink(new UserLoanRequestAuthorizationItemModel(item));
				if (appSms != null)
				{
					return Success(new AppSmsModel(appSms), "عملیات با موفقیت انجام شد.");
				}
			}
			catch (CustomException ex)
			{
				return Error<AppSmsModel>(null, ex.Message);
			}
			return Error<AppSmsModel>(null, "پیامک ارسال نشد");
		});
	}

	[HttpPost("[action]")]
	public Task<ResponseState<AppSmsModel>> SendWalletCharge(long id)
	{
		return TryCatch(async () =>
		{
			User member = await _memberService.GetById(id);
			if (member == null)
			{
				return Error<AppSmsModel>(null, "اطلاعات مورد نظر یافت نشد.");
			}
			if ((await _appSmsService.GetByCondition((AppSms x) => x.To == member.Mobile && (int)x.AppSmsType == 11 && x.Date >= DateTime.Now.AddMinutes(-2.0), false)).Any())
			{
				return Error<AppSmsModel>(null, "از آخرین ارسال باید حداقل 2 دقیقه گذشته باشد.");
			}
			try
			{
				AppSms appSms = await _integratedSmsSenderService.SendWalletCharge(new WalletChargeModel
				{
					Date = DateTime.Now,
					Member = new Member2Model(member)
				});
				if (appSms != null)
				{
					return Success(new AppSmsModel(appSms), "عملیات با موفقیت انجام شد.");
				}
			}
			catch (CustomException ex)
			{
				return Error<AppSmsModel>(null, ex.Message);
			}
			return Error<AppSmsModel>(null, "پیامک ارسال نشد");
		});
	}
}
