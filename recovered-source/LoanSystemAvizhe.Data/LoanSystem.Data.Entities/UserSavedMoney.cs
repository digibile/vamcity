using System;

namespace LoanSystem.Data.Entities;

public class UserSavedMoney : BaseEntity
{
	public long MemberId { get; set; }

	public virtual User Member { get; set; }

	public string Description { get; set; }

	public decimal Amount { get; set; }

	public string SavedBy { get; set; }

	public DateTime SaveDate { get; set; }

	public DateTime TargetMonth { get; set; }

	public string FileMonth { get; set; }
}
