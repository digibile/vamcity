using System;

namespace LoanSystem.Data.Entities;

public class BusinessAuthorizationItem : BaseEntity
{
    public decimal Cost { get; set; }

    public Guid AuthorizationItemId { get; set; }

    public virtual AuthorizationItem AuthorizationItem { get; set; }

    public Guid CustomerId { get; set; }

    public virtual Customer Customer { get; set; }
}
