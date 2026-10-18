using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LoanSystem.Data;
using LoanSystem.Data.Entities;
using LoanSystemAvizhe.Core;
using Microsoft.EntityFrameworkCore;
using Spm.Services.Infrastructure;

namespace LoanSystem.Services.Base;

public class OrganizationService : BaseService, IOrganizationService
{
    public OrganizationService(AppDbContext context, ICurrentUserService currentUserService)
        : base(context, currentUserService)
    {
    }

    public Task<List<Organization>> GetAllOrganizations()
    {
        return Table<Organization>().ToListAsync();
    }

    public Task<Organization> GetById(Guid entityId, params string[] includs)
    {
        return GetById<Organization>(entityId, includs);
    }

    public Task Insert(Organization entity)
    {
        entity.CustomerId = _currentUserService.CustomerId.Value;
        Insert<Organization>(entity);
        return Save();
    }

    public Task Update(Organization entity)
    {
        entity.CustomerId = _currentUserService.CustomerId.Value;
        Update(entity, true);
        return Save();
    }

    public Task Delete(Organization entity)
    {
        Delete<Organization>(entity);
        return Save();
    }
}
