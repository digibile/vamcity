using System;

namespace LoanSystem.Data.Entities;

public class AgentLoan : BaseEntity
{
    public long AgentId { get; set; }

    public virtual User Agent { get; set; }

    public Guid LoanId { get; set; }

    public virtual Loan Loan { get; set; }

    public decimal Percent { get; set; }

    public byte DefaultCheckoutDays { get; set; }

    public decimal ProductPercent { get; set; }

    public byte ProductCheckoutDays { get; set; }

    public decimal ContactPercent { get; set; }

    public string OtherPercentsJson { get; set; }

    public Guid? ContactId { get; set; }

    public virtual Contact Contact { get; set; }

    public Guid CustomerId { get; set; }

    public virtual Customer Customer { get; set; }
}
