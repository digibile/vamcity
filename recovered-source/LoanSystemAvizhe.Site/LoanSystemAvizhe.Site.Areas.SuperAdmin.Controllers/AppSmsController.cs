using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Areas.SuperAdmin.Controllers;
using LoanSystem.Site.Helpers.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoanSystemAvizhe.Site.Areas.SuperAdmin.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "SuperAdmin")]
public class AppSmsController : BaseSuperAdminController
{
	private readonly IAppSmsService _appSmsService;

	public AppSmsController(ILogger<AppSmsController> logger, IAppSmsService appSmsService)
		: base(logger)
	{
		_appSmsService = appSmsService;
	}

	[HttpGet("[action]")]
	public Task<ResponseState<List<AppSmsModel>>> List(DateTime? start, DateTime? end, string mobile)
	{
		return TryCatch(async () =>
		{
			List<AppSmsModel> data = (from x in await _appSmsService.GetByCondition((AppSms x) => (((DateTime?)start).HasValue ? (x.Date.Date >= ((DateTime?)start).Value.Date) : true) && (((DateTime?)end).HasValue ? (x.Date.Date <= ((DateTime?)end).Value.Date) : true) && ((!string.IsNullOrEmpty(mobile)) ? (x.To == mobile) : true), true, "Customer")
				select new AppSmsModel(x) into x
				orderby x.Date descending
				select x).ToList();
			return Success(data);
		});
	}
}
