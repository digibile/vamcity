using System;
using System.ComponentModel.DataAnnotations;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Models;

public class ProductModel : BaseModel
{
    [Required(ErrorMessage = "کد کالا الزامی است.")]
    public string Code { get; set; }

    [Required(ErrorMessage = "نام کالا الزامی است.")]
    public string Name { get; set; }

    public Guid CustomerId { get; set; }

    public ProductModel()
    {
    }

    public ProductModel(Product product)
    {
        Id = product.Id;
        Code = product.Code;
        Name = product.Name;
        CustomerId = product.CustomerId;
    }
}
