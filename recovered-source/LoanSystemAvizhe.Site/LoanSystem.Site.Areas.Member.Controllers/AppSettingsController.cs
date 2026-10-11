using System.Text.Json;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Data.Enums;
using LoanSystemAvizhe.Services.Models.IranValidity;
using LoanSystemAvizhe.Services.Sms.Models.MelliPayamak;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoanSystem.Site.Areas.Member.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Member")]
public class AppSettingsController : BaseMemberController
{
	private readonly IAppSettingsService _appSettingsService;

	public AppSettingsController(ILogger<AppSettingsController> logger, IAppSettingsService appSettingsService)
		: base(logger)
	{
		_appSettingsService = appSettingsService;
	}

	[HttpGet("[action]")]
	public Task<ResponseState<AppSettingsForMemberPanelModel>> Get()
	{
		return TryCatch(async () =>
		{
			AppSettings appSettings = await _appSettingsService.GetAppSettings();
			AppSettingsForMemberPanelModel appSettingsForMemberPanelModel = new AppSettingsForMemberPanelModel();
			if (appSettings != null)
			{
				appSettingsForMemberPanelModel.IsOnlinePaymentActive = appSettings?.IsOnlinePaymentActive ?? false;
				appSettingsForMemberPanelModel.BankInfo = appSettings.BankInfo;
				appSettingsForMemberPanelModel.IcePrice = ((!string.IsNullOrEmpty(appSettings.IceConfigJson)) ? (JsonSerializer.Deserialize<IceConfig>(appSettings.IceConfigJson)?.Price ?? 0) : 0);
				appSettingsForMemberPanelModel.Ice_OnlinePaymentGateway = ((!string.IsNullOrEmpty(appSettings.IceConfigJson)) ? (JsonSerializer.Deserialize<IceConfig>(appSettings.IceConfigJson)?.IcePaymentGateway ?? OnlinePaymentGateways.None) : OnlinePaymentGateways.None);
				appSettingsForMemberPanelModel.IranValidityPrice = ((!string.IsNullOrEmpty(appSettings.IranValidityConfigJson)) ? (JsonSerializer.Deserialize<IranValidityConfig>(appSettings.IranValidityConfigJson)?.Price ?? 0) : 0);
				appSettingsForMemberPanelModel.IranValidity_OnlinePaymentGateway = ((!string.IsNullOrEmpty(appSettings.IranValidityConfigJson)) ? (JsonSerializer.Deserialize<IranValidityConfig>(appSettings.IranValidityConfigJson)?.PaymentGateway ?? OnlinePaymentGateways.None) : OnlinePaymentGateways.None);
				appSettingsForMemberPanelModel.ShowSubmitFirstLoanRequestNotifInMemberPanel = appSettings.ShowSubmitFirstLoanRequestNotifInMemberPanel;
				appSettingsForMemberPanelModel.CalculatorType = appSettings.CalculatorType;
			}
			return Success(appSettingsForMemberPanelModel);
		});
	}
}
