using System;
using System.ComponentModel.DataAnnotations;
using LoanSystem.Core;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Models;

public class CustomerOrderModel : BaseModel
{
	[Required(ErrorMessage = "نام الزامی است.")]
	[MaxLength(100, ErrorMessage = "طول کاراکتر نام و نام خانوادگی نباید بیش از 100 کاراکتر باشد.")]
	public string Name { get; set; }

	[Required(ErrorMessage = "تلفن تماس الزامی است.")]
	[MaxLength(20, ErrorMessage = "طول کاراکتر شماره تماس نباید بیش از 20 کاراکتر باشد.")]
	public string Phone { get; set; }

	[Required(ErrorMessage = "کد ملی الزامی است.")]
	[MaxLength(20, ErrorMessage = "طول کاراکتر کد ملی نباید بیش از 20 کاراکتر باشد.")]
	public string NationalCode { get; set; }

	[Required(ErrorMessage = "نام کالای مورد نظر الزامی است.")]
	[MaxLength(200, ErrorMessage = "طول کاراکتر کالای مورد نظر نباید بیش از 200 کاراکتر باشد.")]
	public string ProductName { get; set; }

	[MaxLength(1000, ErrorMessage = "طول کاراکتر شرح نباید بیش از 1000 کاراکتر باشد.")]
	public string Description { get; set; }

	public bool IsSeen { get; set; }

	[MaxLength(1000, ErrorMessage = "طول کاراکتر نتیجه تماس نباید بیش از 1000 کاراکتر باشد.")]
	public string CallResult { get; set; }

	public DateTime SaveDate { get; set; } = DateTime.Now;

	public PersianDate SaveDate_Persian => SaveDate.ToPersianDateObject();

	public int CustomerCode { get; set; }

	public CustomerOrderModel()
	{
	}

	public CustomerOrderModel(CustomerOrder customerOrder)
	{
		Id = customerOrder.Id;
		Name = customerOrder.Name;
		Phone = customerOrder.Phone;
		ProductName = customerOrder.ProductName;
		NationalCode = customerOrder.NationalCode;
		Description = customerOrder.Description;
		IsSeen = customerOrder.IsSeen;
		CallResult = customerOrder.CallResult;
		SaveDate = customerOrder.SaveDate;
	}
}
