using System;

namespace LoanSystem.Services.Models.Landa;

public class CreateInvoicePaymentResponse
{
	public Guid id { get; set; }

	public Guid customerId { get; set; }

	public decimal amount { get; set; }

	public string subject { get; set; }
}
