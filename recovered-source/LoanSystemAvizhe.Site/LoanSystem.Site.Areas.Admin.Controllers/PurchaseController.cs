using System;
using System.Threading.Tasks;
using LoanSystem.Core.Configuration;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Services.Models.ZarinPal;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Data.Enums;
using LoanSystemAvizhe.Services.ThirdParty;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LoanSystem.Site.Areas.Admin.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Admin")]
public class PurchaseController : BaseAdminController
{
	private readonly IPurchaseService _purchaseService;

	private readonly ZarinPalService _zarinPalService;

	private readonly AppConfig _appConfig;

	private readonly IOptions<ZarinPalConfig> _zarinPalConfig;

	private readonly ICurrentUserService _currentUserService;

	private readonly ICustomerService _customerService;

	public PurchaseController(ILogger<PurchaseController> logger, IPurchaseService purchaseService, ZarinPalService zarinPalService, IOptions<ZarinPalConfig> zarinPalConfig, IOptions<AppConfig> appConfig, ICurrentUserService currentUserService, ICustomerService customerService)
		: base(logger)
	{
		_purchaseService = purchaseService;
		_zarinPalService = zarinPalService;
		_zarinPalConfig = zarinPalConfig;
		_appConfig = appConfig.Value;
		_currentUserService = currentUserService;
		_customerService = customerService;
	}

	[HttpPost("[action]")]
	public Task<ResponseState<string>> Create([FromBody] PurchaseModel model)
	{
		return TryCatch(async () =>
		{
			Purchase entity = new Purchase
			{
				Amount = GetPurchasePrice(model),
				UserId = _currentUserService.UserId.Value,
				CustomerId = _currentUserService.CustomerId.Value,
				Description = GetPurchaseDescription(model),
				PaymentMethod = model.PaymentMethod,
				PaymentStep = PaymentSteps.Start,
				ProductType = model.ProductType,
				StartDate = DateTime.Now,
				AffCode = model.AffCode,
				LoanCount = model.LoanCount
			};
			await _purchaseService.Insert(entity);
			Customer customer = await _customerService.GetById(entity.CustomerId);
			(string, ZarinPalRequestResult) zRequest = await _zarinPalService.Request(new ZarinPalRequestWage
			{
				amount = (entity.Amount * 10m).ToString(),
				callback_url = _zarinPalConfig.Value.CallbackUrl,
				description = entity.Description,
				merchant_id = _zarinPalConfig.Value.MerchantID,
				metadata = new ZarinPalRequestMetadata
				{
					email = customer?.Email,
					mobile = customer?.Mobile
				}
			}, (ZarinPalRequestMode)_zarinPalConfig.Value.Mode);
			entity.PaymentStep = PaymentSteps.ResponsedInvokePayment;
			ZarinPalRequestResultData data = zRequest.Item2.data;
			if (data == null || data.code != 100)
			{
				entity.ZarinPalStatus = zRequest.Item2.data?.code ?? 0;
				await _purchaseService.Update(entity);
				return Error("", "در هدایت به درگاه خطایی رخ داده است");
			}
			entity.ZarinPalAuthority = zRequest.Item2.data.authority;
			await _purchaseService.Update(entity);
			return Success(zRequest.Item1, "");
		});
	}

	private decimal GetPurchasePrice(PurchaseModel model)
	{
		switch (model.ProductType)
		{
		case ProductTypes.Permanent:
			return _appConfig.PermanentPrice;
		case ProductTypes.Cloud:
		{
			int num = model.LoanCount * _appConfig.CloudPrice;
			int num2 = Math.Min(model.LoanCount / 100 * 5, 50);
			num -= num * num2 / 100;
			return num;
		}
		case ProductTypes.SMS:
		case ProductTypes.Inquiry:
			return model.Amount;
		default:
			return 0m;
		}
	}

	private string GetPurchaseDescription(PurchaseModel model)
	{
		return model.ProductType switch
		{
			ProductTypes.Permanent => "فعالسازی دائمی نرم افزار جت اقساط", 
			ProductTypes.Cloud => "شارژ یکساله نرم افزار جت اقساط " + $", {model.LoanCount} پرونده", 
			ProductTypes.SMS => "شارژ اعتبار پیامک نرم افزار جت اقساط", 
			ProductTypes.Inquiry => "شارژ اعتبار استعلام گیری نرم افزار جت اقساط", 
			_ => "نامعلوم", 
		};
	}
}
