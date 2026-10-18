using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Base;

public interface IAuthorizationItemService
{
    Task<List<AuthorizationItem>> GetByCondition(Expression<Func<AuthorizationItem, bool>> condition, params string[] includs);

    Task Insert(AuthorizationItem entity);

    Task Update(AuthorizationItem entity);

    Task Delete(AuthorizationItem entity);
}
