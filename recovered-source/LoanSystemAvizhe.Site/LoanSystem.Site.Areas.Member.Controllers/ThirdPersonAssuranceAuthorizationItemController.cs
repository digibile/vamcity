using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Services.FileHelper;
using LoanSystemAvizhe.Site.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoanSystem.Site.Areas.Member.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Member")]
public class ThirdPersonAssuranceAuthorizationItemController : BaseMemberController
{
	private readonly IThirdPersonAssuranceAuthorizationItemService _thirdPersonAssuranceAuthorizationItemService;

	private readonly IUserLoanRequestService _userLoanRequestService;

	private readonly ILoanRequestStatusService _loanRequestStatusService;

	private readonly IUserLoanRequestAuthorizationItemService _userLoanRequestAuthorizationItemService;

	private readonly IAuthorizationItemService _authorizationItemService;

	private readonly IntegratedSmsSenderService _integratedSmsSenderService;

	private readonly IFileHelper _fileHelper;

	private readonly ICurrentUserService _currentUserService;

	public ThirdPersonAssuranceAuthorizationItemController(ILogger<ThirdPersonAssuranceAuthorizationItemController> logger, IThirdPersonAssuranceAuthorizationItemService thirdPersonAssuranceAuthorizationItemService, ICurrentUserService currentUserService, IFileHelper fileHelper, IUserLoanRequestService userLoanRequestService, ILoanRequestStatusService loanRequestStatusService, IUserLoanRequestAuthorizationItemService userLoanRequestAuthorizationItemService, IAuthorizationItemService authorizationItemService, IntegratedSmsSenderService integratedSmsSenderService)
		: base(logger)
	{
		_thirdPersonAssuranceAuthorizationItemService = thirdPersonAssuranceAuthorizationItemService;
		_currentUserService = currentUserService;
		_userLoanRequestService = userLoanRequestService;
		_loanRequestStatusService = loanRequestStatusService;
		_userLoanRequestAuthorizationItemService = userLoanRequestAuthorizationItemService;
		_authorizationItemService = authorizationItemService;
		_integratedSmsSenderService = integratedSmsSenderService;
		_fileHelper = fileHelper;
	}

	[HttpGet("[action]")]
	public Task<ResponseState<List<ThirdPersonAssuranceAuthorizationItemModel>>> GetAssurancedItems()
	{
		return TryCatch(async () =>
		{
			List<ThirdPersonAssuranceAuthorizationItemModel> data = (await _thirdPersonAssuranceAuthorizationItemService.GetByCondition((ThirdPersonAssuranceAuthorizationItem x) => x.UserId == _currentUserService.UserId && x.CustomerId == _currentUserService.CustomerId, "AuthorizationItem", "User", "UserLoanRequestAuthorizationItem.UserLoanRequest.User", "UserLoanRequestAuthorizationItem.UserLoanRequest.Loan", "UserLoan.User", "UserLoan.Loan")).Select((ThirdPersonAssuranceAuthorizationItem x) =>
			{
				ThirdPersonAssuranceAuthorizationItemModel thirdPersonAssuranceAuthorizationItemModel = new ThirdPersonAssuranceAuthorizationItemModel(x);
				thirdPersonAssuranceAuthorizationItemModel.SetUserLoanRequestAuthorizationItem(new UserLoanRequestAuthorizationItemModel(x.UserLoanRequestAuthorizationItem));
				return thirdPersonAssuranceAuthorizationItemModel;
			}).ToList();
			return Success(data);
		});
	}

	[HttpGet("[action]/{id}")]
	public Task<ResponseState<ThirdPersonAssuranceAuthorizationItemModel>> Get(Guid id)
	{
		return TryCatch(async () =>
		{
			ThirdPersonAssuranceAuthorizationItemModel data = new ThirdPersonAssuranceAuthorizationItemModel(await _thirdPersonAssuranceAuthorizationItemService.GetById(id, "AuthorizationItem", "User"));
			return Success(data);
		});
	}

