using System.Collections.Generic;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Base;

public interface ISponsorService
{
    Task<List<User>> GetAllSponsors();

    Task<User> GetById(long entityId, params string[] includs);

    Task<User> GetByMobile(string mobile);

    Task Insert(User entity);

    Task Update(User entity);

    Task Delete(User entity);
}
