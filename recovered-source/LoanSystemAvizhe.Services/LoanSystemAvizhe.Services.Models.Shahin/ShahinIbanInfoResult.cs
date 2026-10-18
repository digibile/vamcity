using System;
using System.Text.Json;

namespace LoanSystemAvizhe.Services.Models.Shahin;

public class ShahinIbanInfoResult
{
    public string transactionState { get; set; }

    public long transactionTime { get; set; }

    public Guid uuid { get; set; }

    public string message { get; set; }

    public bool success
    {
        get
        {
            if (!(transactionState?.ToLower() == "success"))
            {
                return transactionState?.ToLower() == "pending";
            }
            return true;
        }
    }

    public ShahinIbanInfoRespObject respObject { get; set; }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}
