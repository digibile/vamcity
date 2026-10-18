using System;
using LoanSystem.Core;

namespace LoanSystem.Services.Models;

public class AffiliateCommissionModel : BaseModel
{
    public decimal Amount { get; set; }

    public int PaidToUser { get; set; }

    public DateTime Date { get; set; }

    public PersianDate Date_Persian => Date.ToPersianDateObject();

    public string Description { get; set; }

    public User2Model Marketer { get; set; }

    public PurchaseModel Purchase { get; set; }
}
