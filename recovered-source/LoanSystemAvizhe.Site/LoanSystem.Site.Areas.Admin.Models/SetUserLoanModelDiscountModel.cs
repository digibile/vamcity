using System;
using System.ComponentModel.DataAnnotations;

namespace LoanSystem.Site.Areas.Admin.Models;

public class SetUserLoanModelDiscountModel
{
	public Guid Id { get; set; }

	[Range(0.0, double.MaxValue, ErrorMessage = "مبلغ تخفیف نمی تواند کمتر از صفر باشد")]
	public decimal Discount { get; set; }
}
