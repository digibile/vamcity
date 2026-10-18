using System.ComponentModel.DataAnnotations;
using LoanSystem.Data.Entities;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Services.Models;

public class AuthorizationItemModel : BaseModel
{
    [Required(ErrorMessage = "عنوان مدرک الزامی است.")]
    [StringLength(100, MinimumLength = 0, ErrorMessage = "طول کاراکترهای عنوان نباید بیشتر از 100 باشد")]
    public string Title { get; set; }

    public string Description { get; set; }

    public AuthorizationItemSystemType SystemType { get; set; }

    public bool UseIfAlreadyExist { get; set; }

    public CustomerModel Customer { get; set; }

    public AuthorizationItemModel()
    {
    }

    public AuthorizationItemModel(AuthorizationItem authorizationItem)
    {
        Id = authorizationItem.Id;
        Title = authorizationItem.Title;
        Description = authorizationItem.Description;
        SystemType = authorizationItem.SystemType;
        UseIfAlreadyExist = authorizationItem.UseIfAlreadyExist;
        if (authorizationItem.Customer != null)
        {
            Customer = new CustomerModel(authorizationItem.Customer);
        }
    }
}
