using System;

namespace LoanSystem.Data.Entities;

public class Product : BaseEntity
{
    public string Code { get; set; }

    public string Name { get; set; }

    public Guid CustomerId { get; set; }

    public virtual Customer Customer { get; set; }
}
