using System;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Data.Entities;

public class TicketResponse : BaseEntity
{
    public DateTime CreatedAt { get; set; }

    public string Description { get; set; }

    public string FileName { get; set; }

    public bool FromAdmin { get; set; }

    public bool IsSeen { get; set; }

    public Guid TicketId { get; set; }

    public TicketRoles Role { get; set; }

    public virtual Ticket Ticket { get; set; }

    public long CreatedById { get; set; }

    public virtual User CreatedBy { get; set; }

    public Guid? CustomerId { get; set; }

    public virtual Customer Customer { get; set; }
}
