using System.ComponentModel.DataAnnotations;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystemAvizhe.Site.Models;

public class LoginOTP
{
	public int CustomerCode { get; set; }

	public string OTPCode { get; set; }

	public string Role { get; set; }

	[StringLength(11, MinimumLength = 0, ErrorMessage = "طول کاراکترهای موبایل نباید بیشتر از {1} باشد")]
	public string Mobile { get; set; }

	public AppSmsType AppSmsType { get; set; } = AppSmsType.LoginOTP;
}
