using System;

namespace LoanSystem.Data.Entities;

public class OffDate : BaseEntity
{
    public DateTime Date { get; set; }

    public string Title { get; set; }

    public Guid CustomerId { get; set; }

    public virtual Customer Customer { get; set; }
}
