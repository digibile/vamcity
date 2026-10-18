using System;

namespace LoanSystem.Services.Models.OnlineMoneyTransferPanel;

public class OnlineMoneyTransferPanelCompleteTransfer
{
    public Guid Id { get; set; }

    public decimal Amount { get; set; }

    public string From { get; set; }

    public string To { get; set; }

    public string OTP { get; set; }

    public string ClientTraceNumber { get; set; }
}
