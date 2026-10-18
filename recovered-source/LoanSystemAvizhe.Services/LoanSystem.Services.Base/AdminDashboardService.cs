using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Core;
using LoanSystem.Data;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Models;
using LoanSystem.Services.Users;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Spm.Services.Infrastructure;

namespace LoanSystem.Services.Base;

public class AdminDashboardService : BaseService, IAdminDashboardService
{
    private readonly UserManager _userManager;

    private readonly IUserPaymentService _userPaymentService;

    private readonly IUserLoanRequestService _userLoanRequestService;

    private readonly IUserLoanContactService _userLoanContactService;

    private readonly ILoanRequestStatusService _loanRequestStatusService;

    public AdminDashboardService(AppDbContext context, UserManager userManager, IUserPaymentService userPaymentService, ICurrentUserService currentUserService, IUserLoanRequestService userLoanRequestService, IUserLoanContactService userLoanContactService, ILoanRequestStatusService loanRequestStatusService)
        : base(context, currentUserService)
    {
        _userManager = userManager;
        _userPaymentService = userPaymentService;
        _userLoanRequestService = userLoanRequestService;
        _userLoanContactService = userLoanContactService;
        _loanRequestStatusService = loanRequestStatusService;
    }

    public async Task<int> GetPendingCustomerOrdersCount()
    {
        return await (from x in _context.CustomerOrders.AsNoTracking()
                      where !x.IsSeen
                      select x).CountAsync();
    }

    public async Task<int> GetPendingTicketsCount()
    {
        return await (from x in _context.Tickets.AsNoTracking()
                      where (int)x.Status == 0
                      select x).CountAsync();
    }

    public async Task<dynamic> GetAgentFactorGatewayCount()
    {
        List<AgentFactorModel> list = (await _context.AgentFactors.Include((AgentFactor x) => x.MemberPurchase).Include((AgentFactor x) => x.WithdrawAgentFactors).ThenInclude((WithdrawAgentFactor x) => x.Withdraw)
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync()).Select((AgentFactor x) => new AgentFactorModel(x)).ToList();
        return new
        {
            TotalCount = list.Count,
            PayedByCustomer = list.Sum((AgentFactorModel x) => PayedAmountOfFactorByCustomer(x)),
            Commission = list.Sum((AgentFactorModel x) => x.Commission),
            NotCheckoutCount = list.Count((AgentFactorModel x) => PayedAmountOfFactorByCustomer(x) - x.SumWithdraws - x.Commission > 0m),
            NotCheckoutAmount = list.Where((AgentFactorModel x) => PayedAmountOfFactorByCustomer(x) - x.SumWithdraws - x.Commission > 0m).Sum((AgentFactorModel x) => PayedAmountOfFactorByCustomer(x) - x.SumWithdraws - x.Commission)
        };
        static decimal PayedAmountOfFactorByCustomer(AgentFactorModel agentFactor)
        {
            if (agentFactor.CheckoutType == 0)
            {
                return 0m;
            }
            if (agentFactor.CheckoutType != AgentFactorCheckoutTypes.آنلاین)
            {
                return agentFactor.Amount;
            }
            return (agentFactor?.MemberPurchase?.Amount).GetValueOrDefault();
        }
    }

    public async Task<dynamic> GetMemberAuthorizationCount()
    {
        return new
        {
            AdminPending = ((await _context.MemberAuthorizationItems.GroupBy((MemberAuthorizationItem x) => x.Status, (LoanRequestAuthorizationStatus key, IEnumerable<MemberAuthorizationItem> y) => new
            {
                Status = key,
                Count = y.Count()
            }).ToListAsync()).FirstOrDefault(x => x.Status == LoanRequestAuthorizationStatus.در_انتطار_بررسی_کارشناس)?.Count ?? 0)
        };
    }

    public async Task<List<dynamic>> GetLastLoginMembers()
    {
        return (from x in (from x in (await _userManager.GetUsersInRoleAsync("Member")).ToList()
                           where x.LastLoginDate != DateTime.MinValue
                           orderby x.LastLoginDate descending
                           select x).Take(10)
                select new
                {
                    Id = x.Id,
                    FullName = x.FullName,
                    NationalCode = x.NationalCode,
                    LastLoginDate = x.LastLoginDate.ToPersianDateTime()
                }).Cast<object>().ToList();
    }

