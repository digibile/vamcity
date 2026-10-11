using System.Collections.Generic;
using LoanSystem.Core;

namespace LoanSystem.Services.Models;

public class AdminDashboardModel
{
	public List<dynamic> LoanChart { get; set; }

	public List<dynamic> PaymentChart { get; set; }

	public int MemberCount { get; set; }

	public decimal Balance { get; set; }

	public decimal TotalLoans { get; set; }

	public decimal TotalProfitLoans { get; set; }

	public decimal TotalProfitDelayLoans { get; set; }

	public decimal TotalUnpaid { get; set; }

	public decimal TotalNonDueDated { get; set; }

	public decimal TotalPaid { get; set; }

	public decimal TotalSave { get; set; }

	public decimal MonthSaveds { get; set; }

	public decimal MonthSavedsCounts { get; set; }

	public decimal MonthInstallments { get; set; }

	public decimal MonthInstallmentsCounts { get; set; }

	public decimal MonthLoans { get; set; }

	public decimal MonthBalance { get; set; }

	public decimal CostShare { get; set; }

	public decimal Expense { get; set; }

	public int CheckoutedThisMonthCount { get; set; }

	public int AllCheckoutedCount { get; set; }

	public int AllLoansCount { get; set; }

	public int NewLoanRequestCount { get; set; }

	public int TodayLoanRequestCount { get; set; }

	public int NewReviewLoanRequestCount { get; set; }

	public string NewLoanRequestTitle { get; set; }

	public string NewReviewLoanRequestTitle { get; set; }

	public dynamic AgentOverdue { get; set; }

	public dynamic ContactOverdue { get; set; }

	public PersianDate LastMonthPersianDate { get; set; }

	public OverDueInstallmentsModel OverDueInstallments { get; set; }
}
