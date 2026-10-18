using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Core.Configuration;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Services.Users;
using LoanSystem.Site.Areas.Admin.Controllers;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LoanSystemAvizhe.Site.Areas.Admin.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Admin")]
public class CustomerMarketerController : BaseAdminController
{
    private readonly ICustomerMarketerService _customerMarketerService;

    private readonly ITransactionService _transactionService;

    private readonly ICustomerMarketerCommissionService _customerMarketerCommissionService;

    private readonly IUserLoanRequestService _userLoanRequestService;

    private readonly IMemberService _memberService;

    private readonly ICurrentUserService _currentUserService;

    private readonly ICustomerService _customerService;

    private readonly IVisitorLogService _visitorLogService;

    private readonly UserManager _userManager;

    private readonly AppConfig _appConfig;

    public CustomerMarketerController(ILogger<CustomerMarketerController> logger, IOptions<AppConfig> appConfig, ICustomerMarketerService customerMarketerService, IUserLoanRequestService userLoanRequestService, UserManager userManager, ICustomerMarketerCommissionService customerMarketerCommissionService, ITransactionService transactionService, IMemberService memberService, ICurrentUserService currentUserService, ICustomerService customerService, IVisitorLogService visitorLogService)
        : base(logger)
    {
        _customerMarketerService = customerMarketerService;
        _appConfig = appConfig.Value;
        _userManager = userManager;
        _customerMarketerCommissionService = customerMarketerCommissionService;
        _transactionService = transactionService;
        _userLoanRequestService = userLoanRequestService;
        _memberService = memberService;
        _currentUserService = currentUserService;
        _customerService = customerService;
        _visitorLogService = visitorLogService;
    }

    [HttpGet("[action]")]
    public Task<ResponseState<List<CustomerMarketerModel>>> List()
    {
        return TryCatch(async () =>
        {
            List<User> marketers = await _customerMarketerService.GetAllMarketers();
            if (marketers == null || !marketers.Any())
            {
                return Success(new List<CustomerMarketerModel>());
            }
            List<long> marketerIds = marketers.Select((User x) => x.Id).ToList();
            List<CustomerMarketerCommission> commissions = await _customerMarketerCommissionService.GetByCondition((CustomerMarketerCommission x) => marketerIds.Contains(x.MarketerId));
            List<Guid> commissionIds = commissions.Select((CustomerMarketerCommission x) => x.Id).ToList();
            List<Transaction> withdraws = await _transactionService.GetByCondition((Transaction x) => x.IsWithdraw && x.CustomerMarketerCommissionId.HasValue && commissionIds.Contains((Guid)x.CustomerMarketerCommissionId));
            List<User> members = await _memberService.GetAllMembers();
            List<UserLoanRequest> userLoanRequests = await _userLoanRequestService.GetByCondition((UserLoanRequest x) => x.User.CustomerMarketerId.HasValue && marketerIds.Contains((long)x.User.CustomerMarketerId), false, "User");
            List<CustomerMarketerModel> list = marketers.Select((User x) => new CustomerMarketerModel(x)).ToList();
            list.ForEach(async (CustomerMarketerModel marketer) =>
            {
                IEnumerable<CustomerMarketerCommission> currentMarketerCommissions = commissions.Where((CustomerMarketerCommission c) => c.MarketerId == marketer.Id);
                marketer.ReffererCount = _visitorLogService.GetCountByMrkCode(marketer.AffiliateCode);
                marketer.TotalCommission = currentMarketerCommissions.Sum((CustomerMarketerCommission c) => c.Amount);
                marketer.TotalReceives = withdraws.Where((Transaction x) => x.CustomerMarketerCommissionId.HasValue && currentMarketerCommissions.Select((CustomerMarketerCommission com) => com.Id).ToList().Contains(x.CustomerMarketerCommissionId.Value)).Sum((Transaction c) => c.Amount);
                marketer.RegisterCount = members.Count((User x) => x.CustomerMarketerId == marketer.Id);
                marketer.LoanRequstCounts = userLoanRequests.Count((UserLoanRequest x) => x.User.CustomerMarketerId == marketer.Id);
                marketer.UserLoanCounts = currentMarketerCommissions.Select((CustomerMarketerCommission x) => x.UserLoanId).Distinct().Count();
            });
            return Success(list);
        });
    }

