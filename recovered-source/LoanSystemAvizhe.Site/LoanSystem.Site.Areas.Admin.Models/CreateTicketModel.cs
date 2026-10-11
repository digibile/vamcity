using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace LoanSystem.Site.Areas.Admin.Models;

public class CreateTicketModel
{
	[Required(ErrorMessage = "شرح پیام الزامی است.")]
	public string Description { get; set; }

	[Required(ErrorMessage = "عنوان پیام الزامی است.")]
	public string Title { get; set; }

	public CreateTicketTargets CreateTicketTarget { get; set; }

	public IFormFile File { get; set; }

	public List<long> UserIds { get; set; }

	public bool AsMessage { get; set; }

	public Guid? UserLoanId { get; set; }

	public Guid? UserLoanRequestId { get; set; }
}
