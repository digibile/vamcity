using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Base;

public interface IAssuranceRequirementService
{
    Task<List<AssuranceRequirement>> GetByCondition(Expression<Func<AssuranceRequirement, bool>> condition, params string[] includs);

    Task Insert(AssuranceRequirement entity);

    Task Update(AssuranceRequirement entity);

    Task Delete(AssuranceRequirement entity);
}
