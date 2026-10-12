using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Core.Configuration;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Controllers;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LoanSystem.Site.Controllers;

// Compatibility endpoint for the recovered Vue frontend. Keep its public URL stable.
[ApiController]
[Route("appSettings")]
public sealed class LandingSettingsCompatibilityController : BaseDefaultController
{
    private readonly IAppSettingsService _appSettingsService;
    private readonly ICustomerService _customerService;
    private readonly ICurrentUserService _currentUserService;
    private readonly AppConfig _appConfig;

    public LandingSettingsCompatibilityController(
        ILogger<LandingSettingsCompatibilityController> logger,
        ICustomerService customerService,
        IAppSettingsService appSettingsService,
        ICurrentUserService currentUserService,
        IOptions<AppConfig> appConfig) : base(logger)
    {
        _customerService = customerService;
        _appSettingsService = appSettingsService;
        _currentUserService = currentUserService;
        _appConfig = appConfig.Value;
    }

    [HttpGet("GetLandingSettings/{customerCode?}")]
    public Task<ResponseState<LandingSettingsModel>> GetLandingSettings(int? customerCode)
    {
        return TryCatch(async () =>
        {
            Customer customer;
            if (!customerCode.HasValue || customerCode.Value == 0)
            {
                if (_appConfig.IsCloud)
                    return Success<LandingSettingsModel>();
                customer = (await _customerService.GetAllCustomers()).FirstOrDefault();
            }
            else
            {
                customer = await _customerService.GetByCode(customerCode.Value);
            }

            if (customer == null)
                return Success<LandingSettingsModel>();

            _currentUserService.CustomerId = customer.Id;
            AppSettings appSettings = await _appSettingsService.GetAppSettings(customer.Id);
            if (appSettings == null)
                return Success<LandingSettingsModel>();

            AppSettingsModel model = new AppSettingsModel(appSettings);
            return Success(new LandingSettingsModel
            {
                SiteTheme = appSettings.SiteTheme,
                Address = appSettings.Address,
                Phones = appSettings.Phones,
                Email = appSettings.Email,
                ShopLink = appSettings.ShopLink,
                ShopTitle = model.ShopTitle,
                BusinessTitle = appSettings.BusinessTitle,
                EnamadLink = appSettings.EnamadLink,
                EtehadieLink = appSettings.EtehadieLink,
                SamandehiLink = appSettings.SamandehiLink,
                AnjomanSenfiLink = model.AnjomanSenfiLink,
                ApplicationLink = appSettings.ApplicationLink,
                WhatsappLink = appSettings.WhatsappLink,
                TelegramLink = appSettings.TelegramLink,
                InstagramLink = appSettings.InstagramLink,
                RubikaLink = appSettings.RubikaLink,
                AparatLink = model.AparatLink,
                ThemeSettings = model.ThemeSettings,
                KasboKarhayeInternetiTehranLink = model.KasboKarhayeInternetiTehranLink,
                AgentOfficePhone = model.AgentOfficePhone,
                CalculatorType = model.CalculatorType,
                AgentFactorOnlinePaymentGateway = model.AgentFactorOnlinePaymentGateway,
                InquiryBankInRegister = model.InquiryBankInRegister,
                InquiryPersonInfoInRegister = model.InquiryPersonInfoInRegister,
                InquiryPostalAddressInRegister = model.InquiryPostalAddressInRegister,
                HasRegisterOtpSms = appSettings.RegisterOtpSmsPatternId.HasValue
            });
        });
    }

    [HttpGet("GetThemeSettings/{customerCode?}")]
    public Task<ResponseState<ThemeSettingsModel>> GetThemeSettings(int? customerCode)
    {
        return TryCatch(async () =>
        {
            Customer customer;
            if (!customerCode.HasValue || customerCode.Value == 0)
            {
                if (_appConfig.IsCloud)
                    return Success<ThemeSettingsModel>();
                customer = (await _customerService.GetAllCustomers()).FirstOrDefault();
            }
            else
            {
                customer = await _customerService.GetByCode(customerCode.Value);
            }

            if (customer == null)
                return Success<ThemeSettingsModel>();

            AppSettings appSettings = await _appSettingsService.GetAppSettings(customer.Id);
            if (appSettings == null)
                return Success<ThemeSettingsModel>();

            return Success(new AppSettingsModel(appSettings).ThemeSettings);
        });
    }
}
