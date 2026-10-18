using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LoanSystem.Services.Models;

namespace LoanSystem.Services.Base;

public interface IReportService
{
    Task<List<dynamic>> CheckoutThisMonth(DateTime? dateTime = null);

    Task<List<dynamic>> CheckoutedMembers();

    Task<List<dynamic>> LatestMembers();

    Task<List<dynamic>> RemainInstallmentCount();

    Task<List<dynamic>> AllMembers();

    Task<List<dynamic>> LoanStatistics(DateTime? start, DateTime? end);

    Task<List<dynamic>> MemberLoansByDate(DateTime? start, DateTime? end, bool? isCheckout);

    Task<List<dynamic>> SponsoredLoans(DateTime? start, DateTime? end);

    Task<List<dynamic>> SponsoredUserLoansByLoanId(DateTime? start, DateTime? end, Guid loanId);

    Task<List<TransactionModel>> SponsoredPaids(DateTime? start, DateTime? end, Guid loanId);

    Task<List<dynamic>> InstallmentsByMonth(DateTime? start, DateTime? end);

    Task<List<dynamic>> PaidInstallmentsByMonth(DateTime? start, DateTime? end);

    Task<List<dynamic>> OrganizationDueDated(DateTime? start, DateTime? end);

    Task<List<dynamic>> Assurances(DateTime? start, DateTime? end, long memberId);

    List<dynamic> SaveMoniesByMonth(DateTime? start, DateTime? end);

    Task<dynamic> MonthlyBalance(string month, int saveType);

    Task<List<string>> GetMonths();

    Task<EmailReportModel> GetDailyReport(AppSettingsModel appSettingsModel);

    Task<OverDueInstallmentsModel> GetOverDueInstallments(long? agentId = null);
}
