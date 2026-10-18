using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LoanSystem.Core.Exceptions;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Services.Models.IranValidity;
using LoanSystem.Services.Users;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Data.Enums;
using LoanSystemAvizhe.Services.FileHelper;
using LoanSystemAvizhe.Services.Models.IranValidity;
using LoanSystemAvizhe.Services.ThirdParty;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoanSystem.Site.Areas.Member.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Member,Admin,Agent")]
public class IranValidityReportController : BaseMemberController
{
    private readonly IIranValidityReportService _iranValidityReportService;

    private readonly ICurrentUserService _currentUserService;

    private readonly IAppSettingsService _appSettingsService;

    private readonly IHostingEnvironment _hostingEnvironment;

    private readonly UserManager _userManager;

    public IranValidityReportController(ILogger<IranValidityReportController> logger, IIranValidityReportService iranValidityReportService, ICurrentUserService currentUserService, UserManager userManager, IAppSettingsService appSettingsService, IHostingEnvironment hostingEnvironment)
        : base(logger)
    {
        _currentUserService = currentUserService;
        _iranValidityReportService = iranValidityReportService;
        _userManager = userManager;
        _appSettingsService = appSettingsService;
        _hostingEnvironment = hostingEnvironment;
    }

    [HttpGet("[action]/{memberId}")]
    public Task<ResponseState<List<IranValidityReportModel>>> ListByMemberId(long memberId)
    {
        return TryCatch(async () =>
        {
            List<IranValidityReportModel> data = (from x in await _iranValidityReportService.GetByCondition((IranValidityReport x) => (long?)x.MemberId == ((memberId == 0) ? _currentUserService.UserId : ((long?)memberId)))
                                                  select new IranValidityReportModel(x) into x
                                                  orderby x.Date descending
                                                  select x).ToList();
            return Success(data);
        });
    }

    [HttpPost("[action]/{userId?}")]
    public Task<ResponseState<Guid>> Create(long? userId)
    {
        return TryCatch(async () =>
        {
            long? id = userId ?? _currentUserService.UserId;
            if (!id.HasValue)
            {
                return Error("دسترسی غیرمجاز");
            }
            if (!_currentUserService.IsInRole("Admin") && !_currentUserService.IsInRole("Agent") && userId.HasValue && userId.Value != _currentUserService.UserId)
            {
                return Error("دسترسی غیرمجاز");
            }
            User user = await _userManager.GetById(id.Value);
            if (user == null)
            {
                return Error("کاربر جاری معتبر نمی باشد");
            }
            if (string.IsNullOrEmpty(user.Mobile))
            {
                return Error("ابتدا شماره موبایل خود را از طریق ویرایش پروفایل ثبت نمایید.");
            }
            AppSettings appSettings = await _appSettingsService.GetAppSettings();
            IranValidityConfig iranValidityConfig = ((!string.IsNullOrEmpty(appSettings.IranValidityConfigJson)) ? JsonSerializer.Deserialize<IranValidityConfig>(appSettings.IranValidityConfigJson) : null);
            if (iranValidityConfig == null)
            {
                return Error("در حال حاضر امکان ثبت درخواست اعتبارسنجی وجود ندارد، تنظیمات اعتبارسنجی توسط ادمین انجام نشده است.");
            }
            if (user.Credit < (decimal)iranValidityConfig.Price)
            {
                return Error("اعتبار کیف پول شما جهت اعتبارسنجی کافی نمی باشد.");
            }
            List<IranValidityReport> list = await _iranValidityReportService.GetByCondition((IranValidityReport x) => (long?)x.MemberId == id && (int)x.Status == 2);
            if (list != null && list.Any())
            {
                return Error("قبلا درخواست اعتبارسنجی دیگری توسط شما صادر شده است و تا اتمام مراحل آن، امکان ثبت درخواست جدید وجود ندارد.");
            }
            try
            {
                IranValidityService iranValidityService = new IranValidityService(iranValidityConfig, _hostingEnvironment.EnvironmentName == "Development");
                IranValidityAddFromFirmResultData iranValidityAddFromFirmResultData = await iranValidityService.FromFirmRequest(new IranValidityFromFirmRequest
                {
                    MobileNumber = user.Mobile,
                    RealPersonNationalCode = user.NationalCode
                });
                IranValidityReport entity = new IranValidityReport
                {
                    Date = DateTime.Now,
                    MemberId = user.Id,
                    Price = iranValidityConfig.Price,
                    ReportID = (iranValidityAddFromFirmResultData.HashCode ?? ""),
                    Status = IranValidityReportStatus.در_انتظار_ثبت_رمز_پیامکی_توسط_کاربر,
                    Messages = "",
                    Url = ""
                };
                await _iranValidityReportService.Insert(entity);
                return Success(entity.Id);
            }
            catch (CustomException ex)
            {
                return Error(ex.Message);
            }
            catch (Exception)
            {
                throw;
            }
        });
    }

