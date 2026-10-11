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
public class AuthorizationItemController : BaseAdminController
{
	private readonly IAuthorizationItemService _authorizationItemService;

	private readonly ICurrentUserService _currentUserService;

	public AuthorizationItemController(ILogger<AuthorizationItemController> logger, IAuthorizationItemService authorizationItemService, ICurrentUserService currentUserService)
		: base(logger)
	{
		_authorizationItemService = authorizationItemService;
		_currentUserService = currentUserService;
	}

	[HttpGet("[action]")]
	public Task<ResponseState<List<AuthorizationItemModel>>> List()
	{
		return TryCatch(async () =>
		{
			List<AuthorizationItemModel> data = (await _authorizationItemService.GetByCondition((AuthorizationItem _) => true)).Select((AuthorizationItem x) => new AuthorizationItemModel(x)).ToList();
			return Success(data);
		});
	}

	[HttpPost("[action]")]
	public Task<ResponseState<Guid>> Create([FromBody] AuthorizationItemModel model)
	{
		return TryCatch(async () =>
		{
			AuthorizationItem entity = new AuthorizationItem
			{
				Title = model.Title,
				Description = model.Description,
				UseIfAlreadyExist = model.UseIfAlreadyExist,
				SystemType = model.SystemType
			};
			await _authorizationItemService.Insert(entity);
			return Success(entity.Id, "عملیات با موفقیت انجام شد.");
		});
	}

	[HttpPost("[action]")]
	public Task<ResponseState<Guid>> Edit([FromBody] AuthorizationItemModel model)
	{
		return TryCatch(async () =>
		{
			AuthorizationItem entity = (await _authorizationItemService.GetByCondition((AuthorizationItem x) => x.Id == model.Id)).FirstOrDefault();
			if (entity == null)
			{
				return Error("آیتم یافت نشد.");
			}
			entity.UseIfAlreadyExist = model.UseIfAlreadyExist;
			entity.Title = model.Title;
			entity.Description = model.Description;
			entity.SystemType = model.SystemType;
			await _authorizationItemService.Update(entity);
			return Success(entity.Id, "عملیات با موفقیت انجام شد.");
		});
	}

	[HttpPost("[action]/{id}")]
	public Task<DefaultResponseState> Delete(Guid id)
	{
		return TryCatch(async () =>
		{
			AuthorizationItem authorizationItem = (await _authorizationItemService.GetByCondition((AuthorizationItem x) => x.Id == id)).FirstOrDefault();
			if (authorizationItem == null)
			{
				return Error("امکان حذف وجود ندارد. لطفا بعدا دوباره تلاش نمایید");
			}
			await _authorizationItemService.Delete(authorizationItem);
			return Success();
		});
	}
}
