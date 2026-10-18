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

public class AppEmailService : BaseService, IAppEmailService
{
    public AppEmailService(AppDbContext context, ICurrentUserService currentUserService)
        : base(context, currentUserService)
    {
    }

    public Task<int> GetCountByCondition(Expression<Func<AppEmail, bool>> condition, bool ignoreQueryFilter = false, params string[] includs)
    {
        IQueryable<AppEmail> source = Table<AppEmail>();
        if (ignoreQueryFilter)
        {
            source = source.IgnoreQueryFilters();
        }
        foreach (string navigationPropertyPath in includs)
        {
            source = source.Include(navigationPropertyPath);
        }
        source = source.Where(condition);
        return source.CountAsync();
    }

    public Task<List<AppEmail>> GetByCondition(Expression<Func<AppEmail, bool>> condition, bool ignoreQueryFilter = false, params string[] includs)
    {
        IQueryable<AppEmail> source = Table<AppEmail>();
        if (ignoreQueryFilter)
        {
            source = source.IgnoreQueryFilters();
        }
        foreach (string navigationPropertyPath in includs)
        {
            source = source.Include(navigationPropertyPath);
        }
        source = source.Where(condition);
        return source.ToListAsync();
    }

    public Task<List<AppEmail>> GetAll(Guid? customerId)
    {
        IQueryable<AppEmail> source = Table<AppEmail>();
        if (customerId.HasValue)
        {
            source = from x in source.IgnoreQueryFilters()
                     where x.CustomerId == customerId
                     select x;
        }
        return source.ToListAsync();
    }

    public Task<AppEmail> GetById(Guid entityId, params string[] includs)
    {
        return GetById<AppEmail>(entityId, includs);
    }

    public async Task Insert(AppEmail entity)
    {
        entity.CustomerId = _currentUserService.CustomerId.Value;
        Insert<AppEmail>(entity);
        await Save();
    }

    public Task Update(AppEmail entity)
    {
        entity.CustomerId = _currentUserService.CustomerId.Value;
        Update(entity, true);
        return Save();
    }

    public Task Delete(AppEmail entity)
    {
        Delete<AppEmail>(entity);
        return Save();
    }
}
