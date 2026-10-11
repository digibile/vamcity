using System;
using System.Text.Json;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Models.Landa;
using LoanSystemAvizhe.Services.Models.Digibile;

namespace LoanSystem.Services.Models;

public class ThirdPartyTransferModel : BaseModel
{
	public string EntityId { get; set; }

	public string EntityType { get; set; }

	public string EntityTag { get; set; }

	public LandaTransferModel LandaTransfer { get; set; }

	public DigibileTransferModel DigibileTransfer { get; set; }

	public NikanTransferModel NikanTransfer { get; set; }

	public Guid CustomerId { get; set; }

	public ThirdPartyTransferModel()
	{
	}

	public ThirdPartyTransferModel(ThirdPartyTransfer thirdPartyTransfer)
	{
		Id = thirdPartyTransfer.Id;
		EntityId = thirdPartyTransfer.EntityId;
		EntityType = thirdPartyTransfer.EntityType;
		CustomerId = thirdPartyTransfer.CustomerId;
		EntityTag = thirdPartyTransfer.EntityTag;
		if (!string.IsNullOrEmpty(thirdPartyTransfer.LandaJson))
		{
			LandaTransfer = JsonSerializer.Deserialize<LandaTransferModel>(thirdPartyTransfer.LandaJson);
		}
		if (!string.IsNullOrEmpty(thirdPartyTransfer.DigibileJson))
		{
			DigibileTransfer = JsonSerializer.Deserialize<DigibileTransferModel>(thirdPartyTransfer.DigibileJson);
		}
		if (!string.IsNullOrEmpty(thirdPartyTransfer.NikanJson))
		{
			NikanTransfer = JsonSerializer.Deserialize<NikanTransferModel>(thirdPartyTransfer.NikanJson);
		}
	}
}
