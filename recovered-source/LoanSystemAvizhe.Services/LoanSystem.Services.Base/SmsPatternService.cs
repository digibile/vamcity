using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Data;
using LoanSystem.Data.Entities;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Spm.Services.Infrastructure;

namespace LoanSystem.Services.Base;

public class SmsPatternService : BaseService, ISmsPatternService
{
    public SmsPatternService(AppDbContext context, ICurrentUserService currentUserService)
        : base(context, currentUserService)
    {
    }

    public Task<List<SmsPattern>> GetAll(Guid? customerId, SmsPatternStatus? status, params string[] includs)
    {
        IQueryable<SmsPattern> source = Table<SmsPattern>();
        foreach (string navigationPropertyPath in includs)
        {
            source = source.Include(navigationPropertyPath);
        }
        if (customerId.HasValue)
        {
            source = source.Where((SmsPattern x) => x.CustomerId == customerId);
        }
        if (status.HasValue)
        {
            source = source.Where((SmsPattern x) => (int?)x.Status == (int?)status);
        }
        return source.ToListAsync();
    }

    public Task<int> GetCount(Guid? customerId, SmsPatternStatus? status)
    {
        IQueryable<SmsPattern> source = Table<SmsPattern>();
        if (customerId.HasValue)
        {
            source = source.Where((SmsPattern x) => x.CustomerId == customerId);
        }
        if (status.HasValue)
        {
            source = source.Where((SmsPattern x) => (int?)x.Status == (int?)status);
        }
        return source.CountAsync();
    }

    public Task<SmsPattern> GetById(Guid entityId, params string[] includs)
    {
        return GetById<SmsPattern>(entityId, includs);
    }

    public Task Insert(SmsPattern entity, Guid? customerId = null)
    {
        entity.CustomerId = (customerId.HasValue ? customerId.Value : _currentUserService.CustomerId.Value);
        Insert<SmsPattern>(entity);
        return Save();
    }

    public Task Update(SmsPattern entity)
    {
        Update(entity, true);
        return Save();
    }

    public Task Delete(SmsPattern entity)
    {
        Delete<SmsPattern>(entity);
        return Save();
    }
}
