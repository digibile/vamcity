using System.Threading.Tasks;
using LoanSystem.Services.Models;

namespace LoanSystem.Services.Base;

public interface ICustomerMarketerDashboardService
{
    Task<CustomerMarketerDashboardModel> GetData(long agentId);
}
