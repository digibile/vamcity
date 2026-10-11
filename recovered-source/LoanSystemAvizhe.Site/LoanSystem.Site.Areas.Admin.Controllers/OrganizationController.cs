using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Helpers.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoanSystem.Site.Areas.Admin.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Admin")]
public class OrganizationController : BaseAdminController
{
	private readonly IOrganizationService _organizationService;

	public OrganizationController(ILogger<OrganizationController> logger, IOrganizationService organizationService)
		: base(logger)
	{
		_organizationService = organizationService;
	}

	[HttpGet("[action]")]
	public Task<ResponseState<List<OrganizationModel>>> List()
	{
		return TryCatch(async () =>
		{
			List<OrganizationModel> data = (await _organizationService.GetAllOrganizations()).Select((Organization x) => new OrganizationModel(x)).ToList();
			return Success(data);
		});
	}

	[HttpGet("[action]/{id}")]
	public Task<ResponseState<OrganizationModel>> Get(Guid id)
	{
		return TryCatch(async () =>
		{
			if (id == Guid.Empty)
			{
				return Success(new OrganizationModel());
			}
			Organization organization = await _organizationService.GetById(id);
			if (organization == null)
			{
				return Error<OrganizationModel>(null, "اطلاعات مورد نظر یافت نشد");
			}
			OrganizationModel data = new OrganizationModel(organization);
			return Success(data);
		});
	}

	[HttpPost("[action]")]
	public Task<ResponseState<Guid>> Create([FromBody] OrganizationModel model)
	{
		return TryCatch(async () =>
		{
			Organization entity = new Organization
			{
				Id = Guid.Empty,
				Name = model.Name
			};
			await _organizationService.Insert(entity);
			return Success(entity.Id, "عملیات با موفقیت انجام شد.");
		});
	}

	[HttpPost("[action]")]
	public Task<ResponseState<Guid>> Edit([FromBody] OrganizationModel model)
	{
		return TryCatch(async () =>
		{
			Organization entity = await _organizationService.GetById(model.Id);
			entity.Name = model.Name;
			await _organizationService.Update(entity);
			return Success(entity.Id, "عملیات با موفقیت انجام شد.");
		});
	}

	[HttpPost("[action]/{id}")]
	public Task<DefaultResponseState> Delete(Guid id)
	{
		return TryCatch(async () =>
		{
			Organization organization = await _organizationService.GetById(id);
			if (organization == null)
			{
				return Error("امکان حذف وجود ندارد. لطفا بعدا دوباره تلاش نمایید");
			}
			await _organizationService.Delete(organization);
			return Success();
		});
	}
}
