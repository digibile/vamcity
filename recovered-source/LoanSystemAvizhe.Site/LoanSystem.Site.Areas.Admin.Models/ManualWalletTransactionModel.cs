using LoanSystem.Data.Entities;

namespace LoanSystem.Site.Areas.Admin.Models;

public class ManualWalletTransactionModel
{
    public long MemberId { get; set; }

    public MemberPurchaseTypes Type { get; set; }

    public decimal Amount { get; set; }
}
