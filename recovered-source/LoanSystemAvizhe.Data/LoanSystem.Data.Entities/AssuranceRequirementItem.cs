using System;

namespace LoanSystem.Data.Entities;

public class AssuranceRequirementItem : BaseEntity
{
    public string Title { get; set; }

    public int Order { get; set; }

    public Guid AssuranceRequirementId { get; set; }

    public virtual AssuranceRequirement AssuranceRequirement { get; set; }

    public Guid CustomerId { get; set; }

    public virtual Customer Customer { get; set; }
}
