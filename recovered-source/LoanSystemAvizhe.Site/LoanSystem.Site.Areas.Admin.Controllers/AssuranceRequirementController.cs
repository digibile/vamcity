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
public class AssuranceRequirementController : BaseAdminController
{
	private readonly IAssuranceRequirementService _assuranceRequirementService;

	private readonly ICurrentUserService _currentUserService;

	public AssuranceRequirementController(ILogger<AssuranceRequirementController> logger, IAssuranceRequirementService assuranceRequirementService, ICurrentUserService currentUserService)
		: base(logger)
	{
		_assuranceRequirementService = assuranceRequirementService;
		_currentUserService = currentUserService;
	}

	[HttpGet("[action]")]
	public Task<ResponseState<List<AssuranceRequirementModel>>> List()
	{
		return TryCatch(async () =>
		{
			List<AssuranceRequirementModel> data = (await _assuranceRequirementService.GetByCondition((AssuranceRequirement _) => true, "AssuranceRequirementItems")).Select((AssuranceRequirement x) => new AssuranceRequirementModel(x)).ToList();
			return Success(data);
		});
	}

	[HttpPost("[action]")]
	public Task<ResponseState<Guid>> Create([FromBody] AssuranceRequirementModel model)
	{
		return TryCatch(async () =>
		{
			List<AssuranceRequirementItemModel> assuranceRequirementItems = model.AssuranceRequirementItems;
			if (assuranceRequirementItems == null || !assuranceRequirementItems.Any())
			{
				return Error("باید شامل حداقل یک آیتم باشد.");
			}
			AssuranceRequirement entity = new AssuranceRequirement
			{
				Title = model.Title,
				Order = model.Order,
				AssuranceRequirementItems = model.AssuranceRequirementItems.Select((AssuranceRequirementItemModel x) => new AssuranceRequirementItem
				{
					CustomerId = _currentUserService.CustomerId.Value,
					Order = x.Order,
					Title = x.Title
				}).ToList()
			};
			await _assuranceRequirementService.Insert(entity);
			return Success(entity.Id, "عملیات با موفقیت انجام شد.");
		});
	}

	[HttpPost("[action]")]
	public Task<ResponseState<Guid>> Edit([FromBody] AssuranceRequirementModel model)
	{
		return TryCatch(async () =>
		{
			List<AssuranceRequirementItemModel> assuranceRequirementItems = model.AssuranceRequirementItems;
			if (assuranceRequirementItems == null || !assuranceRequirementItems.Any())
			{
				return Error("باید شامل حداقل یک آیتم باشد.");
			}
			AssuranceRequirement entity = (await _assuranceRequirementService.GetByCondition((AssuranceRequirement x) => x.Id == model.Id, "AssuranceRequirementItems")).FirstOrDefault();
			if (entity == null)
			{
				return Error("آیتم یافت نشد.");
			}
			entity.Title = model.Title;
			entity.Order = model.Order;
			entity.AssuranceRequirementItems = model.AssuranceRequirementItems.Select((AssuranceRequirementItemModel x) => new AssuranceRequirementItem
			{
				Id = x.Id,
				CustomerId = _currentUserService.CustomerId.Value,
				Order = x.Order,
				Title = x.Title,
				AssuranceRequirementId = entity.Id
			}).ToList();
			await _assuranceRequirementService.Update(entity);
			return Success(entity.Id, "عملیات با موفقیت انجام شد.");
		});
	}

	[HttpPost("[action]/{id}")]
	public Task<DefaultResponseState> Delete(Guid id)
	{
		return TryCatch(async () =>
		{
			AssuranceRequirement assuranceRequirement = (await _assuranceRequirementService.GetByCondition((AssuranceRequirement x) => x.Id == id)).FirstOrDefault();
			if (assuranceRequirement == null)
			{
				return Error("امکان حذف وجود ندارد. لطفا بعدا دوباره تلاش نمایید");
			}
			await _assuranceRequirementService.Delete(assuranceRequirement);
			return Success();
		});
	}
}
