using System;
using System.Text.Json;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Models.Digibile;
using LoanSystem.Services.Models.Landa;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Services.Models;

public class ThirdPartySettingsModel : BaseModel
{
	public ThirdPart DefaultThirdParty { get; set; }

	public LandaSettings LandaSettings { get; set; } = new LandaSettings();

	public DigibileSettings DigibileSettings { get; set; } = new DigibileSettings();

	public Guid CustomerId { get; set; }

	public ThirdPartySettingsModel()
	{
	}

	public ThirdPartySettingsModel(ThirdPartySettings settings)
	{
		if (settings != null)
		{
			Id = settings.Id;
			DefaultThirdParty = settings.DefaultThirdParty;
			CustomerId = settings.CustomerId;
			if (!string.IsNullOrEmpty(settings.LandaSettingsJson))
			{
				LandaSettings = JsonSerializer.Deserialize<LandaSettings>(settings.LandaSettingsJson);
			}
			if (!string.IsNullOrEmpty(settings.DigibileSettingsJson))
			{
				DigibileSettings = JsonSerializer.Deserialize<DigibileSettings>(settings.DigibileSettingsJson);
			}
		}
	}
}
