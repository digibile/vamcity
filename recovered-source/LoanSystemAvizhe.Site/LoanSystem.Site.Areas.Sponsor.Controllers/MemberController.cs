using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Core.Configuration;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Services.Users;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LoanSystem.Site.Areas.Sponsor.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Sponsor")]
public class MemberController : BaseSponsorController
{
    private readonly IMemberService _memberService;

    private readonly ICurrentUserService _currentUserService;

    private readonly UserManager _userManager;

    private readonly IUserLoanService _userLoanService;

    private readonly ICustomerService _customerService;

    private readonly IAppSettingsService _appSettingsService;

    private readonly IAppSmsService _appSmsService;

    private readonly IntegratedSmsSenderService _integratedSmsSenderService;

    private readonly IntegratedEmailSenderService _integratedEmailSenderService;

    private readonly AppConfig _appConfig;

    public MemberController(ILogger<MemberController> logger, IOptions<AppConfig> appConfig, IMemberService memberService, ICurrentUserService currentUserService, IUserLoanService userLoanService, UserManager userManager, ICustomerService customerService, IAppSettingsService appSettingsService, IAppSmsService appSmsService, IntegratedSmsSenderService integratedSmsSenderService, IntegratedEmailSenderService integratedEmailSenderService)
        : base(logger)
    {
        _memberService = memberService;
        _currentUserService = currentUserService;
        _appConfig = appConfig.Value;
        _userLoanService = userLoanService;
        _userManager = userManager;
        _customerService = customerService;
        _appSettingsService = appSettingsService;
        _appSmsService = appSmsService;
        _integratedSmsSenderService = integratedSmsSenderService;
        _integratedEmailSenderService = integratedEmailSenderService;
    }

    [HttpGet("[action]/{nationalCode}")]
    public Task<ResponseState<MemberModel>> GetByNationalCode(string nationalCode)
    {
        return TryCatch(async () =>
        {
            MemberModel model = new MemberModel();
            User user = await _memberService.GetByNationalCode(nationalCode);
            if (user == null)
            {
                return Success(model);
            }
            model.Id = user.Id;
            model.FirstName = user.FirstName;
            model.LastName = user.LastName;
            model.NationalCode = user.NationalCode;
            return Success(model);
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<long>> CreateAsThirdPerson([FromBody] MemberModel model)
    {
        return TryCatch(async () =>
        {
            Customer customer = await _customerService.GetById(_currentUserService.CustomerId.Value);
            model.UserName = GenerateUserName(customer.Code, model);
            if (await _userManager.FindByUserName(model.UserName) != null)
            {
                return Error(0L, "نام کاربری وارد شده تکراری است.");
            }
            User user = await _memberService.GetByNationalCode(model.NationalCode);
            if (user != null && user.CustomerId == customer.Id)
            {
                return Error(0L, "کد ملی وارد شده تکراری است.");
            }
            if (string.IsNullOrEmpty(model.MobileOtp))
            {
                return Error(0L, "کد تاییدیه موبایل وارد نشده است.");
            }
            List<AppSms> list = await _appSmsService.GetByCondition((AppSms x) => x.To == model.Mobile && (int)x.AppSmsType == 8 && x.Date >= DateTime.Now.AddMinutes(-2.0) && x.Tag == model.MobileOtp, false);
            if (list == null || !list.Any())
            {
                return Error(0L, "کد تاییدیه موبایل معتبر نمی باشد.");
            }
            User entity = new User
            {
                Id = model.Id,
                UserName = model.UserName,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Address = model.Address,
                PostalCode = model.PostalCode,
                Gender = model.Gender,
                LastLoginDate = model.LastLoginDate,
                Mobile = model.Mobile,
                MobileVerified = true,
                Email = model.Email,
                BirthDate = model.BirthDate,
                Phone = model.Phone,
                NationalCode = model.NationalCode,
                RegDate = DateTime.Now,
                RegDate2 = model.RegDate2,
                Familiar1FirstName = model.Familiar1FirstName,
                Familiar1LastName = model.Familiar1LastName,
                Familiar1Mobile = model.Familiar1Mobile,
                Familiar2FirstName = model.Familiar1FirstName,
                Familiar2LastName = model.Familiar2LastName,
                Familiar2Mobile = model.Familiar2Mobile,
                FatherName = model.FatherName,
                City = model.City,
                BankAccountsJson = model.BankAccountsJson,
                IsActive = true,
                AgentId = model.AgentId,
                UserCode = model.UserCode
            };
            entity.CustomerId = _currentUserService.CustomerId.Value;
            entity.UserRoleNames = new List<string> { "Member" };
            PasswordHasher<User> passwordHasher = new PasswordHasher<User>();
            string passwordHash = passwordHasher.HashPassword(entity, (!string.IsNullOrEmpty(model.Password)) ? model.Password : model.NationalCode);
            entity.PasswordHash = passwordHash;
            IdentityResult identityResult = await _userManager.Insert(entity, (!string.IsNullOrEmpty(model.Password)) ? model.Password : model.NationalCode, entity.UserRoleNames);
            return identityResult.Succeeded ? Success(entity.Id, "عملیات با موفقیت انجام شد.") : Error(500L, string.Join(".", identityResult.Errors.Select((IdentityError x) => x.Description)));
        });
    }

    private string GenerateUserName(int customerCode, MemberModel model)
    {
        if (_appConfig.IsCloud)
        {
            return $"{customerCode}-{model.NationalCode}";
        }
        return model.NationalCode;
    }
}
