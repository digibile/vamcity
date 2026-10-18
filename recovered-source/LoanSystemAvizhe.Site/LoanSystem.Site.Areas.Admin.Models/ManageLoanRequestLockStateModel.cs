using System;

namespace LoanSystem.Site.Areas.Admin.Models;

public class ManageLoanRequestLockStateModel
{
    public Guid UserLoanRequestId { get; set; }

    public bool Lock { get; set; }
}
