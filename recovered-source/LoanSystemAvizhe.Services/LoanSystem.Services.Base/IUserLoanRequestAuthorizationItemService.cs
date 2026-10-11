using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Models;

namespace LoanSystem.Services.Base;

public interface IUserLoanRequestAuthorizationItemService
{
	Task<string> ReplaceDataByTags(UserLoanRequestAuthorizationItemModel userLoanRequestAuthorizationItemModel, Dictionary<string, string> manualTags, string pattern);

	Task<string> ValidateOtpAndVerify(UserLoanRequestAuthorizationItem entity, long memberId, string otp);

	Task<string> ValidateOtpAndSignContract(UserLoanRequestAuthorizationItem authorizeItem, string otp);

	Task<UserLoanRequestAuthorizationItem> GetById(Guid entityId, params string[] includs);

	Task<List<UserLoanRequestAuthorizationItem>> GetByCondition(Expression<Func<UserLoanRequestAuthorizationItem, bool>> condition, params string[] includs);

	Task Insert(UserLoanRequestAuthorizationItem entity);

	Task InsertBatch(List<UserLoanRequestAuthorizationItem> entities);

	Task Update(UserLoanRequestAuthorizationItem entity);

	Task Delete(UserLoanRequestAuthorizationItem entity);
}
