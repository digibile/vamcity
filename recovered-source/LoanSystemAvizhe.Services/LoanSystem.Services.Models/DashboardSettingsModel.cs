namespace LoanSystem.Services.Models;

public class DashboardSettingsModel
{
    public long UserId { get; set; }

    public bool Stats { get; set; } = true;

    public bool OverDueInstallments { get; set; } = true;

    public bool AgentOverdue { get; set; } = true;

    public bool PendingPaymentRequests { get; set; }

    public bool SponsoreDueDate { get; set; }

    public bool DepositChargeDueDate { get; set; }

    public bool LoanRequests { get; set; }

    public bool SmsCredit { get; set; } = true;

    public bool ContactOverdue { get; set; }

    public bool CashTransactions { get; set; } = true;

    public bool BankTransactions { get; set; } = true;

    public bool OnlineTransactions { get; set; } = true;

    public bool GoldTransactions { get; set; }

    public bool Ticket { get; set; }

    public bool CustomerOrders { get; set; }

    public bool AgentFactorGateway { get; set; }

    public bool MemberAuthorization { get; set; }
}
