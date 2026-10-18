using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Data.Entities;

public class UserLoanPayment : BaseEntity
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Code { get; set; }

    public Guid UserLoanId { get; set; }

    public virtual UserLoan UserLoan { get; set; }

    public int InstallmentNumber { get; set; }

    public decimal InstallmentAmount { get; set; }

    public decimal Amount { get; set; }

    public decimal SumAmount { get; set; }

    public string SavedBy { get; set; }

    public DateTime SaveDate { get; set; }

    public DateTime TargetMonth { get; set; }

    public string FileMonth { get; set; }

    public int DelayDay { get; set; }

    public decimal DelayPrice { get; set; }

    public PaymentMethods PaymentMethod { get; set; }

    public bool IsGoldCheckouted { get; set; }

    public double GoldWeight { get; set; }

    public double GoldRate { get; set; }

    public string Description { get; set; }

    public bool SentBeforeDueDateSms { get; set; }

    public bool SentBeforeDueDateEmail { get; set; }

    public bool SentDueDateSms { get; set; }

    public bool SentDueDateEmail { get; set; }

    public DateTime? LastOverDueSmsDate { get; set; }

    public bool SentOverDueDateEmail { get; set; }

    public virtual ICollection<MemberPurchase> MemberPurchases { get; set; } = new HashSet<MemberPurchase>();

    public virtual ICollection<UserLoanPaymentRequest> UserLoanPaymentRequests { get; set; } = new HashSet<UserLoanPaymentRequest>();

    public virtual ICollection<UserLoanPaymentOther> UserLoanPaymentOthers { get; set; } = new HashSet<UserLoanPaymentOther>();
}
