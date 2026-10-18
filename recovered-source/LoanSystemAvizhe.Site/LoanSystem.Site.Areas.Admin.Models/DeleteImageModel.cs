using System.ComponentModel.DataAnnotations;

namespace LoanSystem.Site.Areas.Admin.Models;

public class DeleteImageModel
{
    [Required(ErrorMessage = "نام فایل تصویر الزامی است.")]
    public string FileName { get; set; }
}
