using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Base;

public interface IVisitorLogService
{
	int GetCountByMrkCode(string mrkCode);

	Task<List<VisitorLog>> GetAll();

	Task<VisitorLog> GetById(Guid entityId);

	Task<List<VisitorLog>> GetByCondition(Expression<Func<VisitorLog, bool>> condition, params string[] includs);

	Task<List<VisitorLog>> GetByMrkCode(string mrkCode);

	Task Insert(VisitorLog entity);

	Task Update(VisitorLog entity);

	Task Delete(VisitorLog entity);
}
