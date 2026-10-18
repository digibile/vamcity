using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoanSystem.Data;
using LoanSystem.Data.Entities;
using LoanSystemAvizhe.Core;
using Microsoft.EntityFrameworkCore;
using Spm.Services.Infrastructure;

namespace LoanSystem.Services.Base;

public class OffDateService : BaseService, IOffDateService
{
    public OffDateService(AppDbContext context, ICurrentUserService currentUserService)
        : base(context, currentUserService)
    {
    }

    public Task<List<OffDate>> GetByCondition(Expression<Func<OffDate, bool>> condition)
    {
        return Table<OffDate>().Where(condition).ToListAsync();
    }

    public Task<List<OffDate>> GetAll(Guid? customerId)
    {
        IQueryable<OffDate> source = Table<OffDate>();
        if (customerId.HasValue)
        {
            source = from x in source.IgnoreQueryFilters()
                     where x.CustomerId == customerId
                     select x;
        }
        return source.ToListAsync();
    }

    public Task<OffDate> GetById(Guid entityId, params string[] includs)
    {
        return GetById<OffDate>(entityId, includs);
    }

    public async Task Modify(List<OffDate> entities)
    {
        List<OffDate> source = await GetAll(null);
        List<OffDate> list = entities.Where((OffDate x) => x.Id == Guid.Empty).ToList();
        List<OffDate> list2 = source.Where((OffDate x) => !entities.Select((OffDate e) => e.Id).ToList().Contains(x.Id)).ToList();
        list.ForEach((OffDate x) =>
        {
            x.CustomerId = _currentUserService.CustomerId.Value;
        });
        if (list.Any())
        {
            _context.Set<OffDate>().AddRange(list);
        }
        if (list2.Any())
        {
            _context.Set<OffDate>().RemoveRange(list2);
        }
        await Save();
    }

    public async Task Insert(OffDate entity)
    {
        entity.CustomerId = _currentUserService.CustomerId.Value;
        Insert<OffDate>(entity);
        await Save();
    }

    public Task Update(OffDate entity)
    {
        entity.CustomerId = _currentUserService.CustomerId.Value;
        Update(entity, true);
        return Save();
    }

    public Task Delete(OffDate entity)
    {
        Delete<OffDate>(entity);
        return Save();
    }
}
