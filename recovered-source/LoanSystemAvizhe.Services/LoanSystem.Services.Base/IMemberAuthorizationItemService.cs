using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Models;
using LoanSystemAvizhe.Services.Inquiry.Models;

namespace LoanSystem.Services.Base;

public interface IMemberAuthorizationItemService
{
	Task<MemberAuthorizationItem> GetById(Guid entityId, params string[] includs);

	Task<List<MemberAuthorizationItem>> GetByCondition(Expression<Func<MemberAuthorizationItem, bool>> condition, params string[] includs);

	Task Insert(MemberAuthorizationItem entity);

	Task InsertBatch(List<MemberAuthorizationItem> entities);

	Task Update(MemberAuthorizationItem entity);

	Task Delete(MemberAuthorizationItem entity);

	Task<string> ValidateOtpAndVerify(MemberAuthorizationItem entity, long memberId, string otp);

	Task<string> InquiryRequest(InquiryRequestModel inquiryRequestModel);

	Task<List<MemberAuthorizationItemModel>> SyncAndGetMemberAuthorizationsItems(long memberId);
}
