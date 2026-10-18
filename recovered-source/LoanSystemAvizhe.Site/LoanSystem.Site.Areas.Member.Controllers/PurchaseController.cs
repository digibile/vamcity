using System;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Site.Areas.Member.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoanSystem.Site.Areas.Member.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Member,Admin,Agent")]
public class PurchaseController : BaseMemberController
{
    private readonly IUserPaymentService _userPaymentService;

    private readonly IMemberPurchaseService _memberPurchaseService;

    public PurchaseController(ILogger<PurchaseController> logger, IUserPaymentService userPaymentService, IMemberPurchaseService memberPurchaseService)
        : base(logger)
    {
        _userPaymentService = userPaymentService;
        _memberPurchaseService = memberPurchaseService;
    }

    [HttpPost("[action]")]
    public Task<ResponseState<MemberPurchaseCreateResult>> Create([FromBody] MemberPurchaseModel model)
    {
        return TryCatch(async () =>
        {
            new MemberPurchaseCreateResult();
            MemberPurchaseCreateResult data;
            try
            {
                if (model.UserLoanPayment != null)
                {
                    string text = await _userPaymentService.ValidateLoanPayment(new UserLoanPayment
                    {
                        Id = model.UserLoanPayment.Id,
                        InstallmentNumber = model.UserLoanPayment.InstallmentNumber
                    });
                    if (!string.IsNullOrEmpty(text))
                    {
                        return Error<MemberPurchaseCreateResult>(null, text);
                    }
                }
                data = await _memberPurchaseService.Create(model);
            }
            catch (Exception)
            {
                return Error<MemberPurchaseCreateResult>(null, "در هدایت به درگاه خطایی رخ داده است");
            }
            return Success(data);
        });
    }
}
