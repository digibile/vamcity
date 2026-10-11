using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text.Json;
using LoanSystem.Data.Entities;
using LoanSystemAvizhe.Data.Enums;
using LoanSystemAvizhe.Services.FileHelper;
using Microsoft.AspNetCore.Http;

namespace LoanSystem.Services.Models;

public class LoanModel : BaseModel
{
	[Required(ErrorMessage = "نوع محاسبه سود الزامی است.")]
	public LoanProfitType LoanProfitType { get; set; }

	[Required(ErrorMessage = "عنوان وام الزامی است.")]
	public string Title { get; set; }

	[Required(ErrorMessage = "شماره وام الزامی است.")]
	public int Code { get; set; }

	[Range(0.0, double.MaxValue, ErrorMessage = "درصد بهره نمی تواند کمتر از صفر باشد")]
	public double ProfitPercent { get; set; }

	[Range(0, 100, ErrorMessage = "درصد زودکرد نمی تواند کمتر از صفر باشد")]
	public double EarlyPercent { get; set; }

	[Range(0, 100, ErrorMessage = "درصد دیرکرد نمی تواند کمتر از صفر باشد")]
	public double DelayPercent { get; set; }

	public bool IsSponsored { get; set; }

	public bool IsSmsActive { get; set; }

	[Range(0.0, double.MaxValue, ErrorMessage = "حداکثر تعداد فاصله بین اقساط نمی تواند کمتر از صفر باشد")]
	public int? MaxMonthGap { get; set; }

	[Range(0.0, double.MaxValue, ErrorMessage = "حداکثر مبلغ وام نمی تواند کمتر از صفر باشد")]
	public decimal? MaxAmount { get; set; }

	public bool IncludePlan { get; set; }

	[StringLength(100, MinimumLength = 0, ErrorMessage = "طول کاراکترهای عنوان نباید بیشتر از 100 باشد")]
	public string PlanTitle { get; set; }

	[StringLength(1000, MinimumLength = 0, ErrorMessage = "طول کاراکترهای توضیحات کوتاه نباید بیشتر از 1000 باشد")]
	public string PlanShortDescription { get; set; } = "";

	public string PlanHtml { get; set; } = "";

	public IFormFile File { get; set; }

	public string FileName { get; set; }

	public string Base64 { get; set; }

	public string ImageUrl { get; set; }

	public string CostsJson { get; set; }

	public string AllowedInstallments { get; set; }

	public bool Disable { get; set; }

	public bool HasSponsore { get; set; }

	public string DisableText { get; set; }

	public List<LoanAuthorizationItemModel> LoanAuthorizationItems { get; set; }

	public List<LoanAuthorizationItemModel> AgentLoanAuthorizationItems { get; set; }

	public List<LoanContactModel> LoanContacts { get; set; }

	public List<TagModel> Tags { get; set; }

	public Guid CustomerId { get; set; }

	public int DefaultSponsoreCheckoutDays { get; set; }

	public LoanModel()
	{
	}

	public LoanModel(Loan loan)
	{
		Id = loan.Id;
		Title = loan.Title;
		Code = loan.Code;
		LoanProfitType = loan.LoanProfitType;
		ProfitPercent = loan.ProfitPercent;
		EarlyPercent = loan.EarlyPercent;
		DelayPercent = loan.DelayPercent;
		IsSponsored = loan.IsSponsored;
		IsSmsActive = loan.IsSmsActive;
		MaxAmount = loan.MaxAmount;
		MaxMonthGap = loan.MaxMonthGap;
		IncludePlan = loan.IncludePlan;
		PlanTitle = loan.PlanTitle;
		PlanShortDescription = loan.PlanShortDescription ?? "";
		PlanHtml = loan.PlanHtml ?? "";
		FileName = loan.FileName;
		AllowedInstallments = loan.AllowedInstallments;
		CostsJson = loan.CostsJson;
		CustomerId = loan.CustomerId;
		Disable = loan.Disable;
		HasSponsore = loan.HasSponsore;
		DisableText = loan.DisableText ?? "";
		DefaultSponsoreCheckoutDays = loan.DefaultSponsoreCheckoutDays;
		ICollection<LoanAuthorizationItem> loanAuthorizationItems = loan.LoanAuthorizationItems;
		if (loanAuthorizationItems != null && loanAuthorizationItems.Any())
		{
			LoanAuthorizationItems = loan.LoanAuthorizationItems?.Select((LoanAuthorizationItem l) => new LoanAuthorizationItemModel(l)).ToList();
		}
		ICollection<AgentLoanAuthorizationItem> agentLoanAuthorizationItems = loan.AgentLoanAuthorizationItems;
		if (agentLoanAuthorizationItems != null && agentLoanAuthorizationItems.Any())
		{
			AgentLoanAuthorizationItems = loan.AgentLoanAuthorizationItems?.Select((AgentLoanAuthorizationItem l) => new LoanAuthorizationItemModel(l)).ToList();
		}
		ICollection<LoanContact> loanContacts = loan.LoanContacts;
		if (loanContacts != null && loanContacts.Any())
		{
			LoanContacts = loan.LoanContacts?.Select((LoanContact l) => new LoanContactModel(l)).ToList();
		}
		ICollection<LoanTag> loanTags = loan.LoanTags;
		if (loanTags != null && loanTags.Any())
		{
			Tags = loan.LoanTags?.Select((LoanTag l) => new TagModel(l.Tag)).ToList();
		}
	}

	public void FillImageBase64Prop(IFileHelper fileHelper)
	{
		if (!string.IsNullOrEmpty(FileName) && !(CustomerId == Guid.Empty))
		{
			string relativePath = Path.Combine(Directories.Loan(CustomerId.ToString()), FileName);
			string text = fileHelper.ReadFileAsBase64(relativePath);
			if (!string.IsNullOrEmpty(text))
			{
				Base64 = "data:image/jpeg;base64," + text;
			}
		}
	}

	public void FillImageUrlProp(IFileHelper fileHelper)
	{
		if (!string.IsNullOrEmpty(FileName) && !(CustomerId == Guid.Empty))
		{
			string text = Path.Combine(Directories.Loan(CustomerId.ToString()), FileName);
			ImageUrl = "/" + text.Replace("\\", "/");
		}
	}

	public bool ValidateAllowedMonth(int month)
	{
		if (string.IsNullOrEmpty(AllowedInstallments) || AllowedInstallments == "0")
		{
			return true;
		}
		if (!AllowedInstallments.Contains('['))
		{
			return month <= Convert.ToInt32(AllowedInstallments);
		}
		JsonSerializerOptions options = new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true
		};
		return JsonSerializer.Deserialize<List<AllowedInstallment>>(AllowedInstallments, options).Any((AllowedInstallment x) => x.Value == month);
	}
}
