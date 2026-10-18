using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Helpers.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoanSystem.Site.Areas.Admin.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Admin")]
public class ReportController : BaseAdminController
{
    private readonly IReportService _reportService;

    public ReportController(ILogger<ReportController> logger, IReportService reportService)
        : base(logger)
    {
        _reportService = reportService;
    }

    [HttpGet("[action]")]
    public Task<ResponseState<OverDueInstallmentsModel>> OverDueInstallments()
    {
        return TryCatch(async () => Success(await _reportService.GetOverDueInstallments()));
    }

    [HttpGet("[action]")]
    public Task<ResponseState<List<dynamic>>> CheckoutThisMonth()
    {
        return TryCatch(async () => Success(await _reportService.CheckoutThisMonth()));
    }

    [HttpGet("[action]")]
    public Task<ResponseState<List<dynamic>>> CheckoutedMembers()
    {
        return TryCatch(async () => Success(await _reportService.CheckoutedMembers()));
    }

    [HttpGet("[action]")]
    public Task<ResponseState<List<dynamic>>> LatestMembers()
    {
        return TryCatch(async () => Success(await _reportService.LatestMembers()));
    }

    [HttpGet("[action]")]
    public Task<ResponseState<List<dynamic>>> RemainInstallmentCount()
    {
        return TryCatch(async () => Success(await _reportService.RemainInstallmentCount()));
    }

    [HttpGet("[action]")]
    public Task<ResponseState<List<dynamic>>> Members()
    {
        return TryCatch(async () => Success(await _reportService.AllMembers()));
    }

    [HttpGet("[action]")]
    public Task<ResponseState<List<dynamic>>> LoanStatistics(DateTime? start, DateTime? end)
    {
        return TryCatch(async () =>
        {
            List<object> data = await _reportService.LoanStatistics(start, end);
            return Success(data);
        });
    }

    [HttpGet("[action]")]
    public Task<ResponseState<List<dynamic>>> MemberLoansByDate(DateTime? start, DateTime? end, bool? isCheckout)
    {
        return TryCatch(async () =>
        {
            List<object> data = await _reportService.MemberLoansByDate(start, end, isCheckout);
            return Success(data);
        });
    }

    [HttpGet("[action]")]
    public Task<ResponseState<List<dynamic>>> SponsoredLoans(DateTime? start, DateTime? end)
    {
        return TryCatch(async () =>
        {
            List<object> data = await _reportService.SponsoredLoans(start, end);
            return Success(data);
        });
    }

    [HttpGet("[action]")]
    public Task<ResponseState<List<dynamic>>> SponsoredUserLoans(DateTime? start, DateTime? end, Guid loanId)
    {
        return TryCatch(async () =>
        {
            List<object> data = await _reportService.SponsoredUserLoansByLoanId(start, end, loanId);
            return Success(data);
        });
    }

    [HttpGet("[action]")]
    public Task<ResponseState<List<TransactionModel>>> SponsoredPaids(DateTime? start, DateTime? end, Guid loanId)
    {
        return TryCatch(async () =>
        {
            List<TransactionModel> data = await _reportService.SponsoredPaids(start, end, loanId);
            return Success(data);
        });
    }

    [HttpGet("[action]")]
    public Task<ResponseState<List<dynamic>>> InstallmentsByMonth(DateTime? start, DateTime? end)
    {
        return TryCatch(async () =>
        {
            List<object> data = await _reportService.InstallmentsByMonth(start, end);
            return Success(data);
        });
    }

    [HttpGet("[action]")]
    public Task<ResponseState<List<dynamic>>> PaidInstallmentsByMonth(DateTime? start, DateTime? end)
    {
        return TryCatch(async () =>
        {
            List<object> data = await _reportService.PaidInstallmentsByMonth(start, end);
            return Success(data);
        });
    }

    [HttpGet("[action]")]
    public Task<ResponseState<List<dynamic>>> OrganizationDueDated(DateTime? start, DateTime? end)
    {
        return TryCatch(async () =>
        {
            List<object> data = await _reportService.OrganizationDueDated(start, end);
            return Success(data);
        });
    }

    [HttpGet("[action]")]
    public Task<ResponseState<List<dynamic>>> Assurances(DateTime? start, DateTime? end, long memberId = 0L)
    {
        return TryCatch(async () =>
        {
            List<object> data = await _reportService.Assurances(start, end, memberId);
            return Success(data);
        });
    }

    [HttpGet("[action]")]
    public Task<ResponseState<List<dynamic>>> SaveMoniesByMonth(DateTime? start, DateTime? end)
    {
        return TryCatch(async () =>
        {
            List<object> data = _reportService.SaveMoniesByMonth(start, end);
            return Success(data);
        });
    }

    [HttpGet("[action]")]
    public Task<ResponseState<dynamic>> MonthlyBalance(string month, int saveType)
    {
        return TryCatch<object>(async () =>
        {
            dynamic val = await _reportService.MonthlyBalance(month, saveType);
            return Success(data: val);
        });
    }

    [HttpGet("[action]")]
    public Task<ResponseState<List<string>>> GetMonths()
    {
        return TryCatch(async () => Success(await _reportService.GetMonths()));
    }
}
