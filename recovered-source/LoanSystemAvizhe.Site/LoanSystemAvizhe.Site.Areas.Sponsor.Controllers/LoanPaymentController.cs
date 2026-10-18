using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Areas.Sponsor.Controllers;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoanSystemAvizhe.Site.Areas.Sponsor.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Sponsor")]
public class LoanPaymentController : BaseSponsorController
{
    private readonly IUserPaymentService _userPaymentService;

    private readonly IAppContext _appContext;

    private readonly ICurrentUserService _currentUserService;

    public LoanPaymentController(ILogger<LoanPaymentController> logger, IUserPaymentService userPaymentService, ICurrentUserService currentUserService, IAppContext appContext)
        : base(logger)
    {
        _userPaymentService = userPaymentService;
        _currentUserService = currentUserService;
        _appContext = appContext;
    }

    [HttpGet("[action]/{id}")]
    public Task<ResponseState<List<LoanPaymentModel>>> GetLoanPayments(Guid id)
    {
        return TryCatch(async () =>
        {
            List<LoanPaymentModel> models = new List<LoanPaymentModel>();
            if (id == Guid.Empty)
            {
                return Error<List<LoanPaymentModel>>(null, "وام یافت نشد.");
            }
            List<UserLoanPayment> list = await _userPaymentService.GetPaymentsByUserLoanId(id, includeMemberPurchases: true, includePaymentRequest: true);
            if (list.Any())
            {
                if (!_currentUserService.IsInRole("Admin") && list.First().UserLoan.Loan.HasSponsore)
                {
                    list.Clear();
                }
                models = (from x in list
                          orderby x.TargetMonth
                          select new LoanPaymentModel(x)).ToList();
                await _userPaymentService.SetPaymentsDelays(models);
            }
            return Success(models);
        });
    }
}
