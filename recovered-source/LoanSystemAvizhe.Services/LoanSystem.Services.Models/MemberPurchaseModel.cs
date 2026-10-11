using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using LoanSystem.Core;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Models.IPGParsian;
using LoanSystem.Services.Models.NextPay;
using LoanSystem.Services.Models.Sep;
using LoanSystem.Services.Models.Sepehr;
using LoanSystem.Services.Models.ZarinPal;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Services.Models;

public class MemberPurchaseModel : BaseModel
{
	public int Code { get; set; }

	public PaymentSteps PaymentStep { get; set; }

	public OnlinePaymentGateways OnlinePaymentGateway { get; set; }

	public string OnlinePaymentGatewayText => OnlinePaymentGateway switch
	{
		OnlinePaymentGateways.None => "نامشخص", 
		OnlinePaymentGateways.ZarinPal => "زرین پال", 
		OnlinePaymentGateways.JetAqsaat => "جت اقساط", 
		OnlinePaymentGateways.Sep => "بانک سامان", 
		OnlinePaymentGateways.NextPay => "نکست پی", 
		OnlinePaymentGateways.IPGParsian => "بانک پارسیان", 
		OnlinePaymentGateways.Sadad => "بانک ملی (سداد)", 
		OnlinePaymentGateways.Sepehr => "بانک صادرات (سپهر)", 
		_ => "نامشخص", 
	};

	public string TransactionCode
	{
		get
		{
			switch (OnlinePaymentGateway)
			{
			case OnlinePaymentGateways.None:
				return "نامشخص";
			case OnlinePaymentGateways.ZarinPal:
				if (ZarinPalPurchase != null)
				{
					return ZarinPalPurchase.RefId;
				}
				return "";
			case OnlinePaymentGateways.JetAqsaat:
				return Code.ToString();
			case OnlinePaymentGateways.Sep:
				if (SepPurchase != null)
				{
					return SepPurchase.RefNum;
				}
				return "";
			case OnlinePaymentGateways.NextPay:
				if (NextPayPurchase != null)
				{
					return NextPayPurchase.TransId;
				}
				return "";
			case OnlinePaymentGateways.IPGParsian:
				if (IPGParsianPurchase != null)
				{
					return IPGParsianPurchase.RRN.ToString();
				}
				return "";
			case OnlinePaymentGateways.Sadad:
				if (IPGParsianPurchase != null)
				{
					return SadadPurchase.RetrivalRefNo;
				}
				return "";
			default:
				return "نامشخص";
			}
		}
	}

	public User2Model User { get; set; }

	public User2Model CreatorUser { get; set; }

	public LoanPaymentModel UserLoanPayment { get; set; }

	public AgentFactorModel AgentFactor { get; set; }

	[Range(0.0, double.MaxValue, ErrorMessage = "مبلغ نمی تواند کمتر از صفر باشد")]
	public decimal Amount { get; set; }

	public string Description { get; set; }

	public string ZarinPalPurchaseJson { get; set; }

	public ZarinPalPurchase ZarinPalPurchase { get; set; }

	public string SepPurchaseJson { get; set; }

	public SepPurchase SepPurchase { get; set; }

	public string SepehrPurchaseJson { get; set; }

	public SepehrPurchase SepehrPurchase { get; set; }

	public string NextPayPurchaseJson { get; set; }

	public NextPayPurchase NextPayPurchase { get; set; }

	public string IPGParsianPurchaseJson { get; set; }

	public IPGParsianPurchase IPGParsianPurchase { get; set; }

	public string SadadPurchaseJson { get; set; }

	public SadadPurchase SadadPurchase { get; set; }

	public Guid CustomerId { get; set; }

	public bool IsPaymentSuccess { get; set; }

	public DateTime StartDate { get; set; }

	public PersianDate StartDate_Persian => StartDate.ToPersianDateObject();

	public int CustomerCode { get; set; }

	public MemberPurchaseTypes MemberPurchaseType { get; set; }

	public MemberPurchaseModel()
	{
	}

	public MemberPurchaseModel(MemberPurchase memberPurchase)
	{
		Id = memberPurchase.Id;
		Code = memberPurchase.Code;
		Amount = memberPurchase.Amount;
		OnlinePaymentGateway = memberPurchase.OnlinePaymentGateway;
		PaymentStep = memberPurchase.PaymentStep;
		if (memberPurchase.User != null)
		{
			User = new User2Model
			{
				Id = memberPurchase.UserId,
				FirstName = memberPurchase.User.FirstName,
				LastName = memberPurchase.User.LastName,
				Mobile = memberPurchase.User.Mobile,
				UserName = memberPurchase.User.UserName
			};
		}
		if (memberPurchase.CreatorUser != null)
		{
			User = new User2Model
			{
				Id = memberPurchase.CreatorUserId.Value
			};
		}
		Description = memberPurchase.Description;
		ZarinPalPurchaseJson = memberPurchase.ZarinPalJson;
		if (!string.IsNullOrEmpty(memberPurchase.ZarinPalJson))
		{
			ZarinPalPurchase = JsonSerializer.Deserialize<ZarinPalPurchase>(memberPurchase.ZarinPalJson);
		}
		SepPurchaseJson = memberPurchase.SepJson;
		if (!string.IsNullOrEmpty(memberPurchase.SepJson))
		{
			SepPurchase = JsonSerializer.Deserialize<SepPurchase>(memberPurchase.SepJson);
		}
		SepehrPurchaseJson = memberPurchase.SepehrJson;
		if (!string.IsNullOrEmpty(memberPurchase.SepehrJson))
		{
			SepehrPurchase = JsonSerializer.Deserialize<SepehrPurchase>(memberPurchase.SepehrJson);
		}
		NextPayPurchaseJson = memberPurchase.NextPayJson;
		if (!string.IsNullOrEmpty(memberPurchase.NextPayJson))
		{
			NextPayPurchase = JsonSerializer.Deserialize<NextPayPurchase>(memberPurchase.NextPayJson);
		}
		IPGParsianPurchaseJson = memberPurchase.IPGParsianJson;
		if (!string.IsNullOrEmpty(memberPurchase.IPGParsianJson))
		{
			IPGParsianPurchase = JsonSerializer.Deserialize<IPGParsianPurchase>(memberPurchase.IPGParsianJson);
		}
		SadadPurchaseJson = memberPurchase.SadadJson;
		if (!string.IsNullOrEmpty(memberPurchase.SadadJson))
		{
			SadadPurchase = JsonSerializer.Deserialize<SadadPurchase>(memberPurchase.SadadJson);
		}
		IsPaymentSuccess = memberPurchase.IsPaymentSuccess;
		StartDate = memberPurchase.StartDate;
		CustomerId = memberPurchase.CustomerId;
		MemberPurchaseType = memberPurchase.MemberPurchaseType;
	}
}
