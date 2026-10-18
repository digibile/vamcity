using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoanSystem.Data.Entities;

public class UserLoanRequestAuthorizationItem : BaseEntity
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Code { get; set; }

    public Guid UserLoanRequestId { get; set; }

    public virtual UserLoanRequest UserLoanRequest { get; set; }

    public Guid AuthorizationItemId { get; set; }

    public virtual AuthorizationItem AuthorizationItem { get; set; }

    public Guid? ThirdPersonAssuranceAuthorizationItemId { get; set; }

    public virtual ThirdPersonAssuranceAuthorizationItem ThirdPersonAssuranceAuthorizationItem { get; set; }

    public Guid? EmzameContractAuthorizationItemId { get; set; }

    public virtual EmzameContractAuthorizationItem EmzameContractAuthorizationItem { get; set; }

    public DateTime CreateDate { get; set; }

    public DateTime LastChangeDate { get; set; }

    public string Tag { get; set; }

    public string Attachment { get; set; }

    public string MemberDescription { get; set; }

    public string AdminDescription { get; set; }

    public bool AttachmentByAdmin { get; set; }

    public LoanRequestAuthorizationStatus Status { get; set; }

    public Guid CustomerId { get; set; }

    public virtual ICollection<UserLoanRequestAuthorizationItemAttachment> Attachments { get; set; } = new HashSet<UserLoanRequestAuthorizationItemAttachment>();
}
