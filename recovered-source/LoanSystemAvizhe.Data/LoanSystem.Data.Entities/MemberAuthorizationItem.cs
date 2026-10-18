using System;
using System.Collections.Generic;

namespace LoanSystem.Data.Entities;

public class MemberAuthorizationItem : BaseEntity
{
    public long MemberId { get; set; }

    public virtual User Member { get; set; }

    public Guid AuthorizationItemId { get; set; }

    public virtual AuthorizationItem AuthorizationItem { get; set; }

    public DateTime CreateDate { get; set; }

    public DateTime LastChangeDate { get; set; }

    public string LastChangeBy { get; set; }

    public string MemberDescription { get; set; }

    public string AdminDescription { get; set; }

    public string Tag { get; set; }

    public LoanRequestAuthorizationStatus Status { get; set; }

    public Guid CustomerId { get; set; }

    public virtual ICollection<MemberAuthorizationItemAttachment> Attachments { get; set; } = new HashSet<MemberAuthorizationItemAttachment>();
}
