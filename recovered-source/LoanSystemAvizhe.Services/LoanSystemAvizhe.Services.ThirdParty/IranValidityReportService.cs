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

namespace LoanSystemAvizhe.Services.ThirdParty;

public class IranValidityReportService : BaseService, IIranValidityReportService
{
    public IranValidityReportService(AppDbContext context, ICurrentUserService currentUserService)
        : base(context, currentUserService)
    {
    }

    public IQueryable<IranValidityReport> AsQueryable()
    {
        return Table<IranValidityReport>();
    }

    public Task<int> GetCountByCondition(Expression<Func<IranValidityReport, bool>> condition)
    {
        return Table<IranValidityReport>().Where(condition).CountAsync();
    }

    public Task<List<IranValidityReport>> GetByCondition(Expression<Func<IranValidityReport, bool>> condition, params string[] includs)
    {
        IQueryable<IranValidityReport> source = Table<IranValidityReport>();
        foreach (string navigationPropertyPath in includs)
        {
            source = source.Include(navigationPropertyPath);
        }
        source = source.Where(condition);
        return source.ToListAsync();
    }

    public Task<List<IranValidityReport>> GetAll(Guid? customerId)
    {
        IQueryable<IranValidityReport> source = Table<IranValidityReport>();
        if (customerId.HasValue)
        {
            source = from x in source.IgnoreQueryFilters()
                     where x.CustomerId == customerId
                     select x;
        }
        return source.ToListAsync();
    }

    public Task<IranValidityReport> GetById(Guid entityId, params string[] includs)
    {
        return GetById<IranValidityReport>(entityId, includs);
    }

    public async Task Insert(IranValidityReport entity)
    {
        entity.CustomerId = _currentUserService.CustomerId.Value;
        Insert<IranValidityReport>(entity);
        await Save();
    }

    public Task Update(IranValidityReport entity)
    {
        Update(entity, true);
        return Save();
    }

    public Task Delete(IranValidityReport entity)
    {
        Delete<IranValidityReport>(entity);
        return Save();
    }
}
