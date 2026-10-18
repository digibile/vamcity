using System.Threading.Tasks;
using LoanSystem.Core.Configuration;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Site.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LoanSystem.Site.Controllers;

[Route("[controller]")]
public class SiteController : BaseDefaultController
{
    private readonly AppConfig _appConfig;

    public SiteController(ILogger<SiteController> logger, IOptions<AppConfig> appConfig)
        : base(logger)
    {
        _appConfig = appConfig.Value;
    }

    [HttpPost("[action]")]
    public Task<ResponseState<AppConfigDto>> SiteConfig()
    {
        return TryCatch(async () =>
        {
            AppConfigDto data = new AppConfigDto
            {
                CustomerCode = _appConfig.CustomerCode,
                IsCloud = _appConfig.IsCloud,
                CloudPrice = _appConfig.CloudPrice,
                PermanentPrice = _appConfig.PermanentPrice,
                BaseUrl = _appConfig.BaseUrl,
                TitlePart1 = _appConfig.TitlePart1,
                TitlePart2 = _appConfig.TitlePart2,
                FullTitle = _appConfig.FullTitle,
                LongTitle = _appConfig.LongTitle,
                RouteToPlansAsDefault = _appConfig.RouteToPlansAsDefault,
                ShowSponsoreCheck = _appConfig.ShowSponsoreCheck,
                ActiveParts = _appConfig.ActiveParts
            };
            return Success(data);
        });
    }

    [HttpPost("[action]")]
    public ActionResult KeepAlive()
    {
        return Ok();
    }
}
