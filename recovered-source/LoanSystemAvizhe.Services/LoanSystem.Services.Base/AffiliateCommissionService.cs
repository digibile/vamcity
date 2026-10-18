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

public class AffiliateCommissionService : BaseService, IAffiliateCommissionService
{
    public AffiliateCommissionService(AppDbContext context, ICurrentUserService currentUserService)
        : base(context, currentUserService)
    {
    }

    public Task<List<AffiliateCommission>> GetAll(params string[] includs)
    {
        IQueryable<AffiliateCommission> source = Table<AffiliateCommission>();
        foreach (string navigationPropertyPath in includs)
        {
            source = source.Include(navigationPropertyPath);
        }
        return source.ToListAsync();
    }

    public Task<AffiliateCommission> GetById(Guid entityId, params string[] includs)
    {
        IQueryable<AffiliateCommission> source = _context.AffiliateCommissions;
        foreach (string navigationPropertyPath in includs)
        {
            source = source.Include(navigationPropertyPath);
        }
        return source.SingleOrDefaultAsync((AffiliateCommission x) => x.Id == entityId);
    }

    public Task<List<AffiliateCommission>> GetByUserId(long userId, params string[] includs)
    {
        IQueryable<AffiliateCommission> source = Table<AffiliateCommission>();
        foreach (string navigationPropertyPath in includs)
        {
            source = source.Include(navigationPropertyPath);
        }
        return source.Where((AffiliateCommission x) => x.UserId == userId).ToListAsync();
    }

    public Task Insert(AffiliateCommission entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException("entity");
        }
        _context.AffiliateCommissions.Add(entity);
        return Save();
    }

    public Task Update(AffiliateCommission entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException("entity");
        }
        _context.AffiliateCommissions.Update(entity);
        return Save();
    }

    public Task Delete(AffiliateCommission entity)
    {
        _context.AffiliateCommissions.Remove(entity);
        return Save();
    }
}
