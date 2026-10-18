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
using LoanSystemAvizhe.Data.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LoanSystem.Site.Areas.Admin.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Admin")]
public class AppSmsController : BaseAdminController
{
    private readonly IAppSmsService _appSmsService;

    private readonly ICurrentUserService _currentUserService;

    private readonly IntegratedSmsSenderService _integratedSmsSenderService;

    private readonly AppConfig _appConfig;

    private readonly IUserPaymentService _userPaymentService;

    private readonly IUserLoanService _userLoanService;

    private readonly IMemberService _memberService;

    private readonly IUserLoanRequestAuthorizationItemService _userLoanRequestAuthorizationItemService;

    private readonly IThirdPersonAssuranceAuthorizationItemService _thirdPersonAssuranceAuthorizationItemService;

    public AppSmsController(ILogger<AppSmsController> logger, IAppSmsService appSmsService, ICurrentUserService currentUserService, IntegratedSmsSenderService integratedSmsSenderService, IUserPaymentService userPaymentService, IUserLoanService userLoanService, IOptions<AppConfig> appConfig, IMemberService memberService, IUserLoanRequestAuthorizationItemService userLoanRequestAuthorizationItemService, IThirdPersonAssuranceAuthorizationItemService thirdPersonAssuranceAuthorizationItemService)
        : base(logger)
    {
        _currentUserService = currentUserService;
        _appSmsService = appSmsService;
        _integratedSmsSenderService = integratedSmsSenderService;
        _userPaymentService = userPaymentService;
        _userLoanService = userLoanService;
        _appConfig = appConfig.Value;
        _memberService = memberService;
        _userLoanRequestAuthorizationItemService = userLoanRequestAuthorizationItemService;
        _thirdPersonAssuranceAuthorizationItemService = thirdPersonAssuranceAuthorizationItemService;
    }

