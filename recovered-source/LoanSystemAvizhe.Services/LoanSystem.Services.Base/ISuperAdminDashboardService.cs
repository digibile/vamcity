using System.Threading.Tasks;
using LoanSystem.Services.Models;

namespace LoanSystem.Services.Base;

public interface ISuperAdminDashboardService
{
	Task<SuperAdminDashboardModel> GetData();
}
