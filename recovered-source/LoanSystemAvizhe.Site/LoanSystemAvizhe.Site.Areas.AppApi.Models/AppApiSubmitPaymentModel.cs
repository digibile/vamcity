using System;
using System.ComponentModel.DataAnnotations;
using LoanSystem.Services.Models;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystemAvizhe.Site.Areas.AppApi.Models;

public class AppApiSubmitPaymentModel : BaseModel
{
	[Range(0.0, double.MaxValue, ErrorMessage = "مبلغ نمی تواند کمتر از صفر باشد")]
	public decimal Amount { get; set; }

	[Required(ErrorMessage = "تاریخ پرداخت الزامی است")]
	public DateTime PaymentDate { get; set; }

	public PaymentMethods PaymentMethod { get; set; }

	public string Description { get; set; }

	public bool SendSms { get; set; }
}
