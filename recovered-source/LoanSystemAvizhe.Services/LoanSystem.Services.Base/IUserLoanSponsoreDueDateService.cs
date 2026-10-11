using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Base;

public interface IUserLoanSponsoreDueDateService
{
	Task<List<UserLoanSponsoreDueDate>> GetByCondition(Expression<Func<UserLoanSponsoreDueDate, bool>> condition, params string[] includs);

	Task Insert(UserLoanSponsoreDueDate entity);

	Task Update(UserLoanSponsoreDueDate entity);

	Task Delete(UserLoanSponsoreDueDate entity);
}
