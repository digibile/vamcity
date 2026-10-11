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

namespace LoanSystem.Site.Areas.Admin.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Admin")]
public class LoanAuthorizationItemController : BaseAdminController
{
	private readonly ILoanAuthorizationItemService _loanAuthorizationItemService;

	private readonly ICurrentUserService _currentUserService;

	public LoanAuthorizationItemController(ILogger<LoanAuthorizationItemController> logger, ILoanAuthorizationItemService loanAuthorizationItemService, ICurrentUserService currentUserService)
		: base(logger)
	{
		_loanAuthorizationItemService = loanAuthorizationItemService;
		_currentUserService = currentUserService;
	}

	[HttpGet("[action]/{id}")]
	public Task<ResponseState<List<LoanAuthorizationItemModel>>> ListByLoanId(Guid id)
	{
		return TryCatch(async () =>
		{
			List<LoanAuthorizationItemModel> data = (await _loanAuthorizationItemService.GetByCondition((LoanAuthorizationItem x) => x.LoanId == id, "Loan", "AuthorizationItem")).Select((LoanAuthorizationItem x) => new LoanAuthorizationItemModel(x)).ToList();
			return Success(data);
		});
	}

	[HttpPost("[action]")]
	public Task<ResponseState<Guid>> Create([FromBody] LoanAuthorizationItemModel model)
	{
		return TryCatch(async () =>
		{
			LoanAuthorizationItem entity = new LoanAuthorizationItem
			{
				LoanId = model.Loan.Id,
				AuthorizationItemId = model.AuthorizationItem.Id
			};
			await _loanAuthorizationItemService.Insert(entity);
			return Success(entity.Id, "عملیات با موفقیت انجام شد.");
		});
	}

	[HttpPost("[action]")]
	public Task<ResponseState<Guid>> Edit([FromBody] LoanAuthorizationItemModel model)
	{
		return TryCatch(async () =>
		{
			LoanAuthorizationItem entity = (await _loanAuthorizationItemService.GetByCondition((LoanAuthorizationItem x) => x.Id == model.Id)).FirstOrDefault();
			if (entity == null)
			{
				return Error("آیتم یافت نشد.");
			}
			entity.AuthorizationItemId = model.AuthorizationItem.Id;
			entity.IsPrimary = model.IsPrimary;
			await _loanAuthorizationItemService.Update(entity);
			return Success(entity.Id, "عملیات با موفقیت انجام شد.");
		});
	}

	[HttpPost("[action]/{id}")]
	public Task<DefaultResponseState> Delete(Guid id)
	{
		return TryCatch(async () =>
		{
			LoanAuthorizationItem loanAuthorizationItem = (await _loanAuthorizationItemService.GetByCondition((LoanAuthorizationItem x) => x.Id == id)).FirstOrDefault();
			if (loanAuthorizationItem == null)
			{
				return Error("امکان حذف وجود ندارد. لطفا بعدا دوباره تلاش نمایید");
			}
			await _loanAuthorizationItemService.Delete(loanAuthorizationItem);
			return Success();
		});
	}
}
