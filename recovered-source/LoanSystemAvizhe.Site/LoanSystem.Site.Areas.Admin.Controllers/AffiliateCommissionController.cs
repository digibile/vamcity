using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Areas.SuperAdmin.Controllers;
using LoanSystem.Site.Helpers.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoanSystem.Site.Areas.Admin.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "SuperAdmin")]
public class AffiliateCommissionController : BaseSuperAdminController
{
	private readonly IAffiliateCommissionService _affiliateCommissionService;

	public AffiliateCommissionController(ILogger<LoanController> logger, IAffiliateCommissionService affiliateCommissionService)
		: base(logger)
	{
		_affiliateCommissionService = affiliateCommissionService;
	}

	[HttpGet("[action]/{marketerId}")]
	public Task<ResponseState<List<AffiliateCommissionModel>>> List(long marketerId)
	{
		return TryCatch(async () =>
		{
			List<AffiliateCommissionModel> data = (await _affiliateCommissionService.GetByUserId(marketerId, "Purchase")).Select((AffiliateCommission x) => new AffiliateCommissionModel
			{
				Id = x.Id,
				Amount = x.Amount,
				Description = x.Description,
				Date = x.Date,
				PaidToUser = x.PaidToUser,
				Purchase = new PurchaseModel(x.Purchase)
			}).ToList();
			return Success(data);
		});
	}

	[HttpPost("[action]")]
	public Task<DefaultResponseState> CommissionPayment([FromBody] AffiliateCommissionPaymentModel model)
	{
		return TryCatch(async () =>
		{
			AffiliateCommission affiliateCommission = await _affiliateCommissionService.GetById(model.Id);
			if (affiliateCommission == null)
			{
				return Error("اطلاعات مورد نظر یافت نشد.");
			}
			if (model.PaidToUser > affiliateCommission.Amount)
			{
				return Error("مبلغ تسویه پرداختی نباید از مبلغ پورسانت بیشتر باشد");
			}
			affiliateCommission.PaidToUser = model.PaidToUser;
			await _affiliateCommissionService.Update(affiliateCommission);
			return Success("عملیات با موفقیت انجام شد.");
		});
	}
}
