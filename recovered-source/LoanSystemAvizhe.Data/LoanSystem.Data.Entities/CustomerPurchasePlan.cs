using System;

namespace LoanSystem.Data.Entities;

public class CustomerPurchasePlan : BaseEntity
{
    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public int MaxLoanCount { get; set; }

    public int RemainLoanCount { get; set; }

    public Guid CustomerId { get; set; }

    public virtual Customer Customer { get; set; }

    public Guid? PurchaseId { get; set; }

    public virtual Purchase Purchase { get; set; }
}
