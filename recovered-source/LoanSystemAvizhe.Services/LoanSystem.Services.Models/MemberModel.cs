using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using LoanSystem.Core;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Models;

public class MemberModel
{
	public long Id { get; set; }

	public string UserName { get; set; }

	[Required(ErrorMessage = "نام الزامی است.")]
	public string FirstName { get; set; }

	[Required(ErrorMessage = "نام خانوادگی الزامی است.")]
	public string LastName { get; set; }

	public bool Gender { get; set; }

	public string Address { get; set; }

	public string PostalCode { get; set; }

	[RegularExpression("^[a-zA-Z0-9_.-]+@[a-zA-Z0-9-]+.[a-zA-Z0-9-.]+$", ErrorMessage = "ایمیل وارد شده صحیح نمی باشد.")]
	public string Email { get; set; }

	[Required(ErrorMessage = "موبایل الزامی است.")]
	public string Mobile { get; set; }

	public string UserCode { get; set; }

	public string MobileOtp { get; set; }

	public bool MobileVerified { get; set; }

	public string City { get; set; }

	public string BankAccountsJson { get; set; }

	public string Phone { get; set; }

	public DateTime LastLoginDate { get; set; }

	public PersianDate LastLoginDate_Persian => LastLoginDate.ToPersianDateObject();

	public bool IsActive { get; set; }

	public bool WalletIsLock { get; set; }

	[Required(ErrorMessage = "کد ملی الزامی است.")]
	public string NationalCode { get; set; }

	public decimal TotalLoans { get; set; }

	public decimal TotalRemain { get; set; }

	public decimal TotalSavedMoney { get; set; }

	public int LoanCount { get; set; }

	public decimal CurrentMonthSavedMoney { get; set; }

	public decimal CurrentMonthInstallment { get; set; }

	public DateTime RegDate2 { get; set; }

	public PersianDate RegDate2_Persian => RegDate2.ToPersianDateObject();

	public string FullName => FirstName + " " + LastName;

	[StringLength(100, ErrorMessage = "کلمه عبور عبور باید حداقل {2} کاراکتر باشد.", MinimumLength = 8)]
	public string Password { get; set; }

	public string ConfirmPassword { get; set; }

	public string FatherName { get; set; }

	public string Familiar1FirstName { get; set; }

	public string Familiar1LastName { get; set; }

	public string Familiar1Mobile { get; set; }

	public string Familiar2FirstName { get; set; }

	public string Familiar2LastName { get; set; }

	public string Familiar2Mobile { get; set; }

	public string SpreadInstallments { get; set; }

	public long? AgentId { get; set; }

	public string AgentName { get; set; }

	public string MarketerName { get; set; }

	public decimal Credit { get; set; }

	public DateTime? BirthDate { get; set; }

	public PersianDate BirthDate_Persian => BirthDate?.ToPersianDateObject();

	public List<AppSmsModel> AppSmsList { get; set; } = new List<AppSmsModel>();

	public bool SendSms { get; set; }

	public ReliabilityModel Reliability { get; set; }

	public int ThirdPersonAssuranceCount { get; set; }

	public MemberModel()
	{
	}

	public MemberModel(User user)
	{
		Id = user.Id;
		UserName = user.UserName;
		FirstName = user.FirstName;
		LastName = user.LastName;
		Gender = user.Gender;
		NationalCode = user.NationalCode;
		WalletIsLock = user.WalletIsLock;
		LastLoginDate = user.LastLoginDate;
		RegDate2 = user.RegDate2;
		UserCode = user.UserCode;
		BirthDate = user.BirthDate;
		Mobile = user.Mobile;
		MobileVerified = user.MobileVerified;
		Phone = user.Phone;
		Address = user.Address;
		Email = user.Email;
		PostalCode = user.PostalCode;
		TotalSavedMoney = user.TotalSavedMoney;
		FatherName = user.FatherName;
		Familiar1FirstName = user.Familiar1FirstName;
		Familiar1LastName = user.Familiar1LastName;
		Familiar1Mobile = user.Familiar1Mobile;
		Familiar2FirstName = user.Familiar2FirstName;
		Familiar2LastName = user.Familiar2LastName;
		Familiar2Mobile = user.Familiar2Mobile;
		Credit = user.Credit;
		City = user.City;
		BankAccountsJson = user.BankAccountsJson;
		AgentName = user.Agent?.BusinessName;
		AgentId = user.Agent?.Id;
		MarketerName = user.CustomerMarketer?.BusinessName;
		if (user.Reliability != null)
		{
			Reliability = new ReliabilityModel(user.Reliability);
		}
	}
}
