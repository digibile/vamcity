using System;
using System.Collections.Generic;

namespace LoanSystem.Services.Models.Landa;

public class LandaSettings
{
    public string Token { get; set; }

    public Guid? BankId { get; set; }

    public Guid? CashId { get; set; }

    public Dictionary<Guid, string> ProductMap { get; set; }
}
