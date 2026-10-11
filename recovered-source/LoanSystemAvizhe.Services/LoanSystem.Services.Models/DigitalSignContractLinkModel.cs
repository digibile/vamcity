using System;
using LoanSystem.Core;

namespace LoanSystem.Services.Models;

public class DigitalSignContractLinkModel
{
	public string MemberMobile { get; set; }

	public int LoanRequestCode { get; set; }

	public int LoanRequestAutorizationItemCode { get; set; }

	public DateTime Date { get; set; }

	public PersianDate PersianDate => Date.ToPersianDateObject();
}
