using System.Collections.Generic;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Base;

public interface IAgentService
{
	Task<List<User>> GetAllAgents();

	Task<List<User>> GetSubAgents(long? parentId, bool includeParent = false);

	Task<User> GetById(long entityId, params string[] includs);

	Task<User> GetByNationalCode(string nationalCode);

	Task<User> GetByMobile(string mobile);

	Task Insert(User entity);

	Task Update(User entity);

	Task Delete(User entity);
}
