using System;

namespace LoanSystem.Data.Entities;

public class CustomerMarketerCommission : BaseEntity
{
    public decimal Amount { get; set; }

    public DateTime Date { get; set; }

    public string Description { get; set; }

    public long MarketerId { get; set; }

    public virtual User Marketer { get; set; }

    public Guid UserLoanId { get; set; }

    public virtual UserLoan UserLoan { get; set; }
}
