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

public class UserLoanSponsoreDueDateService : BaseService, IUserLoanSponsoreDueDateService
{
    public UserLoanSponsoreDueDateService(AppDbContext context, ICurrentUserService currentUserService)
        : base(context, currentUserService)
    {
    }

    public Task<List<UserLoanSponsoreDueDate>> GetByCondition(Expression<Func<UserLoanSponsoreDueDate, bool>> condition, params string[] includs)
    {
        IQueryable<UserLoanSponsoreDueDate> source = Table<UserLoanSponsoreDueDate>();
        foreach (string navigationPropertyPath in includs)
        {
            source = source.Include(navigationPropertyPath);
        }
        source = source.Where(condition);
        return source.ToListAsync();
    }

    public async Task Insert(UserLoanSponsoreDueDate entity)
    {
        entity.CustomerId = _currentUserService.CustomerId.Value;
        Insert<UserLoanSponsoreDueDate>(entity);
        await Save();
    }

    public Task Update(UserLoanSponsoreDueDate entity)
    {
        entity.CustomerId = _currentUserService.CustomerId.Value;
        Update(entity, true);
        return Save();
    }

    public Task Delete(UserLoanSponsoreDueDate entity)
    {
        Delete<UserLoanSponsoreDueDate>(entity);
        return Save();
    }
}
