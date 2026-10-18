using System;

namespace LoanSystem.Data.Entities;

public class EmzameContractAuthorizationItem : BaseEntity
{
    public Guid? UserLoanRequestAuthorizationItemId { get; set; }

    public virtual UserLoanRequestAuthorizationItem UserLoanRequestAuthorizationItem { get; set; }

    public Guid? UserLoanId { get; set; }

    public virtual UserLoan UserLoan { get; set; }

    public long? UserId { get; set; }

    public virtual User User { get; set; }

    public string ContractJson { get; set; }

    public Guid AuthorizationItemId { get; set; }

    public virtual AuthorizationItem AuthorizationItem { get; set; }

    public string Comment { get; set; }

    public string Tag { get; set; }

    public Guid CustomerId { get; set; }
}
