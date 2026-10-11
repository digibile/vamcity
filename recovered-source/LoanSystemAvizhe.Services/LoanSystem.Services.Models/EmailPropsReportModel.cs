namespace LoanSystem.Services.Models;

public class EmailPropsReportModel
{
	public int LoanRequestCount { get; set; }

	public int DueDatedLoanPaymentCount { get; set; }

	public int OverDueDatedLoanPaymentCount { get; set; }

	public int LoanCount { get; set; }

	public int MemberCount { get; set; }

	public int Ticket { get; set; }

	public int ClosedTicket { get; set; }

	public int RepliedTicket { get; set; }

	public int OnlinePayment { get; set; }

	public int CashPayment { get; set; }

	public int BankPayment { get; set; }

	public int WalletCharge { get; set; }

	public int AgentTransaction { get; set; }

	public int SponsorDueDate { get; set; }

	public decimal OnlinePaymentPrice { get; set; }

	public decimal WalletChargePrice { get; set; }

	public decimal CashPaymentPrice { get; set; }

	public decimal BankPaymentPrice { get; set; }

	public decimal AgentTransactionPrice { get; set; }
}
