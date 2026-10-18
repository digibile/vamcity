using System;

namespace LoanSystemAvizhe.Site.Models;

public class PreAgentFactorPaymentResult
{
    public string Customer { get; set; }

    public string Member { get; set; }

    public long MemberId { get; set; }

    public int CustomerCode { get; set; }

    public string Agent { get; set; }

    public long AgentId { get; set; }

    public decimal AgentFactorAmount { get; set; }

    public Guid AgentFactorId { get; set; }
}
