using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Helpers.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoanSystem.Site.Areas.Admin.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Admin")]
public class UserLoanRequestStatusHistoryController : BaseAdminController
{
	private readonly IUserLoanRequestStatusHistoryService _userLoanRequestStatusHistoryService;

	public UserLoanRequestStatusHistoryController(ILogger<UserLoanRequestStatusHistoryController> logger, IUserLoanRequestStatusHistoryService userLoanRequestStatusHistoryService)
		: base(logger)
	{
		_userLoanRequestStatusHistoryService = userLoanRequestStatusHistoryService;
	}

	[HttpGet("[action]/{loanRequestId}/{start?}/{end?}")]
	public Task<ResponseState<List<UserLoanRequestStatusHistoryModel>>> ListByLoanRequestId(Guid loanRequestId, DateTime? start, DateTime? end)
	{
		return TryCatch(async () =>
		{
			List<UserLoanRequestStatusHistoryModel> data = (from x in await _userLoanRequestStatusHistoryService.GetByUserLoanRequestId(loanRequestId)
				select new UserLoanRequestStatusHistoryModel(x) into x
				orderby x.DateTime
				select x).ToList();
			return Success(data);
		});
	}

	[HttpGet("[action]/{loanRequestId}")]
	public Task<ResponseState<int>> CountByLoanRequestId(Guid loanRequestId)
	{
		return TryCatch(async () =>
		{
			try
			{
				int data = await _userLoanRequestStatusHistoryService.GetCountByCondition((UserLoanRequestStatusHistory x) => x.UserLoanRequestId == loanRequestId);
				return Success(data);
			}
			catch (Exception)
			{
				throw;
			}
		});
	}
}
