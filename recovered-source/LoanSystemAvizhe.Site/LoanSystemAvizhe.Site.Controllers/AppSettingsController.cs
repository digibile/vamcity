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

namespace LoanSystemAvizhe.Site.Controllers;

[ApiController]
[Route("[controller]")]
public class AppSettingsController : BaseDefaultController
{
    private readonly IAppSettingsService _appSettingsService;

    private readonly ICustomerService _customerService;

    private readonly ICurrentUserService _currentUserService;

    private readonly AppConfig _appConfig;

    public AppSettingsController(ILogger<AppSettingsController> logger, ICustomerService customerService, IAppSettingsService appSettingsService, ICurrentUserService currentUserService, IOptions<AppConfig> appConfig)
        : base(logger)
    {
        _appSettingsService = appSettingsService;
        _customerService = customerService;
        _currentUserService = currentUserService;
        _appConfig = appConfig.Value;
    }

    [HttpGet("[action]/{customerCode?}")]
    public Task<ResponseState<ThemeSettingsModel>> GetThemeSettings(int? customerCode)
    {
        return TryCatch(async () =>
        {
            Customer customer;
            if (!customerCode.HasValue || customerCode.Value == 0)
            {
                if (_appConfig.IsCloud)
                {
                    return Success<ThemeSettingsModel>();
                }
                customer = (await _customerService.GetAllCustomers()).FirstOrDefault();
            }
            else
            {
                customer = await _customerService.GetByCode(customerCode.Value);
            }
            if (customer == null)
            {
                return Success<ThemeSettingsModel>();
            }
            AppSettings appSettings = await _appSettingsService.GetAppSettings(customer.Id);
            if (appSettings == null)
            {
                return Success<ThemeSettingsModel>();
            }
            AppSettingsModel appSettingsModel = new AppSettingsModel(appSettings);
            return Success(appSettingsModel.ThemeSettings);
        });
    }

    [HttpGet("[action]/{customerCode?}")]
    public Task<ResponseState<LandingSettingsModel>> GetLandingSettings(int? customerCode)
    {
        return TryCatch(async () =>
        {
            Customer customer;
            if (!customerCode.HasValue || customerCode.Value == 0)
            {
                if (_appConfig.IsCloud)
                {
                    return Success<LandingSettingsModel>();
                }
                customer = (await _customerService.GetAllCustomers()).FirstOrDefault();
            }
            else
            {
                customer = await _customerService.GetByCode(customerCode.Value);
            }
            if (customer == null)
            {
                return Success<LandingSettingsModel>();
            }
            _currentUserService.CustomerId = customer.Id;
            AppSettings appSettings = await _appSettingsService.GetAppSettings(customer.Id);
            if (appSettings == null)
            {
                return Success<LandingSettingsModel>();
            }
            AppSettingsModel appSettingsModel = new AppSettingsModel(appSettings);
            LandingSettingsModel data = new LandingSettingsModel
            {
                SiteTheme = appSettings.SiteTheme,
                Address = appSettings.Address,
                Phones = appSettings.Phones,
                Email = appSettings.Email,
                ShopLink = appSettings.ShopLink,
                ShopTitle = appSettingsModel.ShopTitle,
                BusinessTitle = appSettings.BusinessTitle,
                EnamadLink = appSettings.EnamadLink,
                EtehadieLink = appSettings.EtehadieLink,
                SamandehiLink = appSettings.SamandehiLink,
                AnjomanSenfiLink = appSettingsModel.AnjomanSenfiLink,
                ApplicationLink = appSettings.ApplicationLink,
                WhatsappLink = appSettings.WhatsappLink,
                TelegramLink = appSettings.TelegramLink,
                InstagramLink = appSettings.InstagramLink,
                RubikaLink = appSettings.RubikaLink,
                AparatLink = appSettingsModel.AparatLink,
                ThemeSettings = appSettingsModel.ThemeSettings,
                KasboKarhayeInternetiTehranLink = appSettingsModel.KasboKarhayeInternetiTehranLink,
                AgentOfficePhone = appSettingsModel.AgentOfficePhone,
                CalculatorType = appSettingsModel.CalculatorType,
                AgentFactorOnlinePaymentGateway = appSettingsModel.AgentFactorOnlinePaymentGateway,
                InquiryBankInRegister = appSettingsModel.InquiryBankInRegister,
                InquiryPersonInfoInRegister = appSettingsModel.InquiryPersonInfoInRegister,
                InquiryPostalAddressInRegister = appSettingsModel.InquiryPostalAddressInRegister,
                HasRegisterOtpSms = appSettings.RegisterOtpSmsPatternId.HasValue
            };
            return Success(data);
        });
    }
}
