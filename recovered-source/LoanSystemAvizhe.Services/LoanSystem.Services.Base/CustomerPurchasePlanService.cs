using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Data;
using LoanSystem.Data.Entities;
using LoanSystemAvizhe.Core;
using Microsoft.EntityFrameworkCore;
using Spm.Services.Infrastructure;

namespace LoanSystem.Services.Base;

public class CustomerPurchasePlanService : BaseService, ICustomerPurchasePlanService
{
    public CustomerPurchasePlanService(AppDbContext context, ICurrentUserService currentUserService)
        : base(context, currentUserService)
    {
    }

    public Task<List<CustomerPurchasePlan>> GetAll(bool currentCustomer, params string[] includs)
    {
        IQueryable<CustomerPurchasePlan> source = Table<CustomerPurchasePlan>();
        foreach (string navigationPropertyPath in includs)
        {
            source = source.Include(navigationPropertyPath);
        }
        if (currentCustomer)
        {
            source = source.Where((CustomerPurchasePlan x) => x.CustomerId == _currentUserService.CustomerId);
        }
        return source.ToListAsync();
    }

    public Task<CustomerPurchasePlan> GetById(Guid entityId, params string[] includs)
    {
        return GetById<CustomerPurchasePlan>(entityId, includs);
    }

    public Task<DateTime> GetMaxEndDateForCustomer(Guid customerId)
    {
        return (from x in Table<CustomerPurchasePlan>()
                where x.CustomerId == customerId
                select x).MaxAsync((CustomerPurchasePlan x) => x.EndDate);
    }

    public Task<CustomerPurchasePlan> GetFirstActiveForSetRemain(Guid customerId, int remain = 1)
    {
        return Table<CustomerPurchasePlan>().FirstOrDefaultAsync((CustomerPurchasePlan x) => x.CustomerId == customerId && x.RemainLoanCount >= remain);
    }

    public Task Insert(CustomerPurchasePlan entity)
    {
        Insert<CustomerPurchasePlan>(entity);
        return Save();
    }

    public Task Update(CustomerPurchasePlan entity)
    {
        Update(entity, true);
        return Save();
    }

    public Task Delete(CustomerPurchasePlan entity)
    {
        Delete<CustomerPurchasePlan>(entity);
        return Save();
    }
}
