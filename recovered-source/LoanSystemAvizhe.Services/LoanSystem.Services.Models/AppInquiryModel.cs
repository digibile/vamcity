using System;
using System.Collections.Generic;
using System.Text.Json;
using LoanSystem.Core;
using LoanSystem.Data.Entities;
using LoanSystemAvizhe.Data.Enums;
using LoanSystemAvizhe.Services.Sms.Models.MelliPayamak;

namespace LoanSystem.Services.Models;

public class AppInquiryModel : BaseModel
{
	public DateTime Date { get; set; }

	public PersianDate Date_Persian => Date.ToPersianDateObject();

	public InquiryPanel InquiryPanel { get; set; }

	public string InquiryPanelText => InquiryPanel switch
	{
		InquiryPanel.Unknown => "نامعلوم", 
		InquiryPanel.JetAqsaat_ApiIR => "جت اقساط", 
		InquiryPanel.Dedicate_ApiIR => "api.ir", 
		_ => "نامعلوم", 
	};

	public AppInquiryType AppInquiryType { get; set; }

	public string AppInquiryTypeText => AppInquiryType.ToString();

	public decimal Price { get; set; }

	public decimal MemberPrice { get; set; }

	public bool IsSuccess { get; set; }

	public string PanelResultJson { get; set; }

	public string Result { get; set; }

	public string Key { get; set; }

	public string Tag { get; set; }

	public UserModel Member { get; set; }

	public CustomerModel Customer { get; set; }

	public AppInquiryModel()
	{
	}

	public AppInquiryModel(AppInquiry appInquiry)
	{
		Id = appInquiry.Id;
		Date = appInquiry.Date;
		InquiryPanel = appInquiry.InquiryPanel;
		Price = appInquiry.Price;
		MemberPrice = appInquiry.MemberPrice;
		AppInquiryType = appInquiry.AppInquiryType;
		IsSuccess = appInquiry.IsSuccess;
		PanelResultJson = appInquiry.PanelResultJson;
		Result = appInquiry.Result;
		Key = appInquiry.Key;
		Tag = appInquiry.Tag;
		if (appInquiry.Member != null)
		{
			Member = new UserModel(appInquiry.Member);
		}
		if (appInquiry.Customer != null)
		{
			Customer = new CustomerModel(appInquiry.Customer);
		}
	}

	public static AppInquiryType? MapAuthorizationItemSystemTypeToInquiryType(AuthorizationItemSystemType authorizationItemSystemType)
	{
		return new Dictionary<AuthorizationItemSystemType, AppInquiryType>
		{
			{
				AuthorizationItemSystemType.Shahkar1,
				AppInquiryType.شاهکار
			},
			{
				AuthorizationItemSystemType.Shahkar2,
				AppInquiryType.شاهکار3
			},
			{
				AuthorizationItemSystemType.Sana,
				AppInquiryType.ثنا
			},
			{
				AuthorizationItemSystemType.UnpaidCheque,
				AppInquiryType.چک_برگشتی
			},
			{
				AuthorizationItemSystemType.MilitaryStatus,
				AppInquiryType.خدمت_سربازی
			}
		}.GetValueOrDefault(authorizationItemSystemType);
	}

	public bool AcceptanceDetection()
	{
		if (string.IsNullOrEmpty(PanelResultJson))
		{
			return false;
		}
		bool result = false;
		try
		{
			switch (AppInquiryType)
			{
			case AppInquiryType.شاهکار:
				result = JsonSerializer.Deserialize<ShahkarResult>(PanelResultJson, new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				}).IsMatch;
				break;
			case AppInquiryType.شاهکار3:
				result = JsonSerializer.Deserialize<ShahkarResult>(PanelResultJson, new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				}).IsMatch;
				break;
			case AppInquiryType.ثنا:
				result = JsonSerializer.Deserialize<SanaResult>(PanelResultJson, new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				}).HasSana;
				break;
			case AppInquiryType.چک_برگشتی:
				result = JsonSerializer.Deserialize<UnpaidChequeResult>(PanelResultJson, new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				}).Count == 0;
				break;
			case AppInquiryType.مشخصات_هویتی:
				result = !string.IsNullOrWhiteSpace(JsonSerializer.Deserialize<PersonInfoResult>(PanelResultJson, new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				}).FatherName);
				break;
			case AppInquiryType.کدپستی:
				result = !string.IsNullOrWhiteSpace(JsonSerializer.Deserialize<PostalAddressResult>(PanelResultJson, new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				}).City);
				break;
			case AppInquiryType.پلاک_فعال:
				result = !string.IsNullOrWhiteSpace(JsonSerializer.Deserialize<ActivePlatesResult>(PanelResultJson, new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				}).Code);
				break;
			case AppInquiryType.مشخصات_شبا:
				result = !string.IsNullOrWhiteSpace(JsonSerializer.Deserialize<IbanInfoResult>(PanelResultJson, new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				}).Name);
				break;
			case AppInquiryType.خدمت_سربازی:
				result = JsonSerializer.Deserialize<MilitaryStatusResult>(PanelResultJson, new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				}).Qualified;
				break;
			case AppInquiryType.مطابقت_کدملی_کارت_بانکی:
				result = JsonSerializer.Deserialize<CardMatchResult>(PanelResultJson, new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				}).IsMatch;
				break;
			case AppInquiryType.تسیلات_فعال_بانکی:
				result = JsonSerializer.Deserialize<ActiveLoansResult>(PanelResultJson, new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				}).Count > 0;
				break;
			case AppInquiryType.رنگ_چک_صیادی:
				result = !string.IsNullOrWhiteSpace(JsonSerializer.Deserialize<ChequeColorResult>(PanelResultJson, new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				}).ChequeColor);
				break;
			case AppInquiryType.مشخصات_چک_صیادی:
				result = !string.IsNullOrWhiteSpace(JsonSerializer.Deserialize<ChequeInfoResult>(PanelResultJson, new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				}).SerialNumber);
				break;
			}
		}
		catch (Exception)
		{
		}
		return result;
	}
}
