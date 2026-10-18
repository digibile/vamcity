using System;

namespace LoanSystem.Data.Entities;

public class LoanTag : BaseEntity
{
    public Guid LoanId { get; set; }

    public virtual Loan Loan { get; set; }

    public Guid TagId { get; set; }

    public virtual Tag Tag { get; set; }
}
