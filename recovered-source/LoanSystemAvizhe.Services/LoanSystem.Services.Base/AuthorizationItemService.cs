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

public class AuthorizationItemService : BaseService, IAuthorizationItemService
{
    public AuthorizationItemService(AppDbContext context, ICurrentUserService currentUserService)
        : base(context, currentUserService)
    {
    }

    public Task<List<AuthorizationItem>> GetByCondition(Expression<Func<AuthorizationItem, bool>> condition, params string[] includs)
    {
        IQueryable<AuthorizationItem> source = Table<AuthorizationItem>();
        foreach (string navigationPropertyPath in includs)
        {
            source = source.Include(navigationPropertyPath);
        }
        source = source.Where(condition);
        return source.ToListAsync();
    }

    public async Task Insert(AuthorizationItem entity)
    {
        entity.CustomerId = _currentUserService.CustomerId.Value;
        Insert<AuthorizationItem>(entity);
        await Save();
    }

    public Task Update(AuthorizationItem entity)
    {
        entity.CustomerId = _currentUserService.CustomerId.Value;
        Update(entity, true);
        return Save();
    }

    public Task Delete(AuthorizationItem entity)
    {
        Delete<AuthorizationItem>(entity);
        return Save();
    }
}
