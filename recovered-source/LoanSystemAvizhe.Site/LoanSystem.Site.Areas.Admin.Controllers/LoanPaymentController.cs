using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Core;
using LoanSystem.Core.Exceptions;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoanSystem.Site.Areas.Admin.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Member,Admin")]
public class LoanPaymentController : BaseAdminController
{
    private readonly IUserPaymentService _userPaymentService;

    private readonly IAppContext _appContext;

    private readonly ICurrentUserService _currentUserService;

    public LoanPaymentController(ILogger<LoanPaymentController> logger, IUserPaymentService userPaymentService, ICurrentUserService currentUserService, IAppContext appContext)
        : base(logger)
    {
        _userPaymentService = userPaymentService;
        _currentUserService = currentUserService;
        _appContext = appContext;
    }

    [HttpGet("[action]/{id}")]
    public Task<ResponseState<List<LoanPaymentModel>>> GetLoanPayments(Guid id)
    {
        return TryCatch(async () =>
        {
            List<LoanPaymentModel> models = new List<LoanPaymentModel>();
            if (id == Guid.Empty)
            {
                return Error<List<LoanPaymentModel>>(null, "وام یافت نشد.");
            }
            List<UserLoanPayment> list = await _userPaymentService.GetPaymentsByUserLoanId(id, includeMemberPurchases: true, includePaymentRequest: true);
            if (list.Any())
            {
                if (!_currentUserService.IsInRole("Admin") && list.First().UserLoan.Loan.HasSponsore)
                {
                    list.Clear();
                }
                models = (from x in list
                          orderby x.TargetMonth
                          select new LoanPaymentModel(x)).ToList();
                await _userPaymentService.SetPaymentsDelays(models);
                await _userPaymentService.SetAppSms(models, ignoreText: true);
            }
            return Success(models);
        });
    }

    [HttpGet("[action]/{persianDate}")]
    public Task<ResponseState<List<LoanPaymentByMonthModel>>> GetListByTargetMonth(string persianDate)
    {
        return TryCatch(async () =>
        {
            persianDate = persianDate.Replace("-", "/");
            DateTime value = persianDate.ToGregorianDateTime().Value;
            (string, string) startEndOfMonthInPersian = DateTimeExtentions.GetStartEndOfMonthInPersian(value);
            DateTime value2 = startEndOfMonthInPersian.Item1.ToGregorianDateTime().Value;
            DateTime value3 = startEndOfMonthInPersian.Item2.ToGregorianDateTime().Value;
            List<LoanPaymentByMonthModel> data = await _userPaymentService.GetListByTargetMonth(value2, value3);
            return Success(data);
        });
    }

    [HttpGet("[action]/{id}")]
    [Authorize(Roles = "Admin")]
    public Task<ResponseState<LoanPaymentModel>> Get(Guid id)
    {
        return TryCatch(async () =>
        {
            if (id == Guid.Empty)
            {
                return Success(new LoanPaymentModel
                {
                    TargetMonth = DateTime.Now,
                    SaveDate = DateTime.Now
                });
            }
            UserLoanPayment userLoanPayment = await _userPaymentService.GetById(id);
            if (userLoanPayment == null)
            {
                return Error<LoanPaymentModel>(null, "اطلاعات مورد نظر یافت نشد");
            }
            LoanPaymentModel loanPaymentModel = new LoanPaymentModel(userLoanPayment);
            if (loanPaymentModel.SaveDate == DateTime.MinValue)
            {
                loanPaymentModel.SaveDate = DateTime.Now;
            }
            return Success(loanPaymentModel);
        });
    }

