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

public class CustomerService : BaseService, ICustomerService
{
    public CustomerService(AppDbContext context, ICurrentUserService currentUserService)
        : base(context, currentUserService)
    {
    }

    public Task<List<Customer>> GetAllCustomers()
    {
        IQueryable<Customer> query = Table<Customer>();
        // Tenant query filters are correct for ordinary users, but the SuperAdmin
        // customer registry must show every tenant or the customer/plan screens
        // appear empty even when the restored database contains customer rows.
        if (_currentUserService.IsInRole("SuperAdmin"))
        {
            query = query.IgnoreQueryFilters();
        }
        return query.OrderByDescending(x => x.RegDate).ToListAsync();
    }

    public Task<Customer> GetById(Guid entityId, params string[] includs)
    {
        IQueryable<Customer> source = Table<Customer>();
        foreach (string navigationPropertyPath in includs)
        {
            source = source.Include(navigationPropertyPath);
        }
        return source.SingleOrDefaultAsync((Customer x) => x.Id == entityId);
    }

    public Task<Customer> GetByCode(int code, params string[] includs)
    {
        IQueryable<Customer> source = Table<Customer>();
        foreach (string navigationPropertyPath in includs)
        {
            source = source.Include(navigationPropertyPath);
        }
        return source.SingleOrDefaultAsync((Customer x) => x.Code == code);
    }

    public Task<Customer> GetByDomain(string domain, params string[] includs)
    {
        IQueryable<Customer> source = Table<Customer>();
        foreach (string navigationPropertyPath in includs)
        {
            source = source.Include(navigationPropertyPath);
        }
        return source.SingleOrDefaultAsync((Customer x) => x.ParkDomain.Contains(domain));
    }

    public Task<Customer> GetByMobile(string mobile)
    {
        return _context.Customers.FirstOrDefaultAsync((Customer x) => x.Mobile == mobile);
    }

    public async Task SetFirstLoginAsDone(Guid id)
    {
        Customer customer = await _context.Customers.FirstOrDefaultAsync((Customer x) => x.Id == id);
        if (customer != null)
        {
            customer.FirstLoginDone = true;
        }
        await _context.SaveChangesAsync();
    }

    public Task Insert(Customer entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException("entity");
        }
        _context.Customers.Add(entity);
        return Save();
    }

    public Task Update(Customer entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException("entity");
        }
        _context.Customers.Update(entity);
        return Save();
    }

    public Task Delete(Customer entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException("entity");
        }
        _context.Customers.Remove(entity);
        return Save();
    }
}
