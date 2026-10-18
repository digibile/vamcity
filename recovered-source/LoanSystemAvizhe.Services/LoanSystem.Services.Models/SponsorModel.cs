using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using LoanSystem.Core;
using LoanSystem.Data.Entities;
using LoanSystemAvizhe.Services.Infrastructure.Attributes;

namespace LoanSystem.Services.Models;

public class SponsorModel
{
    public long Id { get; set; }

    public SponsorMetaDataModel MetaData { get; set; }

    public string UserName { get; set; }

    [Required(ErrorMessage = "نام الزامی است.")]
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string Address { get; set; }

    public string PostalCode { get; set; }

    [Required(ErrorMessage = "موبایل الزامی است.")]
    public string Mobile { get; set; }

    [Required(ErrorMessage = "نام تجاری الزامی است.")]
    public string BusinessName { get; set; }

    [RegularExpression("^[a-zA-Z0-9_.-]+@[a-zA-Z0-9-]+.[a-zA-Z0-9-.]+$", ErrorMessage = "ایمیل وارد شده صحیح نمی باشد.")]
    public string Email { get; set; }

    public string Phone { get; set; }

    public DateTime LastLoginDate { get; set; }

    public PersianDate LastLoginDate_Persian => LastLoginDate.ToPersianDateObject();

    public bool IsActive { get; set; }

    public int LoanCount { get; set; }

    public decimal TotalLoans { get; set; }

    public string FullName => FirstName + " " + LastName;

    [StringLengthIgnoreEditMode(100, 8, "کلمه عبور باید حداقل {2} کاراکتر باشد.")]
    public string Password { get; set; }

    [Compare("Password", ErrorMessage = "تکرار کلمه عبور با کلمه عبور مطابقت ندارد.")]
    public string ConfirmPassword { get; set; }

    public List<AgentLoanModel> SponsorLoans { get; set; }

    public List<dynamic> DynamicLoanRequstCounts { get; set; }

    public string BankAccountsJson { get; set; }

    public string City { get; set; }

    public bool SmsActiveForMembers { get; set; }

    public SponsorModel()
    {
    }

    public SponsorModel(User user)
    {
        Id = user.Id;
        UserName = user.UserName;
        FirstName = user.FirstName;
        LastName = user.LastName;
        Mobile = user.Mobile;
        Email = user.Email;
        Phone = user.Phone;
        Address = user.Address;
        PostalCode = user.PostalCode;
        BusinessName = user.BusinessName;
        LastLoginDate = user.LastLoginDate;
        IsActive = user.IsActive;
        BankAccountsJson = user.BankAccountsJson;
        City = user.City;
        SmsActiveForMembers = user.SmsActiveForMembers;
        ICollection<AgentLoan> agentLoans = user.AgentLoans;
        SponsorLoans = ((agentLoans != null && agentLoans.Any()) ? user.AgentLoans.Select((AgentLoan x) => new AgentLoanModel(x)).ToList() : new List<AgentLoanModel>());
        if (user.AgentMetaData != null)
        {
            MetaData = new SponsorMetaDataModel(user.AgentMetaData);
        }
    }
}
