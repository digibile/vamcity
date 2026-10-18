using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Services.Base;

public interface IAppSmsService
{
    Task<List<AppSms>> GetAll(Guid? customerId);

    Task<AppSms> GetById(Guid entityId, params string[] includs);

    Task Insert(AppSms entity);

    Task UpdateStatus(Guid entityId, AppSmsStatus newStatus);

    Task Update(AppSms entity);

    Task Delete(AppSms entity);

    Task<int> GetCountByCondition(Expression<Func<AppSms, bool>> condition, bool ignoreQueryFilter = false, params string[] includs);

    Task<List<AppSms>> GetByCondition(Expression<Func<AppSms, bool>> condition, bool ignoreQueryFilter = false, params string[] includs);
}
