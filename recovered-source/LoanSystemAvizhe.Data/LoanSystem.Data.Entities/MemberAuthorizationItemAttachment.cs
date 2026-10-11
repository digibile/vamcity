using System;

namespace LoanSystem.Data.Entities;

public class MemberAuthorizationItemAttachment : BaseEntity
{
	public Guid MemberAuthorizationItemId { get; set; }

	public virtual MemberAuthorizationItem MemberAuthorizationItem { get; set; }

	public string Attachment { get; set; }

	public string MemberDescription { get; set; }

	public bool AttachmentByAdmin { get; set; }

	public Guid CustomerId { get; set; }
}
