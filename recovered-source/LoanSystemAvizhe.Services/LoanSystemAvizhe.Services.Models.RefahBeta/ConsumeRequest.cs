using System;

namespace LoanSystemAvizhe.Services.Models.RefahBeta;

public class ConsumeRequest
{
    public string Title { get; set; }

    public DateTime StartDate { get; set; }

    public decimal Amount { get; set; }

    public int NumberOfInstallments { get; set; }

    public string NationalCode { get; set; }

    public string Otp { get; set; }

    public Guid RequestId { get; set; }

    public string StartDateYearMonth { get; set; }
}
