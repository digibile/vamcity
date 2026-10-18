using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoanSystem.Data;
using LoanSystem.Data.Entities;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Spm.Services.Infrastructure;

namespace LoanSystem.Services.Base;

public class AppInquiryService : BaseService, IAppInquiryService
{
    public AppInquiryService(AppDbContext context, ICurrentUserService currentUserService)
        : base(context, currentUserService)
    {
    }

    public Task<int> GetCountByCondition(Expression<Func<AppInquiry, bool>> condition, bool ignoreQueryFilter = false, params string[] includs)
    {
        IQueryable<AppInquiry> source = Table<AppInquiry>();
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

    public Task<List<AppInquiry>> GetByCondition(Expression<Func<AppInquiry, bool>> condition, bool ignoreQueryFilter = false, params string[] includs)
    {
        IQueryable<AppInquiry> source = Table<AppInquiry>();
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

    public async Task UpdateMember(List<Guid> inquiryIds, long memberId)
    {
        List<Guid> list = inquiryIds;
        if (list == null || !list.Any())
        {
            return;
        }
        foreach (AppInquiry item in await (from x in Table<AppInquiry>()
                                           where inquiryIds.Contains(x.Id)
                                           select x).ToListAsync())
        {
            item.MemberId = memberId;
            Update(item, true);
        }
        await Save();
    }

    public Task<List<AppInquiry>> GetAll(Guid? customerId)
    {
        IQueryable<AppInquiry> source = Table<AppInquiry>();
        if (customerId.HasValue)
        {
            source = from x in source.IgnoreQueryFilters()
                     where x.CustomerId == customerId
                     select x;
        }
        return source.ToListAsync();
    }

    public Task<AppInquiry> GetById(Guid entityId, params string[] includs)
    {
        return GetById<AppInquiry>(entityId, includs);
    }

    public async Task Insert(AppInquiry entity)
    {
        entity.CustomerId = _currentUserService.CustomerId.Value;
        Insert<AppInquiry>(entity);
        await Save();
    }

    public Task Update(AppInquiry entity)
    {
        entity.CustomerId = _currentUserService.CustomerId.Value;
        Update(entity, true);
        return Save();
    }

    public Task Delete(AppInquiry entity)
    {
        Delete<AppInquiry>(entity);
        return Save();
    }

    public async Task<AppInquiry> GetInquiryByKey(string key, AppInquiryType type, int maxPastMinutes = 60)
    {
        IQueryable<AppInquiry> source = Table<AppInquiry>();
        source = source.Where((AppInquiry x) => x.Key == key && (int)x.AppInquiryType == (int)type && x.IsSuccess);
        if (maxPastMinutes > 0)
        {
            source = source.Where((AppInquiry x) => x.Date >= DateTime.Now.AddMinutes(-60.0));
        }
        return await source.FirstOrDefaultAsync();
    }
}
