using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Core.Exceptions;
using LoanSystem.Data;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Services.Users;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Data.Enums;
using LoanSystemAvizhe.Site.Areas.AppApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Spm.Services.Infrastructure;

namespace LoanSystem.Site.Areas.AppApi.Controllers;

[Route("[area]/[controller]")]
public class UserLoanController : BaseApiController
{
    private readonly ILoanService _loanService;

    private readonly ICustomerPurchasePlanService _customerPurchasePlanService;

    private readonly ICurrentUserService _currentUserService;

    private readonly IMemberPurchaseService _memberPurchaseService;

    private readonly IUserLoanService _userLoanService;

    private readonly IUserPaymentService _userPaymentService;

    private readonly UserManager _userManager;

    private readonly AppDbContext _context;

    private readonly IAppSettingsService _appSettingsService;

    public UserLoanController(ILogger<UserLoanController> logger, IUserPaymentService userPaymentService, ICustomerPurchasePlanService customerPurchasePlanService, ILoanService loanService, IUserLoanService userLoanService, IAppSettingsService appSettingsService, ICurrentUserService currentUserService, IMemberPurchaseService memberPurchaseService, UserManager userManager, AppDbContext context)
        : base(logger)
    {
        _loanService = loanService;
        _currentUserService = currentUserService;
        _memberPurchaseService = memberPurchaseService;
        _userPaymentService = userPaymentService;
        _userLoanService = userLoanService;
        _appSettingsService = appSettingsService;
        _customerPurchasePlanService = customerPurchasePlanService;
        _userManager = userManager;
        _context = context;
        if (!_currentUserService.TrySetCustomerIdFromToken())
        {
            throw new Exception("Token is not valid, please try get token again.");
        }
    }

