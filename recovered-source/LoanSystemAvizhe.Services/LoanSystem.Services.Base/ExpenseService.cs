using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LoanSystem.Data;
using LoanSystem.Data.Entities;
using LoanSystemAvizhe.Core;
using Microsoft.EntityFrameworkCore;
using Spm.Services.Infrastructure;

namespace LoanSystem.Services.Base;

public class ExpenseService : BaseService, IExpenseService
{
    public ExpenseService(AppDbContext context, ICurrentUserService currentUserService)
        : base(context, currentUserService)
    {
    }

    public Task<List<Expense>> GetAllExpenses()
    {
        return Table<Expense>().ToListAsync();
    }

    public Task<Expense> GetById(Guid entityId, params string[] includs)
    {
        return GetById<Expense>(entityId, includs);
    }

    public Task Insert(Expense entity)
    {
        entity.CustomerId = _currentUserService.CustomerId.Value;
        Insert<Expense>(entity);
        return Save();
    }

    public Task Update(Expense entity)
    {
        entity.CustomerId = _currentUserService.CustomerId.Value;
        Update(entity, true);
        return Save();
    }

    public Task Delete(Expense entity)
    {
        Delete<Expense>(entity);
        return Save();
    }
}
