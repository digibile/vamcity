using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Base;

public interface IAgentFactorService
{
    Task<List<AgentFactor>> GetAllAgentFactors(params string[] includs);

    Task<AgentFactor> GetById(Guid entityId, params string[] includs);

    Task<List<AgentFactor>> GetByUserId(long userId, params string[] includs);

    Task<List<AgentFactor>> GetByAgentId(long agentId, params string[] includs);

    Task Insert(AgentFactor entity);

    Task Update(AgentFactor entity);

    Task Delete(AgentFactor entity);

    Task<List<AgentFactor>> GetByCondition(Expression<Func<AgentFactor, bool>> condition, bool asTracking = false, params string[] includs);
}
