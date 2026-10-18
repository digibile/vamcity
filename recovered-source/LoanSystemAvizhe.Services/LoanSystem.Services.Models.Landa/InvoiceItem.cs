using System;

namespace LoanSystem.Services.Models.Landa;

public class InvoiceItem
{
    public Guid productId { get; set; } = Guid.Empty;

    public string productCode { get; set; }

    public decimal quantity { get; set; }

    public decimal price { get; set; }

    public decimal vatPercent { get; set; }

    public decimal discount { get; set; }

    public string comment { get; set; }
}
