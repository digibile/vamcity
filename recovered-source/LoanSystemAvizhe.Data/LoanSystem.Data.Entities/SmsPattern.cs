using System;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Data.Entities;

public class SmsPattern : BaseEntity
{
	public string Title { get; set; }

	public string Pattern { get; set; }

	public bool IsSystem { get; set; }

	public SmsPatternStatus Status { get; set; }

	public SmsPatternCategory Category { get; set; }

	public SmsPanel SmsPatternPanel { get; set; }

	public bool IsPatternBase { get; set; }

	public string PatternBaseCode { get; set; }

	public string RejectReason { get; set; }

	public string IppanelKeysJson { get; set; }

	public Guid CustomerId { get; set; }

	public virtual Customer Customer { get; set; }
}
