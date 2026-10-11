using System;

namespace LoanSystem.Services.Models.Landa;

public class CreateInvoiceResponse
{
	public Guid id { get; set; }

	public int no { get; set; }

	public Guid customerId { get; set; }

	public decimal amount { get; set; }

	public decimal receivedAmount { get; set; }
}
