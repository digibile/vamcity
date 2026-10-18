using System.Threading.Tasks;
using LoanSystem.Services.Models;

namespace LoanSystem.Services.Base;

public interface IAgentDashboardService
{
    Task<AgentDashboardModel> GetData(long agentId);
}
