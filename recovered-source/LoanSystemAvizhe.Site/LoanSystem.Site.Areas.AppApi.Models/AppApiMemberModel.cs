using System.ComponentModel.DataAnnotations;
using LoanSystem.Data.Entities;

namespace LoanSystem.Site.Areas.AppApi.Models;

public class AppApiMemberModel
{
    public long Id { get; set; }

    public string UserName { get; set; }

    [Required(ErrorMessage = "نام الزامی است.")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "نام خانوادگی الزامی است.")]
    public string LastName { get; set; }

    public string FatherName { get; set; }

    [Required(ErrorMessage = "کد ملی الزامی است.")]
    public string NationalCode { get; set; }

    [Required(ErrorMessage = "موبایل الزامی است.")]
    public string Mobile { get; set; }

    public string Address { get; set; }

    public string PostalCode { get; set; }

    public bool IsActive { get; set; }

    public string Password { get; set; }

    public string EncryptedPassword { get; set; }

    public string Email { get; set; }

    public string City { get; set; }

    public AppApiMemberModel()
    {
    }

    public AppApiMemberModel(User user)
    {
        Id = user.Id;
        UserName = user.UserName;
        FirstName = user.FirstName;
        LastName = user.LastName;
        NationalCode = user.NationalCode;
        Mobile = user.Mobile;
        Address = user.Address;
        PostalCode = user.PostalCode;
        FatherName = user.FatherName;
    }
}
