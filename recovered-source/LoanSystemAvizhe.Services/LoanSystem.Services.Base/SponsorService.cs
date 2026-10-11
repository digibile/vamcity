using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Data;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Users;
using LoanSystemAvizhe.Core;
using Microsoft.EntityFrameworkCore;
using Spm.Services.Infrastructure;

namespace LoanSystem.Services.Base;

public class SponsorService : BaseService, ISponsorService
{
	private readonly UserManager _userManager;

	public SponsorService(AppDbContext context, ICurrentUserService currentUserService, UserManager userManager)
		: base(context, currentUserService)
	{
		_userManager = userManager;
	}

	public async Task<List<User>> GetAllSponsors()
	{
		List<User> users = (await _userManager.GetUsersInRoleAsync("Sponsor")).ToList();
		List<AgentLoan> sponsorLoans = await _context.AgentLoans.AsNoTracking().ToListAsync();
		users.ForEach((User user) =>
		{
			user.AgentLoans = sponsorLoans.Where((AgentLoan x) => x.AgentId == user.Id).ToList();
		});
		return users;
	}

	public Task<User> GetById(long entityId, params string[] includs)
	{
		IQueryable<User> source = _context.Users;
		foreach (string navigationPropertyPath in includs)
		{
			source = source.Include(navigationPropertyPath);
		}
		return source.SingleOrDefaultAsync((User x) => x.Id == entityId);
	}

	public Task<User> GetByMobile(string mobile)
	{
		return _context.Users.FirstOrDefaultAsync((User x) => x.Mobile == mobile);
	}

	public Task Insert(User entity)
	{
		if (entity == null)
		{
			throw new ArgumentNullException("entity");
		}
		_context.Users.Add(entity);
		return Save();
	}

	public Task Update(User entity)
	{
		if (entity == null)
		{
			throw new ArgumentNullException("entity");
		}
		_context.Users.Update(entity);
		return Save();
	}

	public Task Delete(User entity)
	{
		if (entity == null)
		{
			throw new ArgumentNullException("entity");
		}
		_context.Users.Remove(entity);
		return Save();
	}
}
