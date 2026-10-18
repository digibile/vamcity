using System;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Data.Entities;

public class UserLoanPaymentRequest : BaseEntity
{
    public Guid UserLoanPaymentId { get; set; }

    public virtual UserLoanPayment UserLoanPayment { get; set; }

    public DateTime RequestDate { get; set; }

    public DateTime PaymentDate { get; set; }

    public decimal Amount { get; set; }

    public string MemberDescription { get; set; }

    public string AdminDescription { get; set; }

    public UserLoanPaymentRequestStatus Status { get; set; }

    public PaymentMethods PaymentMethod { get; set; }

    public string TrackNumber { get; set; }

    public long? AdminAcceptorId { get; set; }

    public virtual User AdminAcceptor { get; set; }

    public DateTime AdminAcceptDate { get; set; }

    public bool AffectOnMainPayment { get; set; }

    public string FileName { get; set; }
}
