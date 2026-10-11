using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Models;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Services.ThirdParty;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoanSystem.Site.Areas.Admin.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Admin")]
public class OnlineMoneyTransferController : BaseAdminController
{
	private readonly ICurrentUserService _currentUserService;

	private readonly IOnlineMoneyTransferService _onlineMoneyTransferService;

	public OnlineMoneyTransferController(ILogger<OnlineMoneyTransferController> logger, ICurrentUserService currentUserService, IOnlineMoneyTransferService onlineMoneyTransferService)
		: base(logger)
	{
		_currentUserService = currentUserService;
		_onlineMoneyTransferService = onlineMoneyTransferService;
	}

	[HttpGet("[action]")]
	public Task<ResponseState<List<OnlineMoneyTransferModel>>> List(DateTime? start, DateTime? end)
	{
		return TryCatch(async () =>
		{
			List<OnlineMoneyTransferModel> data = (from x in await _onlineMoneyTransferService.GetByCondition((OnlineMoneyTransfer x) => x.CustomerId == _currentUserService.CustomerId && (((DateTime?)start).HasValue ? (x.SaveDate.Date >= ((DateTime?)start).Value.Date) : true) && (((DateTime?)end).HasValue ? (x.SaveDate.Date <= ((DateTime?)end).Value.Date) : true))
				select new OnlineMoneyTransferModel(x) into x
				orderby x.SaveDate descending
				select x).ToList();
			return Success(data);
		});
	}
}
