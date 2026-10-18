using System.Text.Json;
using LoanSystem.Services.Models.EFarda;

namespace LoanSystem.Services.Models.OnlineMoneyTransferPanel;

public class OnlineMoneyTransferPanelCompleteTransferResult
{
    public EFardaResult result { get; set; }

    public bool success { get; set; }

    public string message { get; set; }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }

    public string ToEFardaJson()
    {
        if (!success)
        {
            result = new EFardaResult
            {
                description = message,
                amount = ""
            };
        }
        return JsonSerializer.Serialize(result);
    }
}
