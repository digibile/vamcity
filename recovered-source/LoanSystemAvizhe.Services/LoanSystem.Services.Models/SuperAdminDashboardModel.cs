namespace LoanSystem.Services.Models;

public class SuperAdminDashboardModel
{
    public int TotalRegister { get; set; }

    public int TotalCustomer { get; set; }

    public int TotalCloudCustomer { get; set; }

    public int TotalPermanentCustomer { get; set; }

    public decimal TotalSale { get; set; }

    public decimal TotalCommission { get; set; }

    public decimal TotalProfit { get; set; }

    public decimal TotalSmsCharge { get; set; }

    public decimal TotalSmsCredits { get; set; }

    public int TotalUserLoan { get; set; }

    public int CurrentMonthRegister { get; set; }

    public int CurrentMonthCustomer { get; set; }

    public int CurrentMonthCloudCustomer { get; set; }

    public int CurrentMonthPermanentCustomer { get; set; }

    public decimal CurrentMonthSale { get; set; }

    public decimal CurrentMonthCommission { get; set; }

    public decimal CurrentMonthProfit { get; set; }

    public decimal CurrentMonthSmsCharge { get; set; }

    public int CurrentMonthUserLoan { get; set; }

    public int PendingSmsPatterns { get; set; }

    public int UnSeenedConsultants { get; set; }

    public dynamic CustomerChart { get; set; }

    public dynamic SaleChart { get; set; }
}
