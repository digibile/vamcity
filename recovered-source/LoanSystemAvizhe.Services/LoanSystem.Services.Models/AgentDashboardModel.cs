namespace LoanSystem.Services.Models;

public class AgentDashboardModel
{
	public int MemberCount { get; set; }

	public int AllLoansCount { get; set; }

	public int AllLoanRequestsCount { get; set; }

	public int TicketsCount { get; set; }

	public decimal TotalLoans { get; set; }

	public decimal TotalUnpaid { get; set; }

	public decimal TotalPaid { get; set; }

	public int AllCheckoutedCount { get; set; }

	public decimal TotalAgentShare { get; set; }

	public decimal TotalReceivedsBySponsore { get; set; }

	public dynamic SubAgentOverdue { get; set; }

	public dynamic AgentOverdue { get; set; }

	public OverDueInstallmentsModel OverDueInstallments { get; set; }
}
