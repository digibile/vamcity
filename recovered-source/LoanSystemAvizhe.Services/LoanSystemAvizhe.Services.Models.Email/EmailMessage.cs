using System;
using System.Collections.Generic;

namespace LoanSystemAvizhe.Services.Models.Email;

public class EmailMessage
{
	public List<EmailAttachment> Attachments { get; set; }

	public EmailAddress ToAddress { get; set; }

	public string Subject { get; set; }

	public string Content { get; set; }

	public Guid AppEmailId { get; set; }

	public EmailMessage()
	{
		ToAddress = new EmailAddress();
		Attachments = new List<EmailAttachment>();
	}
}
