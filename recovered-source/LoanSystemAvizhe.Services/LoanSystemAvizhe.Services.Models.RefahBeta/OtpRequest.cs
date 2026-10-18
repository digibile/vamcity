using System;

namespace LoanSystemAvizhe.Services.Models.RefahBeta;

public class OtpRequest
{
    public string Title { get; set; }

    public DateTime StartDate { get; set; }

    public long Amount { get; set; }

    public int NumberOfInstallments { get; set; }

    public string NationalCode { get; set; }

    public string StartDateYearMonth { get; set; }
}
