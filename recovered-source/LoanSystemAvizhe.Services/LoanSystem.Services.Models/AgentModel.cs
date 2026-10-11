using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using LoanSystem.Core;
using LoanSystem.Data.Entities;
using LoanSystemAvizhe.Services.FileHelper;
using LoanSystemAvizhe.Services.Infrastructure.Attributes;

namespace LoanSystem.Services.Models;

public class AgentModel
{
	public long Id { get; set; }

	public AgentMetaDataModel MetaData { get; set; }

	public string UserName { get; set; }

	[Required(ErrorMessage = "نام الزامی است.")]
	public string FirstName { get; set; }

	public string LastName { get; set; }

	public string NationalCode { get; set; }

	public string Shenasname { get; set; }

	public string FatherName { get; set; }

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

	public decimal TotalLoans { get; set; }

	public decimal TotalAgentShare { get; set; }

	public decimal TotalReceives { get; set; }

	public int LoanCount { get; set; }

	public string FullName => FirstName + " " + LastName;

	[StringLengthIgnoreEditMode(100, 8, "کلمه عبور باید حداقل {2} کاراکتر باشد.")]
	public string Password { get; set; }

	[Compare("Password", ErrorMessage = "تکرار کلمه عبور با کلمه عبور مطابقت ندارد.")]
	public string ConfirmPassword { get; set; }

	public List<AgentLoanModel> AgentLoans { get; set; }

	public List<dynamic> DynamicLoanRequstCounts { get; set; }

	public string BankAccountsJson { get; set; }

	public string ParentAgentName { get; set; }

	public string ParentAgentBusinessName { get; set; }

	public long? AgentId { get; set; }

	public int SubAgentCount { get; set; }

	public string City { get; set; }

	public bool SmsActiveForMembers { get; set; }

	public List<ParentAgent> Parents { get; set; }

	public AgentModel()
	{
	}

	public AgentModel(User user)
	{
		Id = user.Id;
		UserName = user.UserName;
		FirstName = user.FirstName;
		LastName = user.LastName;
		NationalCode = user.NationalCode;
		Mobile = user.Mobile;
		Shenasname = user.Shenasname;
		FatherName = user.FatherName;
		Email = user.Email;
		Phone = user.Phone;
		Address = user.Address;
		PostalCode = user.PostalCode;
		BusinessName = user.BusinessName;
		LastLoginDate = user.LastLoginDate;
		IsActive = user.IsActive;
		BankAccountsJson = user.BankAccountsJson;
		AgentId = user.AgentId;
		City = user.City;
		SmsActiveForMembers = user.SmsActiveForMembers;
		ICollection<AgentLoan> agentLoans = user.AgentLoans;
		AgentLoans = ((agentLoans != null && agentLoans.Any()) ? user.AgentLoans.Select((AgentLoan x) => new AgentLoanModel(x)).ToList() : new List<AgentLoanModel>());
		if (user.Agent != null)
		{
			ParentAgentBusinessName = user.Agent.BusinessName;
			ParentAgentName = user.Agent.FirstName;
		}
		if (user.AgentMetaData != null)
		{
			MetaData = new AgentMetaDataModel(user.AgentMetaData);
		}
	}

	public void FillImageBase64Props(IFileHelper fileHelper, Guid customerId)
	{
		if (customerId == Guid.Empty)
		{
			return;
		}
		if (!string.IsNullOrEmpty(MetaData?.KartMelliFileName))
		{
			string relativePath = Path.Combine(Directories.AgentAttachments(customerId.ToString()), MetaData.KartMelliFileName);
			string text = fileHelper.ReadFileAsBase64(relativePath);
			if (!string.IsNullOrEmpty(text))
			{
				MetaData.KartMelliBase64 = "data:image/jpeg;base64," + text;
			}
		}
		if (!string.IsNullOrEmpty(MetaData?.JavazKasbFileName))
		{
			string relativePath2 = Path.Combine(Directories.AgentAttachments(customerId.ToString()), MetaData.JavazKasbFileName);
			string text2 = fileHelper.ReadFileAsBase64(relativePath2);
			if (!string.IsNullOrEmpty(text2))
			{
				MetaData.JavazKasbBase64 = "data:image/jpeg;base64," + text2;
			}
		}
		if (!string.IsNullOrEmpty(MetaData?.RoznameRasmiFileName))
		{
			string relativePath3 = Path.Combine(Directories.AgentAttachments(customerId.ToString()), MetaData.RoznameRasmiFileName);
			string text3 = fileHelper.ReadFileAsBase64(relativePath3);
			if (!string.IsNullOrEmpty(text3))
			{
				MetaData.RoznameRasmiBase64 = "data:image/jpeg;base64," + text3;
			}
		}
		if (!string.IsNullOrEmpty(MetaData?.ThumbnailFileName))
		{
			string relativePath4 = Path.Combine(Directories.AgentAttachments(customerId.ToString()), MetaData.ThumbnailFileName);
			string text4 = fileHelper.ReadFileAsBase64(relativePath4);
			if (!string.IsNullOrEmpty(text4))
			{
				MetaData.ThumbnailBase64 = "data:image/jpeg;base64," + text4;
			}
		}
		if (!string.IsNullOrEmpty(MetaData?.EjarenameFileName))
		{
			string relativePath5 = Path.Combine(Directories.AgentAttachments(customerId.ToString()), MetaData.EjarenameFileName);
			string text5 = fileHelper.ReadFileAsBase64(relativePath5);
			if (!string.IsNullOrEmpty(text5))
			{
				MetaData.EjarenameBase64 = "data:image/jpeg;base64," + text5;
			}
		}
		if (!string.IsNullOrEmpty(MetaData?.AsasnameFileName))
		{
			string relativePath6 = Path.Combine(Directories.AgentAttachments(customerId.ToString()), MetaData.AsasnameFileName);
			string text6 = fileHelper.ReadFileAsBase64(relativePath6);
			if (!string.IsNullOrEmpty(text6))
			{
				MetaData.AsasnameBase64 = "data:image/jpeg;base64," + text6;
			}
		}
		if (!string.IsNullOrEmpty(MetaData?.PledgeChequeFileName))
		{
			string relativePath7 = Path.Combine(Directories.AgentAttachments(customerId.ToString()), MetaData.PledgeChequeFileName);
			string text7 = fileHelper.ReadFileAsBase64(relativePath7);
			if (!string.IsNullOrEmpty(text7))
			{
				MetaData.PledgeChequeBase64 = "data:image/jpeg;base64," + text7;
			}
		}
	}
}
