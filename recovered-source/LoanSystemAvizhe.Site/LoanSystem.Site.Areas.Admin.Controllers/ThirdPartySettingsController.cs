using System;
using System.Text.Json;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Data.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoanSystem.Site.Areas.Admin.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Admin")]
public class ThirdPartySettingsController : BaseAdminController
{
    private readonly IThirdPartySettingsService _thirdPartySettingsService;

    private readonly ICurrentUserService _currentUserService;

    private readonly ICustomerService _customerService;

    public ThirdPartySettingsController(ILogger<ThirdPartySettingsController> logger, IThirdPartySettingsService thirdPartySettingsService, ICurrentUserService currentUserService, ICustomerService customerService)
        : base(logger)
    {
        _currentUserService = currentUserService;
        _customerService = customerService;
        _thirdPartySettingsService = thirdPartySettingsService;
    }

    [HttpGet("[action]")]
    public Task<ResponseState<ThirdPartySettingsModel>> Get()
    {
        return TryCatch(async () =>
        {
            ThirdPartySettingsModel data = new ThirdPartySettingsModel(await _thirdPartySettingsService.GetThirdPartySettings());
            return Success(data);
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<Guid>> Edit([FromBody] ThirdPartySettingsModel model)
    {
        return TryCatch(async () =>
        {
            if (string.IsNullOrEmpty(model.LandaSettings.Token))
            {
                return Error("توکن لاندا ثبت نشده است.");
            }
            ThirdPartySettings entity = await _thirdPartySettingsService.GetThirdPartySettings();
            if (entity == null)
            {
                entity = new ThirdPartySettings();
            }
            entity.DefaultThirdParty = model.DefaultThirdParty;
            if (model.LandaSettings != null)
            {
                entity.LandaSettingsJson = JsonSerializer.Serialize(model.LandaSettings);
            }
            else
            {
                entity.LandaSettingsJson = "";
            }
            if (model.DigibileSettings != null)
            {
                entity.DigibileSettingsJson = JsonSerializer.Serialize(model.DigibileSettings);
            }
            else
            {
                entity.DigibileSettingsJson = "";
            }
            await _thirdPartySettingsService.Update(entity);
            return Success(entity.Id, "عملیات با موفقیت انجام شد.");
        });
    }

    [HttpGet("[action]/{thirdParty}")]
    public Task<ResponseState<Guid>> SetDefault(ThirdPart thirdParty)
    {
        return TryCatch(async () =>
        {
            ThirdPartySettings entity = await _thirdPartySettingsService.GetThirdPartySettings();
            if (entity == null)
            {
                entity = new ThirdPartySettings();
            }
            entity.DefaultThirdParty = thirdParty;
            await _thirdPartySettingsService.Update(entity);
            return Success(entity.Id, "عملیات با موفقیت انجام شد.");
        });
    }
}
