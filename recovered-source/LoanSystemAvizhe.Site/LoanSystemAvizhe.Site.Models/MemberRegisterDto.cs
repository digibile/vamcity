using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LoanSystemAvizhe.Site.Models;

public class MemberRegisterDto
{
    [StringLength(100, MinimumLength = 0, ErrorMessage = "طول کاراکترهای نام نباید بیشتر از {1} باشد")]
    public string FirstName { get; set; }

    [StringLength(100, MinimumLength = 0, ErrorMessage = "طول کاراکترهای نام خانوادگی نباید بیشتر از {1} باشد")]
    public string LastName { get; set; }

    [StringLength(11, MinimumLength = 0, ErrorMessage = "طول کاراکترهای موبایل نباید بیشتر از {1} باشد")]
    public string Mobile { get; set; }

    [StringLength(10, MinimumLength = 0, ErrorMessage = "طول کاراکترهای کد ملی نباید بیشتر از {1} باشد")]
    public string NationalCode { get; set; }

    [StringLength(100, ErrorMessage = "کلمه عبور باید حداقل {2} کاراکتر باشد.", MinimumLength = 8)]
    public string Password { get; set; }

    [Compare("Password", ErrorMessage = "تکرار کلمه عبور با کلمه عبور مطابقت ندارد.")]
    public string ConfirmPassword { get; set; }

    public string Email { get; set; }

    public string Address { get; set; }

    public string PostalCode { get; set; }

    public string City { get; set; }

    public string FatherName { get; set; }

    public DateTime? BirthDate { get; set; }

    public int CustomerCode { get; set; }

    public string MrkCode { get; set; }

    public string AgnCode { get; set; }

    public string BankAccountsJson { get; set; }

    public List<Guid> InquiryIds { get; set; }

    public string Familiar1FirstName { get; set; }

    public string Familiar1LastName { get; set; }

    public string Familiar1Mobile { get; set; }

    public string Familiar2FirstName { get; set; }

    public string Familiar2LastName { get; set; }

    public string Familiar2Mobile { get; set; }
}
