using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Core;
using LoanSystem.Core.Configuration;
using LoanSystem.Core.Exceptions;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Data.Enums;
using Microsoft.Extensions.Options;

namespace LoanSystemAvizhe.Services.Facades;

public class UserLoanFacade : IUserLoanFacade
{
    private readonly ILoanService _loanService;

    private readonly IUserLoanService _userLoanService;

    private readonly IUserLoanRequestService _userLoanRequestService;

    private readonly IUserPaymentService _userPaymentService;

    private readonly IMemberPurchaseService _memberPurchaseService;

    private readonly IMemberService _memberService;

    private readonly ICurrentUserService _currentUserService;

    private readonly ICustomerPurchasePlanService _customerPurchasePlanService;

    private readonly IAppSettingsService _appSettingsService;

    private readonly ITransactionService _transactionService;

    private readonly IUserLoanContactService _userLoanContactService;

    private readonly IUserLoanSponsoreDueDateService _userLoanSponsoreDueDateService;

    private readonly ICustomerMarketerService _customerMarketerService;

    private readonly ICustomerMarketerCommissionService _customerMarketerCommissionService;

    private readonly IAgentService _agentService;

    private readonly AppConfig _appConfig;

    public UserLoanFacade(ILoanService loanService, IUserLoanService userLoanService, IUserPaymentService userPaymentService, IAppSettingsService appSettingsService, ICustomerPurchasePlanService customerPurchasePlanService, ICurrentUserService currentUserService, IMemberPurchaseService memberPurchaseService, IUserLoanRequestService userLoanRequestService, ITransactionService transactionService, IUserLoanContactService userLoanContactService, IMemberService memberService, ICustomerMarketerService customerMarketerService, ICustomerMarketerCommissionService customerMarketerCommissionService, IAgentService agentService, IUserLoanSponsoreDueDateService userLoanSponsoreDueDateService, IOptions<AppConfig> appConfig)
    {
        _loanService = loanService;
        _userPaymentService = userPaymentService;
        _userLoanService = userLoanService;
        _appSettingsService = appSettingsService;
        _customerPurchasePlanService = customerPurchasePlanService;
        _currentUserService = currentUserService;
        _memberPurchaseService = memberPurchaseService;
        _userLoanRequestService = userLoanRequestService;
        _transactionService = transactionService;
        _userLoanContactService = userLoanContactService;
        _memberService = memberService;
        _customerMarketerService = customerMarketerService;
        _customerMarketerCommissionService = customerMarketerCommissionService;
        _agentService = agentService;
        _userLoanSponsoreDueDateService = userLoanSponsoreDueDateService;
        _appConfig = appConfig.Value;
    }

    public async Task<FacadeResponse<List<UserLoanModel>>> List()
    {
        List<UserLoanModel> data = (await _userLoanService.GetAllUserLoans("Loan")).Select((UserLoan x) => new UserLoanModel(x)).ToList();
        return new FacadeResponse<List<UserLoanModel>>
        {
            Success = true,
            Data = data
        };
    }

    public async Task<FacadeResponse<UserLoanModel>> Get(Guid id)
    {
        if (id == Guid.Empty)
        {
            return new FacadeResponse<UserLoanModel>
            {
                Data = new UserLoanModel
                {
                    StartDate = DateTime.Now
                },
                Success = true
            };
        }
        UserLoan userLoan = await _userLoanService.GetById(id, "Loan", "User.Reliability", "Agent.AgentLoans.Contact", "UserLoanPayments", "UserLoanContacts.Contact", "UserLoanSponsoreDueDates");
        if (userLoan == null)
        {
            return new FacadeResponse<UserLoanModel>
            {
                Message = "اطلاعات مورد نظر یافت نشد"
            };
        }
        UserLoanModel model = new UserLoanModel(userLoan);
        model.LoanPayments = userLoan.UserLoanPayments.Select((UserLoanPayment p) => new LoanPaymentModel(p)).ToList();
        model.PaidCount = model.LoanPayments.Count((LoanPaymentModel x) => x.SumAmount > 0m);
        await _userPaymentService.SetPaymentsDelays(model.LoanPayments);
        return new FacadeResponse<UserLoanModel>
        {
            Success = true,
            Data = model
        };
    }

