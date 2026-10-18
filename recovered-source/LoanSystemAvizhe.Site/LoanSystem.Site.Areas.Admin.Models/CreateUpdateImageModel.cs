using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace LoanSystem.Site.Areas.Admin.Models;

public class CreateUpdateImageModel
{
    [Required(ErrorMessage = "دیتای تصویر الزامی است.")]
    public IFormFile File { get; set; }

    public string FileName { get; set; }
}
