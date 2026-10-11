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
public class AgentLoanAuthorizationItemController : BaseAdminController
{
	private readonly IAgentLoanAuthorizationItemService _agentLoanAuthorizationItemService;

	private readonly ICurrentUserService _currentUserService;

	public AgentLoanAuthorizationItemController(ILogger<AgentLoanAuthorizationItemController> logger, IAgentLoanAuthorizationItemService agentLoanAuthorizationItemService, ICurrentUserService currentUserService)
		: base(logger)
	{
		_agentLoanAuthorizationItemService = agentLoanAuthorizationItemService;
		_currentUserService = currentUserService;
	}

	[HttpGet("[action]/{id}")]
	public Task<ResponseState<List<LoanAuthorizationItemModel>>> ListByLoanId(Guid id)
	{
		return TryCatch(async () =>
		{
			List<LoanAuthorizationItemModel> data = (await _agentLoanAuthorizationItemService.GetByCondition((AgentLoanAuthorizationItem x) => x.LoanId == id, "Loan", "AuthorizationItem")).Select((AgentLoanAuthorizationItem x) => new LoanAuthorizationItemModel(x)).ToList();
			return Success(data);
		});
	}

	[HttpPost("[action]")]
	public Task<ResponseState<Guid>> Create([FromBody] LoanAuthorizationItemModel model)
	{
		return TryCatch(async () =>
		{
			AgentLoanAuthorizationItem entity = new AgentLoanAuthorizationItem
			{
				LoanId = model.Loan.Id,
				AuthorizationItemId = model.AuthorizationItem.Id
			};
			await _agentLoanAuthorizationItemService.Insert(entity);
			return Success(entity.Id, "عملیات با موفقیت انجام شد.");
		});
	}

	[HttpPost("[action]")]
	public Task<ResponseState<Guid>> Edit([FromBody] LoanAuthorizationItemModel model)
	{
		return TryCatch(async () =>
		{
			AgentLoanAuthorizationItem entity = (await _agentLoanAuthorizationItemService.GetByCondition((AgentLoanAuthorizationItem x) => x.Id == model.Id)).FirstOrDefault();
			if (entity == null)
			{
				return Error("آیتم یافت نشد.");
			}
			entity.AuthorizationItemId = model.AuthorizationItem.Id;
			await _agentLoanAuthorizationItemService.Update(entity);
			return Success(entity.Id, "عملیات با موفقیت انجام شد.");
		});
	}

	[HttpPost("[action]/{id}")]
	public Task<DefaultResponseState> Delete(Guid id)
	{
		return TryCatch(async () =>
		{
			AgentLoanAuthorizationItem agentLoanAuthorizationItem = (await _agentLoanAuthorizationItemService.GetByCondition((AgentLoanAuthorizationItem x) => x.Id == id)).FirstOrDefault();
			if (agentLoanAuthorizationItem == null)
			{
				return Error("امکان حذف وجود ندارد. لطفا بعدا دوباره تلاش نمایید");
			}
			await _agentLoanAuthorizationItemService.Delete(agentLoanAuthorizationItem);
			return Success();
		});
	}
}
