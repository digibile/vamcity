using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Areas.Admin.Models;
using LoanSystem.Site.Areas.SuperAdmin.Controllers;
using LoanSystem.Site.Helpers.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoanSystemAvizhe.Site.Areas.SuperAdmin.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "SuperAdmin")]
public class AppInquiryController : BaseSuperAdminController
{
	private readonly IAppInquiryService _appInquiryService;

	public AppInquiryController(ILogger<AppInquiryController> logger, IAppInquiryService appInquiryService)
		: base(logger)
	{
		_appInquiryService = appInquiryService;
	}

	[HttpGet("[action]")]
	public Task<ResponseState<List<AppInquiryModel>>> List(DateTime? start, DateTime? end)
	{
		return TryCatch(async () =>
		{
			List<AppInquiryModel> data = (from x in await _appInquiryService.GetByCondition((AppInquiry x) => (((DateTime?)start).HasValue ? (x.Date.Date >= ((DateTime?)start).Value.Date) : true) && (((DateTime?)end).HasValue ? (x.Date.Date <= ((DateTime?)end).Value.Date) : true), true, "Customer")
				select new AppInquiryModel(x) into x
				orderby x.Date descending
				select x).ToList();
			return Success(data);
		});
	}

	[HttpPost("[action]")]
	public Task<ResponseState<List<AppInquiryModel>>> ListByIds([FromBody] IdList list)
	{
		return TryCatch(async () =>
		{
			List<AppInquiryModel> data = (from x in await _appInquiryService.GetByCondition((AppInquiry x) => list.Ids.Contains(x.Id), false)
				select new AppInquiryModel(x) into x
				orderby x.Date descending
				select x).ToList();
			return Success(data);
		});
	}
}
