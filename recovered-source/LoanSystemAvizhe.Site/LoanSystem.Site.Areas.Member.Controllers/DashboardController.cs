using System.Threading.Tasks;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Helpers.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoanSystem.Site.Areas.Member.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Member")]
public class DashboardController : BaseMemberController
{
	private readonly IMemberDashboardService _memberDashboardService;

	private readonly IAppContext _appContext;

	public DashboardController(ILogger<DashboardController> logger, IMemberDashboardService memberDashboardService, IAppContext appContext)
		: base(logger)
	{
		_memberDashboardService = memberDashboardService;
		_appContext = appContext;
	}

	[HttpGet("[action]")]
	public Task<ResponseState<MemberDashboardModel>> Get()
	{
		return TryCatch(async () => (_appContext.User == null) ? Error<MemberDashboardModel>(null, "کاربر یافت نشد") : Success(await _memberDashboardService.GetData(_appContext.User.Id)));
	}
}
