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
public class UserLoanRequestAuthorizationItemController : BaseDefaultController
{
	private readonly IUserLoanRequestAuthorizationItemService _userLoanRequestAuthorizationItemService;

	private readonly ICustomerService _customerService;

	private readonly ICurrentUserService _currentUserService;

	private readonly IntegratedSmsSenderService _integratedSmsSenderService;

	public UserLoanRequestAuthorizationItemController(ILogger<UserLoanRequestAuthorizationItemController> logger, IUserLoanRequestAuthorizationItemService userLoanRequestAuthorizationItemService, ICustomerService customerService, ICurrentUserService currentUserService, IntegratedSmsSenderService integratedSmsSenderService)
		: base(logger)
	{
		_userLoanRequestAuthorizationItemService = userLoanRequestAuthorizationItemService;
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
			UserLoanRequestAuthorizationItem userLoanRequestAuthorizationItem = (await _userLoanRequestAuthorizationItemService.GetByCondition((UserLoanRequestAuthorizationItem x) => x.Code == model.ACode && x.UserLoanRequest.CustomerId == customer.Id, "UserLoanRequest.User")).FirstOrDefault();
			if (userLoanRequestAuthorizationItem == null)
			{
				return Error<PreDigitalSignContractResult>(null, "کد مدارک معتبر نمی باشد");
			}
			if (userLoanRequestAuthorizationItem.UserLoanRequest.RequestDate < DateTime.Now.AddDays(-7.0))
			{
				return Error<PreDigitalSignContractResult>(null, "از تاریخ صدور قرارداد بیش از 7 روز گذشته است و در این بخش قابل نمایش نمی باشد. لطفا از پنل کاربری خود نسبت به مشاهده و امضا اقدام فرمایید.");
			}
			PreDigitalSignContractResult data = new PreDigitalSignContractResult
			{
				Member = (userLoanRequestAuthorizationItem.UserLoanRequest.User?.FullName ?? "نامشخص"),
				Mobile = (userLoanRequestAuthorizationItem.UserLoanRequest.User?.Mobile ?? "نامشخص"),
				AuthorizationItem = new { userLoanRequestAuthorizationItem.Id, userLoanRequestAuthorizationItem.MemberDescription, userLoanRequestAuthorizationItem.Status }
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
			UserLoanRequestAuthorizationItem entity = (await _userLoanRequestAuthorizationItemService.GetByCondition((UserLoanRequestAuthorizationItem x) => x.Id == model.Id && x.UserLoanRequest.CustomerId == _currentUserService.CustomerId, "UserLoanRequest.User", "UserLoanRequest.Agent", "UserLoanRequest.Loan", "AuthorizationItem")).FirstOrDefault();
			if (entity == null)
			{
				return Error("آیتم قرارداد یافت نشد.");
			}
			string text = await _userLoanRequestAuthorizationItemService.ValidateOtpAndSignContract(entity, model.Otp);
			if (!string.IsNullOrWhiteSpace(text))
			{
				return Error(text);
			}
			try
			{
				await _integratedSmsSenderService.SendDigitalSignComplete(new UserLoanRequestAuthorizationItemModel(entity));
			}
			catch (Exception)
			{
			}
			return Success("عملیات با موفقیت انجام شد.");
		});
	}
}
