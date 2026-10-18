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

public class AgentService : BaseService, IAgentService
{
    private readonly UserManager _userManager;

    public AgentService(AppDbContext context, ICurrentUserService currentUserService, UserManager userManager)
        : base(context, currentUserService)
    {
        _userManager = userManager;
    }

    public async Task<List<User>> GetAllAgents()
    {
        List<User> users = (await _userManager.GetUsersInRoleAsync("Agent")).ToList();
        List<AgentLoan> agentLoans = await _context.AgentLoans.Include((AgentLoan x) => x.Contact).AsNoTracking().ToListAsync();
        users.ForEach((User user) =>
        {
            user.AgentLoans = agentLoans.Where((AgentLoan x) => x.AgentId == user.Id).ToList();
        });
        return users;
    }

    public async Task<List<User>> GetSubAgents(long? parentId, bool includeParent = false)
    {
        List<User> allAgents = (await _userManager.GetUsersInRoleAsync("Agent")).ToList();
        new List<User>();
        List<User> subAgents = ((!parentId.HasValue) ? allAgents.Where((User x) => x.AgentId.HasValue).ToList() : allAgents.Where((User x) => x.AgentId == parentId).ToList());
        if (includeParent)
        {
            subAgents.ForEach((User user) =>
            {
                user.Agent = allAgents.FirstOrDefault((User x) => x.Id == user.AgentId);
            });
        }
        List<AgentLoan> agentLoans = await _context.AgentLoans.AsNoTracking().ToListAsync();
        subAgents.ForEach((User user) =>
        {
            user.AgentLoans = agentLoans.Where((AgentLoan x) => x.AgentId == user.Id).ToList();
        });
        return subAgents;
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

    public Task<User> GetByNationalCode(string nationalCode)
    {
        return _context.Users.FirstOrDefaultAsync((User x) => x.NationalCode == nationalCode);
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
