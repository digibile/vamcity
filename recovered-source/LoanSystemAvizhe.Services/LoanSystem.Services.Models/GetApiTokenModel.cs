using System.ComponentModel.DataAnnotations;

namespace LoanSystem.Services.Models;

public class GetApiTokenModel
{
	[Required(ErrorMessage = "کلید رابط الزامی است.")]
	public string ApiKey { get; set; }

	[Required(ErrorMessage = "کلمه عبور الزامی است.")]
	public string Password { get; set; }
}
