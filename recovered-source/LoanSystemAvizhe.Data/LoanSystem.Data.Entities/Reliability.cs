using System;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Data.Entities;

public class Reliability : BaseEntity
{
    public string Title { get; set; }

    public string Color { get; set; }

    public int From { get; set; }

    public int To { get; set; }

    public int Count { get; set; }

    public ReliabilityRowTypes RowType { get; set; }

    public bool IsNewLoanLock { get; set; }

    public Guid CustomerId { get; set; }

    public virtual Customer Customer { get; set; }
}
