using System;
using LoanSystem.Core;

namespace LoanSystem.Services.Models;

public class SavedMoneyByMonthModel : BaseModel
{
	public string MemberFullName { get; set; }

	public string MemberPersonalCode { get; set; }

	public decimal Amount { get; set; }

	public DateTime TargetMonth { get; set; }

	public PersianDate TargetMonth_Persian => TargetMonth.ToPersianDateObject();
}
