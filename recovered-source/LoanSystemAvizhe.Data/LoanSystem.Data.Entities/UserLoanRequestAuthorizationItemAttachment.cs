using System;

namespace LoanSystem.Data.Entities;

public class UserLoanRequestAuthorizationItemAttachment : BaseEntity
{
	public Guid UserLoanRequestAuthorizationItemId { get; set; }

	public virtual UserLoanRequestAuthorizationItem UserLoanRequestAuthorizationItem { get; set; }

	public string Attachment { get; set; }

	public string MemberDescription { get; set; }

	public bool AttachmentByAdmin { get; set; }

	public Guid CustomerId { get; set; }
}
