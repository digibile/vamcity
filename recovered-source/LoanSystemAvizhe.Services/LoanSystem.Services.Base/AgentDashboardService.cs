using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Data;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Models;
using LoanSystem.Services.Users;
using LoanSystemAvizhe.Core;
using Microsoft.EntityFrameworkCore;
using Spm.Services.Infrastructure;

namespace LoanSystem.Services.Base;

public class AgentDashboardService : BaseService, IAgentDashboardService
{
	private readonly UserManager _userManager;

	public AgentDashboardService(AppDbContext context, UserManager userManager, ICurrentUserService currentUserService)
		: base(context, currentUserService)
	{
		_userManager = userManager;
	}

	public async Task<AgentDashboardModel> GetData(long agentId)
	{
		new AgentDashboardModel();
		return (!((await _context.Users.FirstOrDefaultAsync((User x) => x.Id == agentId))?.AgentId.HasValue ?? false)) ? (await ProcessAgentData(agentId)) : (await ProcessSubAgentData(agentId));
	}

	private async Task<AgentDashboardModel> ProcessAgentData(long agentId)
	{
		AgentDashboardModel result = new AgentDashboardModel();
		List<UserLoan> userLoans = await (from x in _context.UserLoans.AsNoTracking().Include((UserLoan x) => x.UserLoanPayments)
			where x.AgentId == (long?)agentId
			select x).ToListAsync();
		List<long> memberIdsFromUserLoans = userLoans.Select((UserLoan x) => x.UserId).ToList();
		AgentDashboardModel agentDashboardModel = result;
		agentDashboardModel.MemberCount = await _context.Users.AsNoTracking().CountAsync((User x) => x.AgentId == (long?)agentId || memberIdsFromUserLoans.Contains(x.Id));
		result.TotalLoans = userLoans.Sum((UserLoan x) => x.Amount);
		result.AllLoansCount = userLoans.Count();
		result.AllCheckoutedCount = userLoans.Count((UserLoan x) => x.IsCheckouted == true);
		result.TotalAgentShare = userLoans.Sum((UserLoan x) => x.AgentPrice);
		result.TotalPaid = userLoans.SelectMany((UserLoan x) => x.UserLoanPayments).Sum((UserLoanPayment x) => x.SumAmount);
		result.TotalUnpaid = result.TotalLoans - result.TotalPaid;
		agentDashboardModel = result;
		agentDashboardModel.AllLoanRequestsCount = await _context.UserLoanRequests.AsNoTracking().CountAsync((UserLoanRequest x) => x.AgentId == (long?)agentId);
		agentDashboardModel = result;
		agentDashboardModel.TotalReceivedsBySponsore = await (from x in _context.Withdraws.AsNoTracking()
			where x.AgentId == (long?)agentId && x.IsWithdraw
			select x).SumAsync((Transaction x) => x.Amount);
		List<SubAgentTransaction> withdrawUserLoans = await (from x in _context.SubAgentTransactions.AsNoTracking().Include((SubAgentTransaction x) => x.UserLoan)
			where x.IsWithdraw && x.UserLoan.AgentId == (long?)agentId && x.UserLoan.SubAgentId.HasValue
			select x).ToListAsync();
		withdrawUserLoans.Select((SubAgentTransaction x) => x.UserLoanId).ToList();
		int subAgentOverDueCount = userLoans.Count((UserLoan userLoan) =>
		{
			DateTime? agentDueDate = userLoan.AgentDueDate;
			DateTime now = DateTime.Now;
			return agentDueDate.HasValue && agentDueDate.GetValueOrDefault() <= now && userLoan.SubAgentId.HasValue && withdrawUserLoans.Where((SubAgentTransaction w) => w.UserLoanId == userLoan.Id).Sum((SubAgentTransaction w) => w.Amount) < userLoan.SubAgentPrice;
		});
		decimal subAgentOverDueTotal = userLoans.Where((UserLoan userLoan) =>
		{
			DateTime? agentDueDate = userLoan.AgentDueDate;
			DateTime now = DateTime.Now;
			return agentDueDate.HasValue && agentDueDate.GetValueOrDefault() <= now && userLoan.SubAgentId.HasValue && withdrawUserLoans.Where((SubAgentTransaction w) => w.UserLoanId == userLoan.Id).Sum((SubAgentTransaction w) => w.Amount) < userLoan.SubAgentPrice;
		}).Sum((UserLoan x) => x.SubAgentPrice - withdrawUserLoans.Where((SubAgentTransaction w) => w.UserLoanId == x.Id).Sum((SubAgentTransaction w) => w.Amount));
		int agentOverDueCount = userLoans.Count((UserLoan userLoan) => userLoan.AgentDueDate <= DateTime.Now && withdrawUserLoans.Where((SubAgentTransaction w) => w.UserLoanId == userLoan.Id).Sum((SubAgentTransaction w) => w.Amount) < userLoan.AgentPrice);
		decimal agentOverDueTotal = userLoans.Where((UserLoan userLoan) => userLoan.AgentDueDate <= DateTime.Now && withdrawUserLoans.Where((SubAgentTransaction w) => w.UserLoanId == userLoan.Id).Sum((SubAgentTransaction w) => w.Amount) < userLoan.AgentPrice).Sum((UserLoan x) => x.AgentPrice - withdrawUserLoans.Where((SubAgentTransaction w) => w.UserLoanId == x.Id).Sum((SubAgentTransaction w) => w.Amount));
		agentDashboardModel = result;
		agentDashboardModel.TicketsCount = await (from x in _context.Tickets.AsNoTracking()
			where ((long?)x.CreatedById == _currentUserService.UserId || x.TargetUserId == _currentUserService.UserId) && (int)x.Status == 2
			select x.Id).Distinct().CountAsync();
		result.SubAgentOverdue = new
		{
			count = subAgentOverDueCount,
			total = subAgentOverDueTotal
		};
		result.AgentOverdue = new
		{
			count = agentOverDueCount,
			total = agentOverDueTotal
		};
		return result;
	}

