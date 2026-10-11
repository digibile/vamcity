using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Core.Configuration;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Site.Areas.AppApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LoanSystem.Site.Areas.AppApi.Controllers;

[Route("[area]/[controller]")]
public class LoanController : BaseApiController
{
	private readonly ILoanService _loanService;

	private readonly ICurrentUserService _currentUserService;

	private readonly AppConfig _appConfig;

	public LoanController(ILogger<LoanController> logger, IOptions<AppConfig> appConfig, ILoanService loanService, ICurrentUserService currentUserService)
		: base(logger)
	{
		_loanService = loanService;
		_currentUserService = currentUserService;
		_appConfig = appConfig.Value;
		if (!_currentUserService.TrySetCustomerIdFromToken())
		{
			throw new Exception("Token is not valid, please try get token again.");
		}
	}

	[HttpGet("[action]")]
	public Task<ResponseState<List<AppApiLoanModel>>> List()
	{
		return TryCatch(async () =>
		{
			List<AppApiLoanModel> data = (await _loanService.GetAllLoans()).Select((Loan x) => new AppApiLoanModel(x)).ToList();
			return Success(data);
		});
	}

	[HttpPost("[action]")]
	public Task<ResponseState<Guid>> Create([FromBody] AppApiLoanModel model)
	{
		return TryCatch(async () =>
		{
			if (await _loanService.GetByTitle(model.Title) != null)
			{
				return Error("نام وارد شده تکراری است.");
			}
			int num = (await (from x in _loanService.GetAllLoansAsQueryable()
				orderby x.Code descending
				select x).FirstOrDefaultAsync())?.Code ?? 0;
			model.Code = num + 1;
			if (await _loanService.GetByCode(model.Code) != null)
			{
				return Error("شماره وارد شده تکراری است.");
			}
			Loan entity = new Loan
			{
				Id = model.Id,
				Title = model.Title,
				Code = model.Code,
				ProfitPercent = model.ProfitPercent,
				DelayPercent = model.DelayPercent,
				EarlyPercent = model.EarlyPercent,
				IsSponsored = model.IsSponsored,
				IsSmsActive = model.IsSmsActive,
				MaxAmount = model.MaxAmount.GetValueOrDefault(),
				MaxMonthGap = model.MaxMonthGap.GetValueOrDefault(),
				IncludePlan = model.IncludePlan,
				PlanHtml = "",
				PlanShortDescription = model.PlanShortDescription,
				PlanTitle = model.PlanTitle,
				FileName = "",
				CostsJson = "",
				AllowedInstallments = "",
				Disable = model.Disable,
				HasSponsore = model.HasSponsore,
				DisableText = model.DisableText,
				DefaultSponsoreCheckoutDays = model.DefaultSponsoreCheckoutDays,
				LoanProfitType = model.LoanProfitType
			};
			await _loanService.Insert(entity);
			return Success(entity.Id, "عملیات با موفقیت انجام شد.");
		});
	}

	[HttpPost("[action]")]
	public Task<ResponseState<Guid>> Edit([FromBody] AppApiLoanModel model)
	{
		return TryCatch(async () =>
		{
			Loan loan = await _loanService.GetByTitle(model.Title);
			if (loan != null && loan.Id != model.Id)
			{
				return Error("نام وارد شده تکراری است.");
			}
			loan = await _loanService.GetByCode(model.Code);
			if (loan != null && loan.Id != model.Id)
			{
				return Error("شماره وارد شده تکراری است.");
			}
			Loan db = await _loanService.GetById(model.Id);
			db.Title = model.Title;
			db.Code = model.Code;
			db.ProfitPercent = model.ProfitPercent;
			db.LoanProfitType = model.LoanProfitType;
			db.DelayPercent = model.DelayPercent;
			db.EarlyPercent = model.EarlyPercent;
			db.IsSponsored = model.IsSponsored;
			db.MaxAmount = model.MaxAmount.GetValueOrDefault();
			db.MaxMonthGap = model.MaxMonthGap.GetValueOrDefault();
			db.IsSmsActive = model.IsSmsActive;
			db.IncludePlan = model.IncludePlan;
			db.PlanShortDescription = model.PlanShortDescription;
			db.PlanTitle = model.PlanTitle;
			db.Disable = model.Disable;
			db.HasSponsore = model.HasSponsore;
			db.DisableText = model.DisableText;
			db.DefaultSponsoreCheckoutDays = model.DefaultSponsoreCheckoutDays;
			await _loanService.Update(db);
			return Success(db.Id, "عملیات با موفقیت انجام شد.");
		});
	}
}
