using System;
using System.Collections.Generic;

namespace LoanSystem.Services.Models.Landa;

public class CreateInvoicePayment
{
    public Guid selectedInvoiceId { get; set; }

    public string date { get; set; }

    public string summary { get; set; }

    public List<PaymentItem> paymentItems { get; set; }
}
