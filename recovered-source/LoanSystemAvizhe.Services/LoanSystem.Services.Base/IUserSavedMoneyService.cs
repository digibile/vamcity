using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Base;

public interface IUserSavedMoneyService
{
	Task<List<UserSavedMoney>> GetAll(bool includeDetails = false);

	Task<UserSavedMoney> GetById(Guid entityId, params string[] includs);

	Task<List<UserSavedMoney>> GetSavedMoneiesByUserId(Guid userId);
}
