using System;
using System.ComponentModel.DataAnnotations;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Models;

public class ContactModel : BaseModel
{
	[Required(ErrorMessage = "نام الزامی است.")]
	[StringLength(100, MinimumLength = 0, ErrorMessage = "طول کاراکترهای نام نباید بیشتر از 100 باشد")]
	public string FullName { get; set; }

	[Required(ErrorMessage = "نام تجاری الزامی است.")]
	[StringLength(100, MinimumLength = 0, ErrorMessage = "طول کاراکترهای نام تجاری نباید بیشتر از 100 باشد")]
	public string BusinessName { get; set; }

	public string Phone { get; set; }

	public string Address { get; set; }

	public string BankInfo { get; set; }

	public decimal TotalShare { get; set; }

	public decimal LoanCount { get; set; }

	public decimal TotalReceives { get; set; }

	public Guid CustomerId { get; set; }

	public ContactModel()
	{
	}

	public ContactModel(Contact contact)
	{
		Id = contact.Id;
		FullName = contact.FullName;
		BusinessName = contact.BusinessName;
		Phone = contact.Phone;
		Address = contact.Address;
		BankInfo = contact.BankInfo;
		CustomerId = contact.CustomerId;
	}
}
