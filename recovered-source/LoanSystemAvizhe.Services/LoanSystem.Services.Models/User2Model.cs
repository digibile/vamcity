using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Models;

public class User2Model
{
	public long Id { get; set; }

	public string UserCode { get; set; }

	public string FirstName { get; set; }

	public string LastName { get; set; }

	public string UserName { get; set; }

	public string Mobile { get; set; }

	public string BusinessName { get; set; }

	public string Email { get; set; }

	public bool IsSuperAdmin { get; set; }

	public CustomerModel Customer { get; set; }

	public string FullName => FirstName + " " + LastName;

	public User2Model()
	{
	}

	public User2Model(User user)
	{
		Id = user.Id;
		UserCode = user.UserCode;
		FirstName = user.FirstName;
		LastName = user.LastName;
		Mobile = user.Mobile;
		Email = user.Email;
		UserName = user.UserName;
		BusinessName = user.BusinessName;
		if (user.Customer != null)
		{
			Customer = new CustomerModel
			{
				Id = user.Customer.Id,
				Name = user.Customer.Name,
				ExpireDate = user.Customer.ExpireDate,
				Purchased = user.Customer.Purchased,
				Permanent = user.Customer.Permanent
			};
		}
	}
}
