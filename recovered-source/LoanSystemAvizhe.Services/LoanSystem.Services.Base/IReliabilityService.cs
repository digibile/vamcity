using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Base;

public interface IReliabilityService
{
	Task<List<Reliability>> GetAll(Guid? customerId);

	Task<Reliability> GetById(Guid entityId, params string[] includs);

	Task Insert(Reliability entity);

	Task Update(Reliability entity);

	Task Delete(Reliability entity);

	Task SetMembersReliability(List<long> memberIds, Dictionary<long, List<UserLoanPayment>> member_payments_dic);

	Task SetMemberReliability(long memberId, Guid reliabilityId);

	Task<List<Reliability>> GetByCondition(Expression<Func<Reliability, bool>> condition);

	void SetUserPaymentServie(IUserPaymentService userPaymentService);
}
