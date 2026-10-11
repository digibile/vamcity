using System;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Site.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoanSystem.Site.Controllers;

[Route("[controller]")]
[ApiExplorerSettings(GroupName = "GroupA")]
public class ThirdPersonAssuranceAuthorizationItemController : BaseDefaultController
{
	private readonly IThirdPersonAssuranceAuthorizationItemService _thirdPersonAssuranceAuthorizationItemService;

	private readonly ICustomerService _customerService;

	private readonly ICurrentUserService _currentUserService;

	private readonly IntegratedSmsSenderService _integratedSmsSenderService;

	public ThirdPersonAssuranceAuthorizationItemController(ILogger<ThirdPersonAssuranceAuthorizationItemController> logger, IThirdPersonAssuranceAuthorizationItemService thirdPersonAssuranceAuthorizationItemService, ICustomerService customerService, ICurrentUserService currentUserService, IntegratedSmsSenderService integratedSmsSenderService)
		: base(logger)
	{
		_thirdPersonAssuranceAuthorizationItemService = thirdPersonAssuranceAuthorizationItemService;
		_customerService = customerService;
		_currentUserService = currentUserService;
		_integratedSmsSenderService = integratedSmsSenderService;
	}

	[HttpPost("[action]")]
	public Task<ResponseState<PreDigitalSignContractResult>> PreDigitalSignContract([FromBody] PreDigitalSignContractDto model)
	{
		return TryCatch(async () =>
		{
			Customer customer = await _customerService.GetByCode(model.CCode);
			if (customer == null)
			{
				return Error<PreDigitalSignContractResult>(null, "کد پذیرنده معتبر نمی باشد");
			}
			_currentUserService.CustomerId = customer.Id;
			ThirdPersonAssuranceAuthorizationItem thirdPersonAssuranceAuthorizationItem = (await _thirdPersonAssuranceAuthorizationItemService.GetByCondition((ThirdPersonAssuranceAuthorizationItem x) => x.Code == model.ACode && x.CustomerId == customer.Id, "User", "UserLoanRequestAuthorizationItem.UserLoanRequest", "UserLoan")).FirstOrDefault();
			if (thirdPersonAssuranceAuthorizationItem == null)
			{
				return Error<PreDigitalSignContractResult>(null, "کد مدارک معتبر نمی باشد");
			}
			if (thirdPersonAssuranceAuthorizationItem.UserLoanRequestAuthorizationItem != null && thirdPersonAssuranceAuthorizationItem.UserLoanRequestAuthorizationItem.UserLoanRequest.RequestDate < DateTime.Now.AddDays(-7.0))
			{
				return Error<PreDigitalSignContractResult>(null, "از تاریخ صدور قرارداد بیش از 7 روز گذشته است و در این بخش قابل نمایش نمی باشد. لطفا از پنل کاربری خود نسبت به مشاهده و امضا اقدام فرمایید.");
			}
			if (thirdPersonAssuranceAuthorizationItem.UserLoan != null && thirdPersonAssuranceAuthorizationItem.UserLoan.StartDate < DateTime.Now.AddDays(-7.0))
			{
				return Error<PreDigitalSignContractResult>(null, "از تاریخ صدور قرارداد بیش از 7 روز گذشته است و در این بخش قابل نمایش نمی باشد. لطفا از پنل کاربری خود نسبت به مشاهده و امضا اقدام فرمایید.");
			}
			PreDigitalSignContractResult data = new PreDigitalSignContractResult
			{
				Mobile = (thirdPersonAssuranceAuthorizationItem.User?.Mobile ?? "نامشخص"),
				AuthorizationItem = new { thirdPersonAssuranceAuthorizationItem.Id, thirdPersonAssuranceAuthorizationItem.Comment, thirdPersonAssuranceAuthorizationItem.IsSigned }
			};
			return Success(data, "عملیات با موفقیت انجام شد.");
		});
	}

	[HttpPost("[action]")]
	public Task<DefaultResponseState> SignContractWithOtp([FromBody] SignContractWithOtp model)
	{
		return TryCatch(async () =>
		{
			Customer customer = await _customerService.GetByCode(model.CustomerCode.Value);
			if (customer == null)
			{
				return Error("کد پذیرنده معتبر نمی باشد.");
			}
			_currentUserService.CustomerId = customer.Id;
			ThirdPersonAssuranceAuthorizationItem entity = await _thirdPersonAssuranceAuthorizationItemService.GetByIdIncludeRelations(model.Id);
			if (entity == null)
			{
				return Error("آیتم قرارداد یافت نشد.");
			}
			Guid customerId = entity.CustomerId;
			Guid? customerId2 = _currentUserService.CustomerId;
			if (customerId != customerId2)
			{
				return Error("آیتم قرارداد یافت نشد.");
			}
			string text = await _thirdPersonAssuranceAuthorizationItemService.ValidateOtpAndSignContract(entity, model.Otp);
			if (!string.IsNullOrWhiteSpace(text))
			{
				return Error(text);
			}
			try
			{
				await _integratedSmsSenderService.SendThirdPersonDigitalSignComplete(new ThirdPersonAssuranceAuthorizationItemModel(entity));
			}
			catch (Exception)
			{
			}
			return Success("عملیات با موفقیت انجام شد.");
		});
	}
}
