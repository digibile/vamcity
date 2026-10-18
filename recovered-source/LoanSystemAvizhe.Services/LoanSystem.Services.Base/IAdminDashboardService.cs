using System.Collections.Generic;
using System.Threading.Tasks;
using LoanSystem.Services.Models;

namespace LoanSystem.Services.Base;

public interface IAdminDashboardService
{
    Task<AdminDashboardModel> GetData();

    Task<List<dynamic>> GetLastLoginMembers();

    Task<int> GetPendingTicketsCount();

    Task<dynamic> GetAgentFactorGatewayCount();

    Task<dynamic> GetMemberAuthorizationCount();

    Task<int> GetPendingCustomerOrdersCount();
}
