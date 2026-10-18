using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using LoanSystem.Core;
using LoanSystem.Data.Entities;
using LoanSystemAvizhe.Services.Infrastructure.Attributes;

namespace LoanSystem.Services.Models;

public class UserModel
{
    public long Id { get; set; }

    [Required(ErrorMessage = "نام الزامی است.")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "نام خانوادگی الزامی است.")]
    public string LastName { get; set; }

    [Required(ErrorMessage = "نام کاربری الزامی است.")]
    [StringLength(100, ErrorMessage = "نام کاربری باید حداقل {2} کاراکتر باشد.", MinimumLength = 4)]
    public string UserName { get; set; }

    [Required(ErrorMessage = "ایمیل الزامی است.")]
    [RegularExpression("^[a-zA-Z0-9_.-]+@[a-zA-Z0-9-]+.[a-zA-Z0-9-.]+$", ErrorMessage = "ایمیل وارد شده صحیح نمی باشد.")]
    public string Email { get; set; }

    public string UserCode { get; set; }

    public string Address { get; set; }

    public string PostalCode { get; set; }

    public string Phone { get; set; }

    public decimal Credit { get; set; }

    public DateTime LastLoginDate { get; set; }

    public PersianDate LastLoginDate_Persian => LastLoginDate.ToPersianDateObject();

    [StringLengthIgnoreEditMode(100, 8, "کلمه عبور باید حداقل {2} کاراکتر باشد.")]
    public string Password { get; set; }

    [Compare("Password", ErrorMessage = "تکرار کلمه عبور با کلمه عبور مطابقت ندارد.")]
    public string ConfirmPassword { get; set; }

    public DateTime? BirthDate { get; set; }

    public DateTime RegDate2 { get; set; }

    public PersianDate RegDate2_Persian => RegDate2.ToPersianDateObject();

    public string NationalCode { get; set; }

    public int ViewCount { get; set; }

    public string FullName => FirstName + " " + LastName;

    public string FatherName { get; set; }

    public string Familiar1FirstName { get; set; }

    public string Familiar1LastName { get; set; }

    public string Familiar1Mobile { get; set; }

    public string Familiar2FirstName { get; set; }

    public string Familiar2LastName { get; set; }

    public string Familiar2Mobile { get; set; }

    public bool IsSuperAdmin { get; set; }

    public string BusinessName { get; set; }

    public List<UserClaimModel> Claims { get; set; }

    public List<string> UserRoleNames { get; set; }

    public CustomerModel Customer { get; set; }

    public string Mobile { get; set; }

    public string AffiliateCode { get; set; }

    public int CommissionPercent { get; set; }

    public long? AgentId { get; set; }

    public string City { get; set; }

    public string BankAccountsJson { get; set; }

    public bool IsActive { get; set; }

    public bool SmsActiveForMembers { get; set; }

    public AgentMetaDataModel MetaData { get; set; }

    public UserModel()
    {
    }

    public UserModel(User user)
    {
        Id = user.Id;
        BirthDate = user.BirthDate;
        FirstName = user.FirstName;
        LastName = user.LastName;
        Email = user.Email;
        UserCode = user.UserCode;
        Address = user.Address;
        PostalCode = user.PostalCode;
        Phone = user.Phone;
        Mobile = user.Mobile;
        UserName = user.UserName;
        LastLoginDate = user.LastLoginDate;
        ViewCount = user.ViewCount;
        NationalCode = user.NationalCode;
        FatherName = user.FatherName;
        Familiar1FirstName = user.Familiar1FirstName;
        Familiar1LastName = user.Familiar1LastName;
        Familiar1Mobile = user.Familiar1Mobile;
        Familiar2FirstName = user.Familiar2FirstName;
        Familiar2LastName = user.Familiar2LastName;
        Familiar2Mobile = user.Familiar2Mobile;
        UserRoleNames = user.UserRoleNames;
        BusinessName = user.BusinessName;
        AffiliateCode = user.AffiliateCode;
        CommissionPercent = user.CommissionPercent;
        Credit = user.Credit;
        AgentId = user.AgentId;
        RegDate2 = user.RegDate2;
        City = user.City;
        BankAccountsJson = user.BankAccountsJson;
        IsActive = user.IsActive;
        SmsActiveForMembers = user.SmsActiveForMembers;
        if (user.Customer != null)
        {
            Customer = new CustomerModel
            {
                Id = user.Customer.Id,
                Name = user.Customer.Name,
                Code = user.Customer.Code,
                ExpireDate = user.Customer.ExpireDate,
                Purchased = user.Customer.Purchased,
                Permanent = user.Customer.Permanent
            };
        }
        if (user.AgentMetaData != null)
        {
            MetaData = new AgentMetaDataModel(user.AgentMetaData);
        }
    }
}
