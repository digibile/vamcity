using System;
using System.ComponentModel.DataAnnotations.Schema;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Data.Entities;

public class MemberPurchase : BaseEntity
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Code { get; set; }

    public PaymentSteps PaymentStep { get; set; }

    public OnlinePaymentGateways OnlinePaymentGateway { get; set; }

    public long UserId { get; set; }

    public virtual User User { get; set; }

    public Guid CustomerId { get; set; }

    public Guid? UserLoanPaymentId { get; set; }

    public virtual UserLoanPayment UserLoanPayment { get; set; }

    public Guid? AgentFactorId { get; set; }

    public virtual AgentFactor AgentFactor { get; set; }

    public decimal Amount { get; set; }

    public DateTime StartDate { get; set; }

    public string Description { get; set; }

    public string ZarinPalJson { get; set; }

    public string NextPayJson { get; set; }

    public string IPGParsianJson { get; set; }

    public string SepJson { get; set; }

    public string SadadJson { get; set; }

    public string SepehrJson { get; set; }

    public bool IsPaymentSuccess { get; set; }

    public MemberPurchaseTypes MemberPurchaseType { get; set; }

    public long? CreatorUserId { get; set; }

    public virtual User CreatorUser { get; set; }
}
