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

public class ContactService : BaseService, IContactService
{
    public ContactService(AppDbContext context, ICurrentUserService currentUserService)
        : base(context, currentUserService)
    {
    }

    public Task<Contact> GetById(Guid entityId, params string[] includs)
    {
        return GetById<Contact>(entityId, includs);
    }

    public Task<List<Contact>> GetByCondition(Expression<Func<Contact, bool>> condition, params string[] includs)
    {
        IQueryable<Contact> source = Table<Contact>();
        foreach (string navigationPropertyPath in includs)
        {
            source = source.Include(navigationPropertyPath);
        }
        source = source.Where(condition);
        return source.ToListAsync();
    }

    public async Task Insert(Contact entity)
    {
        entity.CustomerId = _currentUserService.CustomerId.Value;
        Insert<Contact>(entity);
        await Save();
    }

    public Task Update(Contact entity)
    {
        entity.CustomerId = _currentUserService.CustomerId.Value;
        Update(entity, true);
        return Save();
    }

    public Task Delete(Contact entity)
    {
        Delete<Contact>(entity);
        return Save();
    }
}
