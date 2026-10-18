using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Base;

public interface ILoanContactService
{
    Task<List<LoanContact>> GetByCondition(Expression<Func<LoanContact, bool>> condition, params string[] includs);

    Task Insert(LoanContact entity);

    Task Update(LoanContact entity);

    Task Delete(LoanContact entity);
}
