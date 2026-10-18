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

public class LoanAuthorizationItemService : BaseService, ILoanAuthorizationItemService
{
    public LoanAuthorizationItemService(AppDbContext context, ICurrentUserService currentUserService)
        : base(context, currentUserService)
    {
    }

    public Task<List<LoanAuthorizationItem>> GetByCondition(Expression<Func<LoanAuthorizationItem, bool>> condition, params string[] includs)
    {
        IQueryable<LoanAuthorizationItem> source = Table<LoanAuthorizationItem>();
        foreach (string navigationPropertyPath in includs)
        {
            source = source.Include(navigationPropertyPath);
        }
        source = source.Where(condition);
        return source.ToListAsync();
    }

    public async Task Insert(LoanAuthorizationItem entity)
    {
        entity.CustomerId = _currentUserService.CustomerId.Value;
        Insert<LoanAuthorizationItem>(entity);
        await Save();
    }

    public Task Update(LoanAuthorizationItem entity)
    {
        entity.CustomerId = _currentUserService.CustomerId.Value;
        Update(entity, true);
        return Save();
    }

    public Task Delete(LoanAuthorizationItem entity)
    {
        Delete<LoanAuthorizationItem>(entity);
        return Save();
    }
}
