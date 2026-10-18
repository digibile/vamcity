using System;
using System.ComponentModel.DataAnnotations;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Models;

public class AssuranceRequirementItemModel : BaseModel
{
    [Required(ErrorMessage = "عنوان الزامی است.")]
    [StringLength(100, MinimumLength = 0, ErrorMessage = "طول کاراکترهای عنوان نباید بیشتر از 100 باشد")]
    public string Title { get; set; }

    public int Order { get; set; }

    public Guid AssuranceRequirementId { get; set; }

    public Guid CustomerId { get; set; }

    public AssuranceRequirementItemModel()
    {
    }

    public AssuranceRequirementItemModel(AssuranceRequirementItem assuranceRequirementItem)
    {
        Id = assuranceRequirementItem.Id;
        Title = assuranceRequirementItem.Title;
        Order = assuranceRequirementItem.Order;
        AssuranceRequirementId = assuranceRequirementItem.AssuranceRequirementId;
    }
}
