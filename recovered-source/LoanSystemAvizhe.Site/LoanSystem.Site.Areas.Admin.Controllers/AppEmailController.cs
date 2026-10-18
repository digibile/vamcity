using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Core.Configuration;
using LoanSystem.Core.Exceptions;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Areas.Admin.Models;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LoanSystem.Site.Areas.Admin.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Admin")]
public class AppEmailController : BaseAdminController
{
    private readonly IAppEmailService _appEmailService;

    private readonly ICurrentUserService _currentUserService;

    private readonly IntegratedEmailSenderService _integratedEmailSenderService;

    private readonly AppConfig _appConfig;

    private readonly IUserPaymentService _userPaymentService;

    private readonly IMemberService _memberService;

    public AppEmailController(ILogger<AppEmailController> logger, IAppEmailService appEmailService, ICurrentUserService currentUserService, IntegratedEmailSenderService integratedEmailSenderService, IUserPaymentService userPaymentService, IOptions<AppConfig> appConfig, IMemberService memberService)
        : base(logger)
    {
        _currentUserService = currentUserService;
        _appEmailService = appEmailService;
        _integratedEmailSenderService = integratedEmailSenderService;
        _userPaymentService = userPaymentService;
        _appConfig = appConfig.Value;
        _memberService = memberService;
    }

    [HttpGet("[action]")]
    public Task<ResponseState<List<AppEmailModel>>> List(DateTime? start, DateTime? end, string email)
    {
        return TryCatch(async () =>
        {
            List<AppEmailModel> data = (from x in await _appEmailService.GetByCondition((AppEmail x) => (((DateTime?)start).HasValue ? (x.Date.Date >= ((DateTime?)start).Value.Date) : true) && (((DateTime?)end).HasValue ? (x.Date.Date <= ((DateTime?)end).Value.Date) : true) && ((!string.IsNullOrEmpty(email)) ? (x.To == email) : true), false)
                                        select new AppEmailModel(x) into x
                                        orderby x.Date descending
                                        select x).ToList();
            return Success(data);
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<List<AppEmailModel>>> ListByIds([FromBody] IdList list)
    {
        return TryCatch(async () =>
        {
            List<AppEmailModel> data = (from x in await _appEmailService.GetByCondition((AppEmail x) => list.Ids.Contains(x.Id), false)
                                        select new AppEmailModel(x) into x
                                        orderby x.Date descending
                                        select x).ToList();
            return Success(data);
        });
    }

    [HttpGet("[action]/{id}")]
    public Task<ResponseState<AppEmailModel>> Get(Guid id)
    {
        return TryCatch(async () =>
        {
            AppEmail appEmail = await _appEmailService.GetById(id);
            if (appEmail == null)
            {
                return Error<AppEmailModel>(null, "اطلاعات مورد نظر یافت نشد.");
            }
            AppEmailModel data = new AppEmailModel(appEmail);
            return Success(data);
        });
    }

    [HttpGet("[action]/{id}")]
    public Task<ResponseState<List<AppEmailModel>>> ListByEntityId(Guid id)
    {
        return TryCatch(async () =>
        {
            List<AppEmailModel> data = (from x in await _appEmailService.GetByCondition((AppEmail x) => x.EntityId == id, false)
                                        select new AppEmailModel(x) into x
                                        orderby x.Date descending
                                        select x).ToList();
            return Success(data);
        });
    }

    [HttpPost("[action]/{id}")]
    public Task<ResponseState<AppEmailModel>> ReSend(Guid id)
    {
        return TryCatch(async () =>
        {
            AppEmail appEmail = await _appEmailService.GetById(id);
            if (appEmail == null)
            {
                return Error<AppEmailModel>(null, "اطلاعات مورد نظر یافت نشد.");
            }
            try
            {
                AppEmail appEmail2 = await _integratedEmailSenderService.ReSend(appEmail);
                if (appEmail2 != null)
                {
                    return Success(new AppEmailModel(appEmail2), "عملیات با موفقیت انجام شد.");
                }
            }
            catch (CustomException ex)
            {
                return Error<AppEmailModel>(null, ex.Message);
            }
            return Error<AppEmailModel>(null, "ایمیل ارسال نشد");
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<AppEmailModel>> SendForUserLoanPayment(Guid id, string type)
    {
        return TryCatch(async () =>
        {
            UserLoanPayment userLoanPayment = await _userPaymentService.GetById(id);
            if (userLoanPayment == null)
            {
                return Error<AppEmailModel>(null, "اطلاعات مورد نظر یافت نشد.");
            }
            try
            {
                AppEmail appEmail;
                switch (type)
                {
                    case "DueDate":
                        appEmail = await _integratedEmailSenderService.SendDueDate(new LoanPaymentModel(userLoanPayment));
                        break;
                    case "Thanks":
                        appEmail = await _integratedEmailSenderService.SendThanks(new LoanPaymentModel(userLoanPayment));
                        break;
                    case "OverDueDate":
                        appEmail = await _integratedEmailSenderService.SendOverDueDate(new LoanPaymentModel(userLoanPayment));
                        break;
                    case "BeforeDueDate":
                        appEmail = await _integratedEmailSenderService.SendBeforeDueDate(new LoanPaymentModel(userLoanPayment));
                        break;
                    default:
                        return Error<AppEmailModel>(null, "نوع ایمیل معتبر نمی باشد");
                }
                if (appEmail != null)
                {
                    return Success(new AppEmailModel(appEmail), "عملیات با موفقیت انجام شد.");
                }
            }
            catch (CustomException ex)
            {
                return Error<AppEmailModel>(null, ex.Message);
            }
            return Error<AppEmailModel>(null, "ایمیل ارسال نشد");
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<AppEmailModel>> SendWalletCharge(long id)
    {
        return TryCatch(async () =>
        {
            User user = await _memberService.GetById(id);
            if (user == null)
            {
                return Error<AppEmailModel>(null, "اطلاعات مورد نظر یافت نشد.");
            }
            try
            {
                AppEmail appEmail = await _integratedEmailSenderService.SendWalletCharge(new WalletChargeModel
                {
                    Date = DateTime.Now,
                    Member = new Member2Model(user)
                });
                if (appEmail != null)
                {
                    return Success(new AppEmailModel(appEmail), "عملیات با موفقیت انجام شد.");
                }
            }
            catch (CustomException ex)
            {
                return Error<AppEmailModel>(null, ex.Message);
            }
            return Error<AppEmailModel>(null, "پیامک ارسال نشد");
        });
    }
}
