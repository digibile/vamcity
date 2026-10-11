using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Core;
using LoanSystem.Core.Configuration;
using LoanSystem.Data;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Models;
using LoanSystem.Services.Users;
using LoanSystemAvizhe.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Spm.Services.Infrastructure;

namespace LoanSystem.Services.Base;

public class CustomerMarketerDashboardService : BaseService, ICustomerMarketerDashboardService
{
	private readonly ICustomerMarketerService _customerMarketerService;

	private readonly ITransactionService _transactionService;

	private readonly ICustomerMarketerCommissionService _customerMarketerCommissionService;

	private readonly IUserLoanRequestService _userLoanRequestService;

	private readonly IMemberService _memberService;

	private readonly ICustomerService _customerService;

	private readonly IVisitorLogService _visitorLogService;

	private readonly UserManager _userManager;

	public CustomerMarketerDashboardService(AppDbContext context, IOptions<AppConfig> appConfig, ICustomerMarketerService customerMarketerService, IUserLoanRequestService userLoanRequestService, UserManager userManager, ICustomerMarketerCommissionService customerMarketerCommissionService, ITransactionService transactionService, IMemberService memberService, ICurrentUserService currentUserService, ICustomerService customerService, IVisitorLogService visitorLogService)
		: base(context, currentUserService)
	{
		_customerMarketerService = customerMarketerService;
		_userManager = userManager;
		_customerMarketerCommissionService = customerMarketerCommissionService;
		_transactionService = transactionService;
		_userLoanRequestService = userLoanRequestService;
		_memberService = memberService;
		_customerService = customerService;
		_visitorLogService = visitorLogService;
	}

	public async Task<CustomerMarketerDashboardModel> GetData(long customerMarketerId)
	{
		CustomerMarketerDashboardModel result = new CustomerMarketerDashboardModel();
		User marketer = await _customerMarketerService.GetById(customerMarketerId);
		if (marketer == null)
		{
			return result;
		}
		DateTime startDate = DateTime.Now.AddYears(-1);
		IQueryable<VisitorLog> source = (from x in _context.VisitorLogs.AsNoTracking()
			where x.Mrk == marketer.AffiliateCode
			select x).AsQueryable();
		result.reffererCount = source.Count();
		result.ReffererChart = CalculateReffereChart(source.Where((VisitorLog x) => x.Date.Date >= ((DateTime)startDate).Date).ToList());
		CustomerMarketerDashboardModel customerMarketerDashboardModel = result;
		customerMarketerDashboardModel.totalReceives = (await _transactionService.GetByCondition((Transaction x) => x.IsWithdraw && x.CustomerMarketerCommission.MarketerId == marketer.Id, "CustomerMarketerCommission")).Sum((Transaction x) => x.Amount);
		customerMarketerDashboardModel = result;
		customerMarketerDashboardModel.totalCommission = (await _customerMarketerCommissionService.GetByCondition((CustomerMarketerCommission x) => x.MarketerId == marketer.Id)).Sum((CustomerMarketerCommission x) => x.Amount);
		customerMarketerDashboardModel = result;
		customerMarketerDashboardModel.registerCount = (await _memberService.GetByCondition((User x) => x.CustomerMarketerId == (long?)marketer.Id)).Count();
		customerMarketerDashboardModel = result;
		customerMarketerDashboardModel.loanRequstCounts = (await _userLoanRequestService.GetByCondition((UserLoanRequest x) => x.User.CustomerMarketerId == (long?)marketer.Id, false, "User")).Count();
		customerMarketerDashboardModel = result;
		customerMarketerDashboardModel.userLoanCounts = (await _customerMarketerCommissionService.GetByCondition((CustomerMarketerCommission x) => x.UserLoan.User.CustomerMarketerId == (long?)marketer.Id, "UserLoan.User")).Count();
		return result;
	}

	private List<dynamic> CalculateReffereChart(List<VisitorLog> visitorLogs)
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
				Amount = visitorLogs.Count((VisitorLog x) =>
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
				})
			});
		}
		return list;
	}
}
