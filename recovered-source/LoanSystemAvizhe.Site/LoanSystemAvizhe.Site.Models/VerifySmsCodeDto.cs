using System.ComponentModel.DataAnnotations;

namespace LoanSystemAvizhe.Site.Models;

public class VerifySmsCodeDto
{
	[Required(ErrorMessage = "شناسه کاربر الزامی است.")]
	public long UserId { get; set; }

	[Required(ErrorMessage = "کد تائیدیه الزامی است.")]
	public int Code { get; set; }
}
