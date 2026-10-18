using System;
using System.Collections.Generic;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Data.Entities;

public class Loan : BaseEntity
{
    public string Title { get; set; }

    public int Code { get; set; }

    public LoanProfitType LoanProfitType { get; set; }

    public double ProfitPercent { get; set; }

    public double EarlyPercent { get; set; }

    public double DelayPercent { get; set; }

    public bool IsSponsored { get; set; }

    public int MaxMonthGap { get; set; }

    public decimal MaxAmount { get; set; }

    public bool IsSmsActive { get; set; }

    public bool IncludePlan { get; set; }

    public string PlanTitle { get; set; }

    public string PlanShortDescription { get; set; }

    public string PlanHtml { get; set; }

    public string FileName { get; set; }

    public string CostsJson { get; set; }

    public string AllowedInstallments { get; set; }

    public bool Disable { get; set; }

    public bool HasSponsore { get; set; }

    public int DefaultSponsoreCheckoutDays { get; set; }

    public string DisableText { get; set; }

    public Guid CustomerId { get; set; }

    public virtual Customer Customer { get; set; }

    public virtual ICollection<LoanContact> LoanContacts { get; set; } = new HashSet<LoanContact>();

    public virtual ICollection<LoanAuthorizationItem> LoanAuthorizationItems { get; set; } = new HashSet<LoanAuthorizationItem>();

    public virtual ICollection<AgentLoanAuthorizationItem> AgentLoanAuthorizationItems { get; set; } = new HashSet<AgentLoanAuthorizationItem>();

    public virtual ICollection<LoanAssuranceRequirement> AssuranceRequirements { get; set; } = new HashSet<LoanAssuranceRequirement>();

    public virtual ICollection<LoanTag> LoanTags { get; set; } = new HashSet<LoanTag>();
}
