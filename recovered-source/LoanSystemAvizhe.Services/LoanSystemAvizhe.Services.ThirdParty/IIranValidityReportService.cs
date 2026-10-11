using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;

namespace LoanSystemAvizhe.Services.ThirdParty;

public interface IIranValidityReportService
{
	IQueryable<IranValidityReport> AsQueryable();

	Task<List<IranValidityReport>> GetAll(Guid? customerId);

	Task<IranValidityReport> GetById(Guid entityId, params string[] includs);

	Task Insert(IranValidityReport entity);

	Task Update(IranValidityReport entity);

	Task Delete(IranValidityReport entity);

	Task<List<IranValidityReport>> GetByCondition(Expression<Func<IranValidityReport, bool>> condition, params string[] includs);
}
