using System;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Data.Entities;

public class LoanRequestStatus : BaseEntity
{
    public string Title { get; set; }

    public LoanRequestStatusCategorys StepCategory { get; set; }

    public bool Changable { get; set; }

    public bool AllowNewRequest { get; set; }

    public int Order { get; set; }

    public SystemRequestStatus? SystemRequestStatus { get; set; }

    public SystemAgentRequestStatus? SystemAgentRequestStatus { get; set; }

    public StatusGroup StatusGroup { get; set; }

    public Guid? SmsPatternId { get; set; }

    public virtual SmsPattern SmsPattern { get; set; }

    public Guid CustomerId { get; set; }

    public virtual Customer Customer { get; set; }
}
