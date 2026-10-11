using System;
using System.ComponentModel.DataAnnotations;
using LoanSystem.Core;
using LoanSystem.Data.Entities;
using LoanSystemAvizhe.Services.Infrastructure.Attributes;

namespace LoanSystem.Services.Models;

public class CustomerMarketerModel
{
	public long Id { get; set; }

	[Required(ErrorMessage = "نام تجاری الزامی است.")]
	public string BusinessName { get; set; }

	public string UserName { get; set; }

	[Required(ErrorMessage = "نام الزامی است.")]
	public string FirstName { get; set; }

	public string LastName { get; set; }

	[Required(ErrorMessage = "ایمیل الزامی است.")]
	[RegularExpression("^[a-zA-Z0-9_.-]+@[a-zA-Z0-9-]+.[a-zA-Z0-9-.]+$", ErrorMessage = "ایمیل وارد شده صحیح نمی باشد.")]
	public string Email { get; set; }

	public string NationalCode { get; set; }

	public bool Gender { get; set; }

	[Required(ErrorMessage = "موبایل الزامی است.")]
	public string Mobile { get; set; }

	[Required(ErrorMessage = "کد معرفی الزامی است.")]
	public string AffiliateCode { get; set; }

	[Required(ErrorMessage = "درصد پورسانت الزامی است.")]
	public int CommissionPercent { get; set; }

	public decimal TotalCommission { get; set; }

	public decimal TotalReceives { get; set; }

	public DateTime LastLoginDate { get; set; }

	public PersianDate LastLoginDate_Persian => LastLoginDate.ToPersianDateObject();

	public bool IsActive { get; set; }

	public string FullName => FirstName + " " + LastName;

	[StringLengthIgnoreEditMode(100, 8, "کلمه عبور باید حداقل {2} کاراکتر باشد.")]
	public string Password { get; set; }

	[Compare("Password", ErrorMessage = "تکرار کلمه عبور با کلمه عبور مطابقت ندارد.")]
	public string ConfirmPassword { get; set; }

	public string BankAccountsJson { get; set; }

	public int ViewCount { get; set; }

	public int ReffererCount { get; set; }

	public int RegisterCount { get; set; }

	public int LoanRequstCounts { get; set; }

	public int UserLoanCounts { get; set; }

	public CustomerMarketerModel()
	{
	}

	public CustomerMarketerModel(User user)
	{
		Id = user.Id;
		UserName = user.UserName;
		BusinessName = user.BusinessName;
		FirstName = user.FirstName;
		LastName = user.LastName;
		NationalCode = user.NationalCode;
		Gender = user.Gender;
		Mobile = user.Mobile;
		Email = user.Email;
		IsActive = user.IsActive;
		LastLoginDate = user.LastLoginDate;
		AffiliateCode = user.AffiliateCode;
		CommissionPercent = user.CommissionPercent;
		ViewCount = user.ViewCount;
		BankAccountsJson = user.BankAccountsJson;
	}
}
