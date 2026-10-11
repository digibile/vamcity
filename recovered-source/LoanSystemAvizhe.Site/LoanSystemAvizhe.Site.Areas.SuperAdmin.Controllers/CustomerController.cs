using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Services.Users;
using LoanSystem.Site.Areas.SuperAdmin.Controllers;
using LoanSystem.Site.Helpers.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LoanSystemAvizhe.Site.Areas.SuperAdmin.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "SuperAdmin")]
public class CustomerController : BaseSuperAdminController
{
	private readonly ICustomerService _customerService;

	private readonly IUserLoanService _userLoanService;

	private readonly ILoanService _loanService;

	private readonly UserManager _userManager;

	public CustomerController(ILogger<CustomerController> logger, UserManager userManager, IUserLoanService userLoanService, ICustomerService customerService, ILoanService loanService)
		: base(logger)
	{
		_userManager = userManager;
		_customerService = customerService;
		_userLoanService = userLoanService;
		_loanService = loanService;
	}

	[HttpGet("[action]")]
	public Task<ResponseState<List<CustomerModel>>> List()
	{
		return TryCatch(async () =>
		{
			List<Customer> source = await _customerService.GetAllCustomers();
			var loan_groupCustomer = _loanService.GetAllLoansAsQueryable().IgnoreQueryFilters().GroupBy((Loan p) => p.CustomerId, (Guid key, IEnumerable<Loan> g) => new
			{
				CustomerId = key,
				MembersCount = g.Count()
			})
				.ToList();
			var userloan_groupCustomer = _userLoanService.GetAllAsQueryable().IgnoreQueryFilters().Include((UserLoan x) => x.Loan)
				.GroupBy((UserLoan p) => p.Loan.CustomerId, (Guid key, IEnumerable<UserLoan> g) => new
				{
					CustomerId = key,
					MembersCount = g.Count()
				})
				.ToList();
			var member_groupCustomer = _userManager.GetAllAsQueryable().IgnoreQueryFilters().GroupBy((User p) => p.CustomerId, (Guid? key, IEnumerable<User> g) => new
			{
				CustomerId = key,
				MembersCount = g.Count()
			})
				.ToList();
			List<CustomerModel> list = (from x in source
				select new CustomerModel(x) into x
				orderby x.RegDate descending
				select x).ToList();
			list.ForEach((CustomerModel x) =>
			{
				x.MembersCount = member_groupCustomer.FirstOrDefault(s => s.CustomerId == x.Id)?.MembersCount ?? 0;
				x.UserLoansCount = userloan_groupCustomer.FirstOrDefault(s => s.CustomerId == x.Id)?.MembersCount ?? 0;
				x.LoansCount = loan_groupCustomer.FirstOrDefault(s => s.CustomerId == x.Id)?.MembersCount ?? 0;
			});
			return Success(list);
		});
	}
}
