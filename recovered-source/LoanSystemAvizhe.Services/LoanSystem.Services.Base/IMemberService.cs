using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Base;

public interface IMemberService
{
    Task<IQueryable<User>> AsQueryable();

    Task<List<User>> GetByCondition(Expression<Func<User, bool>> condition, params string[] includs);

    Task<List<User>> GetAllMembers();

    Task<List<User>> SearchMembers(string search);

    Task<User> GetById(long entityId, params string[] includs);

    Task<User> GetByNationalCode(string nationalCode);

    Task<User> GetByUserCode(string code);

    Task<User> GetByMobile(string mobile);

    Task<List<User>> GetByAgentId(long agentId);

    Task<List<User>> GetByCustomerMarketerId(long agentId);

    Task Insert(User entity);

    Task Update(User entity);

    Task Update(List<User> entities, params Expression<Func<User, object>>[] updatedProperties);

    Task Delete(User entity);
}
