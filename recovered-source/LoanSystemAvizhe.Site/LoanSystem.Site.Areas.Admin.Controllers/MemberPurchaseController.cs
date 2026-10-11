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

namespace LoanSystem.Site.Areas.Admin.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Admin")]
public class MemberPurchaseController : BaseAdminController
{
	private readonly IMemberPurchaseService _memberPurchaseService;

	private readonly IMemberService _memberService;

	private readonly ICurrentUserService _currentUserService;

	private readonly UserManager _userManager;

	public MemberPurchaseController(ILogger<MemberPurchaseController> logger, IMemberService memberService, ICurrentUserService currentUserService, IMemberPurchaseService memberPurchaseService, UserManager userManager)
		: base(logger)
	{
		_currentUserService = currentUserService;
		_memberService = memberService;
		_memberPurchaseService = memberPurchaseService;
		_userManager = userManager;
	}

	[HttpGet("[action]")]
	public Task<ResponseState<List<MemberPurchaseModel>>> List(DateTime? start, DateTime? end, long? memberId)
	{
		return TryCatch(async () =>
		{
			IQueryable<MemberPurchase> allAsQueryable = _memberPurchaseService.GetAllAsQueryable();
			allAsQueryable = allAsQueryable.Include((MemberPurchase x) => x.User);
			allAsQueryable = allAsQueryable.Where((MemberPurchase x) => x.CustomerId == _currentUserService.CustomerId);
			if (start.HasValue)
			{
				allAsQueryable = allAsQueryable.Where((MemberPurchase x) => x.StartDate.Date >= ((DateTime?)start).Value.Date);
			}
			if (end.HasValue)
			{
				allAsQueryable = allAsQueryable.Where((MemberPurchase x) => x.StartDate.Date <= ((DateTime?)end).Value.Date);
			}
			if (memberId.HasValue)
			{
				allAsQueryable = allAsQueryable.Where((MemberPurchase x) => (long?)x.UserId == memberId);
			}
			List<MemberPurchaseModel> data = (from x in await allAsQueryable.ToListAsync()
				select new MemberPurchaseModel(x) into x
				orderby x.StartDate descending
				select x).ToList();
			return Success(data);
		});
	}

	[HttpGet("[action]")]
	public Task<ResponseState<List<MemberPurchaseModel>>> WalletList(DateTime? start, DateTime? end, long? memberId)
	{
		return TryCatch(async () =>
		{
			MemberPurchaseTypes[] types = new MemberPurchaseTypes[3]
			{
				MemberPurchaseTypes.WalletManualCredit,
				MemberPurchaseTypes.WalletCredit,
				MemberPurchaseTypes.WalletWithdraw
			};
			IQueryable<MemberPurchase> allAsQueryable = _memberPurchaseService.GetAllAsQueryable();
			allAsQueryable = allAsQueryable.Include((MemberPurchase x) => x.User);
			allAsQueryable = allAsQueryable.Where((MemberPurchase x) => x.CustomerId == _currentUserService.CustomerId && Enumerable.Contains(types, x.MemberPurchaseType));
			if (start.HasValue)
			{
				allAsQueryable = allAsQueryable.Where((MemberPurchase x) => x.StartDate.Date >= ((DateTime?)start).Value.Date);
			}
			if (end.HasValue)
			{
				allAsQueryable = allAsQueryable.Where((MemberPurchase x) => x.StartDate.Date <= ((DateTime?)end).Value.Date);
			}
			if (memberId.HasValue)
			{
				allAsQueryable = allAsQueryable.Where((MemberPurchase x) => (long?)x.UserId == memberId);
			}
			List<MemberPurchaseModel> data = (from x in await allAsQueryable.ToListAsync()
				select new MemberPurchaseModel(x) into x
				orderby x.StartDate descending
				select x).ToList();
			return Success(data);
		});
	}

	[HttpPost("[action]")]
	public Task<DefaultResponseState> SubmitManualWalletTransaction([FromBody] ManualWalletTransactionModel model)
	{
		return TryCatch(async () =>
		{
			User member = await _memberService.GetById(model.MemberId);
			if (member == null)
			{
				return Error("مشتری یافت نشد.");
			}
			if (model.Type == MemberPurchaseTypes.WalletWithdraw && model.Amount > member.Credit)
			{
				return Error("مبلغ وارد شده از اعتبار کیف پول بیشتر است.");
			}
			IMemberPurchaseService memberPurchaseService = _memberPurchaseService;
			MemberPurchase memberPurchase = new MemberPurchase
			{
				Amount = model.Amount,
				PaymentStep = PaymentSteps.Finish,
				CreatorUserId = _currentUserService.UserId,
				CustomerId = _currentUserService.CustomerId.Value,
				UserId = model.MemberId,
				IsPaymentSuccess = true,
				MemberPurchaseType = model.Type,
				StartDate = DateTime.Now
			};
			MemberPurchase memberPurchase2 = memberPurchase;
			string text = ((model.Type == MemberPurchaseTypes.WalletManualCredit) ? "واریز" : "برداشت");
			memberPurchase2.Description = text + " دستی در پنل ادمین توسط " + (await _userManager.GetById(_currentUserService.UserId.Value))?.FullName;
			await memberPurchaseService.Insert(memberPurchase);
			if (model.Type == MemberPurchaseTypes.WalletManualCredit)
			{
				member.Credit += model.Amount;
			}
			else
			{
				member.Credit -= model.Amount;
			}
			await _memberService.Update(member);
			return Success("عملیات با موفقیت انجام شد.");
		});
	}
}