    public async Task<AdminDashboardModel> GetData()
    {
        AdminDashboardModel result = new AdminDashboardModel();
        DateTime startDate = DateTime.Now.AddYears(-1);
        _context.Loans.Where((Loan x) => x.CustomerId == _currentUserService.CustomerId).Load();
        IQueryable<UserLoan> query = _context.UserLoans.Where((UserLoan x) => x.Loan.CustomerId == _currentUserService.CustomerId && !x.IsCanceled).AsQueryable();
        _context.UserLoanPayments.Where((UserLoanPayment x) => query.Select((UserLoan q) => q.Id).Contains(x.UserLoanId)).Load();
        try
        {
            List<UserLoan> userLoanEntities = await query.ToListAsync();
            List<UserLoanModel> models = userLoanEntities.Select((UserLoan x) => new UserLoanModel(x)
            {
                LoanPayments = x.UserLoanPayments.Select((UserLoanPayment p) => new LoanPaymentModel(p)).ToList()
            }).ToList();
            foreach (UserLoanModel item in models)
            {
                await _userPaymentService.SetPaymentsDelays(item.LoanPayments);
            }
            List<UserLoanPayment> userLoanPayments = (from x in userLoanEntities.SelectMany((UserLoan x) => x.UserLoanPayments)
                                                      where x.TargetMonth >= startDate
                                                      select x).ToList();
            result.LoanChart = CalculateLoansChart(userLoanEntities.Where((UserLoan x) => x.StartDate >= startDate).ToList());
            result.PaymentChart = CalculatePaymentsChart(userLoanPayments);
            result.MemberCount = userLoanEntities.Select((UserLoan x) => x.UserId).Distinct().Count();
            result.TotalLoans = userLoanEntities.Sum((UserLoan x) => x.Amount);
            result.TotalProfitLoans = userLoanEntities.Sum((UserLoan x) => x.PayableAmount);
            result.TotalProfitDelayLoans = models.Sum((UserLoanModel x) => x.Delays) + result.TotalProfitLoans - models.Sum((UserLoanModel x) => x.Discount);
            result.TotalPaid = userLoanEntities.SelectMany((UserLoan x) => x.UserLoanPayments).Sum((UserLoanPayment x) => x.SumAmount);
            result.TotalUnpaid = result.TotalProfitDelayLoans - result.TotalPaid;
            result.TotalNonDueDated = (from x in userLoanEntities.Where((UserLoan x) => !x.IsCheckouted.HasValue || !x.IsCheckouted.Value).SelectMany((UserLoan x) => x.UserLoanPayments)
                                       where x.TargetMonth.Date > DateTime.Now.Date && x.SumAmount == 0m
                                       select x).Sum((UserLoanPayment x) => x.InstallmentAmount);
            result.AllCheckoutedCount = userLoanEntities.Where((UserLoan x) => x.IsCheckouted?.ToString() == "True").Count();
            result.AllLoansCount = userLoanEntities.Count();
            List<LoanRequestStatus> statuses = await _loanRequestStatusService.GetAllLoanRequestStatuses("SmsPattern");
            List<UserLoanRequest> source = await _userLoanRequestService.GetByCondition((UserLoanRequest x) => (int?)x.Status.SystemRequestStatus == (int?)15 || (int?)x.Status.SystemRequestStatus == (int?)18, false, "Status");
            result.NewLoanRequestCount = source.Count((UserLoanRequest x) => x.Status.SystemRequestStatus == SystemRequestStatus.در_انتظار_بررسی_کارشناس);
            result.TodayLoanRequestCount = source.Count((UserLoanRequest x) => x.Status.SystemRequestStatus == SystemRequestStatus.در_انتظار_بررسی_کارشناس && x.RequestDate.Date == DateTime.Now.Date);
            result.NewReviewLoanRequestCount = source.Count((UserLoanRequest x) => x.Status.SystemRequestStatus == SystemRequestStatus.بررسی_مجدد_کارشناس);
            result.NewLoanRequestTitle = statuses.FirstOrDefault((LoanRequestStatus x) => x.SystemRequestStatus == SystemRequestStatus.در_انتظار_بررسی_کارشناس)?.Title;
            result.NewReviewLoanRequestTitle = statuses.FirstOrDefault((LoanRequestStatus x) => x.SystemRequestStatus == SystemRequestStatus.بررسی_مجدد_کارشناس)?.Title;
            List<Transaction> withdrawUserLoans = await (from x in _context.Withdraws.AsNoTracking()
                                                         where x.IsWithdraw && x.AgentId.HasValue && x.UserLoanId.HasValue
                                                         select x).ToListAsync();
            withdrawUserLoans.Select((Transaction x) => x.UserLoanId).ToList();
            int agentOverDueCount = userLoanEntities.Count((UserLoan userLoan) =>
            {
                DateTime? agentDueDate = userLoan.AgentDueDate;
                DateTime now = DateTime.Now;
                return agentDueDate.HasValue && agentDueDate.GetValueOrDefault() <= now && userLoan.AgentId.HasValue && withdrawUserLoans.Where((Transaction w) => w.UserLoanId == userLoan.Id).Sum((Transaction w) => w.Amount) < userLoan.AgentPrice;
            });
            decimal agentOverDueTotal = userLoanEntities.Where((UserLoan userLoan) =>
            {
                DateTime? agentDueDate = userLoan.AgentDueDate;
                DateTime now = DateTime.Now;
                return agentDueDate.HasValue && agentDueDate.GetValueOrDefault() <= now && userLoan.AgentId.HasValue && withdrawUserLoans.Where((Transaction w) => w.UserLoanId == userLoan.Id).Sum((Transaction w) => w.Amount) < userLoan.AgentPrice;
            }).Sum((UserLoan x) => x.AgentPrice - withdrawUserLoans.Where((Transaction w) => w.UserLoanId == x.Id).Sum((Transaction w) => w.Amount));
            List<Transaction> withdrawUserLoanContacts = await (from x in _context.Withdraws.AsNoTracking()
                                                                where x.IsWithdraw && x.ContactId.HasValue && x.UserLoanContactId.HasValue
                                                                select x).ToListAsync();
            List<UserLoanContact> source2 = await _userLoanContactService.GetByCondition((UserLoanContact x) => x.DueDate.Date <= DateTime.Now.Date);
            int count = source2.Count((UserLoanContact userLoanContact) => withdrawUserLoanContacts.Where((Transaction w) => w.UserLoanContactId == userLoanContact.Id).Sum((Transaction w) => w.Amount) < userLoanContact.Share);
            decimal total = source2.Where((UserLoanContact userLoanContact) => withdrawUserLoanContacts.Where((Transaction w) => w.UserLoanContactId == userLoanContact.Id).Sum((Transaction w) => w.Amount) < userLoanContact.Share).Sum((UserLoanContact x) => x.Share);
            result.AgentOverdue = new
            {
                count = agentOverDueCount,
                total = agentOverDueTotal
            };
            result.ContactOverdue = new { count, total };
            return result;
        }
        catch (Exception)
        {
            throw;
        }
    }

