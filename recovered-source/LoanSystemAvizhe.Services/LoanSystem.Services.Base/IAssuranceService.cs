using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using Spm.Services.Infrastructure;

namespace LoanSystem.Services.Base;

public interface IAssuranceService
{
    Task<List<Assurance>> GetAllAssurances();

    Task<List<Assurance>> GetByUserLoanId(Guid userLoanId);

    Task<Assurance> GetById(Guid entityId, params string[] includs);

    Task RunInTransaction(Func<DbTransaction, Task> action);

    Task Insert(Assurance entity);

    Task Update(Assurance entity);

    Task Delete(Assurance entity);
}