    [HttpPost("[action]/{id}")]
    public Task<ResponseState<Guid>> RenewOTP(Guid id)
    {
        return TryCatch(async () =>
        {
            User user = await _userManager.GetById(_currentUserService.UserId.Value);
            if (user == null)
            {
                return Error("کاربر جاری معتبر نمی باشد");
            }
            IranValidityReport iranValidityReport = await _iranValidityReportService.GetById(id);
            if (iranValidityReport == null)
            {
                return Error("گزارش یافت نشد.");
            }
            if (!CheckReportPermission(iranValidityReport.MemberId, user))
            {
                return Error("دسترسی به این گزارش برای شما مجاز نمی باشد.");
            }
            AppSettings appSettings = await _appSettingsService.GetAppSettings();
            IranValidityConfig iranValidityConfig = ((!string.IsNullOrEmpty(appSettings.IranValidityConfigJson)) ? JsonSerializer.Deserialize<IranValidityConfig>(appSettings.IranValidityConfigJson) : null);
            if (iranValidityConfig == null)
            {
                return Error("در حال حاضر امکان ثبت درخواست اعتبارسنجی وجود ندارد، تنظیمات اعتبارسنجی توسط ادمین انجام نشده است.");
            }
            try
            {
                IranValidityService iranValidityService = new IranValidityService(iranValidityConfig, _hostingEnvironment.EnvironmentName == "Development");
                await iranValidityService.RenewOTP(new IranValidityRenewOTPRequest
                {
                    HashCode = iranValidityReport.ReportID
                });
                return Success(iranValidityReport.Id);
            }
            catch (CustomException ex)
            {
                return Error(ex.Message);
            }
            catch (Exception)
            {
                throw;
            }
        });
    }

    [HttpPost("[action]/{id}/{otp}")]
    public Task<ResponseState<Guid>> SubmitOTP(Guid id, string otp)
    {
        return TryCatch(async () =>
        {
            if (string.IsNullOrEmpty(otp))
            {
                return Error("رمز پیامکی وارد نشده است.");
            }
            User user = await _userManager.GetById(_currentUserService.UserId.Value);
            if (user == null)
            {
                return Error("کاربر جاری معتبر نمی باشد");
            }
            IranValidityReport iranValidityReport = await _iranValidityReportService.GetById(id);
            if (iranValidityReport == null)
            {
                return Error("گزارش یافت نشد.");
            }
            if (!CheckReportPermission(iranValidityReport.MemberId, user))
            {
                return Error("دسترسی به این گزارش برای شما مجاز نمی باشد.");
            }
            AppSettings appSettings = await _appSettingsService.GetAppSettings();
            IranValidityConfig iranValidityConfig = ((!string.IsNullOrEmpty(appSettings.IranValidityConfigJson)) ? JsonSerializer.Deserialize<IranValidityConfig>(appSettings.IranValidityConfigJson) : null);
            if (iranValidityConfig == null)
            {
                return Error("در حال حاضر امکان ثبت درخواست اعتبارسنجی وجود ندارد، تنظیمات اعتبارسنجی توسط ادمین انجام نشده است.");
            }
            try
            {
                IranValidityService iranValidityService = new IranValidityService(iranValidityConfig, _hostingEnvironment.EnvironmentName == "Development");
                IranValidityResult<string> iranValidityResult = await iranValidityService.Validate(new IranValidityValidateRequest
                {
                    HashCode = iranValidityReport.ReportID,
                    Token = otp
                });
                if (iranValidityResult.Data == "InProcessing")
                {
                    iranValidityReport.IranValidityReportJson = JsonSerializer.Serialize(iranValidityResult);
                    iranValidityReport.Status = IranValidityReportStatus.در_انتظار_ایجاد_گزارش_توسط_شرکت_رتبه_بندی;
                    await _iranValidityReportService.Update(iranValidityReport);
                    user.Credit -= (decimal)iranValidityConfig.Price;
                    await _userManager.Update(user);
                    return Success(Guid.Empty);
                }
                if (iranValidityResult.HasError)
                {
                    string message = string.Join(',', iranValidityResult.Messages.Select((ResponseMessage x) => x.Message).ToList());
                    return Error(message);
                }
                return Error("خطا در ارسال پیامک اعتبارسنجی");
            }
            catch (CustomException ex)
            {
                return Error(ex.Message);
            }
            catch (Exception)
            {
                throw;
            }
        });
    }

    private bool CheckReportPermission(long memberId, User user)
    {
        if (!_currentUserService.IsInRole("Admin") && !_currentUserService.IsInRole("Agent"))
        {
            return memberId == user.Id;
        }
        return true;
    }

