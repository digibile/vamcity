using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Core;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Data.Enums;
using LoanSystemAvizhe.Services.Facades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;

namespace LoanSystem.Site.Areas.Sponsor.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Sponsor")]
public class UserLoanController : BaseSponsorController
{
    private readonly ILoanService _loanService;

    private readonly IUserLoanService _userLoanService;

    private readonly IUserLoanRequestService _userLoanRequestService;

    private readonly IMemberService _memberService;

    private readonly ICurrentUserService _currentUserService;

    private readonly ICustomerPurchasePlanService _customerPurchasePlanService;

    private readonly IAppSettingsService _appSettingsService;

    private readonly ICustomerMarketerService _customerMarketerService;

    private readonly ICustomerMarketerCommissionService _customerMarketerCommissionService;

    private readonly ISponsorService _sponsorService;

    private readonly IUserLoanFacade _userLoanFacade;

    public UserLoanController(ILogger<UserLoanController> logger, ILoanService loanService, IUserLoanService userLoanService, IAppSettingsService appSettingsService, ICustomerPurchasePlanService customerPurchasePlanService, ICurrentUserService currentUserService, IUserLoanRequestService userLoanRequestService, IMemberService memberService, ICustomerMarketerService customerMarketerService, ICustomerMarketerCommissionService customerMarketerCommissionService, ISponsorService sponsorService, IUserLoanFacade userLoanFacade)
        : base(logger)
    {
        _loanService = loanService;
        _userLoanService = userLoanService;
        _appSettingsService = appSettingsService;
        _customerPurchasePlanService = customerPurchasePlanService;
        _currentUserService = currentUserService;
        _userLoanRequestService = userLoanRequestService;
        _memberService = memberService;
        _customerMarketerService = customerMarketerService;
        _customerMarketerCommissionService = customerMarketerCommissionService;
        _sponsorService = sponsorService;
        _userLoanFacade = userLoanFacade;
    }

