using System;
using LoanSystem.Core;

namespace LoanSystem.Services.Models;

public class WalletChargeModel
{
	public Member2Model Member { get; set; }

	public DateTime Date { get; set; }

	public PersianDate PersianDate => Date.ToPersianDateObject();
}
