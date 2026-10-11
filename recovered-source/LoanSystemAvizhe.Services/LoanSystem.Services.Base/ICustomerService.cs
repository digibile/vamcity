using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Base;

public interface ICustomerService
{
	Task<List<Customer>> GetAllCustomers();

	Task<Customer> GetById(Guid entityId, params string[] includs);

	Task<Customer> GetByCode(int code, params string[] includs);

	Task<Customer> GetByDomain(string domain, params string[] includs);

	Task<Customer> GetByMobile(string mobile);

	Task SetFirstLoginAsDone(Guid id);

	Task Insert(Customer entity);

	Task Update(Customer entity);

	Task Delete(Customer entity);
}
