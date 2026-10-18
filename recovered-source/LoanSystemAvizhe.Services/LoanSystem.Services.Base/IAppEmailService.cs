using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Base;

public interface IAppEmailService
{
    Task<List<AppEmail>> GetAll(Guid? customerId);

    Task<AppEmail> GetById(Guid entityId, params string[] includs);

    Task Insert(AppEmail entity);

    Task Update(AppEmail entity);

    Task Delete(AppEmail entity);

    Task<int> GetCountByCondition(Expression<Func<AppEmail, bool>> condition, bool ignoreQueryFilter = false, params string[] includs);

    Task<List<AppEmail>> GetByCondition(Expression<Func<AppEmail, bool>> condition, bool ignoreQueryFilter = false, params string[] includs);
}
