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

public class TagService : BaseService, ITagService
{
    public TagService(AppDbContext context, ICurrentUserService currentUserService)
        : base(context, currentUserService)
    {
    }

    public Task<List<Tag>> GetByCondition(Expression<Func<Tag, bool>> condition)
    {
        return Table<Tag>().Where(condition).ToListAsync();
    }

    public Task<List<Tag>> GetAll(Guid? customerId, TagCategory? category)
    {
        IQueryable<Tag> source = Table<Tag>();
        if (customerId.HasValue)
        {
            source = from x in source.IgnoreQueryFilters()
                     where x.CustomerId == customerId
                     select x;
        }
        if (category.HasValue)
        {
            source = source.Where((Tag x) => (int?)x.TagCategory == (int?)category);
        }
        return source.ToListAsync();
    }

    public Task<Tag> GetById(Guid entityId, params string[] includs)
    {
        return GetById<Tag>(entityId, includs);
    }

    public async Task Insert(Tag entity)
    {
        entity.CustomerId = _currentUserService.CustomerId.Value;
        Insert<Tag>(entity);
        await Save();
    }

    public Task Update(Tag entity)
    {
        entity.CustomerId = _currentUserService.CustomerId.Value;
        Update(entity, true);
        return Save();
    }

    public Task Delete(Tag entity)
    {
        Delete<Tag>(entity);
        return Save();
    }
}
