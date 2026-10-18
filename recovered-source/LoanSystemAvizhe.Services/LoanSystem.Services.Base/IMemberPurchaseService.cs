using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Models;
using LoanSystemAvizhe.Site.Areas.Member.Models;

namespace LoanSystem.Services.Base;

public interface IMemberPurchaseService
{
    IQueryable<MemberPurchase> GetAllAsQueryable();

    Task<List<MemberPurchase>> GetAll(Guid? customerId, params string[] includs);

    Task<MemberPurchase> GetById(Guid entityId, params string[] includs);

    Task<MemberPurchase> GetByZarinPalAuthority(string authority);

    Task<MemberPurchase> GetBySepehrDigitalReceiptNum(string digitalReceipt);

    Task<MemberPurchase> GetBySepResNum(string resNum);

    Task<MemberPurchase> GetBySepRefNum(string refNum);

    Task<MemberPurchase> GetByIPGRRN(long rrn);

    Task<MemberPurchase> GetBySadadRetrivalRefNo(string retrivalRefNo);

    Task<MemberPurchase> GetByNextPayTransId(string transId);

    Task<MemberPurchase> GetByCode(int code);

    Task<MemberPurchase> GetByUserLoanPaymentId(Guid userLoanPaymentId);

    Task<List<GroupedPurchaseReportModel>> GetGroupedOnTypeByDateAsync(DateTime startDate, DateTime endDate);

    Task ApplyPurchase(Guid purchaseId, bool sendSms = true);

    Task<MemberPurchaseCreateResult> Create(MemberPurchaseModel model);

    Task Insert(MemberPurchase entity);

    Task Update(MemberPurchase entity);

    Task Delete(MemberPurchase entity);

    void DeleteByUserPaymentId(Guid userPaymentId);
}
