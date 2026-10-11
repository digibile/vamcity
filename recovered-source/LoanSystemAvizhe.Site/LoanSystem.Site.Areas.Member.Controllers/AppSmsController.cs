using System;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Core.Exceptions;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Helpers.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoanSystem.Site.Areas.Member.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Member,Admin,Agent")]
public class AppSmsController : BaseMemberController
{
	private readonly IAppSmsService _appSmsService;

	private readonly IntegratedSmsSenderService _integratedSmsSenderService;

	private readonly IMemberService _memberService;

	private readonly IUserLoanRequestAuthorizationItemService _userLoanRequestAuthorizationItemService;

	private readonly IThirdPersonAssuranceAuthorizationItemService _thirdPersonAssuranceAuthorizationItemService;

	public AppSmsController(ILogger<AppSmsController> logger, IAppSmsService appSmsService, IntegratedSmsSenderService integratedSmsSenderService, IMemberService memberService)
		: base(logger)
	{
		_appSmsService = appSmsService;
		_integratedSmsSenderService = integratedSmsSenderService;
		_memberService = memberService;
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