    public async Task<FacadeResponse<(decimal, decimal)>> GetCheckoutAmount(Guid id)
    {
        try
        {
            (decimal, decimal) data = await _userLoanService.GetCheckoutAmount(id);
            return new FacadeResponse<(decimal, decimal)>
            {
                Success = true,
                Data = data
            };
        }
        catch (CustomException ex)
        {
            return new FacadeResponse<(decimal, decimal)>
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    public async Task<FacadeResponse<List<UserLoanModel>>> GetLoansByUserId(long id)
    {
        List<UserLoanModel> models = new List<UserLoanModel>();
        if (id == 0L)
        {
            return new FacadeResponse<List<UserLoanModel>>
            {
                Success = false,
                Message = "کاربر یافت نشد"
            };
        }
        List<UserLoan> source = await _userLoanService.GetByUserId(id, "Loan", "User", "Agent", "UserLoanPayments");
        if (source.Any())
        {
            models = (from x in source.Select((UserLoan x) =>
                {
                    ICollection<UserLoanPayment> userLoanPayments = x.UserLoanPayments;
                    return new UserLoanModel
                    {
                        Id = x.Id,
                        Code = x.Code,
                        SecondaryCode = x.SecondaryCode,
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
                        Discount = x.Discount,
                        StartDate = x.StartDate,
                        PaidCount = userLoanPayments.Count((UserLoanPayment userLoanPayment) => userLoanPayment.SumAmount > 0m),
                        IsCheckouted = x.IsCheckouted,
                        CheckoutDate = x.CheckoutDate,
                        Description = x.Description,
                        PreDepositAmount = x.PreDepositAmount,
                        IsCanceled = x.IsCanceled,
                        LoanPayments = x.UserLoanPayments.Select((UserLoanPayment p) => new LoanPaymentModel(p)).ToList()
                    };
                })
                      orderby x.StartDate descending
                      select x).ToList();
            foreach (UserLoanModel item in models)
            {
                await _userPaymentService.SetPaymentsDelays(item.LoanPayments);
            }
            await _userLoanService.SetAppSms(models, ignoreText: true);
        }
        return new FacadeResponse<List<UserLoanModel>>
        {
            Success = true,
            Data = models
        };
    }

    public async Task<FacadeResponse<Guid>> Edit(UserLoanModel model)
    {
        UserLoan entity = await _userLoanService.GetById(model.Id, "Loan", "UserLoanContacts", "UserLoanSponsoreDueDates");
        if (entity == null)
        {
            return new FacadeResponse<Guid>
            {
                Message = "آبجکت یافت نشد"
            };
        }
        if (entity.IsCanceled)
        {
            return new FacadeResponse<Guid>
            {
                Message = "قرارداد فسخ شده و قابل ویرایش نمی باشد."
            };
        }
        Loan loan = await _loanService.GetById(model.LoanModel.Id);
        if (loan != null)
        {
            if (loan.MaxAmount != 0m && model.Amount > loan.MaxAmount)
            {
                return new FacadeResponse<Guid>
                {
                    Message = "سقف مجاز فاصله بین اقساط برای این نوع وام " + ((double)loan.MaxAmount).ToString("N0") + " می باشد."
                };
            }
            if (loan.MaxMonthGap != 0 && model.MonthGap > loan.MaxMonthGap)
            {
                return new FacadeResponse<Guid>
                {
                    Message = $"سقف مجاز فاصله بین اقساط برای این نوع وام {loan.MaxMonthGap} می باشد."
                };
            }
            if (!new LoanModel(loan).ValidateAllowedMonth(model.Month * model.MonthGap))
            {
                return new FacadeResponse<Guid>
                {
                    Message = "تعداد ماه بازپرداخت از حداکثر بازپرداخت تعریف شده در وام بیشتر است."
                };
            }
        }
        AppSettings appSettings = await _appSettingsService.GetAppSettings();
        if ((appSettings?.PreventCreateLoanNonCurrentDate ?? false) && model.StartDate.Date != entity.StartDate.Date)
        {
            return new FacadeResponse<Guid>
            {
                Message = "با توجه به فعال بودن تنظیمات کنترل تاریخ شروع پرونده با تاریخ روز، امکان تغییر تاریخ شروع پرونده وجود ندارد."
            };
        }
        bool num = entity.Month != model.Month || entity.StartDate.Date != model.StartDate.Date || entity.DepositChargeDueDate != model.DepositChargeDueDate || entity.PayableAmount != model.PayableAmount || entity.Installment != model.Installment;
        entity.Id = model.Id;
        entity.Amount = model.Amount;
        entity.PayableAmount = model.CalcPayableAmount();
        entity.CustomProfitPercent = model.CustomProfitPercent;
        entity.Installment = model.Installment;
        entity.LoanId = model.LoanModel.Id;
        entity.AgentId = model.AgentModel?.Id;
        entity.AgentPrice = model.AgentPrice;
        entity.AgentDueDate = model.AgentDueDate;
        entity.Month = model.Month;
        entity.MonthGap = model.MonthGap;
        entity.Paid = model.Paid;
        entity.Remain = model.Remain;
        entity.StartDate = model.StartDate;
        entity.UserId = model.MemberModel.Id;
        entity.IsCheckouted = model.IsCheckouted;
        entity.CheckoutDate = model.CheckoutDate;
        entity.DepositChargeDueDate = model.DepositChargeDueDate;
        entity.PreDepositAmount = model.PreDepositAmount;
        entity.Description = model.Description;
        entity.Status = model.Status;
        entity.SecondaryCode = model.SecondaryCode;
        if (num)
        {
            List<UserLoanPayment> list = await _userPaymentService.GetPaymentsByUserLoanId(model.Id, includeMemberPurchases: true, includePaymentRequest: false);
            if (list.Any((UserLoanPayment x) => x.SumAmount != 0m))
            {
                return new FacadeResponse<Guid>
                {
                    Message = "بدلیل ثبت پرداخت اقساط برای این وام، امکان ویرایش وجود ندارد."
                };
            }
            if (list.SelectMany((UserLoanPayment x) => x.MemberPurchases).Any((MemberPurchase x) => x.IsPaymentSuccess))
            {
                return new FacadeResponse<Guid>
                {
                    Message = "بدلیل ثبت پرداخت آنلاین قسط برای این وام، امکان ویرایش وجود ندارد."
                };
            }
            foreach (UserLoanPayment item in list)
            {
                _memberPurchaseService.DeleteByUserPaymentId(item.Id);
                item.MemberPurchases?.Clear();
            }
            _userPaymentService.DeleteByUserLoanId(model.Id);
            entity.UserLoanPayments.Clear();
            for (int num2 = 1; num2 <= entity.Month; num2++)
            {
                entity.UserLoanPayments.Add(new UserLoanPayment
                {
                    InstallmentNumber = num2,
                    InstallmentAmount = entity.Installment,
                    TargetMonth = new PersianCalendar().AddMonths(model.DepositChargeDueDate.HasValue ? model.DepositChargeDueDate.Value : model.StartDate, num2 * model.MonthGap)
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
        }
        List<UserLoanContact> list2 = entity.UserLoanContacts.Where((UserLoanContact x) => !model.UserLoanContacts.Select((UserLoanContactModel u) => u.Id).Contains(x.Id)).ToList();
        foreach (UserLoanContact item2 in list2)
        {
            await _userLoanContactService.Delete(item2);
        }
        entity.UserLoanContacts.Where((UserLoanContact x) => model.UserLoanContacts.Select((UserLoanContactModel u) => u.Id).Contains(x.Id)).ToList().ForEach((UserLoanContact x) =>
        {
            UserLoanContactModel userLoanContactModel = model.UserLoanContacts.FirstOrDefault((UserLoanContactModel f) => f.Id == x.Id);
            if (userLoanContactModel != null)
            {
                x.Share = userLoanContactModel.Share;
                x.ContactId = userLoanContactModel.Contact.Id;
                x.DueDate = userLoanContactModel.DueDate;
            }
        });
        foreach (UserLoanContactModel item3 in model.UserLoanContacts.Where((UserLoanContactModel x) => x.Id == Guid.Empty).ToList())
        {
            entity.UserLoanContacts.Add(new UserLoanContact
            {
                Id = item3.Id,
                ContactId = item3.Contact.Id,
                CustomerId = _currentUserService.CustomerId.Value,
                DueDate = item3.DueDate,
                Share = item3.Share,
                UserLoanId = entity.Id
            });
        }
        List<UserLoanSponsoreDueDate> list3 = entity.UserLoanSponsoreDueDates.Where((UserLoanSponsoreDueDate x) => !model.UserLoanSponsoreDueDates.Select((UserLoanSponsoreDueDateModel u) => u.Id).Contains(x.Id)).ToList();
        foreach (UserLoanSponsoreDueDate item4 in list3)
        {
            await _userLoanSponsoreDueDateService.Delete(item4);
        }
        entity.UserLoanSponsoreDueDates.Where((UserLoanSponsoreDueDate x) => model.UserLoanSponsoreDueDates.Select((UserLoanSponsoreDueDateModel u) => u.Id).Contains(x.Id)).ToList().ForEach((UserLoanSponsoreDueDate x) =>
        {
            UserLoanSponsoreDueDateModel userLoanSponsoreDueDateModel = model.UserLoanSponsoreDueDates.FirstOrDefault((UserLoanSponsoreDueDateModel f) => f.Id == x.Id);
            if (userLoanSponsoreDueDateModel != null)
            {
                x.Amount = userLoanSponsoreDueDateModel.Amount;
                x.Description = userLoanSponsoreDueDateModel.Description;
                x.IsCheckouted = userLoanSponsoreDueDateModel.IsCheckouted;
                x.DueDate = userLoanSponsoreDueDateModel.DueDate;
            }
        });
        foreach (UserLoanSponsoreDueDateModel item5 in model.UserLoanSponsoreDueDates.Where((UserLoanSponsoreDueDateModel x) => x.Id == Guid.Empty).ToList())
        {
            entity.UserLoanSponsoreDueDates.Add(new UserLoanSponsoreDueDate
            {
                Id = item5.Id,
                Description = item5.Description,
                CustomerId = _currentUserService.CustomerId.Value,
                DueDate = item5.DueDate,
                Amount = item5.Amount,
                IsCheckouted = item5.IsCheckouted,
                UserLoanId = entity.Id
            });
        }
        CustomerMarketerCommission commission = (await _customerMarketerCommissionService.GetByCondition((CustomerMarketerCommission x) => x.UserLoanId == entity.Id)).FirstOrDefault();
        if (commission != null)
        {
            User user = await _customerMarketerService.GetById(commission.MarketerId);
            if (user != null && user.IsActive && commission.Amount != entity.Amount * (decimal)user.CommissionPercent / 100m)
            {
                commission.Amount = entity.Amount * (decimal)user.CommissionPercent / 100m;
                await _customerMarketerCommissionService.Update(commission);
            }
        }
        await _userLoanService.Update(entity);
        return new FacadeResponse<Guid>
        {
            Success = true,
            Data = entity.Id,
            Message = "عملیات با موفقیت انجام شد."
        };
    }

    public async Task<FacadeResponse<Guid>> Create(UserLoanModel model)
    {
        CustomerPurchasePlan firstActivePlan = await _customerPurchasePlanService.GetFirstActiveForSetRemain(_currentUserService.CustomerId.Value);
        if (firstActivePlan == null)
        {
            return new FacadeResponse<Guid>
            {
                Message = "سقف مجاز ثبت پرونده وام پر شده است، در صورت تمایل می توانید از منوی خرید پلن نسبت به شارژ اقدام فرمایید."
            };
        }
        Loan loan = await _loanService.GetById(model.LoanModel.Id);
        if (loan != null)
        {
            if (loan.MaxAmount != 0m && model.Amount > loan.MaxAmount)
            {
                return new FacadeResponse<Guid>
                {
                    Message = "سقف مبلغ مجاز برای این نوع وام " + ((double)loan.MaxAmount).ToString("N0") + " می باشد."
                };
            }
            if (loan.MaxMonthGap != 0 && model.MonthGap > loan.MaxMonthGap)
            {
                return new FacadeResponse<Guid>
                {
                    Message = $"سقف مجاز فاصله بین اقساط برای این نوع وام {loan.MaxMonthGap} می باشد."
                };
            }
            if (!new LoanModel(loan).ValidateAllowedMonth(model.Month * model.MonthGap))
            {
                return new FacadeResponse<Guid>
                {
                    Message = "تعداد ماه بازپرداخت از حداکثر بازپرداخت تعریف شده در وام بیشتر است."
                };
            }
        }
        if ((await _memberService.GetById(model.MemberModel.Id, "Reliability")).Reliability?.IsNewLoanLock ?? false)
        {
            return new FacadeResponse<Guid>
            {
                Message = "ثبت پرونده برای این مشتری به دلیل وضعیت خوش حسابی مسدود شده است."
            };
        }
        AppSettings appSettings = await _appSettingsService.GetAppSettings();
        if (appSettings != null && appSettings.PreventCreateLoanNonCurrentDate && model.StartDate_Persian.Date != DateTime.Now.ToPersianDateObject().Date)
        {
            return new FacadeResponse<Guid>
            {
                Message = "با توجه به تنظیمات سیستم، تاریخ شروع پرونده باید برابر با تاریخ امروز باشد."
            };
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
            SecondaryCode = model.SecondaryCode,
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
        return new FacadeResponse<Guid>
        {
            Success = true,
            Data = entity.Id
        };
    }

    public async Task<FacadeResponse<Guid>> CreateByUserLoanRequest(UserLoanRequestModel model)
    {
        List<UserLoan> list = await _userLoanService.GetByUserLoanRequestId(model.Id);
        if (list != null && list.Any())
        {
            return new FacadeResponse<Guid>
            {
                Message = $"برای این درخواست قبلا وام با شماره {list.First().Code} ایجاد شده است."
            };
        }
        UserLoanRequest userLoanRequest = await _userLoanRequestService.GetById(model.Id, "Loan.LoanContacts", "Agent.AgentLoans.Contact");
        if (userLoanRequest == null)
        {
            return new FacadeResponse<Guid>
            {
                Message = "درخواست وام یافت نشد."
            };
        }
        decimal num = 0m;
        if (userLoanRequest.Agent != null)
        {
            AgentLoan agentLoan = userLoanRequest.Agent?.AgentLoans?.FirstOrDefault((AgentLoan x) => x.LoanId == userLoanRequest.LoanId);
            if (agentLoan == null)
            {
                return new FacadeResponse<Guid>
                {
                    Message = "وام مرتبط با این درخواست جز وام های مجاز نماینده نمی باشد."
                };
            }
            if (userLoanRequest.ChargeType != AgentLoanRequestChargeTypes.کالا)
            {
                num = ((userLoanRequest.OtherPercent_Percent > 0m) ? userLoanRequest.OtherPercent_Percent : agentLoan.Percent);
            }
            else
            {
                num = agentLoan.ProductPercent;
            }
        }
        UserLoanModel userLoan = new UserLoanModel
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
            },
            Description = model.AdminDescription
        };
        if (model.AgentModel?.AgentId.HasValue ?? false)
        {
            userLoan.SubAgentModel = model.AgentModel;
            userLoan.SubAgentDueDate = model.UserLoan.SubAgentDueDate;
            userLoan.SubAgentPrice = num * model.Amount / 100m;
            User user = await _agentService.GetById(model.AgentModel.AgentId.Value, "AgentLoans");
            if (user != null)
            {
                AgentLoan agentLoan2 = user.AgentLoans?.FirstOrDefault((AgentLoan x) => x.LoanId == userLoanRequest.LoanId);
                if (agentLoan2 == null)
                {
                    return new FacadeResponse<Guid>
                    {
                        Message = "وام مرتبط با این درخواست جز وام های مجاز نماینده سطح بالایی نمی باشد."
                    };
                }
                decimal percent = agentLoan2.Percent;
                userLoan.AgentModel = new AgentModel(user);
                userLoan.AgentDueDate = model.UserLoan.AgentDueDate;
                userLoan.AgentPrice = percent * model.Amount / 100m;
            }
        }
        if (!(userLoanRequest.Loan.LoanContacts?.Any() ?? false))
        {
            User agent = userLoanRequest.Agent;
            if (agent == null || agent.AgentLoans?.Any((AgentLoan x) => x.ContactId.HasValue) != true)
            {
                goto IL_0866;
            }
        }
        List<UserLoanContactModel> list2 = userLoanRequest.Loan.LoanContacts.Select((LoanContact x) => new UserLoanContactModel
        {
            Id = Guid.Empty,
            Contact = new ContactModel
            {
                Id = x.ContactId
            },
            Share = userLoan.Amount * x.Share / 100m,
            DueDate = userLoan.StartDate
        }).ToList();
        if (userLoanRequest.Agent != null)
        {
            foreach (AgentLoan agloan in userLoanRequest.Agent?.AgentLoans?.Where((AgentLoan x) => x.ContactId.HasValue && x.LoanId == userLoanRequest.LoanId))
            {
                UserLoanContactModel userLoanContactModel = list2.FirstOrDefault((UserLoanContactModel x) =>
                {
                    Guid id = x.Contact.Id;
                    Guid? contactId = agloan.ContactId;
                    return id == contactId;
                });
                if (userLoanContactModel != null)
                {
                    userLoanContactModel.Share = userLoan.Amount * agloan.ContactPercent / 100m;
                    continue;
                }
                list2.Add(new UserLoanContactModel
                {
                    Contact = new ContactModel
                    {
                        Id = agloan.ContactId.Value
                    },
                    DueDate = userLoan.StartDate,
                    Share = userLoan.Amount * agloan.ContactPercent / 100m
                });
            }
        }
        userLoan.UserLoanContacts = list2;
        goto IL_0866;
    IL_0866:
        FacadeResponse<Guid> facadeResponse = await Create(userLoan);
        if (!facadeResponse.Success)
        {
            return new FacadeResponse<Guid>
            {
                Message = facadeResponse.Message
            };
        }
        return new FacadeResponse<Guid>
        {
            Success = true,
            Data = facadeResponse.Data
        };
    }

    public async Task<FacadeResponse<Guid>> SetAsCanceled(Guid id)
    {
        UserLoan entity = await _userLoanService.GetById(id, "Loan", "UserLoanContacts", "UserLoanSponsoreDueDates");
        if (entity == null)
        {
            return new FacadeResponse<Guid>
            {
                Message = "آبجکت یافت نشد"
            };
        }
        if (entity.IsCanceled)
        {
            return new FacadeResponse<Guid>
            {
                Message = "این پرونده قبلا فسخ شده است."
            };
        }
        if (entity.IsCheckouted == true)
        {
            return new FacadeResponse<Guid>
            {
                Message = "پرونده ای که تسویه شده باشد قابل فسخ نمی باشد."
            };
        }
        if (entity.Paid > 0m)
        {
            return new FacadeResponse<Guid>
            {
                Message = "پرونده دارای قسط پرداخت شده است و قابل فسخ نمی باشد."
            };
        }
        entity.IsCanceled = true;
        entity.Paid = 0m;
        entity.Remain = 0m;
        entity.PayableAmount = 0m;
        List<UserLoanPayment> source = await _userPaymentService.GetPaymentsByUserLoanId(id, includeMemberPurchases: true, includePaymentRequest: false);
        if (source.Any((UserLoanPayment x) => x.SumAmount != 0m))
        {
            return new FacadeResponse<Guid>
            {
                Message = "بدلیل ثبت پرداخت اقساط برای این وام، امکان فسخ وجود ندارد."
            };
        }
        if (source.SelectMany((UserLoanPayment x) => x.MemberPurchases).Any((MemberPurchase x) => x.IsPaymentSuccess))
        {
            return new FacadeResponse<Guid>
            {
                Message = "بدلیل ثبت پرداخت آنلاین قسط برای این وام، امکان فسخ وجود ندارد."
            };
        }
        _userPaymentService.DeleteByUserLoanId(id);
        entity.UserLoanPayments.Clear();
        await _userLoanService.Update(entity);
        return new FacadeResponse<Guid>
        {
            Success = true,
            Data = entity.Id
        };
    }

    public async Task<FacadeResponse<bool>> Delete(Guid id)
    {
        UserLoan entity = await _userLoanService.GetById(id);
        if (entity == null)
        {
            return new FacadeResponse<bool>
            {
                Message = "امکان حذف وجود ندارد. لطفا بعدا دوباره تلاش نمایید"
            };
        }
        if (_appConfig.IsCloud && entity.CreateAt != DateTime.MinValue && entity.CreateAt < DateTime.Now.AddMonths(-1))
        {
            return new FacadeResponse<bool>
            {
                Message = "در نسخه ابری امکان حذف پرونده ای که یکماه از ایجاد آن گذشته باشد وجود ندارد."
            };
        }
        List<UserLoanPayment> list = await _userPaymentService.GetPaymentsByUserLoanId(id, includeMemberPurchases: true, includePaymentRequest: false);
        if (list.SelectMany((UserLoanPayment x) => x.MemberPurchases).Any((MemberPurchase x) => x.IsPaymentSuccess))
        {
            return new FacadeResponse<bool>
            {
                Message = "بدلیل ثبت پرداخت آنلاین قسط برای این وام، امکان حذف وجود ندارد."
            };
        }
        foreach (UserLoanPayment item in list)
        {
            _memberPurchaseService.DeleteByUserPaymentId(item.Id);
            item.MemberPurchases?.Clear();
        }
        foreach (CustomerMarketerCommission item2 in await _customerMarketerCommissionService.GetByCondition((CustomerMarketerCommission x) => x.UserLoanId == id))
        {
            await _customerMarketerCommissionService.Delete(item2);
        }
        await _userLoanService.Delete(entity);
        if (entity.CustomerPurchasePlanId.HasValue)
        {
            CustomerPurchasePlan customerPurchasePlan = await _customerPurchasePlanService.GetById(entity.CustomerPurchasePlanId.Value);
            if (customerPurchasePlan != null)
            {
                customerPurchasePlan.RemainLoanCount++;
                await _customerPurchasePlanService.Update(customerPurchasePlan);
            }
        }
        return new FacadeResponse<bool>
        {
            Success = true
        };
    }

    public async Task<FacadeResponse<List<UserLoanModel>>> AgentsOverDueList()
    {
        List<UserLoan> entities = await _userLoanService.GetUserLoansHasAgent("Loan", "User", "Agent", "UserLoanRequest.Status", "UserLoanRequest.AgentStatus");
        List<Transaction> withdrawUserLoans = await _transactionService.GetByCondition((Transaction x) => x.IsWithdraw && x.AgentId.HasValue && x.UserLoanId.HasValue);
        withdrawUserLoans.Select((Transaction x) => x.UserLoanId).ToList();
        entities = entities.Where((UserLoan userLoan) =>
        {
            DateTime? agentDueDate = userLoan.AgentDueDate;
            DateTime dateTime = DateTime.Now.AddDays(1.0);
            return agentDueDate.HasValue && agentDueDate.GetValueOrDefault() <= dateTime && userLoan.AgentId.HasValue && withdrawUserLoans.Where((Transaction w) => w.UserLoanId == userLoan.Id).Sum((Transaction w) => w.Amount) < userLoan.AgentPrice;
        }).ToList();
        List<UserLoanModel> list = entities.Select((UserLoan x) => new UserLoanModel(x)).ToList();
        list.ForEach((UserLoanModel userLoan) =>
        {
            userLoan.SumSponsorPayments = withdrawUserLoans.Where((Transaction pay) => pay.IsWithdraw && pay.UserLoanId == userLoan.Id && pay.AgentId.HasValue).Sum((Transaction p) => p.Amount);
            userLoan.ChargeType = userLoan.UserLoanRequest?.ChargeType ?? AgentLoanRequestChargeTypes.پول_نقد;
        });
        return new FacadeResponse<List<UserLoanModel>>
        {
            Success = true,
            Data = list
        };
    }

    public async Task<FacadeResponse<List<UserLoanContactModel>>> ContactsOverDueList()
    {
        List<Guid?> withdrawUserLoanContactIds = (await _transactionService.GetByCondition((Transaction x) => x.IsWithdraw && x.ContactId.HasValue && x.UserLoanContactId.HasValue)).Select((Transaction x) => x.UserLoanContactId).ToList();
        List<UserLoanContactModel> data = (await _userLoanContactService.GetByCondition((UserLoanContact x) => x.DueDate.Date <= DateTime.Now.Date && !withdrawUserLoanContactIds.Contains(x.Id), "Contact", "UserLoan.Loan", "UserLoan.User")).Select((UserLoanContact x) => new UserLoanContactModel(x)).ToList();
        return new FacadeResponse<List<UserLoanContactModel>>
        {
            Success = true,
            Data = data
        };
    }

    public async Task<FacadeResponse<dynamic>> GetDepositChargeDueDate(bool? returnList = false)
    {
        AppSettings appSettings = await _appSettingsService.GetAppSettings();
        AppSettingsModel appSettingsModel = new AppSettingsModel(appSettings);
        int count = 0;
        decimal value = 0m;
        List<UserLoanModel> userLoanModels = new List<UserLoanModel>();
        string[] depositChargeDueDateStatuses = appSettingsModel.DepositChargeDueDateStatuses;
        if (depositChargeDueDateStatuses != null && depositChargeDueDateStatuses.Length != 0)
        {
            List<UserLoan> list = await _userLoanService.GetByCondition((UserLoan x) => x.DepositChargeDueDate != null && x.DepositChargeDueDate.Value.Date <= DateTime.Now.Date && x.UserLoanRequest != null && appSettings.DepositChargeDueDateStatuses.Contains(((object)x.UserLoanRequest.StatusId).ToString()), "UserLoanRequest.Agent", "Loan", "User");
            if (returnList.Value)
            {
                userLoanModels = list.Select((UserLoan x) => new UserLoanModel(x)).ToList();
                userLoanModels.ForEach((UserLoanModel userLoan) =>
                {
                    userLoan.ChargeType = userLoan.UserLoanRequest?.ChargeType ?? AgentLoanRequestChargeTypes.پول_نقد;
                });
            }
            count = list.Count;
            value = list.Sum((UserLoan x) => x.Amount);
        }
        return new FacadeResponse<object>
        {
            Success = true,
            Data = new
            {
                items = (returnList.Value ? userLoanModels : null),
                count = count,
                value = value
            }
        };
    }

    public async Task<FacadeResponse<List<dynamic>>> Search(string search)
    {
        List<object> data = (await _userLoanService.Search(search)).Select((UserLoan x) => new
        {
            x.Id,
            x.Code,
            x.Loan.Title,
            x.UserId,
            x.User.NationalCode,
            x.User.FullName
        }).Cast<object>().ToList();
        return new FacadeResponse<List<object>>
        {
            Success = true,
            Data = data
        };
    }
}