    [HttpGet("[action]")]
    public Task<ResponseState<List<AppSmsModel>>> List(DateTime? start, DateTime? end, string mobile)
    {
        return TryCatch(async () =>
        {
            List<AppSmsModel> data = (from x in await _appSmsService.GetByCondition((AppSms x) => (((DateTime?)start).HasValue ? (x.Date.Date >= ((DateTime?)start).Value.Date) : true) && (((DateTime?)end).HasValue ? (x.Date.Date <= ((DateTime?)end).Value.Date) : true) && ((!string.IsNullOrEmpty(mobile)) ? (x.To == mobile) : true), false)
                                      select new AppSmsModel(x) into x
                                      orderby x.Date descending
                                      select x).ToList();
            return Success(data);
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<List<AppSmsModel>>> ListByIds([FromBody] IdList list)
    {
        return TryCatch(async () =>
        {
            List<AppSmsModel> data = (from x in await _appSmsService.GetByCondition((AppSms x) => list.Ids.Contains(x.Id), false)
                                      select new AppSmsModel(x) into x
                                      orderby x.Date descending
                                      select x).ToList();
            return Success(data);
        });
    }

    [HttpGet("[action]/{id}")]
    public Task<ResponseState<AppSmsModel>> Get(Guid id)
    {
        return TryCatch(async () =>
        {
            AppSms appSms = await _appSmsService.GetById(id);
            if (appSms == null)
            {
                return Error<AppSmsModel>(null, "اطلاعات مورد نظر یافت نشد.");
            }
            AppSmsModel data = new AppSmsModel(appSms);
            return Success(data);
        });
    }

    [HttpGet("[action]/{id}")]
    public Task<ResponseState<List<AppSmsModel>>> ListByEntityId(Guid id)
    {
        return TryCatch(async () =>
        {
            List<AppSmsModel> data = (from x in await _appSmsService.GetByCondition((AppSms x) => x.EntityId == id, false)
                                      select new AppSmsModel(x) into x
                                      orderby x.Date descending
                                      select x).ToList();
            return Success(data);
        });
    }

    [HttpPost("[action]/{id}")]
    public Task<ResponseState<AppSmsModel>> ReSend(Guid id)
    {
        return TryCatch(async () =>
        {
            AppSms appSms = await _appSmsService.GetById(id);
            if (appSms == null)
            {
                return Error<AppSmsModel>(null, "اطلاعات مورد نظر یافت نشد.");
            }
            try
            {
                AppSms appSms2 = await _integratedSmsSenderService.ReSend(appSms);
                if (appSms2 != null)
                {
                    return Success(new AppSmsModel(appSms2), "عملیات با موفقیت انجام شد.");
                }
            }
            catch (CustomException ex)
            {
                return Error<AppSmsModel>(null, ex.Message);
            }
            return Error<AppSmsModel>(null, "پیامک ارسال نشد");
        });
    }

    [HttpPost("[action]/{id}")]
    public Task<ResponseState<string>> CheckStatus(Guid id)
    {
        return TryCatch(async () =>
        {
            AppSms appSms = await _appSmsService.GetById(id);
            if (appSms == null)
            {
                return Error<string>(null, "اطلاعات مورد نظر یافت نشد.");
            }
            if (string.IsNullOrEmpty(appSms.SMSKey))
            {
                return Error<string>(null, "امکان بررسی وضعیت این پیامک وجود ندارد");
            }
            AppSmsStatus appSmsStatus = await _integratedSmsSenderService.CheckDelivery(appSms.SMSKey, id, _currentUserService.CustomerId.Value, _appConfig.IsCloud, continueCheck: false);
            return Success(appSmsStatus.ToString(), "عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<AppSmsModel>> SendForUserLoanPayment(Guid id, string type)
    {
        return TryCatch(async () =>
        {
            UserLoanPayment userLoanPayment = await _userPaymentService.GetById(id, "UserLoanPaymentOthers");
            if (userLoanPayment == null)
            {
                return Error<AppSmsModel>(null, "اطلاعات مورد نظر یافت نشد.");
            }
            try
            {
                AppSms appSms;
                switch (type)
                {
                    case "DueDate":
                        appSms = await _integratedSmsSenderService.SendDueDate(new LoanPaymentModel(userLoanPayment));
                        break;
                    case "Thanks":
                        appSms = await _integratedSmsSenderService.SendThanks(new LoanPaymentModel(userLoanPayment));
                        break;
                    case "OverDueDate":
                        appSms = await _integratedSmsSenderService.SendOverDueDate(new LoanPaymentModel(userLoanPayment), null);
                        break;
                    case "BeforeDueDate":
                        appSms = await _integratedSmsSenderService.SendBeforeDueDate(new LoanPaymentModel(userLoanPayment));
                        break;
                    default:
                        return Error<AppSmsModel>(null, "نوع پیامک معتبر نمی باشد");
                }
                if (appSms != null)
                {
                    return Success(new AppSmsModel(appSms), "عملیات با موفقیت انجام شد.");
                }
            }
            catch (CustomException ex)
            {
                return Error<AppSmsModel>(null, ex.Message);
            }
            return Error<AppSmsModel>(null, "پیامک ارسال نشد");
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<AppSmsModel>> SendForUserLoan(Guid id, string type)
    {
        return TryCatch(async () =>
        {
            UserLoan userLoan = await _userLoanService.GetById(id, "Loan", "User");
            if (userLoan == null)
            {
                return Error<AppSmsModel>(null, "اطلاعات مورد نظر یافت نشد.");
            }
            try
            {
                if (!(type == "NewUserLoan"))
                {
                    return Error<AppSmsModel>(null, "نوع پیامک معتبر نمی باشد");
                }
                AppSms appSms = await _integratedSmsSenderService.SendNewUserLoan(new UserLoanModel(userLoan));
                if (appSms != null)
                {
                    return Success(new AppSmsModel(appSms), "عملیات با موفقیت انجام شد.");
                }
            }
            catch (CustomException ex)
            {
                return Error<AppSmsModel>(null, ex.Message);
            }
            return Error<AppSmsModel>(null, "پیامک ارسال نشد");
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<AppSmsModel>> SendWalletCharge(long id)
    {
        return TryCatch(async () =>
        {
            User member = await _memberService.GetById(id);
            if (member == null)
            {
                return Error<AppSmsModel>(null, "اطلاعات مورد نظر یافت نشد.");
            }
            if ((await _appSmsService.GetByCondition((AppSms x) => x.To == member.Mobile && (int)x.AppSmsType == 11 && x.Date >= DateTime.Now.AddMinutes(-2.0), false)).Any())
            {
                return Error<AppSmsModel>(null, "از آخرین ارسال باید حداقل 2 دقیقه گذشته باشد.");
            }
            try
            {
                AppSms appSms = await _integratedSmsSenderService.SendWalletCharge(new WalletChargeModel
                {
                    Date = DateTime.Now,
                    Member = new Member2Model(member)
                });
                if (appSms != null)
                {
                    return Success(new AppSmsModel(appSms), "عملیات با موفقیت انجام شد.");
                }
            }
            catch (CustomException ex)
            {
                return Error<AppSmsModel>(null, ex.Message);
            }
            return Error<AppSmsModel>(null, "پیامک ارسال نشد");
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<AppSmsModel>> SendDigitalSignLink(Guid id)
    {
        return TryCatch(async () =>
        {
            UserLoanRequestAuthorizationItem item = (await _userLoanRequestAuthorizationItemService.GetByCondition((UserLoanRequestAuthorizationItem x) => x.Id == id, "UserLoanRequest.User", "UserLoanRequest.Agent", "UserLoanRequest.Loan", "AuthorizationItem")).FirstOrDefault();
            if (item == null)
            {
                return Error<AppSmsModel>(null, "اطلاعات مورد نظر یافت نشد.");
            }
            if ((await _appSmsService.GetByCondition((AppSms x) => x.To == item.UserLoanRequest.User.Mobile && (int)x.AppSmsType == 17 && x.Date >= DateTime.Now.AddMinutes(-2.0), false)).Any())
            {
                return Error<AppSmsModel>(null, "از آخرین ارسال باید حداقل 2 دقیقه گذشته باشد.");
            }
            try
            {
                AppSms appSms = await _integratedSmsSenderService.SendDigitalSignLink(new UserLoanRequestAuthorizationItemModel(item));
                if (appSms != null)
                {
                    return Success(new AppSmsModel(appSms), "عملیات با موفقیت انجام شد.");
                }
            }
            catch (CustomException ex)
            {
                return Error<AppSmsModel>(null, ex.Message);
            }
            return Error<AppSmsModel>(null, "پیامک ارسال نشد");
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<AppSmsModel>> SendThirdPersonDigitalSignLink(Guid id)
    {
        return TryCatch(async () =>
        {
            ThirdPersonAssuranceAuthorizationItem item = await _thirdPersonAssuranceAuthorizationItemService.GetByIdIncludeRelations(id);
            if (item == null)
            {
                return Error<AppSmsModel>(null, "اطلاعات مورد نظر یافت نشد.");
            }
            string mobile = item.UserLoanRequestAuthorizationItem?.UserLoanRequest?.User?.Mobile ?? item.UserLoan?.User?.Mobile;
            if ((await _appSmsService.GetByCondition((AppSms x) => x.To == mobile && (int)x.AppSmsType == 23 && x.Date >= DateTime.Now.AddMinutes(-2.0), false)).Any())
            {
                return Error<AppSmsModel>(null, "از آخرین ارسال باید حداقل 2 دقیقه گذشته باشد.");
            }
            try
            {
                AppSms appSms = await _integratedSmsSenderService.SendThirdPersonDigitalSignLink(new ThirdPersonAssuranceAuthorizationItemModel(item));
                if (appSms != null)
                {
                    return Success(new AppSmsModel(appSms), "عملیات با موفقیت انجام شد.");
                }
            }
            catch (CustomException ex)
            {
                return Error<AppSmsModel>(null, ex.Message);
            }
            return Error<AppSmsModel>(null, "پیامک ارسال نشد");
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<AppSmsModel>> SendWelcomeRegister(long id)
    {
        return TryCatch(async () =>
        {
            User user = await _memberService.GetById(id);
            if (user == null)
            {
                return Error<AppSmsModel>(null, "اطلاعات مورد نظر یافت نشد.");
            }
            try
            {
                if (await _integratedSmsSenderService.TrySendRegister(new LoginRegisterModel
                {
                    Date = DateTime.Now,
                    Email = user.Email,
                    Mobile = user.Mobile,
                    User_FullName = user.FullName,
                    User_Id = user.Id,
                    User_UserName = user.UserName
                }, _currentUserService.CustomerId.Value))
                {
                    return Success(new AppSmsModel(), "عملیات با موفقیت انجام شد.");
                }
            }
            catch (CustomException ex)
            {
                return Error<AppSmsModel>(null, ex.Message);
            }
            return Error<AppSmsModel>(null, "پیامک ارسال نشد");
        });
    }
}
