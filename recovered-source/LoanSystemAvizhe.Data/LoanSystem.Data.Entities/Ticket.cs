using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Data.Entities;

public class Ticket : BaseEntity
{
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public int Code { get; set; }

	public DateTime CreatedAt { get; set; }

	public DateTime LastModifiedAt { get; set; }

	public string Title { get; set; }

	public TicketStatus Status { get; set; }

	public TicketRoles Role { get; set; }

	public int ResponseCount { get; set; }

	public bool AsMessage { get; set; }

	public long CreatedById { get; set; }

	public virtual User CreatedBy { get; set; }

	public TicketRoles TargetRole { get; set; }

	public long? TargetUserId { get; set; }

	public virtual User TargetUser { get; set; }

	public Guid? UserLoanId { get; set; }

	public virtual UserLoan UserLoan { get; set; }

	public Guid? UserLoanRequestId { get; set; }

	public virtual UserLoanRequest UserLoanRequest { get; set; }

	public Guid? CustomerId { get; set; }

	public virtual Customer Customer { get; set; }

	public virtual ICollection<TicketResponse> TicketResponses { get; set; } = new HashSet<TicketResponse>();
}
