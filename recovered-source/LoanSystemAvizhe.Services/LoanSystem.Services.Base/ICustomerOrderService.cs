using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Base;

public interface ICustomerOrderService
{
	Task<List<CustomerOrder>> GetAll(bool? isSeen);

	Task<int> GetCount(bool? isSeen);

	Task<CustomerOrder> GetById(Guid entityId);

	Task Insert(CustomerOrder entity);

	Task Update(CustomerOrder entity);

	Task Delete(CustomerOrder entity);
}
