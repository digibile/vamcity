using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Areas.Admin.Models;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Data.Enums;
using LoanSystemAvizhe.Services.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoanSystem.Site.Areas.Admin.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Admin")]
public class DashboardController : BaseAdminController
{
    private readonly IAdminDashboardService _adminDashboardService;

    private readonly ITransactionService _transactionService;

    private readonly IUserPaymentService _userPaymentService;

    private readonly ILoanPaymentOtherService _loanPaymentOtherService;

    private readonly IntegratedSmsSenderService _integratedSmsSenderService;

    private readonly IntegratedEmailSenderService _integratedEmailSenderService;

    public DashboardController(ILogger<DashboardController> logger, IAdminDashboardService adminDashboardService, ITransactionService transactionService, IUserPaymentService userPaymentService, ILoanPaymentOtherService loanPaymentOtherService, IntegratedSmsSenderService integratedSmsSenderService, IntegratedEmailSenderService integratedEmailSenderService)
        : base(logger)
    {
        _adminDashboardService = adminDashboardService;
        _userPaymentService = userPaymentService;
        _transactionService = transactionService;
        _integratedSmsSenderService = integratedSmsSenderService;
        _integratedEmailSenderService = integratedEmailSenderService;
        _loanPaymentOtherService = loanPaymentOtherService;
    }

    [HttpGet("[action]")]
    public Task<ResponseState<AdminDashboardModel>> Get()
    {
        return TryCatch(async () =>
        {
            AdminDashboardModel model = await _adminDashboardService.GetData();
            OverDueInstallmentsModel overDueInstallmentsModel = await _userPaymentService.GetOverDueInstallments(loadAppSms: false);
            overDueInstallmentsModel.LoanPayments.Clear();
            model.OverDueInstallments = overDueInstallmentsModel;
            return Success(model);
        });
    }

    [HttpGet("[action]")]
    public Task<ResponseState<List<dynamic>>> GetLastLoginMembers()
    {
        return TryCatch(async () => Success(await _adminDashboardService.GetLastLoginMembers()));
    }

    [HttpGet("[action]")]
    public Task<ResponseState<int>> GetPendingTicketsCount()
    {
        return TryCatch(async () => Success(await _adminDashboardService.GetPendingTicketsCount()));
    }

    [HttpGet("[action]")]
    public Task<ResponseState<dynamic>> GetAgentFactorGatewayCount()
    {
        return TryCatch<object>(async () => Success(data: (dynamic)(await _adminDashboardService.GetAgentFactorGatewayCount())));
    }

    [HttpGet("[action]")]
    public Task<ResponseState<dynamic>> GetMemberAuthorizationCount()
    {
        return TryCatch<object>(async () => Success(data: (dynamic)(await _adminDashboardService.GetMemberAuthorizationCount())));
    }

    [HttpGet("[action]")]
    public Task<ResponseState<int>> GetPendingCustomerOrdersCount()
    {
        return TryCatch(async () => Success(await _adminDashboardService.GetPendingCustomerOrdersCount()));
    }

    [HttpGet("[action]")]
    public Task<ResponseState<Dictionary<string, object>>> GetSmsCredit()
    {
        return TryCatch(async () =>
        {
            decimal credit = await _integratedSmsSenderService.GetCredit();
            int today = await _integratedSmsSenderService.GetTodaySuccessSmsCount();
            int num = await _integratedEmailSenderService.GetTodaySuccessEmailCount();
            return Success(new Dictionary<string, object>
            {
                { "credit", credit },
                { "today", today },
                { "today_email", num }
            });
        });
    }

    [HttpGet("[action]/{paymentMethod}")]
    public Task<ResponseState<List<DashboardTransactionsViewModel>>> TurnOver(PaymentMethods paymentMethod, DateTime? startDate, DateTime? endDate, bool ignoreLimit = false, bool onlyUnknown = false)
    {
        return TryCatch(async () =>
        {
            List<Transaction> withdraws = await _transactionService.GetAllWithdraws(paymentMethod);
            Expression<Func<UserLoanPayment, bool>> expression = (UserLoanPayment x) => (int)x.PaymentMethod == (int)paymentMethod && x.SumAmount > 0m && !x.IsGoldCheckouted;
            Expression<Func<UserLoanPaymentOther, bool>> other_condition = (UserLoanPaymentOther x) => (int)x.PaymentMethod == (int)paymentMethod && x.Amount > 0m && !x.IsGoldCheckouted;
            if (startDate.HasValue)
            {
                expression = expression.AND((UserLoanPayment x) => x.SaveDate.Date >= ((DateTime?)startDate).Value.Date);
                other_condition = other_condition.AND((UserLoanPaymentOther x) => x.SaveDate.Date >= ((DateTime?)startDate).Value.Date);
                withdraws = withdraws.Where((Transaction x) => x.SaveDate.Date >= startDate.Value.Date).ToList();
            }
            if (endDate.HasValue)
            {
                expression = expression.AND((UserLoanPayment x) => x.SaveDate.Date <= ((DateTime?)endDate).Value.Date);
                other_condition = other_condition.AND((UserLoanPaymentOther x) => x.SaveDate.Date <= ((DateTime?)endDate).Value.Date);
                withdraws = withdraws.Where((Transaction x) => x.SaveDate.Date <= endDate.Value.Date).ToList();
            }
            if (onlyUnknown)
            {
                expression = expression.AND((UserLoanPayment x) => false);
                other_condition = other_condition.AND((UserLoanPaymentOther x) => false);
                withdraws = withdraws.Where((Transaction x) => x.IsUnknown).ToList();
            }
            List<UserLoanPayment> userLoanPayments = await _userPaymentService.GetByCondition(expression, "UserLoan.Loan", "UserLoan.User");
            List<UserLoanPaymentOther> source = await _loanPaymentOtherService.GetByCondition(other_condition, "UserLoanPayment.UserLoan.Loan", "UserLoanPayment.UserLoan.User");
            List<DashboardTransactionsViewModel> list = new List<DashboardTransactionsViewModel>();
            list.AddRange(withdraws.Select((Transaction x) => new DashboardTransactionsViewModel(x)));
            list.AddRange(userLoanPayments.Select((UserLoanPayment x) => new DashboardTransactionsViewModel(x)));
            int order = 0;
            list.AddRange(source.Select((UserLoanPaymentOther x) => new DashboardTransactionsViewModel(x)
            {
                Order = order++
            }));
            list = list.OrderBy((DashboardTransactionsViewModel x) => x.SaveDate).ToList();
            decimal balance = 0m;
            double weightBalance = 0.0;
            list.ForEach((DashboardTransactionsViewModel x) =>
            {
                balance += x.Debit;
                balance -= x.Credit;
                weightBalance += x.GoldWeight;
                x.Balance = balance;
                x.GoldWeightBalance = weightBalance;
            });
            list = ((!ignoreLimit) ? (from x in list
                                      orderby x.SaveDate descending, x.Order descending
                                      select x).Take(100).ToList() : (from x in list
                                                                      orderby x.SaveDate descending, x.Order descending
                                                                      select x).ToList());
            return Success(list);
        });
    }
}
