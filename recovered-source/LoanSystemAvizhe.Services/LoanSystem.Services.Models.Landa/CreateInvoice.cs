using System;
using System.Collections.Generic;

namespace LoanSystem.Services.Models.Landa;

public class CreateInvoice
{
    public string docNo { get; set; }

    public Guid customerId { get; set; }

    public string customerCode { get; set; } = "";

    public string date { get; set; }

    public string summary { get; set; }

    public int transportationCost { get; set; }

    public object customerSummary { get; set; }

    public Guid typeId { get; set; }

    public List<InvoiceItem> invoiceItems { get; set; }

    public List<PaymentItem> paymentItems { get; set; }
}
