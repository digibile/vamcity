using System;
using LoanSystem.Core;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Models;

public class UserSavedMoneyModel : BaseModel
{
	public long MemberId { get; set; }

	public string Description { get; set; }

	public decimal Amount { get; set; }

	public string SavedBy { get; set; }

	public DateTime SaveDate { get; set; }

	public string SaveDate_Persian { get; set; }

	public DateTime TargetMonth { get; set; }

	public PersianDate TargetMonth_Persian => TargetMonth.ToPersianDateObject();

	public UserSavedMoneyModel()
	{
	}

	public UserSavedMoneyModel(UserSavedMoney userSavedMoney)
	{
		Id = userSavedMoney.Id;
		MemberId = userSavedMoney.MemberId;
		Amount = userSavedMoney.Amount;
		SavedBy = userSavedMoney.SavedBy;
		SaveDate = userSavedMoney.SaveDate;
		SaveDate_Persian = userSavedMoney.SaveDate.ToPersianDate();
		Description = userSavedMoney.Description;
		TargetMonth = userSavedMoney.TargetMonth;
	}
}