	[HttpPost("[action]")]
	public Task<ResponseState<Guid>> Create([FromBody] ThirdPersonAssuranceAuthorizationItemModel model)
	{
		return TryCatch(async () =>
		{
			if (!model.UserLoanRequestAuthorizationItemId.HasValue && !model.UserLoanId.HasValue)
			{
				return Error<Guid>(default, "جهت ثبت اطلاعات ضامن باید یکی از اطلاعات درخواست وام یا پرونده وام تعیین شده باشد.");
			}
			ThirdPersonAssuranceAuthorizationItem newItem = new ThirdPersonAssuranceAuthorizationItem
			{
				AuthorizationItemId = model.AuthorizationItemId,
				UserLoanRequestAuthorizationItemId = model.UserLoanRequestAuthorizationItemId,
				UserLoanId = model.UserLoanId,
				UserId = model.User?.Id,
				CustomerId = _currentUserService.CustomerId.Value
			};
			try
			{
				await _thirdPersonAssuranceAuthorizationItemService.Insert(newItem);
				ThirdPersonAssuranceAuthorizationItem thirdperson_auth = await _thirdPersonAssuranceAuthorizationItemService.GetByIdIncludeRelations(newItem.Id);
				ThirdPersonAssuranceAuthorizationItemModel mdl = new ThirdPersonAssuranceAuthorizationItemModel(thirdperson_auth);
				mdl.SetUserLoanRequestAuthorizationItem(new UserLoanRequestAuthorizationItemModel(thirdperson_auth.UserLoanRequestAuthorizationItem));
				ThirdPersonAssuranceAuthorizationItem thirdPersonAssuranceAuthorizationItem = newItem;
				thirdPersonAssuranceAuthorizationItem.Comment = await _thirdPersonAssuranceAuthorizationItemService.ReplaceDataByTags(mdl, new Dictionary<string, string>(), thirdperson_auth.AuthorizationItem.Description);
				await _thirdPersonAssuranceAuthorizationItemService.Update(newItem);
				UserLoanRequestAuthorizationItem userLoanRequestAuthorizationItem = await _userLoanRequestAuthorizationItemService.GetById(thirdperson_auth.UserLoanRequestAuthorizationItemId.Value);
				userLoanRequestAuthorizationItem.ThirdPersonAssuranceAuthorizationItemId = newItem.Id;
				userLoanRequestAuthorizationItem.Status = LoanRequestAuthorizationStatus.ثبت_شده_توسط_کاربر;
				await _userLoanRequestAuthorizationItemService.Update(userLoanRequestAuthorizationItem);
				await _integratedSmsSenderService.SendThirdPersonDigitalSignLink(mdl);
			}
			catch (Exception)
			{
				throw;
			}
			return Success(newItem.Id, "عملیات با موفقیت انجام شد.");
		});
	}

