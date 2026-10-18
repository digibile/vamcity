using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Base;

public interface ILoanAssuranceRequirementService
{
    Task<List<LoanAssuranceRequirement>> GetByCondition(Expression<Func<LoanAssuranceRequirement, bool>> condition, params string[] includs);

    Task Insert(LoanAssuranceRequirement entity);

    Task Update(LoanAssuranceRequirement entity);

    Task Delete(LoanAssuranceRequirement entity);

    Task<List<Guid>> GetMatchLoanIdsByAssuranceRequirement(Dictionary<Guid, Guid?> dic);
}
