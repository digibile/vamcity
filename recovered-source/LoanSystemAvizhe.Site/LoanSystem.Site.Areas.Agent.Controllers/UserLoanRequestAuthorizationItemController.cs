using System;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Site.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoanSystem.Site.Areas.Agent.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Agent,Admin")]
public class UserLoanRequestAuthorizationItemController : BaseAgentController
{
    private readonly IUserLoanRequestAuthorizationItemService _userLoanRequestAuthorizationItemService;

    private readonly ICustomerService _customerService;

    private readonly ICurrentUserService _currentUserService;

    private readonly IntegratedSmsSenderService _integratedSmsSenderService;

    public UserLoanRequestAuthorizationItemController(ILogger<UserLoanRequestAuthorizationItemController> logger, IUserLoanRequestAuthorizationItemService userLoanRequestAuthorizationItemService, ICustomerService customerService, ICurrentUserService currentUserService, IntegratedSmsSenderService integratedSmsSenderService)
        : base(logger)
    {
        _userLoanRequestAuthorizationItemService = userLoanRequestAuthorizationItemService;
        _customerService = customerService;
        _currentUserService = currentUserService;
        _integratedSmsSenderService = integratedSmsSenderService;
    }

    [HttpPost("[action]")]
    public Task<DefaultResponseState> SignContractWithOtp([FromBody] SignContractWithOtp model)
    {
        return TryCatch(async () =>
        {
            UserLoanRequestAuthorizationItem entity = (await _userLoanRequestAuthorizationItemService.GetByCondition((UserLoanRequestAuthorizationItem x) => x.Id == model.Id && x.UserLoanRequest.CustomerId == _currentUserService.CustomerId, "UserLoanRequest.User", "UserLoanRequest.Agent", "UserLoanRequest.Loan", "AuthorizationItem")).FirstOrDefault();
            if (entity == null)
            {
                return Error("آیتم قرارداد یافت نشد.");
            }
            string text = await _userLoanRequestAuthorizationItemService.ValidateOtpAndSignContract(entity, model.Otp);
            if (!string.IsNullOrWhiteSpace(text))
            {
                return Error(text);
            }
            try
            {
                await _integratedSmsSenderService.SendDigitalSignComplete(new UserLoanRequestAuthorizationItemModel(entity));
            }
            catch (Exception)
            {
            }
            return Success("عملیات با موفقیت انجام شد.");
        });
    }
}
