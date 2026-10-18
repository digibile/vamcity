using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Services.Models.Ice;
using LoanSystem.Services.Users;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Services.FileHelper;
using LoanSystemAvizhe.Services.Sms.Models.MelliPayamak;
using LoanSystemAvizhe.Services.ThirdParty;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoanSystem.Site.Areas.Member.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Member,Admin,Agent")]
public class IceReportController : BaseMemberController
{
    private readonly IIceReportService _iceReportService;

    private readonly ICurrentUserService _currentUserService;

    private readonly IAppSettingsService _appSettingsService;

    private readonly IHostingEnvironment _hostingEnvironment;

    private readonly UserManager _userManager;

    public IceReportController(ILogger<IceReportController> logger, IIceReportService iceReportService, ICurrentUserService currentUserService, UserManager userManager, IAppSettingsService appSettingsService, IHostingEnvironment hostingEnvironment)
        : base(logger)
    {
        _currentUserService = currentUserService;
        _iceReportService = iceReportService;
        _userManager = userManager;
        _appSettingsService = appSettingsService;
        _hostingEnvironment = hostingEnvironment;
    }

    [HttpGet("[action]/{memberId}")]
    public Task<ResponseState<List<IceReportModel>>> ListByMemberId(long memberId)
    {
        return TryCatch(async () =>
        {
            List<IceReportModel> data = (from x in await _iceReportService.GetByCondition((IceReport x) => (long?)x.MemberId == ((memberId == 0) ? _currentUserService.UserId : ((long?)memberId)))
                                         select new IceReportModel(x) into x
                                         orderby x.Date descending
                                         select x).ToList();
            return Success(data);
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<Guid>> Create()
    {
        return TryCatch<Guid>(async () => Error("سرویس اعتبارسنجی آیس تا اطلاع ثانوی غیرفعال می باشد. در صورتیکه جهت اعتبارسنجی هزینه ای از کیف پول شما کسر شده است با مجموعه ما تماس حاصل فرمایید."));
    }

    [HttpGet("[action]/{id}")]
    public Task<ResponseState<IceReportModel>> GetStatus(Guid id)
    {
        return TryCatch(async () =>
        {
            IceReport iceReport = await _iceReportService.GetById(id);
            if (iceReport == null)
            {
                return Error<IceReportModel>(null, "گزارش یافت نشد.");
            }
            if (iceReport.MemberId != _currentUserService.UserId && !_currentUserService.IsInRole("Admin"))
            {
                return Error<IceReportModel>(null, "مجوز دسترسی به دریافت نتیجه این گزارش را ندارید");
            }
            if (string.IsNullOrEmpty(iceReport.ReportID))
            {
                return Error<IceReportModel>(null, "درخواست به صورت کامل انجام نشده است، لطفا مجددا اقدام به ثبت درخواست نمایید.");
            }
            if (!string.IsNullOrEmpty(iceReport.Url) || !string.IsNullOrEmpty(iceReport.ReportPdfFileName))
            {
                return Error<IceReportModel>(null, "نتیجه گزارش قبلا دریافت شده است.");
            }
            IceConfig iceConfig = JsonSerializer.Deserialize<IceConfig>((await _appSettingsService.GetAppSettings()).IceConfigJson);
            try
            {
                IceService iceService = new IceService(iceConfig, !(_hostingEnvironment.EnvironmentName == "Development") && false);
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
                    if (!string.IsNullOrEmpty(iceResult.Data?.PDFLink))
                    {
                        WebClient webClient = new WebClient();
                        string text = Directories.IceReportPdf(_currentUserService.CustomerId.ToString());
                        if (text != null)
                        {
                            string contentRootPath = _hostingEnvironment.ContentRootPath;
                            string text2 = Path.Combine(contentRootPath, text);
                            if (!Directory.Exists(text2))
                            {
                                Directory.CreateDirectory(text2);
                            }
                            string text3 = Guid.NewGuid().ToString().Replace("-", "") + ".pdf";
                            string fileName = Path.Combine(text2, text3);
                            webClient.DownloadFile(new Uri(iceResult.Data.PDFLink), fileName);
                            iceReport.ReportPdfFileName = text3;
                        }
                    }
                    await _iceReportService.Update(iceReport);
                    return Success(new IceReportModel(iceReport));
                }
                return Error<IceReportModel>(null, "خطا در دریافت استعلام وضعیت درخواست از سامانه اعتبارسنجی آیس");
            }
            catch (Exception)
            {
                throw;
            }
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
