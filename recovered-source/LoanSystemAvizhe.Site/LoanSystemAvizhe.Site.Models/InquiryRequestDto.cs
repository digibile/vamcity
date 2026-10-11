using System.ComponentModel.DataAnnotations;

namespace LoanSystemAvizhe.Site.Models;

public class InquiryRequestDto
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

	public int CustomerCode { get; set; }

	public string MrkCode { get; set; }

	public string AgnCode { get; set; }
}
