using System;

namespace LoanSystem.Data.Entities;

public class ThirdPartyTransfer : BaseEntity
{
	public string EntityId { get; set; }

	public string EntityType { get; set; }

	public string LandaJson { get; set; }

	public string DigibileJson { get; set; }

	public string NikanJson { get; set; }

	public string EntityTag { get; set; }

	public Guid CustomerId { get; set; }

	public virtual Customer Customer { get; set; }
}
