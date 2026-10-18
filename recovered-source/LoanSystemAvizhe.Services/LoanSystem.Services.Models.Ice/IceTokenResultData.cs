using System;

namespace LoanSystem.Services.Models.Ice;

public class IceTokenResultData
{
    public string token { get; set; }

    public string userName { get; set; }

    public DateTime tokenExpireDate { get; set; }
}
