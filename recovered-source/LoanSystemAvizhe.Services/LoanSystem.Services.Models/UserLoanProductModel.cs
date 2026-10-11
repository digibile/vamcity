using System;
using System.ComponentModel.DataAnnotations;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Models;

public class UserLoanProductModel : BaseModel
{
	[Range(0.0, double.MaxValue, ErrorMessage = "مبلغ واحد هر کالا نمی تواند کمتر از 0 باشد")]
	public decimal Price { get; set; }

	[Range(0.0, double.MaxValue, ErrorMessage = "تعداد کالا نمی تواند کمتر از 0 باشد")]
	public decimal Count { get; set; }

	public ProductModel Product { get; set; }

	public Guid CustomerId { get; set; }

	public UserLoanProductModel()
	{
	}

	public UserLoanProductModel(UserLoanProduct userLoanProduct)
	{
		Id = userLoanProduct.Id;
		Price = userLoanProduct.Price;
		Count = userLoanProduct.Count;
		if (userLoanProduct.Product != null)
		{
			Product = new ProductModel(userLoanProduct.Product);
		}
		CustomerId = userLoanProduct.CustomerId;
	}
}
