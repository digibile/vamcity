using System;
using System.ComponentModel.DataAnnotations;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Models;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystemAvizhe.Site.Areas.AppApi.Models;

public class AppApiLoanPaymentModel : BaseModel
{
	public int Code { get; set; }

	public int InstallmentNumber { get; set; }

	public decimal InstallmentAmount { get; set; }

	[Range(0.0, double.MaxValue, ErrorMessage = "مبلغ نمی تواند کمتر از صفر باشد")]
	public decimal Amount { get; set; }

	public int DelayDay { get; set; }

	public decimal DelayPrice { get; set; }

	public string SavedBy { get; set; }

	public DateTime SaveDate { get; set; }

	public DateTime TargetMonth { get; set; }

	public PaymentMethods PaymentMethod { get; set; }

	public double GoldWeight { get; set; }

	public double GoldRate { get; set; }

	public bool IsGoldCheckouted { get; set; }

	public string Description { get; set; }

	public bool SendSms { get; set; }

	public AppApiLoanPaymentModel()
	{
	}

	public AppApiLoanPaymentModel(UserLoanPayment userLoanPayment)
	{
		Id = userLoanPayment.Id;
		Code = userLoanPayment.Code;
		Amount = userLoanPayment.SumAmount;
		SavedBy = userLoanPayment.SavedBy;
		SaveDate = userLoanPayment.SaveDate;
		TargetMonth = userLoanPayment.TargetMonth;
		InstallmentAmount = userLoanPayment.InstallmentAmount;
		InstallmentNumber = userLoanPayment.InstallmentNumber;
		DelayDay = userLoanPayment.DelayDay;
		DelayPrice = userLoanPayment.DelayPrice;
		PaymentMethod = userLoanPayment.PaymentMethod;
		GoldRate = userLoanPayment.GoldRate;
		GoldWeight = userLoanPayment.GoldWeight;
		Description = userLoanPayment.Description;
		IsGoldCheckouted = userLoanPayment.IsGoldCheckouted;
	}

	public AppApiLoanPaymentModel(LoanPaymentModel loanPaymentModel)
	{
		Id = loanPaymentModel.Id;
		Code = loanPaymentModel.Code;
		Amount = loanPaymentModel.SumAmount;
		SavedBy = loanPaymentModel.SavedBy;
		SaveDate = loanPaymentModel.SaveDate;
		TargetMonth = loanPaymentModel.TargetMonth;
		InstallmentAmount = loanPaymentModel.InstallmentAmount;
		InstallmentNumber = loanPaymentModel.InstallmentNumber;
		DelayDay = loanPaymentModel.DelayDay;
		DelayPrice = loanPaymentModel.DelayPrice;
		PaymentMethod = loanPaymentModel.PaymentMethod;
		GoldRate = loanPaymentModel.GoldRate;
		GoldWeight = loanPaymentModel.GoldWeight;
		Description = loanPaymentModel.Description;
		IsGoldCheckouted = loanPaymentModel.IsGoldCheckouted;
	}
}
