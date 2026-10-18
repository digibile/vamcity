using System.Threading.Tasks;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Helpers.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoanSystem.Site.Areas.SuperAdmin.Controllers;

[Area("SuperAdmin")]
[Route("superadmin/dashboard")]
[Authorize(Roles = "SuperAdmin")]
public class DashboardController : BaseSuperAdminController
{
    private readonly ISuperAdminDashboardService _superAdminDashboardService;

    public DashboardController(ILogger<DashboardController> logger, ISuperAdminDashboardService superAdminDashboardService)
        : base(logger)
    {
        _superAdminDashboardService = superAdminDashboardService;
    }

    [HttpGet("get")]
    public Task<ResponseState<SuperAdminDashboardModel>> Get()
    {
        return TryCatch(async () => Success(await _superAdminDashboardService.GetData()));
    }
}
