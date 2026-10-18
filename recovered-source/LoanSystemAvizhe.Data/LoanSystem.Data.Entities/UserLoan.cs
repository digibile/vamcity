using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoanSystem.Data.Entities;

public class UserLoan : BaseEntity
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Code { get; set; }

    public long UserId { get; set; }

    public virtual User User { get; set; }

    public Guid LoanId { get; set; }

    public virtual Loan Loan { get; set; }

    public long? AgentId { get; set; }

    public virtual User Agent { get; set; }

    public decimal AgentPrice { get; set; }

    public DateTime? AgentDueDate { get; set; }

    public long? SubAgentId { get; set; }

    public virtual User SubAgent { get; set; }

    public decimal SubAgentPrice { get; set; }

    public DateTime? SubAgentDueDate { get; set; }

    public DateTime StartDate { get; set; }

    public decimal Amount { get; set; }

    public decimal PayableAmount { get; set; }

    public decimal Installment { get; set; }

    public int Month { get; set; }

    public int MonthGap { get; set; }

    public decimal Paid { get; set; }

    public decimal Remain { get; set; }

    public bool? IsCheckouted { get; set; }

    public string Description { get; set; }

    public string Status { get; set; }

    public decimal Discount { get; set; }

    public decimal? CustomProfitPercent { get; set; }

    public DateTime? CheckoutDate { get; set; }

    public DateTime? DepositChargeDueDate { get; set; }

    public decimal PreDepositAmount { get; set; }

    public DateTime CreateAt { get; set; }

    public long RefahBetaInquiryAmount { get; set; }

    public long RefahBetaOrderId { get; set; }

    public bool IsCanceled { get; set; }

    public string SecondaryCode { get; set; }

    public Guid? CustomerPurchasePlanId { get; set; }

    public virtual CustomerPurchasePlan CustomerPurchasePlan { get; set; }

    public Guid? UserLoanRequestId { get; set; }

    public virtual UserLoanRequest UserLoanRequest { get; set; }

    public virtual ICollection<UserLoanPayment> UserLoanPayments { get; set; } = new HashSet<UserLoanPayment>();

    public virtual ICollection<Assurance> Assurances { get; set; } = new HashSet<Assurance>();

    public virtual ICollection<UserLoanContact> UserLoanContacts { get; set; } = new HashSet<UserLoanContact>();

    public virtual ICollection<UserLoanSponsoreDueDate> UserLoanSponsoreDueDates { get; set; } = new HashSet<UserLoanSponsoreDueDate>();
}
