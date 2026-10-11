using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Models;

namespace LoanSystemAvizhe.Site.Areas.AppApi.Models;

public class AppApiUserLoanModel : BaseModel
{
	public int Code { get; set; }

	[Required]
	public Guid LoanId { get; set; }

	[Required]
	public long MemberId { get; set; }

	public DateTime? DepositChargeDueDate { get; set; }

	[Range(0.0, double.MaxValue, ErrorMessage = "مبلغ پیش پرداخت نمی تواند کمتر از صفر باشد")]
	public decimal PreDepositAmount { get; set; }

	public DateTime StartDate { get; set; }

	public decimal PayableAmount { get; set; }

	[Range(0.0, double.MaxValue, ErrorMessage = "مبلغ وام نمی تواند کمتر از صفر باشد")]
	public decimal Amount { get; set; }

	[Range(0.0, double.MaxValue, ErrorMessage = "مبلغ قسط نمی تواند کمتر از صفر باشد")]
	public decimal Installment { get; set; }

	public int Month { get; set; }

	public int MonthGap { get; set; } = 1;

	public decimal? CustomProfitPercent { get; set; }

	public decimal Paid { get; set; }

	public decimal Remain
	{
		get
		{
			if (IsCheckouted.HasValue && IsCheckouted.Value)
			{
				return 0m;
			}
			return PayableAmount + Delays - Paid;
		}
	}

	public decimal PaidCount { get; set; }

	public bool? IsCheckouted { get; set; }

	public DateTime? CheckoutDate { get; set; }

	public string Description { get; set; }

	public string Status { get; set; }

	public List<AppApiLoanPaymentModel> LoanPayments { get; set; }

	public decimal Delays
	{
		get
		{
			if (IsCheckouted.HasValue && IsCheckouted.Value)
			{
				return Paid - PayableAmount;
			}
			if (LoanPayments == null)
			{
				return 0m;
			}
			return Math.Round(LoanPayments.Sum((AppApiLoanPaymentModel x) => x.DelayPrice * (decimal)((x.DelayDay >= 0) ? 1 : (-1))));
		}
	}

	public AppApiUserLoanModel()
	{
	}

	public AppApiUserLoanModel(UserLoan userLoan)
	{
		Id = userLoan.Id;
		Code = userLoan.Code;
		StartDate = userLoan.StartDate;
		PayableAmount = userLoan.PayableAmount;
		Amount = userLoan.Amount;
		Installment = userLoan.Installment;
		Month = userLoan.Month;
		MonthGap = userLoan.MonthGap;
		Paid = userLoan.Paid;
		IsCheckouted = userLoan.IsCheckouted;
		PreDepositAmount = userLoan.PreDepositAmount;
		CheckoutDate = userLoan.CheckoutDate;
		DepositChargeDueDate = userLoan.DepositChargeDueDate;
		Description = userLoan.Description;
		Status = userLoan.Status;
	}

	public AppApiUserLoanModel(UserLoanModel userLoan)
	{
		Id = userLoan.Id;
		Code = userLoan.Code;
		StartDate = userLoan.StartDate;
		PayableAmount = userLoan.PayableAmount;
		Amount = userLoan.Amount;
		Installment = userLoan.Installment;
		Month = userLoan.Month;
		MonthGap = userLoan.MonthGap;
		Paid = userLoan.Paid;
		IsCheckouted = userLoan.IsCheckouted;
		PreDepositAmount = userLoan.PreDepositAmount;
		CheckoutDate = userLoan.CheckoutDate;
		DepositChargeDueDate = userLoan.DepositChargeDueDate;
		Description = userLoan.Description;
		Status = userLoan.Status;
		CustomProfitPercent = userLoan.CustomProfitPercent;
		List<LoanPaymentModel> loanPayments = userLoan.LoanPayments;
		if (loanPayments != null && loanPayments.Any())
		{
			LoanPayments = userLoan.LoanPayments.Select((LoanPaymentModel x) => new AppApiLoanPaymentModel(x)).ToList();
		}
		if (userLoan.LoanModel != null)
		{
			LoanId = userLoan.LoanModel.Id;
		}
		if (userLoan.MemberModel != null)
		{
			MemberId = userLoan.MemberModel.Id;
		}
	}

	public decimal CalcPayableAmount()
	{
		return (decimal)Month * Installment;
	}
}