    [HttpGet("[action]/{id}")]
    public Task<ResponseState<IranValidityReportModel>> GetStatus(Guid id)
    {
        return TryCatch(async () =>
        {
            IranValidityReport iranReport = await _iranValidityReportService.GetById(id);
            if (iranReport == null)
            {
                return Error<IranValidityReportModel>(null, "گزارش یافت نشد.");
            }
            if (iranReport.MemberId != _currentUserService.UserId && !_currentUserService.IsInRole("Admin") && !_currentUserService.IsInRole("Agent"))
            {
                return Error<IranValidityReportModel>(null, "مجوز دسترسی به دریافت نتیجه این گزارش را ندارید");
            }
            if (string.IsNullOrEmpty(iranReport.ReportID))
            {
                return Error<IranValidityReportModel>(null, "درخواست به صورت کامل انجام نشده است، لطفا مجددا اقدام به ثبت درخواست نمایید.");
            }
            if (!string.IsNullOrEmpty(iranReport.ReportPdfFileName))
            {
                return Error<IranValidityReportModel>(null, "نتیجه گزارش قبلا دریافت شده است.");
            }
            IranValidityConfig iranValidityConfig = JsonSerializer.Deserialize<IranValidityConfig>((await _appSettingsService.GetAppSettings()).IranValidityConfigJson);
            try
            {
                IranValidityService iranValidityService = new IranValidityService(iranValidityConfig, !(_hostingEnvironment.EnvironmentName == "Development") && false);
                IranValidityStatusResultData iranValidityStatusResultData = await iranValidityService.GetReportStatus(new IranValidityStatusRequest
                {
                    HashCode = iranReport.ReportID
                });
                iranReport.IranValidityReportJson = JsonSerializer.Serialize(iranValidityStatusResultData);
                iranReport.Status = ExtractStatus(iranValidityStatusResultData);
                iranReport.Messages = ((!string.IsNullOrEmpty(iranValidityStatusResultData.ReportLink)) ? "گزارش آماده هست" : iranValidityStatusResultData.StatusTitle);
                iranReport.Url = iranValidityStatusResultData.ReportLink;
                if (!string.IsNullOrEmpty(iranValidityStatusResultData.ReportLink))
                {
                    string text = Directories.IranValidityReportPdf(_currentUserService.CustomerId.ToString());
                    if (text != null)
                    {
                        string contentRootPath = _hostingEnvironment.ContentRootPath;
                        string text2 = Path.Combine(contentRootPath, text);
                        if (!Directory.Exists(text2))
                        {
                            Directory.CreateDirectory(text2);
                        }
                        string fileName = Guid.NewGuid().ToString().Replace("-", "") + ".pdf";
                        string path = Path.Combine(text2, fileName);
                        string hashCode = iranValidityStatusResultData.ReportLink.Substring(iranValidityStatusResultData.ReportLink.LastIndexOf('/'));
                        IranValidityPdfReportData iranValidityPdfReportData = await iranValidityService.GetReportPDF(new IranValidityStatusRequest
                        {
                            HashCode = hashCode
                        });
                        if (iranValidityPdfReportData.Data.Length != 0)
                        {
                            System.IO.File.WriteAllBytes(path, iranValidityPdfReportData.Data);
                        }
                        iranReport.ReportPdfFileName = fileName;
                    }
                }
                if (iranReport.Status == IranValidityReportStatus.گزارش_آماده_شده)
                {
                    iranReport.Messages = "";
                }
                await _iranValidityReportService.Update(iranReport);
                return Success(new IranValidityReportModel(iranReport));
            }
            catch (CustomException ex)
            {
                iranReport.Messages = ex.Message;
                await _iranValidityReportService.Update(iranReport);
                return Error<IranValidityReportModel>(null, "خطا در دریافت استعلام وضعیت درخواست از سامانه اعتبارسنجی ایران");
            }
            catch (Exception)
            {
                throw;
            }
        });
    }

    private IranValidityReportStatus ExtractStatus(IranValidityStatusResultData r)
    {
        if (!string.IsNullOrEmpty(r.ReportLink))
        {
            return IranValidityReportStatus.گزارش_آماده_شده;
        }
        switch (r.Status)
        {
            case "OTPSent":
            case "OTPLocked":
                return IranValidityReportStatus.در_انتظار_ثبت_رمز_پیامکی_توسط_کاربر;
            case "OTPAccepted":
            case "IdentityCheckAccepted":
                return IranValidityReportStatus.در_انتظار_ایجاد_گزارش_توسط_شرکت_رتبه_بندی;
            case "OTPCheckFailed":
            case "RequestExpired":
            case "NotMatchingRealPersonNationalCodeAndMobileNumber":
            case "NotMatchingRealPersonAndLegalPerson":
            case "ReportGeneratedButDataLost":
            case "ReportGenerationFailed":
                return IranValidityReportStatus.خطا_در_ایجاد_گزارش_توسط_شرکت_رتبه_بندی;
            default:
                return IranValidityReportStatus.در_انتظار_ایجاد_گزارش_توسط_شرکت_رتبه_بندی;
        }
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
