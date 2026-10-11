using System;
using LoanSystem.Core;

namespace LoanSystem.Services.Models;

public class LoanPaymentByMonthModel : BaseModel
{
	public string MemberFullName { get; set; }

	public string NationalCode { get; set; }

	public string LoanTitle { get; set; }

	public decimal Total { get; set; }

	public decimal Installment { get; set; }

	public decimal Remain { get; set; }

	public DateTime TargetMonth { get; set; }

	public PersianDate TargetMonth_Persian => TargetMonth.ToPersianDateObject();
}
