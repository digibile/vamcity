using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Services.Base;

public interface IAppInquiryService
{
	Task<List<AppInquiry>> GetAll(Guid? customerId);

	Task<AppInquiry> GetById(Guid entityId, params string[] includs);

	Task Insert(AppInquiry entity);

	Task Update(AppInquiry entity);

	Task Delete(AppInquiry entity);

	Task<int> GetCountByCondition(Expression<Func<AppInquiry, bool>> condition, bool ignoreQueryFilter = false, params string[] includs);

	Task<List<AppInquiry>> GetByCondition(Expression<Func<AppInquiry, bool>> condition, bool ignoreQueryFilter = false, params string[] includs);

	Task UpdateMember(List<Guid> inquiryIds, long memberId);

	Task<AppInquiry> GetInquiryByKey(string v1, AppInquiryType AppInquiryType, int maxPastMinutes = 60);
}
