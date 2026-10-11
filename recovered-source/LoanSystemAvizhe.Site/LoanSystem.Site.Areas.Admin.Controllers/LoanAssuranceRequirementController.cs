using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
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
public class LoanAssuranceRequirementController : BaseAdminController
{
	private readonly ILoanAssuranceRequirementService _loanAssuranceRequirementService;

	private readonly ICurrentUserService _currentUserService;

	public LoanAssuranceRequirementController(ILogger<LoanAssuranceRequirementController> logger, ILoanAssuranceRequirementService loanAssuranceRequirementService, ICurrentUserService currentUserService)
		: base(logger)
	{
		_loanAssuranceRequirementService = loanAssuranceRequirementService;
		_currentUserService = currentUserService;
	}

	[HttpGet("[action]/{id}")]
	public Task<ResponseState<List<LoanAssuranceRequirementModel>>> ListByLoanId(Guid id)
	{
		return TryCatch(async () =>
		{
			List<LoanAssuranceRequirementModel> data = (await _loanAssuranceRequirementService.GetByCondition((LoanAssuranceRequirement x) => x.LoanId == id, "Loan")).Select((LoanAssuranceRequirement x) => new LoanAssuranceRequirementModel(x)).ToList();
			return Success(data);
		});
	}

	[HttpPost("[action]")]
	public Task<ResponseState<Guid>> Create([FromBody] LoanAssuranceRequirementModel model)
	{
		return TryCatch(async () =>
		{
			LoanAssuranceRequirement entity = new LoanAssuranceRequirement
			{
				LoanId = model.LoanId,
				AssuranceRequirementsJson = JsonSerializer.Serialize(model.AssuranceRequirementItemIds)
			};
			await _loanAssuranceRequirementService.Insert(entity);
			return Success(entity.Id, "عملیات با موفقیت انجام شد.");
		});
	}

	[HttpPost("[action]")]
	public Task<ResponseState<Guid>> Edit([FromBody] LoanAssuranceRequirementModel model)
	{
		return TryCatch(async () =>
		{
			LoanAssuranceRequirement entity = (await _loanAssuranceRequirementService.GetByCondition((LoanAssuranceRequirement x) => x.Id == model.Id)).FirstOrDefault();
			if (entity == null)
			{
				return Error("آیتم یافت نشد.");
			}
			entity.AssuranceRequirementsJson = JsonSerializer.Serialize(model.AssuranceRequirementItemIds);
			await _loanAssuranceRequirementService.Update(entity);
			return Success(entity.Id, "عملیات با موفقیت انجام شد.");
		});
	}

	[HttpPost("[action]/{id}")]
	public Task<DefaultResponseState> Delete(Guid id)
	{
		return TryCatch(async () =>
		{
			LoanAssuranceRequirement loanAssuranceRequirement = (await _loanAssuranceRequirementService.GetByCondition((LoanAssuranceRequirement x) => x.Id == id)).FirstOrDefault();
			if (loanAssuranceRequirement == null)
			{
				return Error("امکان حذف وجود ندارد. لطفا بعدا دوباره تلاش نمایید");
			}
			await _loanAssuranceRequirementService.Delete(loanAssuranceRequirement);
			return Success();
		});
	}
}
