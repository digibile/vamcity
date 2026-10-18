using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Base;

public interface IUserLoanPaymentRequestService
{
    Task<List<UserLoanPaymentRequest>> GetAll(bool include, Expression<Func<UserLoanPaymentRequest, bool>> condition = null);

    Task<UserLoanPaymentRequest> GetById(Guid entityId, bool include);

    Task<List<UserLoanPaymentRequest>> GetByUserId(long userId);

    Task Insert(UserLoanPaymentRequest entity);

    Task Update(UserLoanPaymentRequest entity, bool sendSms = true);

    Task Delete(UserLoanPaymentRequest entity);
}
