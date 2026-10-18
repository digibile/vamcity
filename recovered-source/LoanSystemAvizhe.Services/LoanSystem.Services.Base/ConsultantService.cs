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

public class ConsultantService : BaseService, IConsultantService
{
    public ConsultantService(AppDbContext context, ICurrentUserService currentUserService)
        : base(context, currentUserService)
    {
    }

    public Task<List<Consultant>> GetAll(bool? isSeen)
    {
        IQueryable<Consultant> source = Table<Consultant>();
        if (isSeen.HasValue)
        {
            source = source.Where((Consultant x) => (bool?)x.IsSeen == isSeen);
        }
        return source.ToListAsync();
    }

    public Task<int> GetCount(bool? isSeen)
    {
        IQueryable<Consultant> source = Table<Consultant>();
        if (isSeen.HasValue)
        {
            source = source.Where((Consultant x) => (bool?)x.IsSeen == isSeen);
        }
        return source.CountAsync();
    }

    public Task<Consultant> GetById(Guid entityId)
    {
        return GetById<Consultant>(entityId);
    }

    public Task Insert(Consultant entity)
    {
        Insert<Consultant>(entity);
        return Save();
    }

    public Task Update(Consultant entity)
    {
        Update(entity, true);
        return Save();
    }

    public Task Delete(Consultant entity)
    {
        Delete<Consultant>(entity);
        return Save();
    }
}
