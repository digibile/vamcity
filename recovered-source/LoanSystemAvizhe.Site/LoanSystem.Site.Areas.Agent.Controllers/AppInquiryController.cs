using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Helpers.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoanSystem.Site.Areas.Agent.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Agent")]
public class AppInquiryController : BaseAgentController
{
	private readonly IAppInquiryService _appInquiryService;

	private readonly IntegratedInquiryService _integratedInquiryService;

	public AppInquiryController(ILogger<AppInquiryController> logger, IAppInquiryService appInquiryService, IntegratedInquiryService integratedInquiryService)
		: base(logger)
	{
		_appInquiryService = appInquiryService;
		_integratedInquiryService = integratedInquiryService;
	}

	[HttpGet("[action]/{id}")]
	public Task<ResponseState<List<AppInquiryModel>>> ListByMemberId(long id)
	{
		return TryCatch(async () =>
		{
			List<AppInquiryModel> data = (from x in await _appInquiryService.GetByCondition((AppInquiry x) => x.MemberId == (long?)id, false)
				select new AppInquiryModel(x) into x
				orderby x.Date descending
				select x).ToList();
			return Success(data);
		});
	}
}
