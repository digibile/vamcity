using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Areas.Admin.Models;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Services.ThirdParty;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoanSystem.Site.Areas.Admin.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Admin")]
public class IranValidityReportController : BaseAdminController
{
	private readonly IIranValidityReportService _iranValidityReportService;

	private readonly IAppSettingsService _appSettingsService;

	private readonly ICurrentUserService _currentUserService;

	private readonly IHostingEnvironment _hostingEnvironment;

	public IranValidityReportController(ILogger<IranValidityReportController> logger, IIranValidityReportService iranValidityReportService, ICurrentUserService currentUserService, IAppSettingsService appSettingsService, IHostingEnvironment hostingEnvironment)
		: base(logger)
	{
		_currentUserService = currentUserService;
		_iranValidityReportService = iranValidityReportService;
		_appSettingsService = appSettingsService;
		_hostingEnvironment = hostingEnvironment;
	}

	[HttpGet("[action]")]
	public Task<ResponseState<List<IranValidityReportModel>>> List(DateTime? start, DateTime? end)
	{
		return TryCatch(async () =>
		{
			List<IranValidityReportModel> data = (from x in await _iranValidityReportService.GetByCondition((IranValidityReport x) => x.CustomerId == _currentUserService.CustomerId && (((DateTime?)start).HasValue ? (x.Date.Date >= ((DateTime?)start).Value.Date) : true) && (((DateTime?)end).HasValue ? (x.Date.Date <= ((DateTime?)end).Value.Date) : true), "Member")
				select new IranValidityReportModel(x) into x
				orderby x.Date descending
				select x).ToList();
			return Success(data);
		});
	}

	[HttpPost("[action]")]
	public Task<DefaultResponseState> SetGrade([FromBody] SetIranValidityGradeModel model)
	{
		return TryCatch(async () =>
		{
			IranValidityReport iranValidityReport = await _iranValidityReportService.GetById(model.ReportId);
			if (iranValidityReport == null)
			{
				return Error("گزارش یافت نشد.");
			}
			iranValidityReport.Result = model.Grade;
			iranValidityReport.GradeNumber = model.GradeNumber;
			await _iranValidityReportService.Update(iranValidityReport);
			return Success("عملیات با موفقیت انجام شد.");
		});
	}

	[HttpGet("[action]/{id}")]
	public Task<ResponseState<IranValidityReportModel>> Get(Guid id)
	{
		return TryCatch(async () =>
		{
			IranValidityReport iranValidityReport = await _iranValidityReportService.GetById(id);
			if (iranValidityReport == null)
			{
				return Error<IranValidityReportModel>(null, "اطلاعات مورد نظر یافت نشد.");
			}
			IranValidityReportModel data = new IranValidityReportModel(iranValidityReport);
			return Success(data);
		});
	}
}
