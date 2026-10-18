using System;
using System.Text.Json;
using System.Threading.Tasks;
using LoanSystem.Core;
using LoanSystem.Core.Exceptions;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Users;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Services.Models.RefahBeta;
using LoanSystemAvizhe.Services.ThirdParty;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LoanSystem.Site.Areas.Member.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Member,Admin,Agent")]
public class UserRefahBetaController : BaseMemberController
{
    private readonly ICurrentUserService _currentUserService;

    private readonly IAppSettingsService _appSettingsService;

    private readonly IUserRefahBetaService _userRefahBetaService;

    private readonly UserManager _userManager;

    private readonly IServiceProvider _serviceProvider;

    public UserRefahBetaController(ILogger<UserRefahBetaController> logger, ICurrentUserService currentUserService, IAppSettingsService appSettingsService, IUserRefahBetaService userRefahBetaService, IServiceProvider serviceProvider, UserManager userManager)
        : base(logger)
    {
        _currentUserService = currentUserService;
        _appSettingsService = appSettingsService;
        _userRefahBetaService = userRefahBetaService;
        _userManager = userManager;
        _serviceProvider = serviceProvider;
    }

    [HttpPost("[action]/{nationalCode}")]
    public Task<ResponseState<long>> GetInquiry(string nationalCode)
    {
        return TryCatch(async () =>
        {
            UserRefahBeta check = await _userRefahBetaService.GetByNationalCode(nationalCode.ToString());
            if (check != null && check.IsSuccess && check.ModifiedAt < DateTime.Now.AddMinutes(-30.0))
            {
                return Success(check.Amount);
            }
            AppSettings appSettings = await _appSettingsService.GetAppSettings();
            RefahBetaConfig refahBetaConfig = ((!string.IsNullOrEmpty(appSettings.RefahBetaConfigJson)) ? JsonSerializer.Deserialize<RefahBetaConfig>(appSettings.RefahBetaConfigJson) : null);
            if (refahBetaConfig == null)
            {
                return Error(0L, "در حال حاضر امکان استعلام توان بازپرداخت از بانک رفاه وجود ندارد.");
            }
            try
            {
                ILoggerFactory requiredService = _serviceProvider.GetRequiredService<ILoggerFactory>();
                ILogger<RefahBetaService> logger = requiredService.CreateLogger<RefahBetaService>();
                RefahBetaService refahBetaService = new RefahBetaService(refahBetaConfig, logger);
                RefahBetaResult<RefahBetaInquiryResultData> refahBetaResult = await refahBetaService.InquiryRequest(new RefahBetaInquiryRequest
                {
                    NationalCode = nationalCode
                });
                if (check == null)
                {
                    check = new UserRefahBeta
                    {
                        ModifiedAt = DateTime.Now,
                        UserId = _currentUserService.UserId.Value
                    };
                }
                if (refahBetaResult.Status != 200)
                {
                    return Error(0L, "پاسخ دریافت شده از بانک رفاه: " + refahBetaResult.Message);
                }
                check.IsSuccess = refahBetaResult.Status == 200;
                check.Amount = ((refahBetaResult.Status == 200) ? (refahBetaResult.Data.credit / 10) : check.Amount);
                check.Message = refahBetaResult.Message;
                if (!(check.Id == Guid.Empty))
                {
                    await _userRefahBetaService.Update(check);
                }
                else
                {
                    await _userRefahBetaService.Insert(check);
                }
                return Success(check.Amount);
            }
            catch (CustomException ex)
            {
                return Error(0L, ex.Message);
            }
            catch (Exception)
            {
                throw;
            }
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<string>> SendOtp([FromBody] OtpRequest otpRequest)
    {
        return TryCatch(async () =>
        {
            if (_currentUserService.IsInRole("Member") && (await _userManager.GetById(_currentUserService.UserId.Value)).NationalCode != otpRequest.NationalCode)
            {
                return Error<string>(null, "مجوز دسترسی به این کاربر را ندارید.");
            }
            AppSettings appSettings = await _appSettingsService.GetAppSettings();
            RefahBetaConfig refahBetaConfig = ((!string.IsNullOrEmpty(appSettings.RefahBetaConfigJson)) ? JsonSerializer.Deserialize<RefahBetaConfig>(appSettings.RefahBetaConfigJson) : null);
            if (refahBetaConfig == null)
            {
                return Error<string>(null, "در حال حاضر امکان ارسال رمز پیامکی از بانک رفاه وجود ندارد.");
            }
            try
            {
                ILoggerFactory requiredService = _serviceProvider.GetRequiredService<ILoggerFactory>();
                ILogger<RefahBetaService> logger = requiredService.CreateLogger<RefahBetaService>();
                RefahBetaService refahBetaService = new RefahBetaService(refahBetaConfig, logger);
                DateTime startDate = ((DateTime.Now.ToPersianDateObject().Day <= 15) ? DateTimeExtentions.GetStartEndOfMonthInPersian(DateTime.Now).Item2.ToGregorianDateTime().Value : DateTimeExtentions.GetEndOfNextPersianMonth(DateTime.Now));
                RefahBetaResult<string> refahBetaResult = await refahBetaService.OtpRequest(new OtpRequest
                {
                    Title = "خرید اقساطی کالا خدمات " + otpRequest.Title.Replace('\r', ' ').Replace("\n", ""),
                    StartDate = startDate,
                    Amount = otpRequest.Amount * 10,
                    NationalCode = otpRequest.NationalCode,
                    NumberOfInstallments = otpRequest.NumberOfInstallments,
                    StartDateYearMonth = startDate.ToString("yyyyMM")
                });
                if (refahBetaResult.Status != 200)
                {
                    return Error<string>(null, $"خطا در ارسال پیامک از سامانه بتا رفاه. کد خطا: [{refahBetaResult.Status}] شرح: " + refahBetaResult.Message);
                }
                return Success(refahBetaResult.Data, "");
            }
            catch (CustomException ex)
            {
                return Error<string>(null, ex.Message);
            }
            catch (Exception)
            {
                throw;
            }
        });
    }
}
