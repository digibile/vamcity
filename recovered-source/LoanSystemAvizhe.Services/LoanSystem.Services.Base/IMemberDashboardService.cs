using System.Threading.Tasks;
using LoanSystem.Services.Models;

namespace LoanSystem.Services.Base;

public interface IMemberDashboardService
{
	Task<MemberDashboardModel> GetData(long memberId);
}
