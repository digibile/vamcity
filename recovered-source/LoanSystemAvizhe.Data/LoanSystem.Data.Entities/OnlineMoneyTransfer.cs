using System;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Data.Entities;

public class OnlineMoneyTransfer : BaseEntity
{
    public decimal Amount { get; set; }

    public DateTime SaveDate { get; set; }

    public OnlineMoneyTransferSteps OnlineMoneyTransferStep { get; set; }

    public string From { get; set; }

    public string To { get; set; }

    public string ToName { get; set; }

    public string OTP { get; set; }

    public DateTime OTPGenerateDateTime { get; set; }

    public string OurTraceNumber { get; set; }

    public string Cif { get; set; }

    public bool IsSuccess { get; set; }

    public OnlineMoneyTransferProviders Provider { get; set; }

    public string EFardaJson { get; set; }

    public string ShahinJson { get; set; }

    public string ShahinResultJson { get; set; }

    public Guid? OnlineMoneyTransferPanelId { get; set; }

    public Guid? TransactionId { get; set; }

    public virtual Transaction Transaction { get; set; }

    public Guid CustomerId { get; set; }

    public virtual Customer Customer { get; set; }
}
