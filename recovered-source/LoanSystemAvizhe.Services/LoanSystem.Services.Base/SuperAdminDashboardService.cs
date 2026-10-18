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
using Microsoft.EntityFrameworkCore;
using Spm.Services.Infrastructure;

namespace LoanSystem.Services.Base;

public class SuperAdminDashboardService : BaseService, ISuperAdminDashboardService
{
    private readonly UserManager _userManager;

    public SuperAdminDashboardService(AppDbContext context, UserManager userManager, ICurrentUserService currentUserService)
        : base(context, currentUserService)
    {
        _userManager = userManager;
    }

    public async Task<SuperAdminDashboardModel> GetData()
    {
        SuperAdminDashboardModel result = new SuperAdminDashboardModel();
        DateTime.Now.AddYears(-1);
        List<Customer> customerEntities = await _context.Customers.AsNoTracking().ToListAsync();
        List<Purchase> purchaseEntities = await (from x in _context.Purchases.AsNoTracking()
                                                 where x.IsPaymentSuccess
                                                 select x).ToListAsync();
        List<AffiliateCommission> affiliateCommissionEntities = await _context.AffiliateCommissions.AsNoTracking().ToListAsync();
        result.CustomerChart = CalculateCustomerChart(customerEntities);
        result.SaleChart = CalculateSaleChart(purchaseEntities, affiliateCommissionEntities);
        result.TotalRegister = customerEntities.Count();
        result.TotalCustomer = customerEntities.Count((Customer x) => x.Purchased);
        result.TotalCloudCustomer = customerEntities.Count((Customer x) => !x.Permanent && x.Purchased);
        result.TotalPermanentCustomer = customerEntities.Count((Customer x) => x.Permanent && x.Purchased);
        result.TotalSale = purchaseEntities.Where((Purchase x) => x.ProductType == ProductTypes.Permanent || x.ProductType == ProductTypes.Cloud).Sum((Purchase x) => x.Amount);
        result.TotalCommission = affiliateCommissionEntities.Sum((AffiliateCommission x) => x.Amount);
        result.TotalProfit = result.TotalSale - result.TotalCommission;
        result.TotalSmsCharge = purchaseEntities.Where((Purchase x) => x.ProductType == ProductTypes.SMS).Sum((Purchase x) => x.Amount);
        result.TotalSmsCredits = _context.AppSettings.AsNoTracking().IgnoreQueryFilters().Sum((AppSettings x) => x.SmsCredit);
        SuperAdminDashboardModel superAdminDashboardModel = result;
        superAdminDashboardModel.TotalUserLoan = await _context.UserLoans.CountAsync();
        DateTime today = DateTime.Now;
        List<Customer> source = customerEntities.Where((Customer x) => x.RegDate.ToPersianDateObject().Month == today.ToPersianDateObject().Month && x.RegDate.ToPersianDateObject().Year == today.ToPersianDateObject().Year).ToList();
        List<Purchase> source2 = purchaseEntities.Where((Purchase x) => x.StartDate.ToPersianDateObject().Month == today.ToPersianDateObject().Month && x.StartDate.ToPersianDateObject().Year == today.ToPersianDateObject().Year).ToList();
        List<AffiliateCommission> source3 = affiliateCommissionEntities.Where((AffiliateCommission x) => x.Date.ToPersianDateObject().Month == today.ToPersianDateObject().Month && x.Date.ToPersianDateObject().Year == today.ToPersianDateObject().Year).ToList();
        result.CurrentMonthRegister = source.Count();
        result.CurrentMonthCustomer = source.Count((Customer x) => x.Purchased);
        result.CurrentMonthCloudCustomer = source.Count((Customer x) => !x.Permanent && x.Purchased);
        result.CurrentMonthPermanentCustomer = source.Count((Customer x) => x.Permanent && x.Purchased);
        result.CurrentMonthSale = source2.Where((Purchase x) => x.ProductType == ProductTypes.Permanent || x.ProductType == ProductTypes.Cloud).Sum((Purchase x) => x.Amount);
        result.CurrentMonthCommission = source3.Sum((AffiliateCommission x) => x.Amount);
        result.CurrentMonthProfit = result.CurrentMonthSale - result.CurrentMonthCommission;
        result.CurrentMonthSmsCharge = source2.Where((Purchase x) => x.ProductType == ProductTypes.SMS).Sum((Purchase x) => x.Amount);
        superAdminDashboardModel = result;
        superAdminDashboardModel.CurrentMonthUserLoan = (await _context.UserLoans.Where((UserLoan x) => x.StartDate > ((DateTime)today).AddDays(-40.0)).ToListAsync()).Count((UserLoan x) => x.StartDate.ToPersianDateObject().Month == today.ToPersianDateObject().Month && x.StartDate.ToPersianDateObject().Year == today.ToPersianDateObject().Year);
        superAdminDashboardModel = result;
        superAdminDashboardModel.PendingSmsPatterns = await _context.SmsPatterns.CountAsync((SmsPattern x) => (int)x.Status == 0);
        superAdminDashboardModel = result;
        superAdminDashboardModel.UnSeenedConsultants = await _context.Consultants.CountAsync((Consultant x) => !x.IsSeen);
        return result;
    }

    private List<dynamic> CalculateCustomerChart(List<Customer> customers)
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
                Register = customers.Count((Customer x) =>
                {
                    DateTime regDate = x.RegDate;
                    DateTime? dateTime2 = start;
                    if (regDate >= dateTime2)
                    {
                        regDate = x.RegDate;
                        dateTime2 = end;
                        return regDate <= dateTime2;
                    }
                    return false;
                }),
                Customer = customers.Count((Customer x) =>
                {
                    DateTime regDate = x.RegDate;
                    DateTime? dateTime2 = start;
                    if (regDate >= dateTime2)
                    {
                        regDate = x.RegDate;
                        dateTime2 = end;
                        if (regDate <= dateTime2)
                        {
                            return x.Purchased;
                        }
                    }
                    return false;
                })
            });
        }
        return list;
    }

    private List<dynamic> CalculateSaleChart(List<Purchase> purchases, List<AffiliateCommission> affiliateCommissions)
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
                Sale = purchases.Where((Purchase x) =>
                {
                    DateTime startDate = x.StartDate;
                    DateTime? dateTime2 = start;
                    if (startDate >= dateTime2)
                    {
                        startDate = x.StartDate;
                        dateTime2 = end;
                        if (startDate <= dateTime2)
                        {
                            if (x.ProductType != ProductTypes.Permanent)
                            {
                                return x.ProductType == ProductTypes.Cloud;
                            }
                            return true;
                        }
                    }
                    return false;
                }).Sum((Purchase x) => x.Amount),
                Commission = affiliateCommissions.Where((AffiliateCommission x) =>
                {
                    DateTime date = x.Date;
                    DateTime? dateTime2 = start;
                    if (date >= dateTime2)
                    {
                        date = x.Date;
                        dateTime2 = end;
                        return date <= dateTime2;
                    }
                    return false;
                }).Sum((AffiliateCommission x) => x.Amount)
            });
        }
        return list;
    }
}
