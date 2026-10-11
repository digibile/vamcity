using System;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystemAvizhe.Services.Inquiry.Models;

public class InquiryRequestModel
{
	public AppInquiryType InquiryType { get; set; }

	public long MemberId { get; set; }

	public Guid? MemberAuthorizationItemId { get; set; }

	public Guid? UserLoanRequestAuthorizationItemId { get; set; }
}
