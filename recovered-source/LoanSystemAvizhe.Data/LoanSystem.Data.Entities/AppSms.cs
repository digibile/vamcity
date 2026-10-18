using System;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Data.Entities;

public class AppSms : BaseEntity
{
    public DateTime Date { get; set; }

    public string Text { get; set; }

    public decimal Price { get; set; }

    public AppSmsType AppSmsType { get; set; }

    public string To { get; set; }

    public string SmsPatternTitle { get; set; }

    public AppSmsStatus Status { get; set; }

    public SmsPanel SmsPanel { get; set; }

    public Guid EntityId { get; set; }

    public string EntityType { get; set; }

    public int DeliverCheckRetryCount { get; set; }

    public string SMSKey { get; set; }

    public string Tag { get; set; }

    public Guid CustomerId { get; set; }

    public virtual Customer Customer { get; set; }
}
