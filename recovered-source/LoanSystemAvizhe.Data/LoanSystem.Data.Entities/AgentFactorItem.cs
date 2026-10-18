using System;

namespace LoanSystem.Data.Entities;

public class AgentFactorItem : BaseEntity
{
    public Guid AgentFactorId { get; set; }

    public virtual AgentFactor AgentFactor { get; set; }

    public string Product { get; set; }

    public decimal Payable { get; set; }

    public decimal Count { get; set; }

    public decimal Fee { get; set; }

    public Guid CustomerId { get; set; }

    public virtual Customer Customer { get; set; }
}
