using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Areas.Admin.Models;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Data.Enums;
using LoanSystemAvizhe.Services.Models.DynamicQueryModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoanSystem.Site.Areas.Admin.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Admin")]
public class ThirdPartyTransferController : BaseAdminController
{
	private readonly IThirdPartyTransferService _thirdPartyTransferService;

	private readonly ICurrentUserService _currentUserService;

	private readonly ICustomerService _customerService;

	public ThirdPartyTransferController(ILogger<ThirdPartyTransferController> logger, IThirdPartyTransferService thirdPartyTransferService, ICurrentUserService currentUserService, ICustomerService customerService)
		: base(logger)
	{
		_currentUserService = currentUserService;
		_customerService = customerService;
		_thirdPartyTransferService = thirdPartyTransferService;
	}

	[HttpPost("[action]")]
	public Task<ResponseState<ThirdPartyTransferListResultModel>> ListNotSent([FromBody] DynamicQueryForThirdPartyModel dynamicQuery)
	{
		return TryCatch(async () =>
		{
			if (string.IsNullOrEmpty(dynamicQuery.EntityType))
			{
				return Error<ThirdPartyTransferListResultModel>(null, "نوع لیست مشخص نشده است.");
			}
			if (dynamicQuery.ThirdParty == ThirdPart.None)
			{
				return Error<ThirdPartyTransferListResultModel>(null, "نرم افزار ثانویه مشخص نشده است.");
			}
			(int, List<ThirdPartyTransferModel>) tuple = default;
			if (dynamicQuery.EntityType == "User")
			{
				tuple = await _thirdPartyTransferService.GetNotSentMembers(dynamicQuery);
			}
			else if (dynamicQuery.EntityType == "UserLoan")
			{
				tuple = await _thirdPartyTransferService.GetNotSentLoans(dynamicQuery);
			}
			else if (dynamicQuery.EntityType == "UserLoanPayment")
			{
				tuple = await _thirdPartyTransferService.GetNotSentLoanPayments(dynamicQuery);
			}
			ThirdPartyTransferListResultModel data = new ThirdPartyTransferListResultModel
			{
				Items = tuple.Item2,
				CountAll = tuple.Item1
			};
			return Success(data, "عملیات با موفقیت انجام شد.");
		});
	}

	[HttpPost("[action]")]
	public Task<ResponseState<ThirdPartyTransferListResultModel>> ListSent([FromBody] DynamicQueryForThirdPartyModel dynamicQuery)
	{
		return TryCatch(async () =>
		{
			if (string.IsNullOrEmpty(dynamicQuery.EntityType))
			{
				return Error<ThirdPartyTransferListResultModel>(null, "نوع لیست مشخص نشده است.");
			}
			if (dynamicQuery.ThirdParty == ThirdPart.None)
			{
				return Error<ThirdPartyTransferListResultModel>(null, "نرم افزار ثانویه مشخص نشده است.");
			}
			new List<ThirdPartyTransferModel>();
			List<ThirdPartyTransfer> source = await _thirdPartyTransferService.GetThirdPartyTransfers(dynamicQuery.EntityId, dynamicQuery.EntityType, dynamicQuery.ThirdParty);
			if (!string.IsNullOrEmpty(dynamicQuery.Search))
			{
				source = source.Where((ThirdPartyTransfer x) => x.EntityTag.Contains(dynamicQuery.Search)).ToList();
			}
			List<ThirdPartyTransferModel> items = (from x in source.OrderByDescending((ThirdPartyTransfer x) => x.Id).Skip((dynamicQuery.Page - 1) * dynamicQuery.ItemsPerPage).Take(dynamicQuery.ItemsPerPage)
					.ToList()
				select new ThirdPartyTransferModel(x)).ToList();
			ThirdPartyTransferListResultModel data = new ThirdPartyTransferListResultModel
			{
				Items = items,
				CountAll = source.Count()
			};
			return Success(data, "عملیات با موفقیت انجام شد.");
		});
	}

	[HttpPost("[action]")]
	public Task<ResponseState<ThirdPartyTransferListResultModel>> List([FromBody] DynamicQueryForThirdPartyModel dynamicQuery)
	{
		return TryCatch(async () =>
		{
			if (string.IsNullOrEmpty(dynamicQuery.EntityType))
			{
				return Error<ThirdPartyTransferListResultModel>(null, "نوع لیست مشخص نشده است.");
			}
			if (dynamicQuery.ThirdParty == ThirdPart.None)
			{
				return Error<ThirdPartyTransferListResultModel>(null, "نرم افزار ثانویه مشخص نشده است.");
			}
			ThirdPartyTransferListResultModel result = new ThirdPartyTransferListResultModel();
			ResponseState<ThirdPartyTransferListResultModel> list1 = await ListNotSent(dynamicQuery);
			ResponseState<ThirdPartyTransferListResultModel> responseState = await ListSent(dynamicQuery);
			if (list1.Success)
			{
				result.Items.AddRange(list1.Data.Items);
				result.CountAll += list1.Data.CountAll;
			}
			if (responseState.Success)
			{
				result.Items.AddRange(responseState.Data.Items);
				result.CountAll += responseState.Data.CountAll;
			}
			return Success(result, "عملیات با موفقیت انجام شد.");
		});
	}

	[HttpPost("[action]")]
	public Task<ResponseState<Dictionary<string, dynamic>>> Send([FromBody] SendThirdPartyTransferModel model)
	{
		return TryCatch(async () =>
		{
			if (string.IsNullOrEmpty(model.EntityType))
			{
				return Error<Dictionary<string, object>>(null, "نوع لیست مشخص نشده است.");
			}
			if (model.ThirdParty == ThirdPart.None)
			{
				return Error<Dictionary<string, object>>(null, "نرم افزار ثانویه مشخص نشده است.");
			}
			Dictionary<string, object> data = new Dictionary<string, object>();
			if (model.EntityType == "User")
			{
				data = await _thirdPartyTransferService.SendMember(model.EntityIds, model.ThirdParty);
			}
			if (model.EntityType == "UserLoan")
			{
				data = await _thirdPartyTransferService.SendInvoice(model.EntityIds, model.ThirdParty);
			}
			if (model.EntityType == "UserLoanPayment")
			{
				data = await _thirdPartyTransferService.SendInvoicePayment(model.EntityIds, model.ThirdParty);
			}
			return Success(data, "عملیات با موفقیت انجام شد.");
		});
	}
}
