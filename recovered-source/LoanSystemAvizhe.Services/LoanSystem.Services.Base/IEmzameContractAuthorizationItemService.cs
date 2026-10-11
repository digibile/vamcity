using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Models;
using LoanSystemAvizhe.Services.Models.Emzame;

namespace LoanSystem.Services.Base;

public interface IEmzameContractAuthorizationItemService
{
	Task<string> ReplaceDataByTags(EmzameContractAuthorizationItemModel emzameContractAuthorizationItemModel, Dictionary<string, string> manualTags, string pattern);

	Task<EmzameContractAuthorizationItem> GetById(Guid entityId, params string[] includs);

	Task<EmzameContractAuthorizationItem> GetByIdIncludeRelations(Guid entityId, params string[] includs);

	Task<int> GetCountByCondition(Expression<Func<EmzameContractAuthorizationItem, bool>> condition);

	Task<List<EmzameContractAuthorizationItem>> GetByCondition(Expression<Func<EmzameContractAuthorizationItem, bool>> condition, params string[] includs);

	Task Insert(EmzameContractAuthorizationItem entity);

	Task InsertBatch(List<EmzameContractAuthorizationItem> entities);

	Task Update(EmzameContractAuthorizationItem entity);

	Task Delete(EmzameContractAuthorizationItem entity);

	Task SendContractToEmzame(EmzameContractAuthorizationItemModel mdl, string contractTitle, string contractDescription);

	Task<SendEmzameContractResult> GetContractFromEmzame(Guid id);
}
