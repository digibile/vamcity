using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LoanSystem.Core.Configuration;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Areas.Admin.Models;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Services.Facades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;

namespace LoanSystem.Site.Areas.Admin.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Admin")]
public class UserLoanController : BaseAdminController
{
	private readonly IUserLoanService _userLoanService;

	private readonly IUserLoanFacade _userLoanFacade;

	private readonly AppConfig _appConfig;

	public UserLoanController(ILogger<UserLoanController> logger, IUserLoanService userLoanService, IUserLoanFacade userLoanFacade)
		: base(logger)
	{
		_userLoanService = userLoanService;
		_userLoanFacade = userLoanFacade;
	}

	[HttpGet("[action]")]
	public Task<ResponseState<List<UserLoanModel>>> List()
	{
		return TryCatch(async () =>
		{
			FacadeResponse<List<UserLoanModel>> facadeResponse = await _userLoanFacade.List();
			return (!facadeResponse.Success) ? Error<List<UserLoanModel>>(null, facadeResponse.Message) : Success(facadeResponse.Data);
		});
	}

	[HttpGet("[action]/{id}")]
	public Task<ResponseState<UserLoanModel>> Get(Guid id)
	{
		return TryCatch(async () =>
		{
			FacadeResponse<UserLoanModel> facadeResponse = await _userLoanFacade.Get(id);
			return (!facadeResponse.Success) ? Error<UserLoanModel>(null, facadeResponse.Message) : Success(facadeResponse.Data);
		});
	}

	[HttpGet("[action]/{id}")]
	public Task<ResponseState<dynamic>> GetCheckoutAmount(Guid id)
	{
		return TryCatch(async () =>
		{
			FacadeResponse<(decimal, decimal)> facadeResponse = await _userLoanFacade.GetCheckoutAmount(id);
			return (!facadeResponse.Success) ? Error<object>(null, facadeResponse.Message) : Success((object)new
			{
				item1 = facadeResponse.Data.Item1,
				item2 = facadeResponse.Data.Item2
			}, "");
		});
	}

	[HttpGet("[action]/{id}")]
	public Task<ResponseState<List<UserLoanModel>>> GetLoansByUserId(long id)
	{
		return TryCatch(async () =>
		{
			FacadeResponse<List<UserLoanModel>> facadeResponse = await _userLoanFacade.GetLoansByUserId(id);
			return (!facadeResponse.Success) ? Error<List<UserLoanModel>>(null, facadeResponse.Message) : Success(facadeResponse.Data);
		});
	}

	[HttpPost("[action]")]
	public Task<ResponseState<Guid>> Create([FromBody] UserLoanModel model)
	{
		foreach (KeyValuePair<string, ModelStateEntry> item in ModelState)
		{
			string key = item.Key;
			if (key.Contains("MemberModel") || key.Contains("LoanModel"))
			{
				ModelState[key].Errors.Clear();
			}
		}
		return TryCatch(async () =>
		{
			FacadeResponse<Guid> facadeResponse = await _userLoanFacade.Create(model);
			return (!facadeResponse.Success) ? Error(facadeResponse.Message) : Success(facadeResponse.Data, "عملیات با موفقیت انجام شد.");
		});
	}

	[HttpPost("[action]")]
	public Task<ResponseState<Guid>> CreateByUserLoanRequest([FromBody] UserLoanRequestModel model)
	{
		foreach (KeyValuePair<string, ModelStateEntry> item in ModelState)
		{
			string key = item.Key;
			if (key.Contains("MemberModel") || key.Contains("LoanModel"))
			{
				ModelState[key].Errors.Clear();
			}
		}
		return TryCatch(async () =>
		{
			FacadeResponse<Guid> facadeResponse = await _userLoanFacade.CreateByUserLoanRequest(model);
			return (!facadeResponse.Success) ? Error(facadeResponse.Message) : Success(facadeResponse.Data, "عملیات با موفقیت انجام شد.");
		});
	}

