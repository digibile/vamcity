using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Services.Models;

public class GroupedTicketReportModel
{
	public TicketStatus Status { get; set; }

	public int Count { get; set; }
}
