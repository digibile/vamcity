using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Models;

public class GroupedPurchaseReportModel
{
    public MemberPurchaseTypes Type { get; set; }

    public int Count { get; set; }

    public decimal Sum { get; set; }
}
