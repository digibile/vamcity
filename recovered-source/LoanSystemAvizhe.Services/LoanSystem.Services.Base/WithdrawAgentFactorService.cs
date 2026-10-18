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

public class WithdrawAgentFactorService : BaseService, IWithdrawAgentFactorService
{
    public WithdrawAgentFactorService(AppDbContext context, ICurrentUserService currentUserService)
        : base(context, currentUserService)
    {
    }

    public Task<List<WithdrawAgentFactor>> GetByCondition(Expression<Func<WithdrawAgentFactor, bool>> condition, bool asTracking = false, params string[] includs)
    {
        IQueryable<WithdrawAgentFactor> source = ((!asTracking) ? Table<WithdrawAgentFactor>() : TableTracking<WithdrawAgentFactor>());
        foreach (string navigationPropertyPath in includs)
        {
            source = source.Include(navigationPropertyPath);
        }
        source = source.Where(condition);
        return source.ToListAsync();
    }

    public Task Insert(WithdrawAgentFactor entity)
    {
        Insert<WithdrawAgentFactor>(entity);
        return Save();
    }

    public Task InsertBulk(List<WithdrawAgentFactor> entities)
    {
        InsertBulk<WithdrawAgentFactor>(entities);
        return Save();
    }

    public Task Delete(List<WithdrawAgentFactor> entities)
    {
        Delete((IEnumerable<WithdrawAgentFactor>)entities);
        return Save();
    }
}
