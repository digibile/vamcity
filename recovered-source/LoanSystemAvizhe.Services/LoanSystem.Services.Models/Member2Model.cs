using System;
using LoanSystem.Core;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Models;

public class Member2Model
{
    public long Id { get; set; }

    public string UserName { get; set; }

    public string FullName { get; set; }

    public string NationalCode { get; set; }

    public string PostalCode { get; set; }

    public DateTime RegDate2 { get; set; }

    public PersianDate RegDate2_Persian => RegDate2.ToPersianDateObject();

    public string FatherName { get; set; }

    public string Mobile { get; set; }

    public string Phone { get; set; }

    public string Address { get; set; }

    public string Email { get; set; }

    public string MarketerName { get; set; }

    public string City { get; set; }

    public string BankAccounts { get; set; }

    public string Familiar1FirstName { get; set; }

    public string Familiar1LastName { get; set; }

    public decimal Credit { get; set; }

    public bool MobileVerified { get; set; }

    public string Familiar1Mobile { get; set; }

    public ReliabilityModel Reliability { get; set; }

    public Member2Model()
    {
    }

    public Member2Model(User user)
    {
        Id = user.Id;
        UserName = user.UserName;
        FullName = user.FullName;
        RegDate2 = user.RegDate2;
        FatherName = user.FatherName;
        Address = user.Address;
        Mobile = user.Mobile;
        Phone = user.Phone;
        Email = user.Email;
        NationalCode = user.NationalCode;
        PostalCode = user.PostalCode;
        City = user.City;
        Familiar1FirstName = user.Familiar1FirstName;
        Familiar1LastName = user.Familiar1LastName;
        Credit = user.Credit;
        Familiar1Mobile = user.Familiar1Mobile;
        BankAccounts = user.BankAccountsJson;
        MobileVerified = user.MobileVerified;
        if (user.Reliability != null)
        {
            Reliability = new ReliabilityModel(user.Reliability);
        }
        if (user.CustomerMarketer != null)
        {
            MarketerName = ((!string.IsNullOrEmpty(user.CustomerMarketer.BusinessName)) ? user.CustomerMarketer.BusinessName : user.CustomerMarketer.FullName);
        }
    }
}
