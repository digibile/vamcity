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

public class UserRefahBetaService : BaseService, IUserRefahBetaService
{
    public UserRefahBetaService(AppDbContext context, ICurrentUserService currentUserService)
        : base(context, currentUserService)
    {
    }

    public Task<List<UserRefahBeta>> GetByCondition(Expression<Func<UserRefahBeta, bool>> condition, params string[] includs)
    {
        IQueryable<UserRefahBeta> source = Table<UserRefahBeta>();
        foreach (string navigationPropertyPath in includs)
        {
            source = source.Include(navigationPropertyPath);
        }
        source = source.Where(condition);
        return source.ToListAsync();
    }

    public Task<List<UserRefahBeta>> GetAll()
    {
        return Table<UserRefahBeta>().ToListAsync();
    }

    public Task<UserRefahBeta> GetById(Guid entityId, params string[] includs)
    {
        return GetById<UserRefahBeta>(entityId, includs);
    }

    public Task<UserRefahBeta> GetByNationalCode(string nationalCode, params string[] includs)
    {
        return Table<UserRefahBeta>().FirstOrDefaultAsync((UserRefahBeta x) => x.User.NationalCode == nationalCode);
    }

    public Task Insert(UserRefahBeta entity)
    {
        entity.CustomerId = _currentUserService.CustomerId.Value;
        Insert<UserRefahBeta>(entity);
        return Save();
    }

    public Task Update(UserRefahBeta entity)
    {
        entity.CustomerId = _currentUserService.CustomerId.Value;
        Update(entity, true);
        return Save();
    }

    public Task Delete(UserRefahBeta entity)
    {
        Delete<UserRefahBeta>(entity);
        return Save();
    }
}