    private async Task<decimal> GetMonthUnpaidUntilMonth(DateTime date)
    {
        return await (from x in _context.UserLoans.AsNoTracking()
                      where x.StartDate <= date
                      select x).SumAsync((UserLoan x) => x.Amount) - await (from x in _context.UserLoanPayments.Include((UserLoanPayment x) => x.UserLoan)
                                                                            where x.UserLoan.StartDate <= date && x.TargetMonth <= date
                                                                            select x).SumAsync((UserLoanPayment x) => x.SumAmount);
    }

    private async Task<decimal> GetTotalExpenses()
    {
        return await _context.Expenses.SumAsync((Expense x) => x.Amount);
    }

    private async Task<int> GetCheckoutedThisMonthCount(DateTime lastMonth)
    {
        List<Guid> userLoanIds = await (from x in _context.UserLoanPayments
                                        where x.TargetMonth.Month == ((DateTime)lastMonth).Month
                                        select x.UserLoanId).ToListAsync();
        return await _context.UserLoans.CountAsync((UserLoan x) => x.Remain == 0m && userLoanIds.Contains(x.Id));
    }

    private async Task<int> GetAllCheckoutedCount()
    {
        int count = await _context.UserLoans.AsNoTracking().CountAsync((UserLoan x) => x.Remain == 0m);
        List<long> adminIds = (await _userManager.GetUsersInRoleAsync("Admin")).Select((User x) => x.Id).ToList();
        List<long> userIds = await (from x in _context.UserLoans.AsNoTracking()
                                    select x.UserId).ToListAsync();
        int num = count;
        return num + await _context.Users.AsNoTracking().CountAsync((User x) => !userIds.Contains(x.Id) && !adminIds.Contains(x.Id));
    }

