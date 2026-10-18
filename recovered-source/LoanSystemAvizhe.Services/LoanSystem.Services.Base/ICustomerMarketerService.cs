using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Base;

public interface ICustomerMarketerService
{
    Task<List<User>> GetAllMarketers();

    Task<User> GetById(long entityId, params string[] includs);

    Task<User> GetByAffliateCode(string affCode, Guid customerId, params string[] includs);

    Task Insert(User entity);

    Task Update(User entity);

    Task Delete(User entity);
}
