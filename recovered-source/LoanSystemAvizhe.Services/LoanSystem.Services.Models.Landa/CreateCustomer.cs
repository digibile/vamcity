using System;
using System.Collections.Generic;

namespace LoanSystem.Services.Models.Landa;

public class CreateCustomer
{
	public string code { get; set; }

	public string comment { get; set; }

	public string aliasName { get; set; }

	public string name { get; set; }

	public int typeId { get; set; }

	public Guid unitId { get; set; }

	public Person person { get; set; }

	public object customerBusiness { get; set; }

	public List<object> customerAddresses { get; set; } = new List<object>();

	public List<object> customerContactEmails { get; set; } = new List<object>();

	public List<object> customerContactMobiles { get; set; } = new List<object>();

	public List<object> customerContactPhones { get; set; } = new List<object>();
}