	[HttpPost("[action]")]
	public Task<ResponseState<Guid>> Edit([FromBody] ThirdPersonAssuranceAuthorizationItemModel model)
	{
		return TryCatch(async () =>
		{
			ThirdPersonAssuranceAuthorizationItem entity = await _thirdPersonAssuranceAuthorizationItemService.GetByIdIncludeRelations(model.Id);
			if (entity == null)
			{
				return Error<Guid>(default, "آیتم ضمانت یافت نشد.");
			}
			if (entity.IsSigned)
			{
				return Error<Guid>(default, "آیتم قبلا امضا شده و اجازه تغییر ندارد.");
			}
			UserLoanRequestAuthorizationItem userLoanRequestAuthorizationItem = entity.UserLoanRequestAuthorizationItem;
			if (userLoanRequestAuthorizationItem != null && userLoanRequestAuthorizationItem.Status == LoanRequestAuthorizationStatus.تایید_شده_توسط_مدیر)
			{
				return Error<Guid>(default, "آیتم در وضعیت تایید شده است و اجازه تغییر ندارد.");
			}
			ThirdPersonAssuranceAuthorizationItemModel mdl = new ThirdPersonAssuranceAuthorizationItemModel(entity);
			mdl.User = model.User;
			mdl.SetUserLoanRequestAuthorizationItem(new UserLoanRequestAuthorizationItemModel(entity.UserLoanRequestAuthorizationItem));
			entity.UserId = model.User.Id;
			ThirdPersonAssuranceAuthorizationItem thirdPersonAssuranceAuthorizationItem = entity;
			thirdPersonAssuranceAuthorizationItem.Comment = await _thirdPersonAssuranceAuthorizationItemService.ReplaceDataByTags(mdl, new Dictionary<string, string>(), entity.AuthorizationItem.Description);
			await _thirdPersonAssuranceAuthorizationItemService.Update(entity);
			await _integratedSmsSenderService.SendThirdPersonDigitalSignLink(mdl);
			return Success(entity.Id, "عملیات با موفقیت انجام شد.");
		});
	}

	[HttpPost("[action]")]
	public Task<DefaultResponseState> UpdateDigitalSignDescription([FromBody] UpdateDigitalSignDescriptionModel model)
	{
		return TryCatch(async () =>
		{
			ThirdPersonAssuranceAuthorizationItem entity = await _thirdPersonAssuranceAuthorizationItemService.GetByIdIncludeRelations(model.ThirdPersonAssuranceAuthorizationItemId);
			if (entity == null)
			{
				return Error("آیتم ضمانت یافت نشد.");
			}
			if (entity.IsSigned)
			{
				return Error("آیتم امضا شده است و اجازه تغییر ندارد.");
			}
			UserLoanRequestAuthorizationItem userLoanRequestAuthorizationItem = entity.UserLoanRequestAuthorizationItem;
			if (userLoanRequestAuthorizationItem != null && userLoanRequestAuthorizationItem.Status == LoanRequestAuthorizationStatus.تایید_شده_توسط_مدیر)
			{
				return Error("آیتم در وضعیت تایید شده است و اجازه تغییر ندارد.");
			}
			ThirdPersonAssuranceAuthorizationItemModel thirdPersonAssuranceAuthorizationItemModel = new ThirdPersonAssuranceAuthorizationItemModel(entity);
			ThirdPersonAssuranceAuthorizationItem thirdPersonAssuranceAuthorizationItem = entity;
			thirdPersonAssuranceAuthorizationItem.Comment = await _thirdPersonAssuranceAuthorizationItemService.ReplaceDataByTags(thirdPersonAssuranceAuthorizationItemModel, model.ManualFields, entity.AuthorizationItem.Description);
			try
			{
				await _thirdPersonAssuranceAuthorizationItemService.Update(entity);
			}
			catch (Exception)
			{
				throw;
			}
			return Success("عملیات با موفقیت انجام شد.");
		});
	}

	[HttpPost("[action]/{id}")]
	public Task<DefaultResponseState> Delete(Guid id)
	{
		return TryCatch(async () =>
		{
			ThirdPersonAssuranceAuthorizationItem thirdPersonAssuranceAuthorizationItem = (await _thirdPersonAssuranceAuthorizationItemService.GetByCondition((ThirdPersonAssuranceAuthorizationItem x) => x.Id == id)).FirstOrDefault();
			if (thirdPersonAssuranceAuthorizationItem == null)
			{
				return Error("امکان حذف وجود ندارد. لطفا بعدا دوباره تلاش نمایید");
			}
			await _thirdPersonAssuranceAuthorizationItemService.Delete(thirdPersonAssuranceAuthorizationItem);
			return Success();
		});
	}

	[HttpPost("[action]")]
	public Task<DefaultResponseState> SignContractWithOtp([FromBody] SignContractWithOtp model)
	{
		return TryCatch(async () =>
		{
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
