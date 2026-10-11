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
public class OffDatesController : BaseAdminController
{
	private readonly IOffDateService _offDateService;

	private readonly ICurrentUserService _currentUserService;

	public OffDatesController(ILogger<OffDatesController> logger, IOffDateService offDateService, ICurrentUserService currentUserService)
		: base(logger)
	{
		_currentUserService = currentUserService;
		_offDateService = offDateService;
	}

	[HttpGet("[action]/{id}")]
	public Task<ResponseState<OffDateModel>> Get(Guid id)
	{
		return TryCatch(async () =>
		{
			OffDate offDate = await _offDateService.GetById(id);
			return Success(new OffDateModel(offDate));
		});
	}

	[HttpGet("[action]")]
	public Task<ResponseState<List<OffDateModel>>> List()
	{
		return TryCatch(async () =>
		{
			List<OffDateModel> data = (from x in await _offDateService.GetAll(_currentUserService.CustomerId)
				select new OffDateModel(x) into x
				orderby x.Date descending
				select x).ToList();
			return Success(data);
		});
	}

	[HttpPost("[action]")]
	public Task<DefaultResponseState> Modify([FromBody] List<OffDateModel> models)
	{
		return TryCatch(async () =>
		{
			List<OffDate> entities = models.Select((OffDateModel x) => new OffDate
			{
				Id = x.Id,
				Date = x.Date,
				Title = x.Title
			}).ToList();
			await _offDateService.Modify(entities);
			return Success("عملیات با موفقیت انجام شد.");
		});
	}
}
