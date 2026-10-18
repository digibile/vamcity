using System;
using System.Text.Json;
using LoanSystem.Core;

namespace LoanSystemAvizhe.Services.Models.Shahin;

public class ShahinTransferResult
{
    public string transactionState { get; set; }

    public long transactionTime { get; set; }

    public PersianDate transactionTime_persian
    {
        get
        {
            if (transactionTime == 0L)
            {
                return new PersianDate();
            }
            return DateTimeOffset.FromUnixTimeMilliseconds(transactionTime).ToLocalTime().DateTime.ToPersianDateObject();
        }
    }

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

    public ShahinRespObject respObject { get; set; }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}
