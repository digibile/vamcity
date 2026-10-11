using System;
using System.Collections.Generic;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Data.Entities;

public class Tag : BaseEntity
{
	public string Title { get; set; }

	public TagCategory TagCategory { get; set; }

	public bool IsPublic { get; set; }

	public Guid CustomerId { get; set; }

	public virtual Customer Customer { get; set; }

	public virtual ICollection<LoanTag> LoanTags { get; set; } = new HashSet<LoanTag>();
}