    [HttpPost("[action]")]
    [Authorize(Roles = "Admin")]
    public Task<ResponseState<Guid>> Create([FromBody] LoanPaymentModel model)
    {
        return TryCatch(async () =>
        {
            UserLoanPayment userLoanPayment = new UserLoanPayment
            {
                Id = Guid.Empty,
                Amount = model.Amount,
                SumAmount = model.Amount,
                InstallmentNumber = model.InstallmentNumber,
                SaveDate = model.SaveDate,
                SavedBy = "به صورت دستی توسط " + _appContext.User?.FullName,
                UserLoanId = model.UserLoanModel.Id,
                TargetMonth = model.TargetMonth,
                Description = model.Description,
                GoldRate = model.GoldRate,
                GoldWeight = model.GoldWeight,
                IsGoldCheckouted = model.IsGoldCheckouted
            };
            UserLoanPayment userLoanPayment2 = userLoanPayment;
            userLoanPayment2.DelayDay = await _userPaymentService.CalcDelayDay(model);
            UserLoanPayment userLoanPayment3 = userLoanPayment;
            userLoanPayment3.DelayPrice = await _userPaymentService.CalcDelayPrice(model);
            UserLoanPayment entity = userLoanPayment;
            await _userPaymentService.Insert(entity);
            return Success(entity.Id, "عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]")]
    [Authorize(Roles = "Admin")]
    public Task<ResponseState<Guid>> Edit([FromBody] LoanPaymentModel model)
    {
        return TryCatch(async () =>
        {
            _ = 3;
            try
            {
                UserLoanPayment entity = await _userPaymentService.GetById(model.Id, "UserLoan");
                if (entity == null)
                {
                    return Error("اطلاعات مورد نظر یافت نشد.");
                }
                if (entity.SumAmount != 0m)
                {
                    return Error("قسط دارای مبلغ پرداخت شده است و قابل ویرایش نمی باشد.");
                }
                entity.InstallmentAmount = model.InstallmentAmount;
                entity.TargetMonth = model.TargetMonth;
                entity.SavedBy = _appContext.User?.FullName;
                if (entity.DelayDay != 0)
                {
                    UserLoanPayment userLoanPayment = entity;
                    userLoanPayment.DelayDay = await _userPaymentService.CalcDelayDay(model);
                    userLoanPayment = entity;
                    userLoanPayment.DelayPrice = await _userPaymentService.CalcDelayPrice(model);
                }
                await _userPaymentService.Update(entity, sendSms: false);
                return Success(entity.Id, "عملیات با موفقیت انجام شد.");
            }
            catch (CustomException ex)
            {
                if (ex.Message == "Settings not set.")
                {
                    return Error("تنظیمات سیستم انجام نشده است.");
                }
                throw ex;
            }
        });
    }

    [HttpPost("[action]/{id}")]
    [Authorize(Roles = "Admin")]
    public Task<DefaultResponseState> Delete(Guid id)
    {
        return TryCatch(async () =>
        {
            UserLoanPayment userLoanPayment = await _userPaymentService.GetById(id);
            if (userLoanPayment == null)
            {
                return Error("امکان حذف وجود ندارد. لطفا بعدا دوباره تلاش نمایید");
            }
            await _userPaymentService.Delete(userLoanPayment);
            return Success();
        });
    }

    [HttpPost("[action]")]
    [Authorize(Roles = "Admin")]
    public Task<ResponseState<Guid>> SubmitPayment([FromBody] LoanPaymentModel model)
    {
        return TryCatch(async () =>
        {
            _ = 4;
            try
            {
                DateTime saveDate = model.SaveDate;
                if (saveDate.Hour == 0)
                {
                    saveDate = new DateTime(saveDate.Year, saveDate.Month, saveDate.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                }
                UserLoanPayment entity = await _userPaymentService.GetById(model.Id);
                if (entity == null)
                {
                    return Error("اطلاعات مورد نظر یافت نشد.");
                }
                string text = await _userPaymentService.ValidateLoanPayment(entity);
                if (!string.IsNullOrWhiteSpace(text))
                {
                    return Error(text);
                }
                model.SumAmount = model.Amount + (model.LoanPaymentOthers?.Sum((LoanPaymentOtherModel x) => x.Amount) ?? 0m);
                entity.Amount = model.Amount;
                entity.SaveDate = saveDate;
                entity.SavedBy = _appContext.User?.FullName;
                entity.PaymentMethod = model.PaymentMethod;
                entity.Description = model.Description;
                entity.GoldRate = model.GoldRate;
                entity.GoldWeight = model.GoldWeight;
                entity.IsGoldCheckouted = model.IsGoldCheckouted;
                UserLoanPayment userLoanPayment = entity;
                userLoanPayment.DelayDay = await _userPaymentService.CalcDelayDay(model);
                userLoanPayment = entity;
                userLoanPayment.DelayPrice = await _userPaymentService.CalcDelayPrice(model);
                entity.UserLoanPaymentOthers = model.LoanPaymentOthers?.Select((LoanPaymentOtherModel x) => new UserLoanPaymentOther
                {
                    Id = x.Id,
                    Amount = x.Amount,
                    Description = x.Description,
                    GoldRate = x.GoldRate,
                    GoldWeight = x.GoldWeight,
                    IsGoldCheckouted = x.IsGoldCheckouted,
                    PaymentMethod = x.PaymentMethod,
                    SaveDate = x.SaveDate,
                    SavedBy = x.SavedBy,
                    UserLoanPaymentId = model.Id
                }).ToList();
                await _userPaymentService.Update(entity, model.SendSms);
                return Success(entity.Id, "عملیات با موفقیت انجام شد.");
            }
            catch (CustomException ex)
            {
                if (ex.Message == "Settings not set.")
                {
                    return Error("تنظیمات سیستم انجام نشده است.");
                }
                throw ex;
            }
        });
    }
}
