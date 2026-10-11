using System.ComponentModel.DataAnnotations;
using LoanSystem.Services.Models;
using Microsoft.AspNetCore.Http;

namespace LoanSystemAvizhe.Site.Models;

public class AgentRegisterDto
{
	[Required(ErrorMessage = "نام الزامی است.")]
	[StringLength(100, MinimumLength = 0, ErrorMessage = "طول کاراکترهای نام نباید بیشتر از {1} باشد")]
	public string FirstName { get; set; }

	[Required(ErrorMessage = "نام تجاری الزامی است.")]
	[StringLength(100, MinimumLength = 0, ErrorMessage = "طول کاراکترهای نام تجاری نباید بیشتر از {1} باشد")]
	public string BusinessName { get; set; }

	public string Shenasname { get; set; }

	[StringLength(11, MinimumLength = 0, ErrorMessage = "طول کاراکترهای موبایل نباید بیشتر از {1} باشد")]
	public string Mobile { get; set; }

	[RegularExpression("^[a-zA-Z0-9_.-]+@[a-zA-Z0-9-]+.[a-zA-Z0-9-.]+$", ErrorMessage = "ایمیل وارد شده صحیح نمی باشد.")]
	public string Email { get; set; }

	[StringLength(10, MinimumLength = 0, ErrorMessage = "طول کاراکترهای کد ملی نباید بیشتر از {1} باشد")]
	public string NationalCode { get; set; }

	[StringLength(100, ErrorMessage = "کلمه عبور باید حداقل {2} کاراکتر باشد.", MinimumLength = 8)]
	public string Password { get; set; }

	[Compare("Password", ErrorMessage = "تکرار کلمه عبور با کلمه عبور مطابقت ندارد.")]
	public string ConfirmPassword { get; set; }

	public int CustomerCode { get; set; }

	[StringLength(50, MinimumLength = 0, ErrorMessage = "طول کاراکترهای شهر نباید بیشتر از {1} باشد")]
	public string City { get; set; }

	[StringLength(200, MinimumLength = 0, ErrorMessage = "طول کاراکترهای آدرس نباید بیشتر از {1} باشد")]
	public string Address { get; set; }

	[StringLength(10, MinimumLength = 0, ErrorMessage = "طول کاراکترهای کد پستی نباید بیشتر از {1} باشد")]
	public string PostalCode { get; set; }

	[StringLength(20, MinimumLength = 0, ErrorMessage = "طول کاراکترهای شماره تماس نباید بیشتر از {1} باشد")]
	public string Phone { get; set; }

	public AgentMetaDataModel MetaData { get; set; }

	public IFormFile ThumbnailFile { get; set; }

	public string ThumbnailFileName { get; set; }

	public string ThumbnailBase64 { get; set; }
}
