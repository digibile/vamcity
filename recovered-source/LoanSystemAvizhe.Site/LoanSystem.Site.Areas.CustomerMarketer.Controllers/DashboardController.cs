using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoanSystem.Site.Areas.CustomerMarketer.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "CustomerMarketer")]
public class DashboardController : BaseCustomerMarketerController
{
	private readonly ICustomerMarketerDashboardService _customerMarketerDashboardService;

	private readonly ICurrentUserService _currentUserService;

	private readonly ILoanService _loanService;

	public DashboardController(ILogger<DashboardController> logger, ICustomerMarketerDashboardService customerMarketerDashboardService, ILoanService loanService, ICurrentUserService currentUserService)
		: base(logger)
	{
		_customerMarketerDashboardService = customerMarketerDashboardService;
		_loanService = loanService;
		_currentUserService = currentUserService;
	}

	[HttpGet("[action]")]
	public Task<ResponseState<CustomerMarketerDashboardModel>> Get()
	{
		return TryCatch(async () => Success(await _customerMarketerDashboardService.GetData(_currentUserService.UserId.Value)));
	}

	[HttpGet("[action]")]
	public Task<ResponseState<List<LoanModel>>> Plans()
	{
		return TryCatch(async () =>
		{
			new List<LoanModel>();
			List<Loan> source = (await _loanService.GetAllLoans()).Where((Loan x) => x.IncludePlan).ToList();
			List<LoanModel> data = source.Select((Loan x) => new LoanModel
			{
				Id = x.Id,
				Title = x.Title,
				Code = x.Code
			}).ToList();
			return Success(data);
		});
	}
}
