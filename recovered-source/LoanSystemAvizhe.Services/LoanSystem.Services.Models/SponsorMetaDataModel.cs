using System;
using System.ComponentModel.DataAnnotations;
using LoanSystem.Data.Entities;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Services.Models;

public class SponsorMetaDataModel : BaseModel
{
	public AgentType SponsorType { get; set; }

	public Guid CustomerId { get; set; }

	[MaxLength(100, ErrorMessage = "حداکثر طول آدرس وب سایت 100 کاراکتر باید باشد.")]
	public string SiteUrl { get; set; }

	public bool IsOnline { get; set; }

	public bool IsPhysical { get; set; }

	public bool CanConvertRequestToLoan { get; set; }

	public SponsorMetaDataModel()
	{
	}

	public SponsorMetaDataModel(AgentMetaData metaData)
	{
		Id = metaData.Id;
		SiteUrl = metaData.SiteUrl;
		IsOnline = metaData.IsOnline;
		IsPhysical = metaData.IsPhysical;
		CustomerId = metaData.CustomerId;
		SponsorType = metaData.AgentType;
		CanConvertRequestToLoan = metaData.CanConvertRequestToLoan;
	}
}
