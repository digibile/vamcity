using System;
using System.ComponentModel.DataAnnotations;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Models;

public class OrganizationModel : BaseModel
{
	[Required(ErrorMessage = "نام سازمان الزامی است")]
	public string Name { get; set; }

	public Guid CustomerId { get; set; }

	public OrganizationModel()
	{
	}

	public OrganizationModel(Organization organization)
	{
		Id = organization.Id;
		Name = organization.Name;
		CustomerId = organization.CustomerId;
	}
}
