using System;

namespace LoanSystemAvizhe.Services.FileHelper;

public static class Directories
{
	private static string _iranValidityReportPdf = "uploads\\{customerId}\\iranValidityReportPdf\\";

	private static string _iceReportPdf = "uploads\\{customerId}\\iceReportPdf\\";

	private static string _ticket = "uploads\\{customerId}\\tickets\\";

	private static string _loan = "uploads\\{customerId}\\loans\\";

	private static string _assurance = "uploads\\{customerId}\\assurances\\";

	private static string _assuranceCheque = "uploads\\{customerId}\\assuranceCheques\\";

	private static string _siteContent = "wwwroot\\siteContents\\{customerCode}\\";

	private static string _authorizeItem = "uploads\\{customerId}\\authorizeItems\\";

	private static string _transaction = "uploads\\{customerId}\\transaction\\";

	private static string _userLoanPaymentRequest = "uploads\\{customerId}\\userLoanPaymentRequest\\";

	private static string _subAgentTransaction = "uploads\\{customerId}\\subAgentTransaction\\";

	private static string _agentAttachments = "uploads\\{customerId}\\agentAttachments\\";

	private static string _emzameContracts = "uploads\\{customerId}\\emzameContracts\\";

	public static Func<string, string> IranValidityReportPdf => (string a) => _iranValidityReportPdf.Replace("{customerId}", a);

	public static Func<string, string> IceReportPdf => (string a) => _iceReportPdf.Replace("{customerId}", a);

	public static Func<string, string> Ticket => (string a) => _ticket.Replace("{customerId}", a);

	public static Func<string, string> Loan => (string a) => _loan.Replace("{customerId}", a);

	public static Func<string, string> Assurance => (string a) => _assurance.Replace("{customerId}", a);

	public static Func<string, string> AssuranceCheque => (string a) => _assuranceCheque.Replace("{customerId}", a);

	public static Func<string, string> SiteContent => (string a) => _siteContent.Replace("{customerCode}", a);

	public static Func<string, string> AuthorizeItem => (string a) => _authorizeItem.Replace("{customerId}", a);

	public static Func<string, string> Transaction => (string a) => _transaction.Replace("{customerId}", a);

	public static Func<string, string> UserLoanPaymentRequest => (string a) => _userLoanPaymentRequest.Replace("{customerId}", a);

	public static Func<string, string> SubAgentTransaction => (string a) => _subAgentTransaction.Replace("{customerId}", a);

	public static Func<string, string> AgentAttachments => (string a) => _agentAttachments.Replace("{customerId}", a);

	public static Func<string, string> EmzameContract => (string a) => _emzameContracts.Replace("{customerId}", a);
}
