using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace LoanSystem.Services.Models;

public class ImportFileModel : BaseModel
{
	[Required(ErrorMessage = "فایل را انتخاب کنید")]
	public IFormFile File { get; set; }
}
