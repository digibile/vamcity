using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LoanSystem.Services.Models;

namespace LoanSystemAvizhe.Services.Facades;

public interface IUserLoanFacade
{
	Task<FacadeResponse<List<UserLoanModel>>> List();

	Task<FacadeResponse<Guid>> Create(UserLoanModel model);

	Task<FacadeResponse<Guid>> CreateByUserLoanRequest(UserLoanRequestModel model);

	Task<FacadeResponse<UserLoanModel>> Get(Guid id);

	Task<FacadeResponse<(decimal, decimal)>> GetCheckoutAmount(Guid id);

	Task<FacadeResponse<List<UserLoanModel>>> GetLoansByUserId(long id);

	Task<FacadeResponse<Guid>> Edit(UserLoanModel model);

	Task<FacadeResponse<Guid>> SetAsCanceled(Guid id);

	Task<FacadeResponse<bool>> Delete(Guid id);

	Task<FacadeResponse<List<UserLoanModel>>> AgentsOverDueList();

	Task<FacadeResponse<List<UserLoanContactModel>>> ContactsOverDueList();

	Task<FacadeResponse<dynamic>> GetDepositChargeDueDate(bool? returnList = false);

	Task<FacadeResponse<List<dynamic>>> Search(string search);
}