    [HttpPost("[action]")]
    public Task<ResponseState<AppApiUserLoanResponseModel>> Create([FromBody] AppApiUserLoanModel model)
    {
        foreach (KeyValuePair<string, ModelStateEntry> item in ModelState)
        {
            string key = item.Key;
            if (key.Contains("MemberModel") || key.Contains("LoanModel"))
            {
                ModelState[key].Errors.Clear();
            }
        }
        return TryCatch(async () =>
        {
            CustomerPurchasePlan firstActivePlan = await _customerPurchasePlanService.GetFirstActiveForSetRemain(_currentUserService.CustomerId.Value);
            if (firstActivePlan == null)
            {
                return Error<AppApiUserLoanResponseModel>(null, "سقف مجاز ثبت پرونده وام پر شده است، در صورت تمایل می توانید از منوی خرید پلن نسبت به شارژ اقدام فرمایید.");
            }
            Loan loan = await _loanService.GetById(model.LoanId);
            if (loan != null)
            {
                if (loan.MaxAmount != 0m && model.Amount > loan.MaxAmount)
                {
                    return Error<AppApiUserLoanResponseModel>(null, "سقف مبلغ مجاز برای این نوع وام " + ((double)loan.MaxAmount).ToString("N0") + " می باشد.");
                }
                if (loan.MaxMonthGap != 0 && model.MonthGap > loan.MaxMonthGap)
                {
                    return Error<AppApiUserLoanResponseModel>(null, $"سقف مجاز فاصله بین اقساط برای این نوع وام {loan.MaxMonthGap} می باشد.");
                }
                if (!new LoanModel(loan).ValidateAllowedMonth(model.Month * model.MonthGap))
                {
                    return Error<AppApiUserLoanResponseModel>(null, "تعداد ماه بازپرداخت از حداکثر بازپرداخت تعریف شده در وام بیشتر است.");
                }
            }
            Guid id = Guid.NewGuid();
            UserLoan entity = new UserLoan
            {
                Id = id,
                Amount = model.Amount,
                PayableAmount = model.CalcPayableAmount(),
                Installment = model.Installment,
                LoanId = model.LoanId,
                Month = model.Month,
                MonthGap = model.MonthGap,
                Paid = 0m,
                StartDate = model.StartDate.Date,
                IsCheckouted = model.IsCheckouted,
                CheckoutDate = model.CheckoutDate,
                Description = model.Description,
                UserId = model.MemberId,
                DepositChargeDueDate = model.DepositChargeDueDate,
                PreDepositAmount = model.PreDepositAmount,
                CustomerPurchasePlanId = firstActivePlan.Id
            };
            for (int i = 1; i <= entity.Month; i++)
            {
                entity.UserLoanPayments.Add(new UserLoanPayment
                {
                    InstallmentNumber = i,
                    InstallmentAmount = entity.Installment,
                    TargetMonth = new PersianCalendar().AddMonths(model.StartDate.Date, i * model.MonthGap)
                });
            }
            AppSettings appSettings = await _appSettingsService.GetAppSettings();
            if (appSettings != null && entity.Installment * (decimal)entity.Month < entity.PayableAmount)
            {
                switch (appSettings.AppendToWhichInstallment)
                {
                    case AppendToWhichInstallment.First:
                        entity.UserLoanPayments.First().InstallmentAmount += entity.PayableAmount - entity.Installment * (decimal)entity.Month;
                        break;
                    case AppendToWhichInstallment.Last:
                        entity.UserLoanPayments.Last().InstallmentAmount += entity.PayableAmount - entity.Installment * (decimal)entity.Month;
                        break;
                }
            }
            if (entity.UserLoanPayments.Sum((UserLoanPayment x) => x.InstallmentAmount) != entity.PayableAmount)
            {
                entity.PayableAmount = entity.UserLoanPayments.Sum((UserLoanPayment x) => x.InstallmentAmount);
            }
            entity.Remain = entity.UserLoanPayments.Sum((UserLoanPayment x) => x.InstallmentAmount);
            await _userLoanService.Insert(entity);
            firstActivePlan.RemainLoanCount--;
            await _customerPurchasePlanService.Update(firstActivePlan);
            return Success(new AppApiUserLoanResponseModel
            {
                Code = entity.Code,
                Id = entity.Id
            }, "عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<AppApiUserLoanResponseModel>> Edit([FromBody] AppApiUserLoanModel model)
    {
        foreach (KeyValuePair<string, ModelStateEntry> item in ModelState)
        {
            string key = item.Key;
            if (key.Contains("MemberModel") || key.Contains("LoanModel"))
            {
                ModelState[key].Errors.Clear();
            }
        }
        return TryCatch(async () =>
        {
            UserLoan entity = await _userLoanService.GetById(model.Id, "Loan", "UserLoanContacts", "UserLoanSponsoreDueDates");
            if (entity == null)
            {
                return Error<AppApiUserLoanResponseModel>(null, "آبجکت پرونده وام یافت نشد.");
            }
            Loan loan = await _loanService.GetById(model.LoanId);
            if (loan != null)
            {
                if (loan.MaxAmount != 0m && model.Amount > loan.MaxAmount)
                {
                    return Error<AppApiUserLoanResponseModel>(null, "سقف مبلغ مجاز برای این نوع وام " + ((double)loan.MaxAmount).ToString("N0") + " می باشد.");
                }
                if (loan.MaxMonthGap != 0 && model.MonthGap > loan.MaxMonthGap)
                {
                    return Error<AppApiUserLoanResponseModel>(null, $"سقف مجاز فاصله بین اقساط برای این نوع وام {loan.MaxMonthGap} می باشد.");
                }
                if (!new LoanModel(loan).ValidateAllowedMonth(model.Month * model.MonthGap))
                {
                    return Error<AppApiUserLoanResponseModel>(null, "تعداد ماه بازپرداخت از حداکثر بازپرداخت تعریف شده در وام بیشتر است.");
                }
            }
            bool flag = entity.Month != model.Month || entity.StartDate.Date != model.StartDate.Date || entity.PayableAmount != model.PayableAmount || entity.Installment != model.Installment;
            entity.Id = model.Id;
            entity.Amount = model.Amount;
            entity.PayableAmount = model.CalcPayableAmount();
            entity.CustomProfitPercent = model.CustomProfitPercent;
            entity.Installment = model.Installment;
            entity.LoanId = model.LoanId;
            entity.Month = model.Month;
            entity.MonthGap = model.MonthGap;
            entity.Paid = model.Paid;
            entity.Remain = model.Remain;
            entity.StartDate = model.StartDate;
            entity.UserId = model.MemberId;
            entity.IsCheckouted = model.IsCheckouted;
            entity.CheckoutDate = model.CheckoutDate;
            entity.DepositChargeDueDate = model.DepositChargeDueDate;
            entity.PreDepositAmount = model.PreDepositAmount;
            entity.Description = model.Description;
            entity.Status = model.Status;
            if (flag)
            {
                List<UserLoanPayment> list = await _userPaymentService.GetPaymentsByUserLoanId(model.Id, includeMemberPurchases: true, includePaymentRequest: false);
                if (list.Any((UserLoanPayment x) => x.SumAmount != 0m))
                {
                    return Error<AppApiUserLoanResponseModel>(null, "بدلیل ثبت پرداخت اقساط برای این وام، امکان ویرایش وجود ندارد.");
                }
                if (list.SelectMany((UserLoanPayment x) => x.MemberPurchases).Any((MemberPurchase x) => x.IsPaymentSuccess))
                {
                    return Error<AppApiUserLoanResponseModel>(null, "بدلیل ثبت پرداخت آنلاین قسط برای این وام، امکان ویرایش وجود ندارد.");
                }
                foreach (UserLoanPayment item2 in list)
                {
                    _memberPurchaseService.DeleteByUserPaymentId(item2.Id);
                    item2.MemberPurchases?.Clear();
                }
                _userPaymentService.DeleteByUserLoanId(model.Id);
                entity.UserLoanPayments.Clear();
                for (int num = 1; num <= entity.Month; num++)
                {
                    entity.UserLoanPayments.Add(new UserLoanPayment
                    {
                        InstallmentNumber = num,
                        InstallmentAmount = entity.Installment,
                        TargetMonth = new PersianCalendar().AddMonths(model.StartDate, num * model.MonthGap)
                    });
                }
                AppSettings appSettings = await _appSettingsService.GetAppSettings();
                if (appSettings != null && entity.Installment * (decimal)entity.Month < entity.PayableAmount)
                {
                    switch (appSettings.AppendToWhichInstallment)
                    {
                        case AppendToWhichInstallment.First:
                            entity.UserLoanPayments.First().InstallmentAmount += entity.PayableAmount - entity.Installment * (decimal)entity.Month;
                            break;
                        case AppendToWhichInstallment.Last:
                            entity.UserLoanPayments.Last().InstallmentAmount += entity.PayableAmount - entity.Installment * (decimal)entity.Month;
                            break;
                    }
                }
                if (entity.UserLoanPayments.Sum((UserLoanPayment x) => x.InstallmentAmount) != entity.PayableAmount)
                {
                    entity.PayableAmount = entity.UserLoanPayments.Sum((UserLoanPayment x) => x.InstallmentAmount);
                }
                entity.Remain = entity.UserLoanPayments.Sum((UserLoanPayment x) => x.InstallmentAmount) - entity.Discount;
            }
            await _userLoanService.Update(entity);
            return Success(new AppApiUserLoanResponseModel
            {
                Code = entity.Code,
                Id = entity.Id
            }, "عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<List<AppApiUserLoanBulkWithPaymentsPerMemberResult>>> CreateBulkWithPayments([FromBody] List<AppApiUserLoanModel> model)
    {
        foreach (KeyValuePair<string, ModelStateEntry> item3 in ModelState)
        {
            string key = item3.Key;
            if (key.Contains("MemberModel") || key.Contains("LoanModel"))
            {
                ModelState[key].Errors.Clear();
            }
        }
        return TryCatch(async () =>
        {
            List<AppApiUserLoanBulkWithPaymentsPerMemberResult> summary = new List<AppApiUserLoanBulkWithPaymentsPerMemberResult>();
            List<Guid> loanIds = model.Select((AppApiUserLoanModel x) => x.LoanId).ToList();
            List<Loan> db_loans = await (from x in _loanService.GetAllLoansAsQueryable()
                                         where loanIds.Contains(x.Id)
                                         select x).ToListAsync();
            List<long> memberIds = model.Select((AppApiUserLoanModel x) => x.MemberId).ToList();
            await (from x in _userManager.GetAllAsQueryable()
                   where memberIds.Contains(x.Id)
                   select x).ToListAsync();
            CustomerPurchasePlan firstActivePlan = await _customerPurchasePlanService.GetFirstActiveForSetRemain(_currentUserService.CustomerId.Value, model.Count);
            if (firstActivePlan == null)
            {
                return Error<List<AppApiUserLoanBulkWithPaymentsPerMemberResult>>(null, "سقف مجاز ثبت پرونده وام پر شده است، در صورت تمایل می توانید از منوی خرید پلن نسبت به شارژ اقدام فرمایید.");
            }
            foreach (AppApiUserLoanModel item in model)
            {
                Loan loan = db_loans.FirstOrDefault((Loan x) => x.Id == item.LoanId);
                if (loan == null)
                {
                    summary.Add(new AppApiUserLoanBulkWithPaymentsPerMemberResult
                    {
                        MemberId = item.MemberId,
                        ErrorMessage = $"وام با شناسه '{item.LoanId}' .در سیستم وجود ندارد"
                    });
                }
                else if (loan.MaxAmount != 0m && item.Amount > loan.MaxAmount)
                {
                    summary.Add(new AppApiUserLoanBulkWithPaymentsPerMemberResult
                    {
                        MemberId = item.MemberId,
                        ErrorMessage = "سقف مبلغ مجاز برای این نوع وام " + ((double)loan.MaxAmount).ToString("N0") + " می باشد."
                    });
                }
                else if (loan.MaxMonthGap != 0 && item.MonthGap > loan.MaxMonthGap)
                {
                    summary.Add(new AppApiUserLoanBulkWithPaymentsPerMemberResult
                    {
                        MemberId = item.MemberId,
                        ErrorMessage = $"سقف مجاز فاصله بین اقساط برای این نوع وام {loan.MaxMonthGap} می باشد."
                    });
                }
                else if (!new LoanModel(loan).ValidateAllowedMonth(item.Month * item.MonthGap))
                {
                    summary.Add(new AppApiUserLoanBulkWithPaymentsPerMemberResult
                    {
                        MemberId = item.MemberId,
                        ErrorMessage = "تعداد ماه بازپرداخت از حداکثر بازپرداخت تعریف شده در وام بیشتر است."
                    });
                }
                else if (item.Month != item.LoanPayments?.Count)
                {
                    summary.Add(new AppApiUserLoanBulkWithPaymentsPerMemberResult
                    {
                        MemberId = item.MemberId,
                        ErrorMessage = "تعداد رکوردهای اقساط ارسال شده با تعداد قسط تعریف شده مغایر است."
                    });
                }
            }
            if (summary.Any((AppApiUserLoanBulkWithPaymentsPerMemberResult x) => !string.IsNullOrEmpty(x.ErrorMessage)))
            {
                return Error(summary, $"در ذخیره سازی اطلاعات {summary.Count} پرونده خطا وجود دارد بنابراین هیچ پرونده ای ذخیره نشده است.");
            }
            using (IDbContextTransaction scope = await _context.Database.BeginTransactionAsync())
            {
                DbTransaction transaction = new DbTransaction(scope);
                foreach (AppApiUserLoanModel item2 in model)
                {
                    try
                    {
                        Guid id = Guid.NewGuid();
                        UserLoan entity = new UserLoan
                        {
                            Id = id,
                            Amount = item2.Amount,
                            PayableAmount = item2.PayableAmount,
                            Installment = item2.Installment,
                            LoanId = item2.LoanId,
                            Month = item2.Month,
                            MonthGap = item2.MonthGap,
                            Paid = item2.Paid,
                            StartDate = item2.StartDate.Date,
                            IsCheckouted = item2.IsCheckouted,
                            CheckoutDate = item2.CheckoutDate,
                            Description = item2.Description,
                            UserId = item2.MemberId,
                            DepositChargeDueDate = item2.DepositChargeDueDate,
                            PreDepositAmount = item2.PreDepositAmount,
                            CustomerPurchasePlanId = firstActivePlan.Id,
                            UserLoanPayments = item2.LoanPayments.Select((AppApiLoanPaymentModel p) => new UserLoanPayment
                            {
                                SumAmount = p.Amount,
                                DelayDay = p.DelayDay,
                                DelayPrice = p.DelayPrice,
                                TargetMonth = p.TargetMonth,
                                Description = p.Description,
                                GoldRate = p.GoldRate,
                                GoldWeight = p.GoldWeight,
                                InstallmentAmount = p.InstallmentAmount,
                                InstallmentNumber = p.InstallmentNumber,
                                IsGoldCheckouted = p.IsGoldCheckouted,
                                PaymentMethod = p.PaymentMethod,
                                SaveDate = p.SaveDate,
                                SavedBy = p.SavedBy
                            }).ToList()
                        };
                        await _userLoanService.Insert(entity);
                        summary.Add(new AppApiUserLoanBulkWithPaymentsPerMemberResult
                        {
                            MemberId = item2.MemberId,
                            UserLoanCode = entity.Code,
                            UserLoanId = entity.Id
                        });
                    }
                    catch (Exception ex)
                    {
                        summary.Add(new AppApiUserLoanBulkWithPaymentsPerMemberResult
                        {
                            MemberId = item2.MemberId,
                            ErrorMessage = "خطا در ذخیره سازی پرونده وام." + ex.Message
                        });
                    }
                }
                if (summary.Any((AppApiUserLoanBulkWithPaymentsPerMemberResult x) => !string.IsNullOrEmpty(x.ErrorMessage)))
                {
                    return Error(summary, $"در ذخیره سازی اطلاعات {summary.Count} پرونده خطا وجود دارد بنابراین هیچ پرونده ای ذخیره نشده است.");
                }
                firstActivePlan.RemainLoanCount -= model.Count;
                await _customerPurchasePlanService.Update(firstActivePlan);
                if (!transaction.Completed)
                {
                    transaction.Commit();
                }
            }
            return Success(summary, "عملیات با موفقیت انجام شد.");
        });
    }

    [HttpGet("[action]/{id}")]
    public Task<ResponseState<List<AppApiUserLoanModel>>> ListByUserId(long id)
    {
        return TryCatch(async () =>
        {
            new List<UserLoanModel>();
            List<AppApiUserLoanModel> models = new List<AppApiUserLoanModel>();
            if (id == 0L)
            {
                return Error<List<AppApiUserLoanModel>>(null, "کاربر یافت نشد");
            }
            List<UserLoan> source = await _userLoanService.GetByUserId(id, "Loan", "User", "Agent", "UserLoanPayments");
            if (source.Any())
            {
                List<UserLoanModel> list = (from x in source.Select((UserLoan x) =>
                    {
                        ICollection<UserLoanPayment> userLoanPayments = x.UserLoanPayments;
                        return new UserLoanModel
                        {
                            Id = x.Id,
                            Code = x.Code,
                            LoanModel = new Loan2Model
                            {
                                Id = x.Loan.Id,
                                Title = x.Loan.Title,
                                DelayPercent = x.Loan.DelayPercent,
                                EarlyPercent = x.Loan.EarlyPercent,
                                ProfitPercent = x.Loan.ProfitPercent,
                                AllowedInstallments = x.Loan.AllowedInstallments
                            },
                            MemberModel = new Member2Model(x.User),
                            AgentModel = ((x.Agent != null) ? new AgentModel(x.Agent) : null),
                            AgentPrice = x.AgentPrice,
                            PayableAmount = x.PayableAmount,
                            Amount = x.Amount,
                            Installment = x.Installment,
                            Month = x.Month,
                            MonthGap = x.MonthGap,
                            Paid = x.Paid,
                            StartDate = x.StartDate,
                            PaidCount = userLoanPayments.Count((UserLoanPayment userLoanPayment) => userLoanPayment.SumAmount > 0m),
                            IsCheckouted = x.IsCheckouted,
                            CheckoutDate = x.CheckoutDate,
                            Description = x.Description,
                            PreDepositAmount = x.PreDepositAmount,
                            LoanPayments = x.UserLoanPayments.Select((UserLoanPayment p) => new LoanPaymentModel(p)).ToList()
                        };
                    })
                                            orderby x.StartDate descending
                                            select x).ToList();
                foreach (UserLoanModel item in list)
                {
                    await _userPaymentService.SetPaymentsDelays(item.LoanPayments);
                    models.Add(new AppApiUserLoanModel(item));
                }
            }
            return Success(models);
        });
    }

    [HttpGet("[action]/{id}")]
    public Task<ResponseState<AppApiUserLoanModel>> Get(Guid id)
    {
        return TryCatch(async () =>
        {
            UserLoan userLoan = await _userLoanService.GetById(id, "User", "Loan", "Agent.AgentLoans.Contact", "UserLoanPayments", "UserLoanContacts.Contact", "UserLoanSponsoreDueDates");
            if (userLoan == null)
            {
                return Error<AppApiUserLoanModel>(null, "اطلاعات مورد نظر یافت نشد");
            }
            UserLoanModel tempModel = new UserLoanModel(userLoan);
            tempModel.LoanPayments = userLoan.UserLoanPayments.Select((UserLoanPayment p) => new LoanPaymentModel(p)).ToList();
            tempModel.PaidCount = tempModel.LoanPayments.Count((LoanPaymentModel x) => x.SumAmount > 0m);
            await _userPaymentService.SetPaymentsDelays(tempModel.LoanPayments);
            AppApiUserLoanModel data = new AppApiUserLoanModel(tempModel);
            return Success(data);
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<AppApiUserLoanModel>> SubmitPayment([FromBody] AppApiSubmitPaymentModel model)
    {
        return TryCatch(async () =>
        {
            _ = 5;
            try
            {
                DateTime saveDate = model.PaymentDate;
                if (saveDate.Hour == 0)
                {
                    saveDate = new DateTime(saveDate.Year, saveDate.Month, saveDate.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                }
                UserLoanPayment entity = await _userPaymentService.GetById(model.Id);
                if (entity == null)
                {
                    return Error<AppApiUserLoanModel>(null, "اطلاعات مورد نظر یافت نشد.");
                }
                if (entity.UserLoan == null)
                {
                    return Error<AppApiUserLoanModel>(null, "اطلاعات مورد نظر یافت نشد.");
                }
                LoanPaymentModel loanPaymentModel = new LoanPaymentModel
                {
                    Id = model.Id,
                    SumAmount = model.Amount,
                    Description = model.Description,
                    SaveDate = model.PaymentDate,
                    UserLoanModel = new UserLoanModel(entity.UserLoan),
                    PaymentMethod = model.PaymentMethod,
                    InstallmentAmount = entity.InstallmentAmount,
                    TargetMonth = entity.TargetMonth
                };
                User user = await _userManager.GetById(_currentUserService.UserId.Value);
                entity.SumAmount = model.Amount;
                entity.SaveDate = saveDate;
                entity.SavedBy = user?.FullName;
                entity.PaymentMethod = model.PaymentMethod;
                entity.Description = model.Description;
                UserLoanPayment userLoanPayment = entity;
                userLoanPayment.DelayDay = await _userPaymentService.CalcDelayDay(loanPaymentModel);
                userLoanPayment = entity;
                userLoanPayment.DelayPrice = await _userPaymentService.CalcDelayPrice(loanPaymentModel);
                await _userPaymentService.Update(entity, model.SendSms);
                ResponseState<AppApiUserLoanModel> responseState = await Get(entity.UserLoanId);
                return Success(responseState.Data, "عملیات با موفقیت انجام شد.");
            }
            catch (CustomException ex)
            {
                if (ex.Message == "Settings not set.")
                {
                    return Error<AppApiUserLoanModel>(null, "تنظیمات سیستم انجام نشده است.");
                }
                throw ex;
            }
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<Guid>> ChangeStatus([FromBody] AppApiChangeUserLoanStatustModel model)
    {
        return TryCatch(async () =>
        {
            UserLoan entity = await _userLoanService.GetById(model.Id);
            if (entity == null)
            {
                return Error(Guid.Empty, "آبجکت یافت نشد");
            }
            entity.Status = model.Status;
            await _userLoanService.Update(entity);
            return Success(entity.Id, "عملیات با موفقیت انجام شد.");
        });
    }
}
