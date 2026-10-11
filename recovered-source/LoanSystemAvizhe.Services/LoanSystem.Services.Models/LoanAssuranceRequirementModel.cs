using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Models;

public class LoanAssuranceRequirementModel : BaseModel
{
	[Required(ErrorMessage = "وام الزامی است.")]
	public Guid LoanId { get; set; }

	public Dictionary<Guid, List<Guid?>> AssuranceRequirementItemIds { get; set; }

	public Guid CustomerId { get; set; }

	public LoanAssuranceRequirementModel()
	{
	}

	public LoanAssuranceRequirementModel(LoanAssuranceRequirement loanAssuranceRequirement)
	{
		Id = loanAssuranceRequirement.Id;
		if (!string.IsNullOrWhiteSpace(loanAssuranceRequirement.AssuranceRequirementsJson))
		{
			AssuranceRequirementItemIds = JsonSerializer.Deserialize<Dictionary<Guid, List<Guid?>>>(loanAssuranceRequirement.AssuranceRequirementsJson);
		}
		LoanId = loanAssuranceRequirement.LoanId;
		CustomerId = loanAssuranceRequirement.CustomerId;
	}
}
