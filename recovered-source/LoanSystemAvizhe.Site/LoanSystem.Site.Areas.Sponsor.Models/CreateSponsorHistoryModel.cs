using System;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Site.Areas.Sponsor.Models;

public class CreateSponsorHistoryModel
{
    public CreateSponsorHistoryType type { get; set; }

    public UserLoanSponsorStatuses Status { get; set; }

    public string Comment { get; set; }

    public Guid UserLoanRequestId { get; set; }
}
