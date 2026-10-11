using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace LoanSystem.Site.Areas.Admin.Models;

public class SubmitAssuranceReturn
{
	public bool IsReturned { get; set; }

	[Required(ErrorMessage = "شناسه ضمانت الزامی است")]
	public Guid AssuranceId { get; set; }

	[Required(ErrorMessage = "تاریخ عودت الزامی است")]
	public DateTime ReturnDateTime { get; set; }

	[Required(ErrorMessage = "تحویل گیرنده الزامی است")]
	public string ReturnedToPerson { get; set; }

	public IFormFile ReturnFile { get; set; }

	public string ReturnFileName { get; set; }

	public string ReturnBase64 { get; set; }

	public long ReturnFileSize { get; set; }

	public string ReturnFileType { get; set; }
}
