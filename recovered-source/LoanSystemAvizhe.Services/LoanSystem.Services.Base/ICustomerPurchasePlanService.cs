using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Base;

public interface ICustomerPurchasePlanService
{
    Task<List<CustomerPurchasePlan>> GetAll(bool currentCustomer, params string[] includs);

    Task<CustomerPurchasePlan> GetById(Guid entityId, params string[] includs);

    Task<DateTime> GetMaxEndDateForCustomer(Guid customerId);

    Task<CustomerPurchasePlan> GetFirstActiveForSetRemain(Guid customerId, int remain = 1);

    Task Insert(CustomerPurchasePlan entity);

    Task Update(CustomerPurchasePlan entity);

    Task Delete(CustomerPurchasePlan entity);
}
