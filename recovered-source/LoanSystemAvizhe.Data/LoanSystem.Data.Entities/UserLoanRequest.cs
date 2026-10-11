using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Data.Entities;

public class UserLoanRequest : BaseEntity
{
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public int Code { get; set; }

	public long UserId { get; set; }

	public virtual User User { get; set; }

	public Guid LoanId { get; set; }

	public virtual Loan Loan { get; set; }

	public long? AgentId { get; set; }

	public virtual User Agent { get; set; }

	public DateTime RequestDate { get; set; }

	public Guid? StatusId { get; set; }

	public virtual LoanRequestStatus Status { get; set; }

	public decimal Amount { get; set; }

	public decimal Installment { get; set; }

	public int InstallmentCount { get; set; }

	public int MonthGap { get; set; }

	public string Comment { get; set; }

	public string AdminDescriptionPrivate { get; set; }

	public string AdminDescription { get; set; }

	public DateTime SponsoreDueDate { get; set; }

	public decimal SponsoreAmount { get; set; }

	public Guid? AgentStatusId { get; set; }

	public virtual LoanRequestStatus AgentStatus { get; }

	public bool IsPhysicalDocsDelivered { get; set; }

	public int Month { get; set; }

	public int Year { get; set; }

	public virtual ICollection<UserLoanRequestAuthorizationItem> UserLoanRequestAuthorizationItems { get; set; } = new HashSet<UserLoanRequestAuthorizationItem>();

	public Guid CustomerId { get; set; }

	public AgentLoanRequestChargeTypes ChargeType { get; set; }

	public int OtherPercent_CheckoutDays { get; set; }

	public decimal OtherPercent_Percent { get; set; }

	public string LockedBy { get; set; }

	public long RefahBetaInquiryAmount { get; set; }

	public long RefahBetaOrderId { get; set; }

	public string Products { get; set; }

	public long? SponsorId { get; set; }

	public virtual User Sponsor { get; set; }

	public UserLoanSponsorStatuses UserLoanSponsorStatus { get; set; }

	public string LastSponsorComment { get; set; }
}
