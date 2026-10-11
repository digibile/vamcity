using System.Collections.Generic;
using LoanSystem.Core;

namespace LoanSystem.Services.Models;

public class MemberDashboardModel
{
	public List<dynamic> PaymentChart { get; set; }

	public List<dynamic> SavedMoneyChart { get; set; }

	public decimal MemberTotalLoans { get; set; }

	public decimal MemberTotalUnpaid { get; set; }

	public decimal MemberTotalPaid { get; set; }

	public decimal MemberTotalSave { get; set; }

	public decimal Balance { get; set; }

	public decimal TotalLoans { get; set; }

	public decimal TotalUnpaid { get; set; }

	public decimal TotalPaid { get; set; }

	public decimal TotalSave { get; set; }

	public decimal MonthSaveds { get; set; }

	public decimal MonthSavedsCounts { get; set; }

	public decimal MonthInstallments { get; set; }

	public decimal MonthInstallmentsCounts { get; set; }

	public decimal MonthBalance { get; set; }

	public decimal CostShare { get; set; }

	public decimal Expense { get; set; }

	public int CheckoutedThisMonthCount { get; set; }

	public int AllCheckoutedCount { get; set; }

	public int LoanRequestCount { get; set; }

	public int TicketsCount { get; set; }

	public PersianDate LastMonthPersianDate { get; set; }
}
