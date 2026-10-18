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

public class LoanContactService : BaseService, ILoanContactService
{
    public LoanContactService(AppDbContext context, ICurrentUserService currentUserService)
        : base(context, currentUserService)
    {
    }

    public Task<List<LoanContact>> GetByCondition(Expression<Func<LoanContact, bool>> condition, params string[] includs)
    {
        IQueryable<LoanContact> source = Table<LoanContact>();
        foreach (string navigationPropertyPath in includs)
        {
            source = source.Include(navigationPropertyPath);
        }
        source = source.Where(condition);
        return source.ToListAsync();
    }

    public async Task Insert(LoanContact entity)
    {
        entity.CustomerId = _currentUserService.CustomerId.Value;
        Insert<LoanContact>(entity);
        await Save();
    }

    public Task Update(LoanContact entity)
    {
        entity.CustomerId = _currentUserService.CustomerId.Value;
        Update(entity, true);
        return Save();
    }

    public Task Delete(LoanContact entity)
    {
        Delete<LoanContact>(entity);
        return Save();
    }
}
