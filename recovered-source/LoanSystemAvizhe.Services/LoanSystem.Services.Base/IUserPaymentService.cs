using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Models;

namespace LoanSystem.Services.Base;

public interface IUserPaymentService
{
	Task<List<UserLoanPayment>> GetByCondition(Expression<Func<UserLoanPayment, bool>> condition, params string[] includs);

	Task<List<UserLoanPayment>> GetAllUserPayments(bool includeDetails = false);

	Task<List<UserLoanPayment>> GetPaymentsByUserLoanId(Guid userLoanId, bool includeMemberPurchases, bool includePaymentRequest);

	Task<List<LoanPaymentByMonthModel>> GetListByTargetMonth(DateTime start, DateTime end);

	Task<List<UserLoanPayment>> GetPaymentsByUserId(long userId);

	Task<Dictionary<long, List<UserLoanPayment>>> GetPaymentsByUserIdsAsDictionary(List<long> userIds);

	Task<UserLoanPayment> GetById(Guid entityId, params string[] includs);

	Task<UserLoanPayment> GetByCode(int code, params string[] includs);

	Task<string> ValidateLoanPayment(UserLoanPayment userLoanPayment);

	Task<Task> Insert(UserLoanPayment entity);

	Task Update(UserLoanPayment entity, bool sendSms = true);

	Task Delete(UserLoanPayment entity);

	void DeleteByUserLoanId(Guid userLoanId);

	Task<int> CalcDelayDay(LoanPaymentModel model);

	Task<decimal> CalcDelayPrice(LoanPaymentModel model, bool checkoutMode = false, int? days = null);

	Task SetPaymentsDelays(List<LoanPaymentModel> loanPayments, bool calcEarly = false, bool checkoutModeOnLoanProfit = false);

	Task<List<GroupedUserPaymentReportModel>> GetGroupedOnMethodByDateAsync(DateTime startDate, DateTime endDate);

	Task SetAppSms(List<LoanPaymentModel> loanPayments, bool ignoreText);

	Task<OverDueInstallmentsModel> GetOverDueInstallments(bool loadAppSms, long? agentId = null);

	Task<List<LoanPaymentModel>> GetDueDatedItemsForSms();

	Task<List<LoanPaymentModel>> GetDueDatedItemsForEmail();

	Task<List<LoanPaymentModel>> GetBeforeDueDatedItemsForSms(int days);

	Task<List<LoanPaymentModel>> GetBeforeDueDatedItemsForEmail(int days);

	Task<List<LoanPaymentModel>> GetOverDueDatedItemsForSms(int daysCount, bool onlyOnce, int gapPeriod);

	Task<List<LoanPaymentModel>> GetOverDueDatedItemsForEmail(int daysCount, bool onlyOnce);

	Task<int> GetDueDatedCountByDateAsync(DateTime startDate, DateTime endDate);

	Task<int> GetOverDueDatedCountByDateAsync(DateTime startDate, DateTime endDate);

	Task MarkAsSentBeforeDueDateSms(Guid id);

	Task MarkAsSentBeforeDueDateEmail(Guid id);

	Task MarkAsSentDueDateSms(Guid id);

	Task MarkAsSentDueDateEmail(Guid id);

	Task MarkAsSentOverDueDateSms(Guid id);

	Task MarkAsSentOverDueDateEmail(Guid id);
}
