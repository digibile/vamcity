using System;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Services.Models;

public class AssuranceReportModel
{
	public Guid UserLoanId { get; set; }

	public int UserLoanCode { get; set; }

	public decimal UserLoanAmount { get; set; }

	public decimal UserLoanPayableAmount { get; set; }

	public string LoanTitle { get; set; }

	public long MemberId { get; set; }

	public string MemberFullName { get; set; }

	public string MemberNationalCode { get; set; }

	public string MemberMobile { get; set; }

	public string MemberUserName { get; set; }

	public decimal InstallmentAmount { get; set; }

	public AssuranceType AssuranceType { get; set; }

	public bool IsReturned { get; set; }

	public string AssurancePersonFullName { get; set; }

	public string AssurancePersonNationalCode { get; set; }

	public string AssurancePersonalCode { get; set; }

	public string OrganizationName { get; set; }

	public Guid? OrganizationId { get; set; }

	public decimal GoldWeight { get; set; }

	public AssuranceCoinType CoinType { get; set; }

	public string Description { get; set; }

	public AssuranceChequeItemModel Cheque { get; set; }
}
