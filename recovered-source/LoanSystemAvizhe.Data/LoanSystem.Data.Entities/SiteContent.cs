using System;

namespace LoanSystem.Data.Entities;

public class SiteContent : BaseEntity
{
    public string Key { get; set; }

    public string Content { get; set; }

    public bool IsJson { get; set; }

    public Guid CustomerId { get; set; }

    public virtual Customer Customer { get; set; }
}
