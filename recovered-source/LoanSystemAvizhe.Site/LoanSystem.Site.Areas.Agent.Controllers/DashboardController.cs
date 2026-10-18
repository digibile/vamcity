using System.Threading.Tasks;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoanSystem.Site.Areas.Agent.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Agent")]
public class DashboardController : BaseAgentController
{
    private readonly IAgentDashboardService _agentDashboardService;

    private readonly ICurrentUserService _currentUserService;

    private readonly IUserPaymentService _userPaymentService;

    public DashboardController(ILogger<DashboardController> logger, IAgentDashboardService agentDashboardService, ICurrentUserService currentUserService, IUserPaymentService userPaymentService)
        : base(logger)
    {
        _agentDashboardService = agentDashboardService;
        _currentUserService = currentUserService;
        _userPaymentService = userPaymentService;
    }

    [HttpGet("[action]")]
    public Task<ResponseState<AgentDashboardModel>> Get()
    {
        return TryCatch(async () =>
        {
            AgentDashboardModel data = await _agentDashboardService.GetData(_currentUserService.UserId.Value);
            OverDueInstallmentsModel overDueInstallmentsModel = await _userPaymentService.GetOverDueInstallments(loadAppSms: false, _currentUserService.UserId.Value);
            overDueInstallmentsModel.LoanPayments.Clear();
            data.OverDueInstallments = overDueInstallmentsModel;
            return Success(data);
        });
    }

    [HttpGet("[action]")]
    public Task<ResponseState<OverDueInstallmentsModel>> OverDueInstallments()
    {
        return TryCatch(async () => Success(await _userPaymentService.GetOverDueInstallments(loadAppSms: false, _currentUserService.UserId.Value)));
    }
}
