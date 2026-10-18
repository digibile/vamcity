using System.Text.Json;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Services.Models.IranValidity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoanSystem.Site.Areas.Sponsor.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Sponsor")]
public class AppSettingsController : BaseSponsorController
{
    private readonly IAppSettingsService _appSettingsService;

    public AppSettingsController(ILogger<AppSettingsController> logger, IAppSettingsService appSettingsService)
        : base(logger)
    {
        _appSettingsService = appSettingsService;
    }

    [HttpGet("[action]")]
    public Task<ResponseState<AppSettingsForSponsorPanelModel>> Get()
    {
        return TryCatch(async () =>
        {
            AppSettings appSettings = await _appSettingsService.GetAppSettings();
            AppSettingsForSponsorPanelModel appSettingsForSponsorPanelModel = new AppSettingsForSponsorPanelModel();
            if (appSettings != null)
            {
                appSettingsForSponsorPanelModel.IranValidityPrice = ((!string.IsNullOrEmpty(appSettings.IranValidityConfigJson)) ? (JsonSerializer.Deserialize<IranValidityConfig>(appSettings.IranValidityConfigJson)?.Price ?? 0) : 0);
                appSettingsForSponsorPanelModel.CalculatorType = appSettings.CalculatorType;
            }
            return Success(appSettingsForSponsorPanelModel);
        });
    }
}
