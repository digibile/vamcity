using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Areas.SuperAdmin.Controllers;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Site.Areas.SuperAdmin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoanSystem.Site.Areas.Admin.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "SuperAdmin")]
public class ConsultantController : BaseSuperAdminController
{
	private readonly IConsultantService _consultantService;

	public ConsultantController(ILogger<ConsultantController> logger, IConsultantService consultantService)
		: base(logger)
	{
		_consultantService = consultantService;
	}

	[HttpGet("[action]/{all}")]
	public Task<ResponseState<SuperAdminConsultant>> List(bool all)
	{
		return TryCatch(async () =>
		{
			List<Consultant> entities = await _consultantService.GetAll(all ? ((bool?)null) : new bool?(false));
			int countAll = (all ? entities.Count : (await _consultantService.GetCount(null)));
			List<ConsultantModel> consultants = entities.Select((Consultant x) => new ConsultantModel(x)).ToList();
			SuperAdminConsultant data = new SuperAdminConsultant
			{
				Consultants = consultants,
				CountAll = countAll
			};
			return Success(data);
		});
	}

	[HttpGet("[action]/{id}")]
	public Task<ResponseState<ConsultantModel>> Get(Guid id)
	{
		return TryCatch(async () =>
		{
			ConsultantModel data = new ConsultantModel();
			if (id != Guid.Empty)
			{
				Consultant consultant = await _consultantService.GetById(id);
				if (consultant == null)
				{
					return Error<ConsultantModel>(null, "اطلاعات مورد نظر یافت نشد");
				}
				data = new ConsultantModel(consultant);
			}
			return Success(data);
		});
	}

	[HttpPost("[action]")]
	public Task<DefaultResponseState> Edit([FromBody] ConsultantModel model)
	{
		return TryCatch(async () =>
		{
			Consultant consultant = await _consultantService.GetById(model.Id);
			if (consultant == null)
			{
				return Error("اطلاعات مورد نظر یافت نشد.");
			}
			consultant.IsSeen = model.IsSeen;
			consultant.CallResult = model.CallResult;
			await _consultantService.Update(consultant);
			return Success("عملیات با موفقیت انجام شد.");
		});
	}

	[HttpPost("[action]/{id}")]
	public Task<DefaultResponseState> Delete(Guid id)
	{
		return TryCatch(async () =>
		{
			Consultant consultant = await _consultantService.GetById(id);
			if (consultant == null)
			{
				return Error("امکان حذف وجود ندارد. لطفا بعدا دوباره تلاش نمایید");
			}
			await _consultantService.Delete(consultant);
			return Success();
		});
	}
}
