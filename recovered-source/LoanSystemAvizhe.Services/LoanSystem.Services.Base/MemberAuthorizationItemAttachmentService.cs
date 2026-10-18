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

public class MemberAuthorizationItemAttachmentService : BaseService, IMemberAuthorizationItemAttachmentService
{
    public MemberAuthorizationItemAttachmentService(AppDbContext context, ICurrentUserService currentUserService)
        : base(context, currentUserService)
    {
    }

    public Task<MemberAuthorizationItemAttachment> GetById(Guid id, params string[] includs)
    {
        return GetById<MemberAuthorizationItemAttachment>(id, includs);
    }

    public Task<List<MemberAuthorizationItemAttachment>> GetByCondition(Expression<Func<MemberAuthorizationItemAttachment, bool>> condition, params string[] includs)
    {
        IQueryable<MemberAuthorizationItemAttachment> source = Table<MemberAuthorizationItemAttachment>();
        foreach (string navigationPropertyPath in includs)
        {
            source = source.Include(navigationPropertyPath);
        }
        source = source.Where(condition);
        return source.ToListAsync();
    }

    public async Task Insert(MemberAuthorizationItemAttachment entity)
    {
        entity.CustomerId = _currentUserService.CustomerId.Value;
        Insert<MemberAuthorizationItemAttachment>(entity);
        await Save();
    }

    public async Task InsertBatch(List<MemberAuthorizationItemAttachment> entities)
    {
        entities.ForEach((MemberAuthorizationItemAttachment x) =>
        {
            x.CustomerId = _currentUserService.CustomerId.Value;
        });
        _context.MemberAuthorizationItemAttachments.AddRange(entities);
        await Save();
    }

    public Task Update(MemberAuthorizationItemAttachment entity)
    {
        entity.CustomerId = _currentUserService.CustomerId.Value;
        Update(entity, true);
        return Save();
    }

    public Task Delete(MemberAuthorizationItemAttachment entity)
    {
        Delete<MemberAuthorizationItemAttachment>(entity);
        return Save();
    }
}
