using System;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Data.Entities;

public class AuthorizationItem : BaseEntity
{
    public string Title { get; set; }

    public string Description { get; set; }

    public AuthorizationItemSystemType SystemType { get; set; }

    public bool UseIfAlreadyExist { get; set; }

    public Guid CustomerId { get; set; }

    public virtual Customer Customer { get; set; }
}
