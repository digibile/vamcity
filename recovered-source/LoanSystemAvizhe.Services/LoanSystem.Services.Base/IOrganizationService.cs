using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Base;

public interface IOrganizationService
{
    Task<List<Organization>> GetAllOrganizations();

    Task<Organization> GetById(Guid entityId, params string[] includs);

    Task Insert(Organization entity);

    Task Update(Organization entity);

    Task Delete(Organization entity);
}
