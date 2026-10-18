using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Base;

public interface IUserLoanRequestAuthorizationItemAttachmentService
{
    Task<UserLoanRequestAuthorizationItemAttachment> GetById(Guid id, params string[] includs);

    Task<List<UserLoanRequestAuthorizationItemAttachment>> GetByCondition(Expression<Func<UserLoanRequestAuthorizationItemAttachment, bool>> condition, params string[] includs);

    Task Insert(UserLoanRequestAuthorizationItemAttachment entity);

    Task InsertBatch(List<UserLoanRequestAuthorizationItemAttachment> entities);

    Task Update(UserLoanRequestAuthorizationItemAttachment entity);

    Task Delete(UserLoanRequestAuthorizationItemAttachment entity);
}
