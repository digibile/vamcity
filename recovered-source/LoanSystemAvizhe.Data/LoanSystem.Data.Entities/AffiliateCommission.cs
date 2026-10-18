using System;

namespace LoanSystem.Data.Entities;

public class AffiliateCommission : BaseEntity
{
    public int Amount { get; set; }

    public int PaidToUser { get; set; }

    public DateTime Date { get; set; }

    public string Description { get; set; }

    public long UserId { get; set; }

    public virtual User User { get; set; }

    public Guid PurchaseId { get; set; }

    public virtual Purchase Purchase { get; set; }
}
