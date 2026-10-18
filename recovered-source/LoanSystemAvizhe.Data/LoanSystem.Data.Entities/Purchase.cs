using System;
using System.Collections.Generic;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Data.Entities;

public class Purchase : BaseEntity
{
    public ProductTypes ProductType { get; set; }

    public PaymentSteps PaymentStep { get; set; }

    public PurchasePaymentMethods PaymentMethod { get; set; }

    public long UserId { get; set; }

    public virtual User User { get; set; }

    public Guid CustomerId { get; set; }

    public virtual Customer Customer { get; set; }

    public decimal Amount { get; set; }

    public DateTime StartDate { get; set; }

    public string Description { get; set; }

    public string ZarinPalAuthority { get; set; }

    public string ZarinPalRefId { get; set; }

    public int ZarinPalStatus { get; set; }

    public bool IsPaymentSuccess { get; set; }

    public string AffCode { get; set; }

    public int LoanCount { get; set; }

    public virtual ICollection<AffiliateCommission> AffiliateCommissions { get; set; } = new HashSet<AffiliateCommission>();
}
