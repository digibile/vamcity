using System;

namespace LoanSystem.Data.Entities;

public class Expense : BaseEntity
{
    public decimal Amount { get; set; }

    public string SavedBy { get; set; }

    public string Title { get; set; }

    public string Comment { get; set; }

    public DateTime CostDate { get; set; }

    public DateTime SaveDate { get; set; }

    public Guid CustomerId { get; set; }

    public virtual Customer Customer { get; set; }
}
