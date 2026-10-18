using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Services.Facades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoanSystem.Site.Areas.Member.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Member")]
public class UserLoanController : BaseMemberController
{
    private readonly IUserLoanService _userLoanService;

    private readonly IAppContext _appContext;

    private readonly IUserLoanFacade _userLoanFacade;

    private readonly IUserPaymentService _userPaymentService;

    public UserLoanController(ILogger<UserLoanController> logger, IUserLoanService userLoanService, IUserPaymentService userPaymentService, IAppContext appContext, IUserLoanFacade userLoanFacade)
        : base(logger)
    {
        _userLoanService = userLoanService;
        _appContext = appContext;
        _userLoanFacade = userLoanFacade;
        _userPaymentService = userPaymentService;
    }

    [HttpGet("[action]")]
    public Task<ResponseState<List<UserLoanModel>>> Get()
    {
        return TryCatch(async () =>
        {
            if (_appContext.User == null)
            {
                return Error<List<UserLoanModel>>(null, "کاربر یافت نشد");
            }
            List<UserLoan> source = await _userLoanService.GetByUserId(_appContext.User.Id, "Loan", "User", "UserLoanPayments");
            List<UserLoanModel> models = new List<UserLoanModel>();
            if (source.Any())
            {
                models = (from x in source
                          select new UserLoanModel
                          {
                              Id = x.Id,
                              Code = x.Code,
                              LoanModel = new Loan2Model
                              {
                                  Id = x.Loan.Id,
                                  Title = x.Loan.Title,
                                  DelayPercent = x.Loan.DelayPercent,
                                  EarlyPercent = x.Loan.EarlyPercent,
                                  ProfitPercent = x.Loan.ProfitPercent
                              },
                              MemberModel = new Member2Model(x.User),
                              Amount = x.Amount,
                              PayableAmount = x.PayableAmount,
                              Installment = x.Installment,
                              Month = x.Month,
                              MonthGap = x.MonthGap,
                              Paid = x.Paid,
                              Discount = x.Discount,
                              StartDate = x.StartDate,
                              Description = x.Description,
                              PaidCount = x.UserLoanPayments.Count((UserLoanPayment userLoanPayment) => userLoanPayment.SumAmount > 0m),
                              IsCheckouted = x.IsCheckouted,
                              CheckoutDate = x.CheckoutDate,
                              PreDepositAmount = x.PreDepositAmount,
                              LoanPayments = x.UserLoanPayments.Select((UserLoanPayment p) => new LoanPaymentModel(p)).ToList()
                          } into x
                          orderby x.StartDate descending
                          select x).ToList();
                foreach (UserLoanModel item in models)
                {
                    await _userPaymentService.SetPaymentsDelays(item.LoanPayments);
                }
            }
            return Success(models);
        });
    }

    [HttpGet("[action]/{id}")]
    public Task<ResponseState<decimal>> GetCheckoutAmount(Guid id)
    {
        return TryCatch(async () =>
        {
            FacadeResponse<(decimal, decimal)> facadeResponse = await _userLoanFacade.GetCheckoutAmount(id);
            return (!facadeResponse.Success) ? Error(0m, facadeResponse.Message) : Success(facadeResponse.Data.Item1);
        });
    }
}
