using System;
using LoanSystem.Core;
using LoanSystem.Data.Entities;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Site.Areas.Admin.Models;

public class DashboardTransactionsViewModel
{
	public int Order { get; set; }

	public Guid? TransactionId { get; set; }

	public Guid? SuccessfullOnlineMoneyTransferId { get; set; }

	public long? MemberId { get; set; }

	public string Member { get; set; }

	public string Loan { get; set; }

	public decimal Debit { get; set; }

	public decimal Credit { get; set; }

	public decimal Balance { get; set; }

	public DateTime SaveDate { get; set; }

	public PaymentMethods PaymentMethod { get; set; }

	public bool IsWithdraw { get; set; }

	public bool IsUnknown { get; set; }

	public double GoldWeightBalance { get; set; }

	public double GoldWeight { get; set; }

	public double GoldRate { get; set; }

	public string Description { get; set; }

	public string SavedBy { get; set; }

	public PersianDate SaveDate_Persian => SaveDate.ToPersianDateObject();

	public DashboardTransactionsViewModel(Transaction transaction)
	{
		TransactionId = transaction.Id;
		SuccessfullOnlineMoneyTransferId = transaction.SuccessfullOnlineMoneyTransferId;
		Member = "";
		Loan = "";
		Debit = ((!transaction.IsWithdraw) ? transaction.Amount : 0m);
		Credit = (transaction.IsWithdraw ? transaction.Amount : 0m);
		SaveDate = transaction.SaveDate;
		PaymentMethod = transaction.PaymentMethod;
		IsWithdraw = transaction.IsWithdraw;
		IsUnknown = transaction.IsUnknown;
		GoldRate = transaction.GoldRate;
		GoldWeight = transaction.GoldWeight;
		SavedBy = transaction.SavedBy;
		Description = (transaction.IsWithdraw ? "برداشت" : "واریز") + " توسط " + SavedBy + "<br/>" + transaction.Description;
	}

	public DashboardTransactionsViewModel(UserLoanPayment userLoanPayment)
	{
		MemberId = userLoanPayment?.UserLoan?.User?.Id;
		Member = userLoanPayment?.UserLoan?.User?.FullName;
		Loan = userLoanPayment?.UserLoan?.Loan?.Title;
		Debit = userLoanPayment.Amount;
		SaveDate = userLoanPayment.SaveDate;
		PaymentMethod = userLoanPayment.PaymentMethod;
		IsWithdraw = false;
		IsUnknown = false;
		GoldRate = userLoanPayment.GoldRate;
		GoldWeight = userLoanPayment.GoldWeight;
		SavedBy = userLoanPayment.SavedBy;
		Description = $"واریز قسط توسط <span class=\"grey--text\" style=\"cursor:pointer\">[{Member}]</span> بابت وام شماره [{userLoanPayment?.UserLoan?.Code}], قسط شماره [{userLoanPayment.InstallmentNumber}]<br/>{userLoanPayment.Description}";
	}

	public DashboardTransactionsViewModel(UserLoanPaymentOther userLoanPaymentOther)
	{
		MemberId = userLoanPaymentOther?.UserLoanPayment?.UserLoan?.User?.Id;
		Member = userLoanPaymentOther?.UserLoanPayment?.UserLoan?.User?.FullName;
		Loan = userLoanPaymentOther?.UserLoanPayment?.UserLoan?.Loan?.Title;
		Debit = userLoanPaymentOther.Amount;
		SaveDate = userLoanPaymentOther.SaveDate;
		PaymentMethod = userLoanPaymentOther.PaymentMethod;
		IsWithdraw = false;
		IsUnknown = false;
		GoldRate = userLoanPaymentOther.GoldRate;
		GoldWeight = userLoanPaymentOther.GoldWeight;
		SavedBy = userLoanPaymentOther.SavedBy;
		Description = $"واریز قسط توسط <span class=\"grey--text\" style=\"cursor:pointer\">[{Member}]</span> بابت وام شماره [{userLoanPaymentOther?.UserLoanPayment?.UserLoan?.Code}], قسط شماره [{userLoanPaymentOther.UserLoanPayment?.InstallmentNumber}]<br/>{userLoanPaymentOther.Description}";
	}
}
