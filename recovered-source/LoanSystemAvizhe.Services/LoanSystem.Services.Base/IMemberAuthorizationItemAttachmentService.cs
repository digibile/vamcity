using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Base;

public interface IMemberAuthorizationItemAttachmentService
{
	Task<MemberAuthorizationItemAttachment> GetById(Guid id, params string[] includs);

	Task<List<MemberAuthorizationItemAttachment>> GetByCondition(Expression<Func<MemberAuthorizationItemAttachment, bool>> condition, params string[] includs);

	Task Insert(MemberAuthorizationItemAttachment entity);

	Task InsertBatch(List<MemberAuthorizationItemAttachment> entities);

	Task Update(MemberAuthorizationItemAttachment entity);

	Task Delete(MemberAuthorizationItemAttachment entity);
}
