using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Base;

public interface IAffiliateCommissionService
{
	Task<List<AffiliateCommission>> GetAll(params string[] includs);

	Task<AffiliateCommission> GetById(Guid entityId, params string[] includs);

	Task<List<AffiliateCommission>> GetByUserId(long userId, params string[] includs);

	Task Insert(AffiliateCommission entity);

	Task Update(AffiliateCommission entity);

	Task Delete(AffiliateCommission entity);
}