	[HttpPost("[action]")]
	public Task<ResponseState<Guid>> Edit([FromBody] UserLoanModel model)
	{
		return TryCatch(async () =>
		{
			FacadeResponse<Guid> facadeResponse = await _userLoanFacade.Edit(model);
			return (!facadeResponse.Success) ? Error(facadeResponse.Message) : Success(facadeResponse.Data, "عملیات با موفقیت انجام شد.");
		});
	}

	[HttpPost("[action]/{id}")]
	public Task<ResponseState<Guid>> SetAsCanceled(Guid id)
	{
		return TryCatch(async () =>
		{
			FacadeResponse<Guid> facadeResponse = await _userLoanFacade.SetAsCanceled(id);
			return (!facadeResponse.Success) ? Error(facadeResponse.Message) : Success(facadeResponse.Data, "عملیات با موفقیت انجام شد.");
		});
	}

	[HttpPost("[action]/{id}")]
	public Task<DefaultResponseState> Delete(Guid id)
	{
		return TryCatch(async () =>
		{
			FacadeResponse<bool> facadeResponse = await _userLoanFacade.Delete(id);
			return (!facadeResponse.Success) ? Error(facadeResponse.Message) : Success();
		});
	}

	[HttpPost("[action]")]
	public Task<DefaultResponseState> SetDiscount([FromBody] SetUserLoanModelDiscountModel model)
	{
		return TryCatch(async () =>
		{
			UserLoan userLoan = await _userLoanService.GetById(model.Id);
			if (userLoan == null)
			{
				return Error("امکان حذف وجود ندارد. لطفا بعدا دوباره تلاش نمایید");
			}
			UserLoanModel userLoanModel = new UserLoanModel(userLoan)
			{
				IsCheckouted = false,
				Discount = model.Discount
			};
			userLoan.Discount = model.Discount;
			userLoan.IsCheckouted = userLoanModel.Remain == 0m;
			await _userLoanService.Update(userLoan);
			return Success();
		});
	}

	[HttpGet("[action]")]
	public Task<ResponseState<List<UserLoanModel>>> AgentsOverDueList()
	{
		return TryCatch(async () =>
		{
			FacadeResponse<List<UserLoanModel>> facadeResponse = await _userLoanFacade.AgentsOverDueList();
			return (!facadeResponse.Success) ? Error<List<UserLoanModel>>(null, facadeResponse.Message) : Success(facadeResponse.Data);
		});
	}

	[HttpGet("[action]")]
	public Task<ResponseState<List<UserLoanContactModel>>> ContactsOverDueList()
	{
		return TryCatch(async () =>
		{
			FacadeResponse<List<UserLoanContactModel>> facadeResponse = await _userLoanFacade.ContactsOverDueList();
			return (!facadeResponse.Success) ? Error<List<UserLoanContactModel>>(null, facadeResponse.Message) : Success(facadeResponse.Data);
		});
	}

	[HttpGet("[action]/{returnList?}")]
	public Task<ResponseState<dynamic>> GetDepositChargeDueDate(bool? returnList = false)
	{
		return TryCatch(async () =>
		{
			FacadeResponse<object> facadeResponse = await _userLoanFacade.GetDepositChargeDueDate(returnList);
			return (!facadeResponse.Success) ? Error<object>(null, facadeResponse.Message) : ((ResponseState<object>)Success(data: (dynamic)facadeResponse.Data));
		});
	}

	[HttpGet("[action]/{search}")]
	public Task<ResponseState<List<dynamic>>> Search(string search)
	{
		return TryCatch(async () =>
		{
			FacadeResponse<List<object>> facadeResponse = await _userLoanFacade.Search(search);
			return (!facadeResponse.Success) ? Error<List<object>>(null, facadeResponse.Message) : Success(facadeResponse.Data);
		});
	}
}
