using System;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Data.Entities;

public class AppEmail : BaseEntity
{
	public DateTime Date { get; set; }

	public string Text { get; set; }

	public AppSmsType AppEmailType { get; set; }

	public string To { get; set; }

	public string PatternTitle { get; set; }

	public EmailPanel EmailPanel { get; set; }

	public Guid EntityId { get; set; }

	public string EntityType { get; set; }

	public string Tag { get; set; }

	public Guid CustomerId { get; set; }

	public virtual Customer Customer { get; set; }
}