    private List<dynamic> CalculateLoansChart(List<UserLoan> userLoans)
    {
        List<object> list = new List<object>();
        for (int i = 0; i < 12; i++)
        {
            DateTime dateTime = DateTime.Now.AddMonths(i * -1);
            (string, string) startEndOfMonthInPersian = DateTimeExtentions.GetStartEndOfMonthInPersian(dateTime);
            DateTime? start = startEndOfMonthInPersian.Item1.ToGregorianDateTime();
            DateTime? end = startEndOfMonthInPersian.Item2.ToGregorianDateTime();
            list.Add(new
            {
                Month = dateTime.ToPersianDateObject().MonthName + " " + dateTime.ToPersianDateObject().Year,
                Amount = userLoans.Where((UserLoan x) =>
                {
                    DateTime startDate = x.StartDate;
                    DateTime? dateTime2 = start;
                    if (startDate >= dateTime2)
                    {
                        startDate = x.StartDate;
                        dateTime2 = end;
                        return startDate <= dateTime2;
                    }
                    return false;
                }).Sum((UserLoan x) => x.Amount)
            });
        }
        return list;
    }

    private List<dynamic> CalculatePaymentsChart(List<UserLoanPayment> userLoanPayments)
    {
        List<object> list = new List<object>();
        for (int i = 0; i < 12; i++)
        {
            DateTime dateTime = DateTime.Now.AddMonths(i * -1);
            (string, string) startEndOfMonthInPersian = DateTimeExtentions.GetStartEndOfMonthInPersian(dateTime);
            DateTime? start = startEndOfMonthInPersian.Item1.ToGregorianDateTime();
            DateTime? end = startEndOfMonthInPersian.Item2.ToGregorianDateTime();
            list.Add(new
            {
                Month = dateTime.ToPersianDateObject().MonthName + " " + dateTime.ToPersianDateObject().Year,
                Amount = userLoanPayments.Where((UserLoanPayment x) =>
                {
                    DateTime targetMonth = x.TargetMonth;
                    DateTime? dateTime2 = start;
                    if (targetMonth >= dateTime2)
                    {
                        targetMonth = x.TargetMonth;
                        dateTime2 = end;
                        return targetMonth <= dateTime2;
                    }
                    return false;
                }).Sum((UserLoanPayment x) => x.SumAmount)
            });
        }
        return list;
    }

    private (decimal, int) CalculateMonthPayments(List<UserLoanPayment> userLoanPayments, PersianDate persianDate)
    {
        return (userLoanPayments.Where((UserLoanPayment x) => x.TargetMonth.ToPersianDateObject().Month == persianDate.Month && x.TargetMonth.ToPersianDateObject().Year == persianDate.Year).Sum((UserLoanPayment x) => x.SumAmount), userLoanPayments.Where((UserLoanPayment x) => x.TargetMonth.ToPersianDateObject().Month == persianDate.Month && x.TargetMonth.ToPersianDateObject().Year == persianDate.Year).Count());
    }

    private async Task<(decimal, int)> CalculateMonthSavedMonies(PersianDate persianDate)
    {
        (string, string) startEndOfMonthInPersian = DateTimeExtentions.GetStartEndOfMonthInPersian(persianDate.Date.ToGregorianDateTime().Value);
        DateTime? start = startEndOfMonthInPersian.Item1.ToGregorianDateTime();
        DateTime? end = startEndOfMonthInPersian.Item2.ToGregorianDateTime();
        List<UserSavedMoney> source = await _context.UserSavedMonies.Where((UserSavedMoney x) => x.TargetMonth >= start && x.TargetMonth <= end).ToListAsync();
        return (source.Sum((UserSavedMoney x) => x.Amount), source.Count());
    }
}
