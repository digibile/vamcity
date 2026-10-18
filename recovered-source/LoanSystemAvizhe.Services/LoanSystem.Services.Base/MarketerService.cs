using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Data;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Users;
using LoanSystemAvizhe.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Spm.Services.Infrastructure;

namespace LoanSystem.Services.Base;

public class MarketerService : BaseService, IMarketerService
{
    private readonly UserManager _userManager;

    public MarketerService(AppDbContext context, ICurrentUserService currentUserService, UserManager userManager)
        : base(context, currentUserService)
    {
        _userManager = userManager;
    }

    public async Task<List<User>> GetAllMarketers()
    {
        List<User> marketers = new List<User>();
        Role marketerRole = await _context.Roles.AsNoTracking().FirstOrDefaultAsync((Role x) => x.Name == "Marketer");
        if (marketerRole != null)
        {
            IQueryable<long> userIds = from x in _context.UserRoles.AsNoTracking()
                                       where x.RoleId == marketerRole.Id
                                       select x.UserId;
            marketers = await (from x in _context.Users.AsNoTracking().IgnoreQueryFilters()
                               where userIds.Contains(x.Id)
                               select x).ToListAsync();
        }
        return marketers;
    }

    public Task<User> GetById(long entityId, params string[] includs)
    {
        IQueryable<User> source = _context.Users;
        foreach (string navigationPropertyPath in includs)
        {
            source = source.Include(navigationPropertyPath);
        }
        return source.IgnoreQueryFilters().SingleOrDefaultAsync((User x) => x.Id == entityId);
    }

    public Task<User> GetByAffliateCode(string affCode, params string[] includs)
    {
        IQueryable<User> source = _context.Users;
        foreach (string navigationPropertyPath in includs)
        {
            source = source.Include(navigationPropertyPath);
        }
        return source.IgnoreQueryFilters().SingleOrDefaultAsync((User x) => x.AffiliateCode == affCode);
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
