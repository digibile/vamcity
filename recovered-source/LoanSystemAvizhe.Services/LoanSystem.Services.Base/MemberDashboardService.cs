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

public class MemberDashboardService : BaseService, IMemberDashboardService
{
	private readonly UserManager _userManager;

	public MemberDashboardService(AppDbContext context, UserManager userManager, ICurrentUserService currentUserService)
		: base(context, currentUserService)
	{
		_userManager = userManager;
	}

	public async Task<MemberDashboardModel> GetData(long memberId)
	{
		MemberDashboardModel result = new MemberDashboardModel();
		DateTime startDate = DateTime.Now.AddYears(-1);
		List<UserLoanPayment> payments = await _context.UserLoanPayments.Where((UserLoanPayment x) => x.UserLoan.UserId == memberId).AsNoTracking().ToListAsync();
		List<UserSavedMoney> source = await _context.UserSavedMonies.Where((UserSavedMoney x) => x.MemberId == memberId).AsNoTracking().ToListAsync();
		result.PaymentChart = CalculatePaymentsChart(payments.Where((UserLoanPayment x) => x.SaveDate >= startDate).ToList());
		result.SavedMoneyChart = CalculateSavedMoniesChart(source.Where((UserSavedMoney x) => x.SaveDate >= startDate).ToList());
		MemberDashboardModel memberDashboardModel = result;
		memberDashboardModel.MemberTotalLoans = await CalculateTotalLoans(memberId);
		result.MemberTotalPaid = CalculateTotalPaid(payments);
		result.MemberTotalUnpaid = Math.Max(0m, result.MemberTotalLoans - result.MemberTotalPaid);
		memberDashboardModel = result;
		memberDashboardModel.MemberTotalSave = await CalculateTotalSaveMonies(memberId);
		UserLoanPayment lastPaymentMonth = await _context.UserLoanPayments.OrderByDescending((UserLoanPayment x) => x.TargetMonth).FirstOrDefaultAsync();
		UserSavedMoney userSavedMoney = await _context.UserSavedMonies.OrderByDescending((UserSavedMoney x) => x.TargetMonth).FirstOrDefaultAsync();
		DateTime dateTime = DateTime.Now;
		if (lastPaymentMonth == null && userSavedMoney != null)
		{
			dateTime = userSavedMoney.TargetMonth;
		}
		else if (lastPaymentMonth != null && userSavedMoney == null)
		{
			dateTime = lastPaymentMonth.TargetMonth;
		}
		else if (lastPaymentMonth != null && userSavedMoney != null)
		{
			dateTime = ((lastPaymentMonth.TargetMonth >= userSavedMoney.TargetMonth) ? lastPaymentMonth.TargetMonth : userSavedMoney.TargetMonth);
		}
		result.LastMonthPersianDate = dateTime.ToPersianDateObject();
		(decimal, int) tuple = CalculateMonthPayments(result.LastMonthPersianDate, startDate);
		result.MonthInstallments = tuple.Item1;
		result.MonthInstallmentsCounts = tuple.Item2;
		(decimal, int) tuple2 = await CalculateMonthSavedMonies(result.LastMonthPersianDate);
		result.MonthSaveds = tuple2.Item1;
		result.MonthSavedsCounts = tuple2.Item2;
		result.CostShare = result.TotalLoans / 100m;
		memberDashboardModel = result;
		memberDashboardModel.Expense = await GetTotalExpenses();
		result.MonthBalance = result.TotalPaid + result.TotalSave - result.TotalLoans + (result.CostShare - result.Expense) - (decimal)((result.TotalLoans > 1000000000m) ? 954803890 : 0);
		memberDashboardModel = result;
		memberDashboardModel.AllCheckoutedCount = await GetAllCheckoutedCount();
		memberDashboardModel = result;
		memberDashboardModel.CheckoutedThisMonthCount = await GetCheckoutedThisMonthCount(lastPaymentMonth?.TargetMonth ?? DateTime.Now);
		memberDashboardModel = result;
		memberDashboardModel.LoanRequestCount = await _context.UserLoanRequests.CountAsync((UserLoanRequest x) => x.UserId == memberId);
		memberDashboardModel = result;
		memberDashboardModel.TicketsCount = await (from x in _context.Tickets.AsNoTracking()
			where ((long?)x.CreatedById == _currentUserService.UserId || x.TargetUserId == _currentUserService.UserId) && (int)x.Status == 2
			select x.Id).Distinct().CountAsync();
		return result;
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

	private async Task<decimal> GetTotalExpenses()
	{
		return await _context.Expenses.SumAsync((Expense x) => x.Amount);
	}

	private (decimal, int) CalculateMonthPayments(PersianDate persianDate, DateTime startDate)
	{
		List<UserLoanPayment> source = _context.UserLoanPayments.Where((UserLoanPayment x) => x.TargetMonth >= startDate).AsNoTracking().ToList();
		return (source.Where((UserLoanPayment x) => x.TargetMonth.ToPersianDateObject().Month == persianDate.Month && x.TargetMonth.ToPersianDateObject().Year == persianDate.Year).Sum((UserLoanPayment x) => x.SumAmount), source.Where((UserLoanPayment x) => x.TargetMonth.ToPersianDateObject().Month == persianDate.Month && x.TargetMonth.ToPersianDateObject().Year == persianDate.Year).Count());
	}

	private async Task<(decimal, int)> CalculateMonthSavedMonies(PersianDate persianDate)
	{
		(string, string) startEndOfMonthInPersian = DateTimeExtentions.GetStartEndOfMonthInPersian(persianDate.Date.ToGregorianDateTime().Value);
		DateTime? start = startEndOfMonthInPersian.Item1.ToGregorianDateTime();
		DateTime? end = startEndOfMonthInPersian.Item2.ToGregorianDateTime();
		List<UserSavedMoney> source = await _context.UserSavedMonies.Where((UserSavedMoney x) => x.TargetMonth >= start && x.TargetMonth <= end).ToListAsync();
		return (source.Sum((UserSavedMoney x) => x.Amount), source.Count());
	}

	private decimal CalculateTotalSaveMonies()
	{
		return _context.UserSavedMonies.Sum((UserSavedMoney x) => x.Amount);
	}

	private Task<decimal> CalculateTotalLoans()
	{
		return _context.UserLoans.SumAsync((UserLoan x) => x.Amount);
	}

	private async Task<decimal> CalculateTotalSaveMonies(long memberId)
	{
		return await _context.UserSavedMonies.Where((UserSavedMoney x) => x.MemberId == memberId).SumAsync((UserSavedMoney x) => x.Amount);
	}

	private decimal CalculateTotalPaid(List<UserLoanPayment> payments)
	{
		return payments.Sum((UserLoanPayment x) => x.SumAmount);
	}

	private async Task<decimal> CalculateTotalLoans(long memberId)
	{
		return await _context.UserLoans.Where((UserLoan x) => x.UserId == memberId).SumAsync((UserLoan x) => x.PayableAmount);
	}

	private List<dynamic> CalculateSavedMoniesChart(List<UserSavedMoney> userSavedMonies)
	{
		List<object> list = new List<object>();
		for (int i = 0; i < 12; i++)
		{
			DateTime dateTime = DateTime.Now.AddMonths(i * -1);
			(string, string) startEndOfMonthInPersian = GetStartEndOfMonthInPersian(dateTime);
			DateTime? start = startEndOfMonthInPersian.Item1.ToGregorianDateTime();
			DateTime? end = startEndOfMonthInPersian.Item2.ToGregorianDateTime();
			list.Add(new
			{
				Month = dateTime.ToPersianDateObject().MonthName + " " + dateTime.ToPersianDateObject().Year,
				Amount = userSavedMonies.Where((UserSavedMoney x) =>
				{
					DateTime saveDate = x.SaveDate;
					DateTime? dateTime2 = start;
					if (saveDate >= dateTime2)
					{
						saveDate = x.SaveDate;
						dateTime2 = end;
						return saveDate <= dateTime2;
					}
					return false;
				}).Sum((UserSavedMoney x) => x.Amount)
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
			(string, string) startEndOfMonthInPersian = GetStartEndOfMonthInPersian(dateTime);
			DateTime? start = startEndOfMonthInPersian.Item1.ToGregorianDateTime();
			DateTime? end = startEndOfMonthInPersian.Item2.ToGregorianDateTime();
			list.Add(new
			{
				Month = dateTime.ToPersianDateObject().MonthName + " " + dateTime.ToPersianDateObject().Year,
				Amount = userLoanPayments.Where((UserLoanPayment x) =>
				{
					DateTime saveDate = x.SaveDate;
					DateTime? dateTime2 = start;
					if (saveDate >= dateTime2)
					{
						saveDate = x.SaveDate;
						dateTime2 = end;
						return saveDate <= dateTime2;
					}
					return false;
				}).Sum((UserLoanPayment x) => x.SumAmount)
			});
		}
		return list;
	}

	private (string, string) GetStartEndOfMonthInPersian(DateTime dateTime)
	{
		string[] array = dateTime.ToPersianDate().Split("/");
		string text = array[0];
		string text2 = array[1];
		string item = text + "/" + text2 + "/01";
		int.TryParse(text2, out var result);
		int.TryParse(text, out var result2);
		result = ((result + 1 > 12) ? 1 : result);
		object arg = ((result == 1) ? (result2 + 1) : result2);
		string arg2 = result.ToString().PadLeft(2, '0');
		string arg3;
		if (result < 6)
		{
			arg3 = "31";
		}
		else
		{
			arg3 = ((result != 12) ? "30" : "29");
		}
		string item2 = $"{arg}/{arg2}/{arg3}";
		return (item, item2);
	}
}
