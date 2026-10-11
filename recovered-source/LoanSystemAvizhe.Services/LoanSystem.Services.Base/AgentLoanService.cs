using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoanSystem.Data;
using LoanSystem.Data.Entities;
using LoanSystemAvizhe.Core;
using Microsoft.EntityFrameworkCore;
using Spm.Services.Infrastructure;

namespace LoanSystem.Services.Base;

public class AgentLoanService : BaseService, IAgentLoanService
{
	private readonly ISponsorService _sponsorService;

	public AgentLoanService(AppDbContext context, ICurrentUserService currentUserService, ISponsorService sponsorService)
		: base(context, currentUserService)
	{
		_sponsorService = sponsorService;
	}

	public Task<List<AgentLoan>> GetByCondition(Expression<Func<AgentLoan, bool>> condition, params string[] includs)
	{
		IQueryable<AgentLoan> source = Table<AgentLoan>();
		foreach (string navigationPropertyPath in includs)
		{
			source = source.Include(navigationPropertyPath);
		}
		source = source.Where(condition);
		return source.ToListAsync();
	}

	public async Task Modify(List<AgentLoan> agentLoans, long agentId)
	{
		List<AgentLoan> entities = await _context.AgentLoans.Where((AgentLoan x) => x.AgentId == agentId).ToListAsync();
		_context.AgentLoans.RemoveRange(entities);
		await _context.AgentLoans.AddRangeAsync(agentLoans);
		await Save();
	}

	public async Task<bool> CheckLoanAssignedToSponsor(Guid loanId)
	{
		List<long> sponsorIds = (await _sponsorService.GetAllSponsors()).Select((User x) => x.Id).ToList();
		return await Table<AgentLoan>().AnyAsync((AgentLoan x) => x.LoanId == loanId && sponsorIds.Contains(x.AgentId));
	}
}
