using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Core.Configuration;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Data.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LoanSystem.Site.Areas.Admin.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Admin")]
public class SmsPatternController : BaseAdminController
{
    private readonly ISmsPatternService _smsPatternService;

    private readonly ICurrentUserService _currentUserService;

    private readonly IAppSettingsService _appSettingsService;

    private readonly IOptions<AppConfig> _appConfig;

    public SmsPatternController(ILogger<SmsPatternController> logger, ISmsPatternService smsPatternService, ICurrentUserService currentUserService, IAppSettingsService appSettingsService, IOptions<AppConfig> appConfig)
        : base(logger)
    {
        _smsPatternService = smsPatternService;
        _appConfig = appConfig;
        _currentUserService = currentUserService;
        _appSettingsService = appSettingsService;
    }

    [HttpGet("[action]")]
    public Task<ResponseState<List<SmsPatternModel>>> List()
    {
        return TryCatch(async () =>
        {
            List<SmsPatternModel> data = (await _smsPatternService.GetAll(_currentUserService.CustomerId, null)).Select((SmsPattern x) => new SmsPatternModel(x)).ToList();
            return Success(data);
        });
    }

    [HttpGet("[action]/{id}")]
    public Task<ResponseState<SmsPatternModel>> Get(Guid id)
    {
        return TryCatch(async () =>
        {
            SmsPattern smsPattern = await _smsPatternService.GetById(id);
            if (smsPattern == null)
            {
                return Error<SmsPatternModel>(null, "اطلاعات مورد نظر یافت نشد.");
            }
            SmsPatternModel data = new SmsPatternModel(smsPattern);
            return Success(data);
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<SmsPattern>> Create([FromBody] SmsPatternModel model)
    {
        return TryCatch(async () =>
        {
            AppSettings appSettings = await _appSettingsService.GetAppSettings();
            if (appSettings == null || appSettings.DefaultSmsPanel == SmsPanel.Unknown)
            {
                return Error<SmsPattern>(null, "پنل پیامکی تعیین نشده است، لطفا ابتدا پنل پیامکی را تعیین کنید");
            }
            SmsPattern entity = new SmsPattern
            {
                Title = model.Title,
                Pattern = model.Pattern,
                CustomerId = _currentUserService.CustomerId.Value,
                IsSystem = false,
                IsPatternBase = model.IsPatternBase,
                PatternBaseCode = model.PatternBaseCode,
                SmsPatternPanel = model.SmsPatternPanel,
                IppanelKeysJson = model.IppanelKeysJson,
                Category = model.Category,
                Status = (((appSettings.DefaultSmsPanel != SmsPanel.JetAqsaat && appSettings.DefaultSmsPanel != SmsPanel.Unknown) || !_appConfig.Value.IsCloud) ? SmsPatternStatus.Approved : SmsPatternStatus.Pending)
            };
            await _smsPatternService.Insert(entity);
            return Success(entity, "عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<SmsPattern>> Edit([FromBody] SmsPatternModel model)
    {
        return TryCatch(async () =>
        {
            if (model.IsSystem)
            {
                return Error<SmsPattern>(null, "قالب سیستمی قابل تغییر نمی باشد.");
            }
            SmsPattern entity = await _smsPatternService.GetById(model.Id);
            if (entity == null)
            {
                return Error<SmsPattern>(null, "اطلاعات مورد نظر یافت نشد.");
            }
            entity.Title = model.Title;
            entity.Pattern = model.Pattern;
            entity.IsPatternBase = model.IsPatternBase;
            entity.PatternBaseCode = model.PatternBaseCode;
            entity.IppanelKeysJson = model.IppanelKeysJson;
            entity.Category = model.Category;
            entity.SmsPatternPanel = model.SmsPatternPanel;
            entity.Status = ((!_appConfig.Value.IsCloud) ? SmsPatternStatus.Approved : SmsPatternStatus.Pending);
            await _smsPatternService.Update(entity);
            return Success(entity, "عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]/{id}")]
    public Task<DefaultResponseState> Delete(Guid id)
    {
        return TryCatch(async () =>
        {
            AppSettings appSettings = await _appSettingsService.GetAppSettings();
            if (appSettings.ThanksPaymentSmsPatternId == id || appSettings.DueDateRememberSmsPatternId == id)
            {
                return Error("این قالب در تنظیمات اپلیکیشن استفاده شده است، جهت حذف ایتدا باید از تنظیمات خارج شود.");
            }
            SmsPattern smsPattern = await _smsPatternService.GetById(id);
            if (smsPattern == null)
            {
                return Error("امکان حذف وجود ندارد. لطفا بعدا دوباره تلاش نمایید");
            }
            await _smsPatternService.Delete(smsPattern);
            return Success();
        });
    }
}
