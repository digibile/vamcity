using System;
using System.ComponentModel.DataAnnotations;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Models;

public class SiteContentModel : BaseModel
{
    [Required(ErrorMessage = "کلید الزامی است.")]
    [StringLength(100, MinimumLength = 0, ErrorMessage = "طول کاراکترهای کلید نباید بیشتر از 100 باشد")]
    public string Key { get; set; }

    [Required(ErrorMessage = "محتوا الزامی است.")]
    public string Content { get; set; }

    public bool IsJson { get; set; }

    public Guid CustomerId { get; set; }

    public SiteContentModel()
    {
    }

    public SiteContentModel(SiteContent siteContent)
    {
        Id = siteContent.Id;
        Key = siteContent.Key;
        Content = siteContent.Content;
        IsJson = siteContent.IsJson;
    }
}
