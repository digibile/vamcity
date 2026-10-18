using System;

namespace LoanSystem.Data.Entities;

public class Organization : BaseEntity
{
    public string Name { get; set; }

    public Guid CustomerId { get; set; }

    public virtual Customer Customer { get; set; }
}
