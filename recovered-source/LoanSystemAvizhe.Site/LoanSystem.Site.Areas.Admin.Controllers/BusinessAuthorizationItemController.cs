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
[Authorize(Roles = "Admin,Member,Agent")]
public class BusinessAuthorizationItemController : BaseAdminController
{
	private readonly IBusinessAuthorizationItemService _businessAuthorizationItemService;

	private readonly ICurrentUserService _currentUserService;

	public BusinessAuthorizationItemController(ILogger<BusinessAuthorizationItemController> logger, IBusinessAuthorizationItemService businessAuthorizationItemService, ICurrentUserService currentUserService)
		: base(logger)
	{
		_businessAuthorizationItemService = businessAuthorizationItemService;
		_currentUserService = currentUserService;
	}

	[HttpGet("[action]")]
	public Task<ResponseState<List<BusinessAuthorizationItemModel>>> List()
	{
		return TryCatch(async () =>
		{
			List<BusinessAuthorizationItemModel> data = (await _businessAuthorizationItemService.GetByCondition((BusinessAuthorizationItem x) => x.CustomerId == _currentUserService.CustomerId, "AuthorizationItem")).Select((BusinessAuthorizationItem x) => new BusinessAuthorizationItemModel(x)).ToList();
			return Success(data);
		});
	}

	[HttpPost("[action]")]
	[Authorize(Roles = "Admin")]
	public Task<ResponseState<Guid>> Create([FromBody] BusinessAuthorizationItemModel model)
	{
		return TryCatch(async () =>
		{
			List<BusinessAuthorizationItem> list = await _businessAuthorizationItemService.GetByCondition((BusinessAuthorizationItem x) => x.AuthorizationItemId == model.AuthorizationItem.Id && x.CustomerId == _currentUserService.CustomerId);
			if (list != null && list.Count > 0)
			{
				return Error("آیتم قبلا ثبت شده است.");
			}
			BusinessAuthorizationItem entity = new BusinessAuthorizationItem
			{
				AuthorizationItemId = model.AuthorizationItem.Id
			};
			await _businessAuthorizationItemService.Insert(entity);
			return Success(entity.Id, "عملیات با موفقیت انجام شد.");
		});
	}

	[HttpPost("[action]")]
	[Authorize(Roles = "Admin")]
	public Task<ResponseState<Guid>> Edit([FromBody] BusinessAuthorizationItemModel model)
	{
		return TryCatch(async () =>
		{
			BusinessAuthorizationItem entity = (await _businessAuthorizationItemService.GetByCondition((BusinessAuthorizationItem x) => x.Id == model.Id)).FirstOrDefault();
			if (entity == null)
			{
				return Error("آیتم یافت نشد.");
			}
			entity.AuthorizationItemId = model.AuthorizationItem.Id;
			entity.Cost = model.Cost;
			await _businessAuthorizationItemService.Update(entity);
			return Success(entity.Id, "عملیات با موفقیت انجام شد.");
		});
	}

	[HttpPost("[action]/{id}")]
	[Authorize(Roles = "Admin")]
	public Task<DefaultResponseState> Delete(Guid id)
	{
		return TryCatch(async () =>
		{
			BusinessAuthorizationItem businessAuthorizationItem = (await _businessAuthorizationItemService.GetByCondition((BusinessAuthorizationItem x) => x.Id == id)).FirstOrDefault();
			if (businessAuthorizationItem == null)
			{
				return Error("امکان حذف وجود ندارد. لطفا بعدا دوباره تلاش نمایید");
			}
			await _businessAuthorizationItemService.Delete(businessAuthorizationItem);
			return Success();
		});
	}
}
