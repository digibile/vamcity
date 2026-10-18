using System;

namespace LoanSystemAvizhe.Services.Models.Sadad;

public class SadadPaymentRequest
{
    public string MerchantId { get; set; }

    public string MerchantKey { get; set; }

    public string TerminalId { get; set; }

    public decimal Amount { get; set; }

    public decimal OrderId { get; set; }

    public DateTime LocalDateTime { get; set; }

    public string ReturnUrl { get; set; }

    public string SignData { get; set; }

    public long UserId { get; set; }

    public string NationalCode { get; set; }
}
