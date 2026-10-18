using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Models;

public class AssuranceRequirementModel : BaseModel
{
    [Required(ErrorMessage = "عنوان الزامی است.")]
    [StringLength(100, MinimumLength = 0, ErrorMessage = "طول کاراکترهای عنوان نباید بیشتر از 100 باشد")]
    public string Title { get; set; }

    public int Order { get; set; }

    public Guid CustomerId { get; set; }

    public List<AssuranceRequirementItemModel> AssuranceRequirementItems { get; set; }

    public AssuranceRequirementModel()
    {
    }

    public AssuranceRequirementModel(AssuranceRequirement assuranceRequirement)
    {
        Id = assuranceRequirement.Id;
        Title = assuranceRequirement.Title;
        Order = assuranceRequirement.Order;
        ICollection<AssuranceRequirementItem> assuranceRequirementItems = assuranceRequirement.AssuranceRequirementItems;
        if (assuranceRequirementItems != null && assuranceRequirementItems.Any())
        {
            AssuranceRequirementItems = assuranceRequirement.AssuranceRequirementItems.Select((AssuranceRequirementItem x) => new AssuranceRequirementItemModel(x)).ToList();
        }
    }
}
