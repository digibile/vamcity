using System;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models.ZarinPal;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Data.Enums;
using LoanSystemAvizhe.Services.ThirdParty;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LoanSystem.Site.Controllers;

[Route("[controller]")]
public class PurchaseController : BaseDefaultController
{
	private readonly IPurchaseService _purchaseService;

	private readonly ZarinPalService _zarinPalService;

	private readonly IOptions<ZarinPalConfig> _zarinPalConfig;

	public PurchaseController(ILogger<PurchaseController> logger, IPurchaseService purchaseService, ZarinPalService zarinPalService, IOptions<ZarinPalConfig> zarinPalConfig)
		: base(logger)
	{
		_purchaseService = purchaseService;
		_zarinPalService = zarinPalService;
		_zarinPalConfig = zarinPalConfig;
	}

	[HttpPost("[action]")]
	public Task<ResponseState<string>> VerifyZarinPal([FromBody] ZarinPalVerify model)
	{
		return TryCatch(async () =>
		{
			if (model.status != "OK")
			{
				return Error("", "پرداخت انجام نشده است.");
			}
			Purchase purchase = await _purchaseService.GetByZarinPalAuthority(model.authority, "Customer");
			if (purchase == null)
			{
				return Error("", "پرداخت با خطا مواجه شده است. ثبت سفارشی با اطلاعات دریافت شده در سیستم یافت نشد.");
			}
			if (purchase.IsPaymentSuccess)
			{
				return Error("", "این سفارش قبلا پردازش شده است.");
			}
			purchase.PaymentStep = PaymentSteps.InvokePaymentVerification;
			await _purchaseService.Update(purchase);
			ZarinPalVerifyResult validate = await _zarinPalService.Verify(new ZarinPalVerify
			{
				amount = (int)purchase.Amount * 10,
				authority = model.authority,
				merchant_id = _zarinPalConfig.Value.MerchantID
			}, (ZarinPalRequestMode)_zarinPalConfig.Value.Mode);
			purchase.ZarinPalRefId = validate.data.ref_id.ToString() ?? "";
			purchase.ZarinPalStatus = validate.data.code;
			purchase.PaymentStep = PaymentSteps.ResponsedInvokePaymentVerification;
			await _purchaseService.Update(purchase);
			ZarinPalVerifyResultData data = validate.data;
			if (data != null && data.code == 101)
			{
				return Success(purchase.ZarinPalRefId, "عملیات با موفقیت انجام شد.");
			}
			ZarinPalVerifyResultData data2 = validate.data;
			if (data2 == null || data2.code != 100)
			{
				return Error("", "شماره سفارش: " + purchase.Id);
			}
			purchase.IsPaymentSuccess = true;
			await _purchaseService.Update(purchase);
			try
			{
				await _purchaseService.ApplyPurchase(purchase);
			}
			catch (Exception ex)
			{
				return Error("", ex.Message + "شماره پیگیری: " + validate.data.ref_id);
			}
			return Success(purchase.ZarinPalRefId, "عملیات با موفقیت انجام شد.");
		});
	}
}
