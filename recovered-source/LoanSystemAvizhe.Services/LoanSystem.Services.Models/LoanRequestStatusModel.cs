using System;
using LoanSystem.Data.Entities;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Services.Models;

public class LoanRequestStatusModel : BaseModel
{
    public string Title { get; set; }

    public LoanRequestStatusCategorys StepCategory { get; set; }

    public SystemRequestStatus? SystemRequestStatus { get; set; } = LoanSystem.Data.Entities.SystemRequestStatus.ایجاد_اولیه;

    public SystemAgentRequestStatus? SystemAgentRequestStatus { get; set; } = LoanSystem.Data.Entities.SystemAgentRequestStatus.بدون_وضعیت;

    public StatusGroup StatusGroup { get; set; }

    public bool Changable { get; set; }

    public bool AllowNewRequest { get; set; }

    public int Order { get; set; }

    public SmsPatternModel SmsPattern { get; set; }

    public Guid CustomerId { get; set; }

    public LoanRequestStatusModel()
    {
    }

    public LoanRequestStatusModel(LoanRequestStatus loanRequestStatus)
    {
        Id = loanRequestStatus.Id;
        Title = loanRequestStatus.Title;
        StepCategory = loanRequestStatus.StepCategory;
        SystemRequestStatus = loanRequestStatus.SystemRequestStatus;
        SystemAgentRequestStatus = loanRequestStatus.SystemAgentRequestStatus;
        StatusGroup = loanRequestStatus.StatusGroup;
        Changable = loanRequestStatus.Changable;
        AllowNewRequest = loanRequestStatus.AllowNewRequest;
        Order = loanRequestStatus.Order;
        if (loanRequestStatus.SmsPattern != null)
        {
            SmsPattern = new SmsPatternModel(loanRequestStatus.SmsPattern);
        }
        CustomerId = loanRequestStatus.CustomerId;
    }
}
