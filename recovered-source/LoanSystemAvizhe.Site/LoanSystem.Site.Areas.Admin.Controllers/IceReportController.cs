using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Services.Models.Ice;
using LoanSystem.Site.Areas.Admin.Models;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Services.Sms.Models.MelliPayamak;
using LoanSystemAvizhe.Services.ThirdParty;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoanSystem.Site.Areas.Admin.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Admin")]
public class IceReportController : BaseAdminController
{
    private readonly IIceReportService _iceReportService;

    private readonly IAppSettingsService _appSettingsService;

    private readonly ICurrentUserService _currentUserService;

    private readonly IHostingEnvironment _hostingEnvironment;

    public IceReportController(ILogger<IceReportController> logger, IIceReportService iceReportService, ICurrentUserService currentUserService, IAppSettingsService appSettingsService, IHostingEnvironment hostingEnvironment)
        : base(logger)
    {
        _currentUserService = currentUserService;
        _iceReportService = iceReportService;
        _appSettingsService = appSettingsService;
        _hostingEnvironment = hostingEnvironment;
    }

    [HttpGet("[action]")]
    public Task<ResponseState<List<IceReportModel>>> List(DateTime? start, DateTime? end)
    {
        return TryCatch(async () =>
        {
            List<IceReportModel> data = (from x in await _iceReportService.GetByCondition((IceReport x) => (((DateTime?)start).HasValue ? (x.Date.Date >= ((DateTime?)start).Value.Date) : true) && (((DateTime?)end).HasValue ? (x.Date.Date <= ((DateTime?)end).Value.Date) : true))
                                         select new IceReportModel(x) into x
                                         orderby x.Date descending
                                         select x).ToList();
            return Success(data);
        });
    }

    [HttpGet("[action]/{id}")]
    public Task<ResponseState<Guid>> GetStatus(Guid id)
    {
        return TryCatch(async () =>
        {
            IceReport iceReport = await _iceReportService.GetById(id);
            if (iceReport == null)
            {
                return Error("گزارش یافت نشد.");
            }
            if (iceReport.MemberId != _currentUserService.UserId)
            {
                return Error("دسترسی به گزارش را ندارید");
            }
            IceConfig iceConfig = JsonSerializer.Deserialize<IceConfig>((await _appSettingsService.GetAppSettings()).IceConfigJson);
            IceService iceService = new IceService(iceConfig, _hostingEnvironment.EnvironmentName == "Development");
            IceResult<IceGetStatusResultData> iceResult = await iceService.GetReportStatus(new IceGetStatusRequest
            {
                ReportID = iceReport.ReportID
            });
            if (iceResult.State != 2)
            {
                iceReport.IceReportJson = JsonSerializer.Serialize(iceResult);
                iceReport.Status = iceResult.Data.ReportState.ToString() ?? "";
                iceReport.Url = iceResult.Data?.ReportLink ?? "";
                iceReport.Errors = ((iceResult.Errors?.Any() ?? false) ? string.Join(',', iceResult.Errors) : "");
                await _iceReportService.Update(iceReport);
                return Success(iceReport.Id);
            }
            return Error("خطا در دریافت استعلام وضعیت درخواست از سامانه اعتبارسنجی آیس");
        });
    }

    [HttpPost("[action]")]
    public Task<DefaultResponseState> SetGrade([FromBody] SetIceGradeModel model)
    {
        return TryCatch(async () =>
        {
            IceReport iceReport = await _iceReportService.GetById(model.IceReportId);
            if (iceReport == null)
            {
                return Error("گزارش یافت نشد.");
            }
            iceReport.Result = model.Grade;
            await _iceReportService.Update(iceReport);
            return Success("عملیات با موفقیت انجام شد.");
        });
    }

    [HttpGet("[action]/{id}")]
    public Task<ResponseState<IceReportModel>> Get(Guid id)
    {
        return TryCatch(async () =>
        {
            IceReport iceReport = await _iceReportService.GetById(id);
            if (iceReport == null)
            {
                return Error<IceReportModel>(null, "اطلاعات مورد نظر یافت نشد.");
            }
            IceReportModel data = new IceReportModel(iceReport);
            return Success(data);
        });
    }
}
