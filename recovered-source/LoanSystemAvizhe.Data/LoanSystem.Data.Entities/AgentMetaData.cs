using System;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Data.Entities;

public class AgentMetaData : BaseEntity
{
	public string SignaturesOwnerNames { get; set; }

	public string SiteUrl { get; set; }

	public string ShomareSabt { get; set; }

	public string ShenaseMelli { get; set; }

	public string CompanyName { get; set; }

	public string AgentCategoryActivities { get; set; }

	public AgentType AgentType { get; set; }

	public bool IsOnline { get; set; }

	public bool IsPhysical { get; set; }

	public long UserId { get; set; }

	public virtual User User { get; set; }

	public Guid CustomerId { get; set; }

	public virtual Customer Customer { get; set; }

	public AgentStatus Status { get; set; }

	public string RejectReason { get; set; }

	public DateTime LastAdminChangeDateTime { get; set; }

	public string CheckedBy { get; set; }

	public string ThumbnailFileName { get; set; }

	public string JavazKasbFileName { get; set; }

	public string KartMelliFileName { get; set; }

	public string EjarenameFileName { get; set; }

	public string AsasnameFileName { get; set; }

	public string RoznameRasmiFileName { get; set; }

	public string PledgeChequeFileName { get; set; }

	public bool UseDefaultCommission { get; set; }

	public decimal AgentFactorCommission { get; set; }

	public decimal MaxAgentFactorCommission { get; set; }

	public bool CanConvertRequestToLoan { get; set; }
}
