using System;

namespace LoanSystemAvizhe.Site.Models;

public class SignContractWithOtp
{
    public Guid Id { get; set; }

    public int? CustomerCode { get; set; }

    public string Otp { get; set; }
}
