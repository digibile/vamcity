using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoanSystem.Data;
using LoanSystem.Data.Entities;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Spm.Services.Infrastructure;

namespace LoanSystem.Services.Base;

public class SubAgentTransactionService : BaseService, ISubAgentTransactionService
{
    public SubAgentTransactionService(AppDbContext context, ICurrentUserService currentUserService)
        : base(context, currentUserService)
    {
    }

    public Task<List<SubAgentTransaction>> GetByCondition(Expression<Func<SubAgentTransaction, bool>> condition, params string[] includs)
    {
        IQueryable<SubAgentTransaction> source = Table<SubAgentTransaction>();
        foreach (string navigationPropertyPath in includs)
        {
            source = source.Include(navigationPropertyPath);
        }
        source = source.Where(condition);
        return source.ToListAsync();
    }

    public Task<List<SubAgentTransaction>> GetAllWithdraws(PaymentMethods? PaymentMethod)
    {
        IQueryable<SubAgentTransaction> source = Table<SubAgentTransaction>();
        if (PaymentMethod.HasValue)
        {
            source = source.Where((SubAgentTransaction x) => (int?)x.PaymentMethod == (int?)PaymentMethod);
        }
        return source.ToListAsync();
    }

    public Task<SubAgentTransaction> GetById(Guid entityId, params string[] includs)
    {
        return GetById<SubAgentTransaction>(entityId, includs);
    }

    public Task Insert(SubAgentTransaction entity)
    {
        entity.CustomerId = _currentUserService.CustomerId.Value;
        Insert<SubAgentTransaction>(entity);
        return Save();
    }

    public Task Update(SubAgentTransaction entity)
    {
        entity.CustomerId = _currentUserService.CustomerId.Value;
        Update(entity, true);
        return Save();
    }

    public Task Delete(SubAgentTransaction entity)
    {
        Delete<SubAgentTransaction>(entity);
        return Save();
    }
}
