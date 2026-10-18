using System;
using LoanSystem.Data.Entities;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Services.Models;

public class TagModel : BaseModel
{
    public string Title { get; set; }

    public TagCategory TagCategory { get; set; }

    public bool IsPublic { get; set; }

    public Guid CustomerId { get; set; }

    public TagModel()
    {
    }

    public TagModel(Tag tag)
    {
        Id = tag.Id;
        Title = tag.Title;
        TagCategory = tag.TagCategory;
        IsPublic = tag.IsPublic;
        CustomerId = tag.CustomerId;
    }
}
