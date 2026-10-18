using System;
using System.Collections.Generic;

namespace LoanSystem.Data.Entities;

public class AssuranceRequirement : BaseEntity
{
    public string Title { get; set; }

    public int Order { get; set; }

    public Guid CustomerId { get; set; }

    public virtual Customer Customer { get; set; }

    public virtual ICollection<AssuranceRequirementItem> AssuranceRequirementItems { get; set; } = new HashSet<AssuranceRequirementItem>();
}
