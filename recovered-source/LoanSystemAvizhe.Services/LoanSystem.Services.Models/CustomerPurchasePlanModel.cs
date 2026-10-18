using System;
using LoanSystem.Core;

namespace LoanSystem.Services.Models;

public class CustomerPurchasePlanModel : BaseModel
{
    public string PlanType { get; set; }

    public DateTime StartDate { get; set; }

    public PersianDate StartDate_Persian => StartDate.ToPersianDateObject();

    public DateTime EndDate { get; set; }

    public PersianDate EndDate_Persian => EndDate.ToPersianDateObject();

    public int MaxLoanCount { get; set; }

    public int RemainLoanCount { get; set; }

    public Guid CustomerId { get; set; }

    public decimal Amount { get; set; }
}
