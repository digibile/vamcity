using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Models;

namespace LoanSystem.Services.Base;

public interface IThirdPersonAssuranceAuthorizationItemService
{
	Task<string> ReplaceDataByTags(ThirdPersonAssuranceAuthorizationItemModel thirdPersonAssuranceAuthorizationItemModel, Dictionary<string, string> manualTags, string pattern);

	Task<string> ValidateOtpAndSignContract(ThirdPersonAssuranceAuthorizationItem authorizeItem, string otp);

	Task<ThirdPersonAssuranceAuthorizationItem> GetById(Guid entityId, params string[] includs);

	Task<ThirdPersonAssuranceAuthorizationItem> GetByIdIncludeRelations(Guid entityId, params string[] includs);

	Task<int> GetCountByCondition(Expression<Func<ThirdPersonAssuranceAuthorizationItem, bool>> condition);

	Task<List<ThirdPersonAssuranceAuthorizationItem>> GetByCondition(Expression<Func<ThirdPersonAssuranceAuthorizationItem, bool>> condition, params string[] includs);

	Task Insert(ThirdPersonAssuranceAuthorizationItem entity);

	Task InsertBatch(List<ThirdPersonAssuranceAuthorizationItem> entities);

	Task Update(ThirdPersonAssuranceAuthorizationItem entity);

	Task Delete(ThirdPersonAssuranceAuthorizationItem entity);
}
