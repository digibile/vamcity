using System;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Data.Entities;

public class UserLoanPaymentOther : BaseEntity
{
    public Guid UserLoanPaymentId { get; set; }

    public virtual UserLoanPayment UserLoanPayment { get; set; }

    public decimal Amount { get; set; }

    public string SavedBy { get; set; }

    public DateTime SaveDate { get; set; }

    public PaymentMethods PaymentMethod { get; set; }

    public bool IsGoldCheckouted { get; set; }

    public double GoldWeight { get; set; }

    public double GoldRate { get; set; }

    public string Description { get; set; }

    public Guid? MemberPurchaseId { get; set; }

    public virtual MemberPurchase MemberPurchase { get; set; }
}
