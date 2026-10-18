using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Base;

public interface ICustomerMarketerCommissionService
{
    Task<List<CustomerMarketerCommission>> GetByCondition(Expression<Func<CustomerMarketerCommission, bool>> condition, params string[] includs);

    Task<List<CustomerMarketerCommission>> GetAll(params string[] includs);

    Task<CustomerMarketerCommission> GetById(Guid entityId, params string[] includs);

    Task<List<CustomerMarketerCommission>> GetByUserId(long marketerId, params string[] includs);

    Task Insert(CustomerMarketerCommission entity);

    Task Update(CustomerMarketerCommission entity);

    Task Delete(CustomerMarketerCommission entity);
}
