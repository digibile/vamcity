using System;
using System.ComponentModel.DataAnnotations;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Models;

public class UserLoanRequestProductModel : BaseModel
{
    [Range(0.0, double.MaxValue, ErrorMessage = "مبلغ واحد هر کالا نمی تواند کمتر از 0 باشد")]
    public decimal Price { get; set; }

    [Range(0.0, double.MaxValue, ErrorMessage = "تعداد کالا نمی تواند کمتر از 0 باشد")]
    public decimal Count { get; set; }

    public ProductModel Product { get; set; }

    public Guid CustomerId { get; set; }

    public UserLoanRequestProductModel()
    {
    }

    public UserLoanRequestProductModel(UserLoanRequestProduct userLoanRequestProduct)
    {
        Id = userLoanRequestProduct.Id;
        Price = userLoanRequestProduct.Price;
        Count = userLoanRequestProduct.Count;
        if (userLoanRequestProduct.Product != null)
        {
            Product = new ProductModel(userLoanRequestProduct.Product);
        }
        CustomerId = userLoanRequestProduct.CustomerId;
    }
}
