using System;

namespace LoanSystem.Services.Models;

public class IdCode
{
    public Guid Id { get; set; }

    public int Code { get; set; }

    public decimal Amount { get; set; }
}
