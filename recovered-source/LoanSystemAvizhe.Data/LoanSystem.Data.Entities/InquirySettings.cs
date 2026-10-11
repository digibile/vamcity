using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Data.Entities;

public class InquirySettings : BaseEntity
{
	public AppInquiryType AppInquiryType { get; set; }

	public decimal ApiIR_PriceFrom { get; set; }

	public decimal ApiIR_PriceTo { get; set; }

	public bool ApiIR_IsActive { get; set; }
}
