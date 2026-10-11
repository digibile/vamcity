using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Core.Exceptions;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Data.Enums;
using LoanSystemAvizhe.Services.Inquiry.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoanSystem.Site.Areas.Member.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Member,Admin,Agent")]
public class AppInquiryController : BaseMemberController
{
	private readonly IAppInquiryService _appInquiryService;

	private readonly IntegratedInquiryService _integratedInquiryService;

	private readonly ICurrentUserService _currentUserService;

	public AppInquiryController(ILogger<AppInquiryController> logger, IAppInquiryService appInquiryService, IntegratedInquiryService integratedInquiryService, ICurrentUserService currentUserService)
		: base(logger)
	{
		_appInquiryService = appInquiryService;
		_integratedInquiryService = integratedInquiryService;
		_currentUserService = currentUserService;
	}

	[HttpGet("[action]/{id}")]
	public Task<ResponseState<List<AppInquiryModel>>> ListByMemberId(long id)
	{
		return TryCatch(async () =>
		{
			if (_currentUserService.IsInRole("Member") && _currentUserService.UserId != id)
			{
				return Error<List<AppInquiryModel>>(null, "دسترسی غیرمجاز");
			}
			List<AppInquiryModel> data = (from x in await _appInquiryService.GetByCondition((AppInquiry x) => x.MemberId == (long?)id, false)
				select new AppInquiryModel(x) into x
				orderby x.Date descending
				select x).ToList();
			return Success(data);
		});
	}

	[HttpGet("[action]/{id}/{type}")]
	public Task<ResponseState<List<AppInquiryModel>>> ListByMemberIdAndType(long id, AppInquiryType type)
	{
		return TryCatch(async () =>
		{
			if (_currentUserService.IsInRole("Member") && _currentUserService.UserId != id)
			{
				return Error<List<AppInquiryModel>>(null, "دسترسی غیرمجاز");
			}
			List<AppInquiryModel> data = (from x in await _appInquiryService.GetByCondition((AppInquiry x) => x.MemberId == (long?)id && (int)x.AppInquiryType == (int)type && x.IsSuccess, false)
				select new AppInquiryModel(x) into x
				orderby x.Date descending
				select x).ToList();
			return Success(data);
		});
	}

	[HttpPost("[action]")]
	public Task<ResponseState<AppInquiryModel>> InquiryRequest([FromBody] InquiryRequestModel inquiryRequestModel)
	{
		return TryCatch(async () =>
		{
			try
			{
				await _integratedInquiryService.InquiryRequest(inquiryRequestModel);
				return Success(new AppInquiryModel(), "عملیات با موفقیت انجام شد.");
			}
			catch (CustomException ex)
			{
				return Error<AppInquiryModel>(null, ex.Message);
			}
		});
	}
}
