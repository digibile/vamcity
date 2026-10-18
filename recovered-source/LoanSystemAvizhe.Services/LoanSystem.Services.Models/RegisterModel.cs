using System.ComponentModel.DataAnnotations;

namespace LoanSystem.Services.Models;

public class RegisterModel : BaseModel
{
    [Required(ErrorMessage = "نام الزامی است.")]
    public string Name { get; set; }

    [Required(ErrorMessage = "شماره موبایل الزامی است.")]
    public string Mobile { get; set; }

    public bool SendSms { get; set; }

    public string AffCode { get; set; }
}
