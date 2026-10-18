using System;
using LoanSystem.Core;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Models;

public class CustomerMarketerCommissionModel : BaseModel
{
    public decimal Amount { get; set; }

    public DateTime Date { get; set; }

    public PersianDate Date_Persian => Date.ToPersianDateObject();

    public string Description { get; set; }

    public User2Model Marketer { get; set; }

    public UserLoanModel UserLoan { get; set; }

    public decimal SumSponsorPayments { get; set; }

    public CustomerMarketerCommissionModel()
    {
    }

    public CustomerMarketerCommissionModel(CustomerMarketerCommission customerMarketerCommission)
    {
        Id = customerMarketerCommission.Id;
        Amount = customerMarketerCommission.Amount;
        Date = customerMarketerCommission.Date;
        Description = customerMarketerCommission.Description;
        if (customerMarketerCommission.Marketer != null)
        {
            Marketer = new User2Model(customerMarketerCommission.Marketer);
        }
        if (customerMarketerCommission.UserLoan != null)
        {
            UserLoan = new UserLoanModel(customerMarketerCommission.UserLoan);
        }
    }
}
