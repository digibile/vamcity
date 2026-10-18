using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Base;

public interface IExpenseService
{
    Task<List<Expense>> GetAllExpenses();

    Task<Expense> GetById(Guid entityId, params string[] includs);

    Task Insert(Expense entity);

    Task Update(Expense entity);

    Task Delete(Expense entity);
}
