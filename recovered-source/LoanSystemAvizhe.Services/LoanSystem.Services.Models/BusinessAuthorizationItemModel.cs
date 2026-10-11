using System.ComponentModel.DataAnnotations;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Models;

public class BusinessAuthorizationItemModel : BaseModel
{
	[Required(ErrorMessage = "نوع مدرک الزامی است.")]
	public AuthorizationItemModel AuthorizationItem { get; set; }

	public decimal Cost { get; set; }

	public CustomerModel Customer { get; set; }

	public BusinessAuthorizationItemModel()
	{
	}

	public BusinessAuthorizationItemModel(BusinessAuthorizationItem businessAuthorizationItemModel)
	{
		Id = businessAuthorizationItemModel.Id;
		Cost = businessAuthorizationItemModel.Cost;
		if (businessAuthorizationItemModel.AuthorizationItem != null)
		{
			AuthorizationItem = new AuthorizationItemModel(businessAuthorizationItemModel.AuthorizationItem);
		}
		if (businessAuthorizationItemModel.Customer != null)
		{
			Customer = new CustomerModel(businessAuthorizationItemModel.Customer);
		}
	}
}
