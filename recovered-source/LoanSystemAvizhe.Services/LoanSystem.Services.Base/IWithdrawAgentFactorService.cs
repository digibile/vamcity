using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Base;

public interface IWithdrawAgentFactorService
{
    Task Insert(WithdrawAgentFactor entity);

    Task InsertBulk(List<WithdrawAgentFactor> entities);

    Task Delete(List<WithdrawAgentFactor> entities);

    Task<List<WithdrawAgentFactor>> GetByCondition(Expression<Func<WithdrawAgentFactor, bool>> condition, bool asTracking = false, params string[] includs);
}
