using System;
using LoanSystem.Core;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Models;

public class CustomerModel : BaseModel
{
    public int Code { get; set; }

    public string Name { get; set; }

    public string Mobile { get; set; }

    public string Email { get; set; }

    public bool FirstLoginDone { get; set; }

    public string InitPassword { get; set; }

    public DateTime RegDate { get; set; }

    public PersianDate RegDate_Persian => RegDate.ToPersianDateObject();

    public DateTime ExpireDate { get; set; }

    public PersianDate ExpireDate_Persian => ExpireDate.ToPersianDateObject();

    public bool Purchased { get; set; }

    public bool Permanent { get; set; }

    public string AffCode { get; set; }

    public int MembersCount { get; set; }

    public int UserLoansCount { get; set; }

    public int LoansCount { get; set; }

    public CustomerModel()
    {
    }

    public CustomerModel(Customer customer)
    {
        Id = customer.Id;
        Code = customer.Code;
        Name = customer.Name;
        Email = customer.Email;
        Mobile = customer.Mobile;
        FirstLoginDone = customer.FirstLoginDone;
        InitPassword = customer.InitPassword;
        RegDate = customer.RegDate;
        ExpireDate = customer.ExpireDate;
        Purchased = customer.Purchased;
        Permanent = customer.Permanent;
        AffCode = customer.AffCode;
    }
}
