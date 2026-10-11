using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Base;

public interface IOffDateService
{
	Task<List<OffDate>> GetAll(Guid? customerId);

	Task<OffDate> GetById(Guid entityId, params string[] includs);

	Task Modify(List<OffDate> entities);

	Task Insert(OffDate entity);

	Task Update(OffDate entity);

	Task Delete(OffDate entity);

	Task<List<OffDate>> GetByCondition(Expression<Func<OffDate, bool>> condition);
}
