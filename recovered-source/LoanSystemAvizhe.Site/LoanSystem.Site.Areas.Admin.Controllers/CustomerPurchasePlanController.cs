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
public class CustomerPurchasePlanController : BaseAdminController
{
	private readonly ICustomerPurchasePlanService _customerPurchasePlanService;

	public CustomerPurchasePlanController(ILogger<LoanController> logger, ICustomerPurchasePlanService customerPurchasePlanService)
		: base(logger)
	{
		_customerPurchasePlanService = customerPurchasePlanService;
	}

	[HttpGet("[action]")]
	public Task<ResponseState<List<CustomerPurchasePlanModel>>> List()
	{
		return TryCatch(async () =>
		{
			List<CustomerPurchasePlanModel> data = (await _customerPurchasePlanService.GetAll(true, "Purchase")).Select((CustomerPurchasePlan x) => new CustomerPurchasePlanModel
			{
				Id = x.Id,
				PlanType = ((x.Purchase != null && x.Purchase.ProductType == ProductTypes.Permanent) ? "دائمی" : "ابری"),
				EndDate = x.EndDate,
				StartDate = x.StartDate,
				MaxLoanCount = x.MaxLoanCount,
				RemainLoanCount = x.RemainLoanCount,
				Amount = (x.Purchase?.Amount ?? 0m)
			}).ToList();
			return Success(data);
		});
	}
}