	private async Task<AgentDashboardModel> ProcessSubAgentData(long agentId)
	{
		AgentDashboardModel result = new AgentDashboardModel();
		List<UserLoan> userLoans = await (from x in _context.UserLoans.AsNoTracking().Include((UserLoan x) => x.UserLoanPayments)
			where x.SubAgentId == (long?)agentId
			select x).ToListAsync();
		List<long> memberIdsFromUserLoans = userLoans.Select((UserLoan x) => x.UserId).ToList();
		AgentDashboardModel agentDashboardModel = result;
		agentDashboardModel.MemberCount = await _context.Users.AsNoTracking().CountAsync((User x) => x.AgentId == (long?)agentId || memberIdsFromUserLoans.Contains(x.Id));
		result.TotalLoans = userLoans.Sum((UserLoan x) => x.Amount);
		result.AllLoansCount = userLoans.Count();
		result.AllCheckoutedCount = userLoans.Count((UserLoan x) => x.IsCheckouted == true);
		result.TotalAgentShare = userLoans.Sum((UserLoan x) => x.SubAgentPrice);
		result.TotalPaid = userLoans.SelectMany((UserLoan x) => x.UserLoanPayments).Sum((UserLoanPayment x) => x.SumAmount);
		result.TotalUnpaid = result.TotalLoans - result.TotalPaid;
		agentDashboardModel = result;
		agentDashboardModel.AllLoanRequestsCount = await _context.UserLoanRequests.AsNoTracking().CountAsync((UserLoanRequest x) => x.AgentId == (long?)agentId);
		agentDashboardModel = result;
		agentDashboardModel.TotalReceivedsBySponsore = await (from x in _context.SubAgentTransactions.AsNoTracking().Include((SubAgentTransaction x) => x.UserLoan)
			where x.IsWithdraw && x.UserLoan.SubAgentId == (long?)agentId
			select x).SumAsync((SubAgentTransaction x) => x.Amount);
		agentDashboardModel = result;
		agentDashboardModel.TicketsCount = await (from x in _context.Tickets.AsNoTracking()
			where ((long?)x.CreatedById == _currentUserService.UserId || x.TargetUserId == _currentUserService.UserId) && (int)x.Status == 2
			select x.Id).Distinct().CountAsync();
		List<SubAgentTransaction> withdrawUserLoans = await (from x in _context.SubAgentTransactions.AsNoTracking().Include((SubAgentTransaction x) => x.UserLoan)
			where x.IsWithdraw && x.UserLoan.SubAgentId == (long?)agentId && x.UserLoan.SubAgentId.HasValue
			select x).ToListAsync();
		int count = userLoans.Count((UserLoan userLoan) => userLoan.SubAgentDueDate <= DateTime.Now && withdrawUserLoans.Where((SubAgentTransaction w) => w.UserLoanId == userLoan.Id).Sum((SubAgentTransaction w) => w.Amount) < userLoan.SubAgentPrice);
		decimal total = userLoans.Where((UserLoan userLoan) => userLoan.SubAgentDueDate <= DateTime.Now && withdrawUserLoans.Where((SubAgentTransaction w) => w.UserLoanId == userLoan.Id).Sum((SubAgentTransaction w) => w.Amount) < userLoan.SubAgentPrice).Sum((UserLoan x) => x.SubAgentPrice - withdrawUserLoans.Where((SubAgentTransaction w) => w.UserLoanId == x.Id).Sum((SubAgentTransaction w) => w.Amount));
		result.AgentOverdue = new { count, total };
		result.SubAgentOverdue = new
		{
			count = 0,
			total = 0
		};
		return result;
	}
}
