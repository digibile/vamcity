using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Base;

public interface ILoanPaymentOtherService
{
    IQueryable<UserLoanPaymentOther> GetAllUserLoanPaymentOthersAsQueryable();

    Task<List<UserLoanPaymentOther>> GetByCondition(Expression<Func<UserLoanPaymentOther, bool>> condition, params string[] includs);

    Task<List<UserLoanPaymentOther>> GetAllUserLoanPaymentOthers();

    Task<UserLoanPaymentOther> GetById(Guid entityId, params string[] includs);

    Task Insert(UserLoanPaymentOther entity);

    Task Update(UserLoanPaymentOther entity);

    Task Delete(UserLoanPaymentOther entity);
}
