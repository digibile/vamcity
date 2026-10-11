using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Base;

public interface ILoanService
{
	IQueryable<Loan> GetAllLoansAsQueryable();

	Task<List<Loan>> GetAllLoans(params string[] includs);

	Task<List<Loan>> GetByCustomerId(Guid customerId, params string[] includs);

	Task<Loan> GetById(Guid entityId, params string[] includs);

	Task<Loan> GetByTitle(string title);

	Task<Loan> GetByCode(int code);

	Task Insert(Loan entity);

	Task Update(Loan entity);

	Task Delete(Loan entity);
}