    [HttpGet("[action]/{id}")]
    public Task<ResponseState<CustomerMarketerModel>> Get(long id)
    {
        return TryCatch(async () =>
        {
            CustomerMarketerModel data = new CustomerMarketerModel
            {
                IsActive = true
            };
            if (id != 0L)
            {
                User user = await _customerMarketerService.GetById(id);
                if (user == null)
                {
                    return Error<CustomerMarketerModel>(null, "اطلاعات مورد نظر یافت نشد");
                }
                data = new CustomerMarketerModel(user)
                {
                    Password = string.Empty,
                    ConfirmPassword = string.Empty
                };
            }
            return Success(data);
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<long>> Create([FromBody] CustomerMarketerModel model)
    {
        return TryCatch(async () =>
        {
            Customer customer = await _customerService.GetById(_currentUserService.CustomerId.Value);
            model.UserName = GenerateUserName(customer.Code, model);
            if (await _userManager.FindByUserName(model.UserName) != null)
            {
                return Error(0L, "نام کاربری وارد شده تکراری است.");
            }
            if (await _userManager.FindByAffiliateCode(model.AffiliateCode, _currentUserService.CustomerId) != null)
            {
                return Error(0L, "کد بازاریابی وارد شده تکراری است.");
            }
            User entity = new User
            {
                Id = model.Id,
                CustomerId = customer.Id,
                UserName = model.UserName,
                FirstName = model.FirstName,
                LastName = model.LastName,
                BusinessName = model.BusinessName,
                NationalCode = model.NationalCode,
                Gender = model.Gender,
                LastLoginDate = model.LastLoginDate,
                Mobile = model.Mobile,
                RegDate = DateTime.Now,
                IsActive = true,
                Email = model.Email,
                AffiliateCode = model.AffiliateCode,
                CommissionPercent = model.CommissionPercent,
                BankAccountsJson = model.BankAccountsJson
            };
            entity.UserRoleNames = new List<string> { "CustomerMarketer" };
            PasswordHasher<User> passwordHasher = new PasswordHasher<User>();
            string passwordHash = passwordHasher.HashPassword(entity, model.Password);
            entity.PasswordHash = passwordHash;
            IdentityResult identityResult = await _userManager.Insert(entity, model.ConfirmPassword, entity.UserRoleNames);
            return identityResult.Succeeded ? Success(entity.Id, "عملیات با موفقیت انجام شد.") : Error(0L, string.Join(".", identityResult.Errors.Select((IdentityError x) => x.Description)));
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<long>> Edit([FromBody] CustomerMarketerModel model)
    {
        return TryCatch(async () =>
        {
            Customer customer = await _customerService.GetById(_currentUserService.CustomerId.Value);
            model.UserName = GenerateUserName(customer.Code, model);
            User user = await _userManager.FindByUserName(model.UserName);
            if (user != null && user.Id != model.Id)
            {
                return Error(0L, "نام کاربری وارد شده تکراری است.");
            }
            user = await _userManager.FindByAffiliateCode(model.AffiliateCode);
            if (user != null && user.Id != model.Id)
            {
                return Error(0L, "کد بازاریابی وارد شده تکراری است.");
            }
            User entity = await _customerMarketerService.GetById(model.Id);
            entity.Id = model.Id;
            entity.UserName = model.UserName;
            entity.BusinessName = model.BusinessName;
            entity.FirstName = model.FirstName;
            entity.LastName = model.LastName;
            entity.NationalCode = model.NationalCode;
            entity.Gender = model.Gender;
            entity.LastLoginDate = model.LastLoginDate;
            entity.Mobile = model.Mobile;
            entity.AffiliateCode = model.AffiliateCode;
            entity.CommissionPercent = model.CommissionPercent;
            entity.Email = model.Email;
            entity.BankAccountsJson = model.BankAccountsJson;
            if (!string.IsNullOrEmpty(model.Password))
            {
                PasswordHasher<User> passwordHasher = new PasswordHasher<User>();
                string passwordHash = passwordHasher.HashPassword(entity, model.Password);
                entity.PasswordHash = passwordHash;
            }
            try
            {
                await _userManager.UpdateAsync(entity);
            }
            catch (Exception)
            {
                throw;
            }
            return Success(entity.Id, "عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]/{id}")]
    public Task<DefaultResponseState> Delete(long id)
    {
        return TryCatch(async () =>
        {
            User user = await _customerMarketerService.GetById(id);
            if (user == null)
            {
                return Error("امکان حذف وجود ندارد. لطفا بعدا دوباره تلاش نمایید");
            }
            await _customerMarketerService.Delete(user);
            return Success();
        });
    }

    private string GenerateUserName(int customerCode, CustomerMarketerModel model)
    {
        if (_appConfig.IsCloud)
        {
            return $"{customerCode}-m-{model.Mobile}";
        }
        return "m" + model.Mobile;
    }
}