    [HttpGet("[action]/{id}")]
    public Task<ResponseState<UserLoanModel>> Get(Guid id)
    {
        return TryCatch(async () =>
        {
            FacadeResponse<UserLoanModel> facadeResponse = await _userLoanFacade.Get(id);
            return (!facadeResponse.Success) ? Error<UserLoanModel>(null, facadeResponse.Message) : Success(facadeResponse.Data);
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<Guid>> Create([FromBody] UserLoanModel model)
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
                return Error("سقف مجاز ثبت پرونده وام پر شده است، در صورت تمایل می توانید از منوی خرید پلن نسبت به شارژ اقدام فرمایید.");
            }
            Loan loan = await _loanService.GetById(model.LoanModel.Id);
            if (loan != null)
            {
                if (loan.MaxAmount != 0m && model.Amount > loan.MaxAmount)
                {
                    return Error("سقف مبلغ مجاز برای این نوع وام " + ((double)loan.MaxAmount).ToString("N0") + " می باشد.");
                }
                if (loan.MaxMonthGap != 0 && model.MonthGap > loan.MaxMonthGap)
                {
                    return Error($"سقف مجاز فاصله بین اقساط برای این نوع وام {loan.MaxMonthGap} می باشد.");
                }
                if (!new LoanModel(loan).ValidateAllowedMonth(model.Month * model.MonthGap))
                {
                    return Error("تعداد ماه بازپرداخت از حداکثر بازپرداخت تعریف شده در وام بیشتر است.");
                }
            }
            if ((await _memberService.GetById(model.MemberModel.Id, "Reliability")).Reliability?.IsNewLoanLock ?? false)
            {
                return Error("ثبت پرونده برای این مشتری به دلیل وضعیت خوش حسابی مسدود شده است.");
            }
            AppSettings appSettings = await _appSettingsService.GetAppSettings();
            if (appSettings != null && appSettings.PreventCreateLoanNonCurrentDate && model.StartDate_Persian.Date != DateTime.Now.ToPersianDateObject().Date)
            {
                return Error("با توجه به تنظیمات سیستم، تاریخ شروع پرونده باید برابر با تاریخ امروز باشد.");
            }
            Guid newGuid = Guid.NewGuid();
            UserLoan entity = new UserLoan
            {
                Id = newGuid,
                Amount = model.Amount,
                PayableAmount = model.CalcPayableAmount(),
                CustomProfitPercent = model.CustomProfitPercent,
                Installment = model.Installment,
                LoanId = model.LoanModel.Id,
                Month = model.Month,
                MonthGap = model.MonthGap,
                Paid = 0m,
                StartDate = model.StartDate.Date,
                IsCheckouted = model.IsCheckouted,
                CheckoutDate = model.CheckoutDate,
                Description = model.Description,
                Status = model.Status,
                CreateAt = DateTime.Now,
                UserId = model.MemberModel.Id,
                AgentId = model.AgentModel?.Id,
                AgentPrice = model.AgentPrice,
                AgentDueDate = model.AgentDueDate,
                SubAgentId = model.SubAgentModel?.Id,
                SubAgentPrice = model.SubAgentPrice,
                SubAgentDueDate = model.SubAgentDueDate,
                DepositChargeDueDate = model.DepositChargeDueDate,
                PreDepositAmount = model.PreDepositAmount,
                CustomerPurchasePlanId = firstActivePlan.Id,
                UserLoanRequestId = model.UserLoanRequest?.Id,
                UserLoanContacts = model.UserLoanContacts.Select((UserLoanContactModel x) => new UserLoanContact
                {
                    ContactId = x.Contact.Id,
                    Share = x.Share,
                    DueDate = x.DueDate,
                    CustomerId = _currentUserService.CustomerId.Value,
                    UserLoanId = newGuid
                }).ToList(),
                UserLoanSponsoreDueDates = model.UserLoanSponsoreDueDates.Select((UserLoanSponsoreDueDateModel x) => new UserLoanSponsoreDueDate
                {
                    Amount = x.Amount,
                    Description = x.Description,
                    IsCheckouted = x.IsCheckouted,
                    DueDate = x.DueDate,
                    CustomerId = _currentUserService.CustomerId.Value,
                    UserLoanId = newGuid
                }).ToList()
            };
            for (int num = 1; num <= entity.Month; num++)
            {
                entity.UserLoanPayments.Add(new UserLoanPayment
                {
                    InstallmentNumber = num,
                    InstallmentAmount = entity.Installment,
                    TargetMonth = new PersianCalendar().AddMonths(model.DepositChargeDueDate.HasValue ? model.DepositChargeDueDate.Value : model.StartDate.Date, num * model.MonthGap)
                });
            }
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
            await _userLoanService.Insert(entity);
            firstActivePlan.RemainLoanCount--;
            await _customerPurchasePlanService.Update(firstActivePlan);
            User user = await _memberService.GetById(entity.UserId);
            if (user.CustomerMarketerId.HasValue)
            {
                User user2 = await _customerMarketerService.GetById(user.CustomerMarketerId.Value);
                if (user2 != null && user2.IsActive)
                {
                    CustomerMarketerCommission entity2 = new CustomerMarketerCommission
                    {
                        Amount = entity.Amount * (decimal)user2.CommissionPercent / 100m,
                        Date = DateTime.Now,
                        MarketerId = user2.Id,
                        UserLoanId = entity.Id
                    };
                    await _customerMarketerCommissionService.Insert(entity2);
                }
            }
            return Success(entity.Id, "عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<Guid>> CreateByUserLoanRequest([FromBody] UserLoanRequestModel model)
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
            User user = await _sponsorService.GetById(_currentUserService.UserId.Value, "AgentMetaData");
            if (user == null || user.AgentMetaData?.CanConvertRequestToLoan != true)
            {
                return Error("مجوز دسترسی به این عملیات را ندارید.");
            }
            List<UserLoan> list = await _userLoanService.GetByUserLoanRequestId(model.Id);
            if (list != null && list.Any())
            {
                return Error($"برای این درخواست قبلا وام با شماره {list.First().Code} ایجاد شده است.");
            }
            UserLoanRequest userLoanRequest = await _userLoanRequestService.GetById(model.Id, "Loan.LoanContacts", "Agent.AgentLoans.Contact");
            if (userLoanRequest == null)
            {
                return Error("درخواست وام یافت نشد.");
            }
            decimal num = 0m;
            if (userLoanRequest.Agent != null)
            {
                AgentLoan agentLoan = userLoanRequest.Agent?.AgentLoans?.FirstOrDefault((AgentLoan x) => x.LoanId == userLoanRequest.LoanId);
                if (agentLoan == null)
                {
                    return Error("وام مرتبط با این درخواست جز وام های مجاز نماینده نمی باشد.");
                }
                num = ((userLoanRequest.ChargeType != AgentLoanRequestChargeTypes.کالا) ? ((userLoanRequest.OtherPercent_Percent > 0m) ? userLoanRequest.OtherPercent_Percent : agentLoan.Percent) : agentLoan.ProductPercent);
            }
            UserLoanModel userLoanModel = new UserLoanModel
            {
                AgentModel = model.AgentModel,
                AgentPrice = num * model.Amount / 100m,
                AgentDueDate = model.UserLoan.AgentDueDate,
                DepositChargeDueDate = model.UserLoan.DepositChargeDueDate,
                Amount = model.Amount,
                Installment = model.Installment,
                LoanModel = new Loan2Model
                {
                    Id = userLoanRequest.LoanId,
                    ProfitPercent = userLoanRequest.Loan.ProfitPercent,
                    DelayPercent = userLoanRequest.Loan.DelayPercent,
                    EarlyPercent = userLoanRequest.Loan.EarlyPercent,
                    AllowedInstallments = userLoanRequest.Loan.AllowedInstallments
                },
                MemberModel = model.MemberModel,
                StartDate = model.UserLoan.StartDate,
                MonthGap = model.MonthGap,
                Month = model.InstallmentCount,
                UserLoanRequest = new UserLoanRequestModel
                {
                    Id = model.Id
                }
            };
            if (model.AgentModel?.AgentId.HasValue ?? false)
            {
                userLoanModel.SubAgentModel = model.AgentModel;
                userLoanModel.SubAgentDueDate = model.UserLoan.SubAgentDueDate;
                userLoanModel.SubAgentPrice = num * model.Amount / 100m;
            }
            ResponseState<Guid> responseState = await Create(userLoanModel);
            return (!responseState.Success) ? Error(responseState.Message) : Success(responseState.Data, "عملیات با موفقیت انجام شد.");
        });
    }

    [HttpGet("[action]/{id}")]
    public Task<ResponseState<dynamic>> GetCheckoutAmount(Guid id)
    {
        return TryCatch(async () =>
        {
            FacadeResponse<(decimal, decimal)> facadeResponse = await _userLoanFacade.GetCheckoutAmount(id);
            return (!facadeResponse.Success) ? Error<object>(null, facadeResponse.Message) : Success((object)new
            {
                item1 = facadeResponse.Data.Item1,
                item2 = facadeResponse.Data.Item2
            }, "");
        });
    }
}
