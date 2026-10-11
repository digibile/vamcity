using System;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Data.Entities;

public class ThirdPartySettings : BaseEntity
{
	public ThirdPart DefaultThirdParty { get; set; }

	public string LandaSettingsJson { get; set; }

	public string DigibileSettingsJson { get; set; }

	public Guid CustomerId { get; set; }

	public virtual Customer Customer { get; set; }
}
