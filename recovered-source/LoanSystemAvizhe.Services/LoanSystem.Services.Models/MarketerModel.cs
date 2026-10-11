using System;
using System.ComponentModel.DataAnnotations;
using LoanSystem.Core;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Models;

public class MarketerModel
{
	public long Id { get; set; }

	public string UserName { get; set; }

	[Required(ErrorMessage = "نام الزامی است.")]
	public string FirstName { get; set; }

	[Required(ErrorMessage = "نام خانوادگی الزامی است.")]
	public string LastName { get; set; }

	[Required(ErrorMessage = "ایمیل الزامی است.")]
	[RegularExpression("^[a-zA-Z0-9_.-]+@[a-zA-Z0-9-]+.[a-zA-Z0-9-.]+$", ErrorMessage = "ایمیل وارد شده صحیح نمی باشد.")]
	public string Email { get; set; }

	public bool Gender { get; set; }

	public string Mobile { get; set; }

	[Required(ErrorMessage = "کد معرفی الزامی است.")]
	public string AffiliateCode { get; set; }

	[Required(ErrorMessage = "درصد پورسانت الزامی است.")]
	public int CommissionPercent { get; set; }

	public int TotalCommission { get; set; }

	public int TotalPaids { get; set; }

	public DateTime LastLoginDate { get; set; }

	public PersianDate LastLoginDate_Persian => LastLoginDate.ToPersianDateObject();

	public bool IsActive { get; set; }

	[Required(ErrorMessage = "کد ملی الزامی است.")]
	public string NationalCode { get; set; }

	public string FullName => FirstName + " " + LastName;

	[StringLength(100, ErrorMessage = "کلمه عبور عبور باید حداقل {2} کاراکتر باشد.", MinimumLength = 8)]
	public string Password { get; set; }

	[Compare("Password", ErrorMessage = "تکرار کلمه عبور با کلمه عبور مطابقت ندارد.")]
	public string ConfirmPassword { get; set; }

	public int ViewCount { get; set; }

	public MarketerModel()
	{
	}

	public MarketerModel(User user)
	{
		Id = user.Id;
		UserName = user.UserName;
		FirstName = user.FirstName;
		LastName = user.LastName;
		Gender = user.Gender;
		NationalCode = user.NationalCode;
		Mobile = user.Mobile;
		Email = user.Email;
		IsActive = user.IsActive;
		LastLoginDate = user.LastLoginDate;
		AffiliateCode = user.AffiliateCode;
		CommissionPercent = user.CommissionPercent;
		ViewCount = user.ViewCount;
	}
}
