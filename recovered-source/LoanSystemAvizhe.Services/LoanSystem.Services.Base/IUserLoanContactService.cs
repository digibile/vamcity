using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Base;

public interface IUserLoanContactService
{
    Task<List<UserLoanContact>> GetByCondition(Expression<Func<UserLoanContact, bool>> condition, params string[] includs);

    Task Insert(UserLoanContact entity);

    Task Update(UserLoanContact entity);

    Task Delete(UserLoanContact entity);
}
