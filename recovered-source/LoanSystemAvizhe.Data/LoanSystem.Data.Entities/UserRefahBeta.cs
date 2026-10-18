using System;

namespace LoanSystem.Data.Entities;

public class UserRefahBeta : BaseEntity
{
    public DateTime ModifiedAt { get; set; }

    public string Message { get; set; }

    public long Amount { get; set; }

    public bool IsSuccess { get; set; }

    public long UserId { get; set; }

    public virtual User User { get; set; }

    public Guid CustomerId { get; set; }

    public virtual Customer Customer { get; set; }
}
