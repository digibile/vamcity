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

public class LoanPaymentOtherService : BaseService, ILoanPaymentOtherService
{
    public LoanPaymentOtherService(AppDbContext context, ICurrentUserService currentUserService)
        : base(context, currentUserService)
    {
    }

    public IQueryable<UserLoanPaymentOther> GetAllUserLoanPaymentOthersAsQueryable()
    {
        return Table<UserLoanPaymentOther>();
    }

    public Task<List<UserLoanPaymentOther>> GetByCondition(Expression<Func<UserLoanPaymentOther, bool>> condition, params string[] includs)
    {
        IQueryable<UserLoanPaymentOther> source = Table<UserLoanPaymentOther>();
        foreach (string navigationPropertyPath in includs)
        {
            source = source.Include(navigationPropertyPath);
        }
        source = source.Where(condition);
        return source.ToListAsync();
    }

    public Task<List<UserLoanPaymentOther>> GetAllUserLoanPaymentOthers()
    {
        return Table<UserLoanPaymentOther>().ToListAsync();
    }

    public Task<UserLoanPaymentOther> GetById(Guid entityId, params string[] includs)
    {
        return GetById<UserLoanPaymentOther>(entityId, includs);
    }

    public Task Insert(UserLoanPaymentOther entity)
    {
        Insert<UserLoanPaymentOther>(entity);
        return Save();
    }

    public Task Update(UserLoanPaymentOther entity)
    {
        Update(entity, true);
        return Save();
    }

    public Task Delete(UserLoanPaymentOther entity)
    {
        Delete<UserLoanPaymentOther>(entity);
        return Save();
    }
}
