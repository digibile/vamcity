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

namespace LoanSystem.Site.Areas.Agent.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Agent")]
public class AppSettingsController : BaseAgentController
{
	private readonly IAppSettingsService _appSettingsService;

	public AppSettingsController(ILogger<AppSettingsController> logger, IAppSettingsService appSettingsService)
		: base(logger)
	{
		_appSettingsService = appSettingsService;
	}

	[HttpGet("[action]")]
	public Task<ResponseState<AppSettingsForAgentPanelModel>> Get()
	{
		return TryCatch(async () =>
		{
			AppSettings appSettings = await _appSettingsService.GetAppSettings();
			AppSettingsForAgentPanelModel appSettingsForAgentPanelModel = new AppSettingsForAgentPanelModel();
			if (appSettings != null)
			{
				appSettingsForAgentPanelModel.IranValidityPrice = ((!string.IsNullOrEmpty(appSettings.IranValidityConfigJson)) ? (JsonSerializer.Deserialize<IranValidityConfig>(appSettings.IranValidityConfigJson)?.Price ?? 0) : 0);
				appSettingsForAgentPanelModel.CalculatorType = appSettings.CalculatorType;
				appSettingsForAgentPanelModel.AgentFactorCommission = appSettings.AgentFactorCommission;
				appSettingsForAgentPanelModel.MaxAgentFactorCommission = appSettings.MaxAgentFactorCommission;
				appSettingsForAgentPanelModel.InquiryBankInRegister = appSettings.InquiryBankInRegister;
				appSettingsForAgentPanelModel.InquiryPersonInfoInRegister = appSettings.InquiryPersonInfoInRegister;
				appSettingsForAgentPanelModel.InquiryPostalAddressInRegister = appSettings.InquiryPostalAddressInRegister;
				appSettingsForAgentPanelModel.HasRegisterOtpSms = appSettings.RegisterOtpSmsPatternId.HasValue;
			}
			return Success(appSettingsForAgentPanelModel);
		});
	}
}
