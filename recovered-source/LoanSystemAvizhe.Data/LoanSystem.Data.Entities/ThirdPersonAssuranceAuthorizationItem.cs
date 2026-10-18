using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoanSystem.Data.Entities;

public class ThirdPersonAssuranceAuthorizationItem : BaseEntity
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Code { get; set; }

    public Guid? UserLoanRequestAuthorizationItemId { get; set; }

    public virtual UserLoanRequestAuthorizationItem UserLoanRequestAuthorizationItem { get; set; }

    public Guid? UserLoanId { get; set; }

    public virtual UserLoan UserLoan { get; set; }

    public long? UserId { get; set; }

    public virtual User User { get; set; }

    public bool IsSigned { get; set; }

    public DateTime LastChangeDate { get; set; }

    public Guid AuthorizationItemId { get; set; }

    public virtual AuthorizationItem AuthorizationItem { get; set; }

    public string Comment { get; set; }

    public string Tag { get; set; }

    public Guid CustomerId { get; set; }
}
