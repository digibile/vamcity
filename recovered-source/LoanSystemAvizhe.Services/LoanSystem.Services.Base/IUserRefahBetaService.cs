using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Base;

public interface IUserRefahBetaService
{
	Task<List<UserRefahBeta>> GetByCondition(Expression<Func<UserRefahBeta, bool>> condition, params string[] includs);

	Task<List<UserRefahBeta>> GetAll();

	Task<UserRefahBeta> GetById(Guid entityId, params string[] includs);

	Task<UserRefahBeta> GetByNationalCode(string nationalCode, params string[] includs);

	Task Insert(UserRefahBeta entity);

	Task Update(UserRefahBeta entity);

	Task Delete(UserRefahBeta entity);
}
