using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Core.Configuration;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Services.Users;
using LoanSystem.Site.Areas.CustomerMarketer.Controllers;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoanSystemAvizhe.Site.Areas.CustomerMarketer.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "CustomerMarketer,Admin")]
public class CommissionsController : BaseCustomerMarketerController
{
	private readonly ICustomerMarketerService _customerMarketerService;

	private readonly ITransactionService _transactionService;

	private readonly ICustomerMarketerCommissionService _customerMarketerCommissionService;

	private readonly IUserLoanRequestService _userLoanRequestService;

	private readonly IMemberService _memberService;

	private readonly ICurrentUserService _currentUserService;

	private readonly ICustomerService _customerService;

	private readonly IVisitorLogService _visitorLogService;

	private readonly UserManager _userManager;

	private readonly AppConfig _appConfig;

	public CommissionsController(ILogger<CommissionsController> logger, ICustomerMarketerCommissionService customerMarketerCommissionService, ITransactionService transactionService)
		: base(logger)
	{
		_customerMarketerCommissionService = customerMarketerCommissionService;
		_transactionService = transactionService;
	}

	[HttpGet("[action]/{marketerId}")]
	public Task<ResponseState<List<CustomerMarketerCommissionModel>>> List(int marketerId)
	{
		return TryCatch(async () =>
		{
			List<CustomerMarketerCommission> commissions = await _customerMarketerCommissionService.GetByCondition((CustomerMarketerCommission x) => x.MarketerId == (long)marketerId, "UserLoan.Loan", "UserLoan.User");
			List<Guid> commissionIds = commissions.Select((CustomerMarketerCommission x) => x.Id).ToList();
			List<Transaction> withdraws = await _transactionService.GetByCondition((Transaction x) => x.IsWithdraw && x.CustomerMarketerCommissionId.HasValue && commissionIds.Contains((Guid)x.CustomerMarketerCommissionId));
			List<CustomerMarketerCommissionModel> list = commissions.Select((CustomerMarketerCommission x) => new CustomerMarketerCommissionModel(x)).ToList();
			list.ForEach(async (CustomerMarketerCommissionModel commission) =>
			{
				commission.SumSponsorPayments = withdraws.Where((Transaction x) => x.CustomerMarketerCommissionId == commission.Id).Sum((Transaction x) => x.Amount);
			});
			return Success(list);
		});
	}
}
