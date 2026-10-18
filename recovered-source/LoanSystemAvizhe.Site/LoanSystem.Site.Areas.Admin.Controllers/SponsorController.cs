using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LoanSystem.Core.Configuration;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Services.Users;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Data.Enums;
using LoanSystemAvizhe.Services.FileHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LoanSystem.Site.Areas.Admin.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Admin")]
[RequestFormLimits(ValueCountLimit = 8192)]
public class SponsorController : BaseAdminController
{
    private readonly ISponsorService _sponsorService;

    private readonly IAgentLoanService _agentLoanService;

    private readonly ITransactionService _transactionService;

    private readonly IFileHelper _fileHelper;

    private readonly ISubAgentTransactionService _subAgentTransactionService;

    private readonly IUserLoanService _userLoanService;

    private readonly IUserLoanRequestService _userLoanRequestService;

    private readonly ICurrentUserService _currentUserService;

    private readonly ICustomerService _customerService;

    private readonly UserManager _userManager;

    private readonly AppConfig _appConfig;

    public SponsorController(ILogger<SponsorController> logger, IOptions<AppConfig> appConfig, ISponsorService sponsorService, IFileHelper fileHelper, ICurrentUserService currentUserService, IUserLoanService userLoanService, IUserLoanRequestService userLoanRequestService, UserManager userManager, ICustomerService customerService, ITransactionService transactionService, IAgentLoanService agentLoanService, ISubAgentTransactionService subAgentTransactionService)
        : base(logger)
    {
        _sponsorService = sponsorService;
        _currentUserService = currentUserService;
        _appConfig = appConfig.Value;
        _userLoanService = userLoanService;
        _userLoanRequestService = userLoanRequestService;
        _userManager = userManager;
        _customerService = customerService;
        _transactionService = transactionService;
        _subAgentTransactionService = subAgentTransactionService;
        _agentLoanService = agentLoanService;
        _fileHelper = fileHelper;
    }

    [HttpGet("[action]")]
    public Task<ResponseState<List<SponsorModel>>> List()
    {
        return TryCatch(async () =>
        {
            List<UserLoanRequest> userLoanRequests = await _userLoanRequestService.GetByCondition((UserLoanRequest x) => x.SponsorId != (long?)null, false);
            List<Guid> userLoanRequestIds = userLoanRequests.Select((UserLoanRequest x) => x.Id).ToList();
            List<UserLoan> userLoans = await _userLoanService.GetByCondition((UserLoan x) => x.UserLoanRequestId != null && userLoanRequestIds.Contains((Guid)x.UserLoanRequestId));
            List<SponsorModel> data = (from x in await _sponsorService.GetAllSponsors()
                                       select new SponsorModel
                                       {
                                           Id = x.Id,
                                           UserName = x.UserName,
                                           FirstName = x.FirstName,
                                           LastName = x.LastName,
                                           Mobile = x.Mobile,
                                           Phone = x.Phone,
                                           LastLoginDate = x.LastLoginDate,
                                           BusinessName = x.BusinessName,
                                           IsActive = x.IsActive,
                                           BankAccountsJson = x.BankAccountsJson,
                                           City = x.City,
                                           DynamicLoanRequstCounts = CalcLoanRequstCounts(userLoanRequests, x.Id),
                                           LoanCount = userLoans.Count((UserLoan ul) => ul.UserLoanRequestId.HasValue && (from ulr in userLoanRequests
                                                                                                                          where ulr.SponsorId == x.Id
                                                                                                                          select ulr.Id).ToList().Contains(ul.UserLoanRequestId.Value)),
                                           TotalLoans = userLoans.Where((UserLoan ul) => ul.UserLoanRequestId.HasValue && (from ulr in userLoanRequests
                                                                                                                           where ulr.SponsorId == x.Id
                                                                                                                           select ulr.Id).ToList().Contains(ul.UserLoanRequestId.Value)).Sum((UserLoan ul) => ul.PayableAmount),
                                           SponsorLoans = x.AgentLoans?.Select((AgentLoan ag) => new AgentLoanModel(ag)).ToList()
                                       } into x
                                       orderby x.Id descending
                                       select x).ToList();
            return Success(data);
        });
    }

    [HttpGet("[action]")]
    public Task<ResponseState<List<SponsorModel>>> ListLight()
    {
        return TryCatch(async () =>
        {
            List<SponsorModel> data = (from x in await _sponsorService.GetAllSponsors()
                                       select new SponsorModel
                                       {
                                           Id = x.Id,
                                           FirstName = x.FirstName,
                                           LastName = x.LastName,
                                           Mobile = x.Mobile,
                                           Phone = x.Phone,
                                           BusinessName = x.BusinessName,
                                           IsActive = x.IsActive
                                       } into x
                                       orderby x.Id descending
                                       select x).ToList();
            return Success(data);
        });
    }

    [HttpGet("[action]")]
    public Task<ResponseState<List<SponsorModel>>> ListByLoanId(Guid loanId)
    {
        return TryCatch(async () =>
        {
            List<long> sponsorIds = (await _agentLoanService.GetByCondition((AgentLoan x) => x.LoanId == loanId)).Select((AgentLoan x) => x.AgentId).ToList();
            List<User> source = (await _sponsorService.GetAllSponsors()).Where((User x) => sponsorIds.Contains(x.Id)).ToList();
            List<SponsorModel> data = (from x in source
                                       select new SponsorModel
                                       {
                                           Id = x.Id,
                                           FirstName = x.FirstName,
                                           LastName = x.LastName,
                                           Mobile = x.Mobile,
                                           Phone = x.Phone,
                                           BusinessName = x.BusinessName,
                                           IsActive = x.IsActive
                                       } into x
                                       orderby x.Id descending
                                       select x).ToList();
            return Success(data);
        });
    }

    private List<dynamic> CalcLoanRequstCounts(List<UserLoanRequest> userLoanRequests, long sponsorId)
    {
        List<object> list = new List<object>();
        foreach (UserLoanSponsorStatuses status in (from x in userLoanRequests
                                                    where x.SponsorId == sponsorId
                                                    select x.UserLoanSponsorStatus).ToList())
        {
            list.Add(new
            {
                title = status.ToString().Replace("_", ""),
                count = userLoanRequests.Count((UserLoanRequest x) => x.SponsorId == sponsorId && x.UserLoanSponsorStatus == status)
            });
        }
        list.Add(new
        {
            title = "تعداد کل",
            count = userLoanRequests.Count((UserLoanRequest x) => x.SponsorId == sponsorId)
        });
        return list;
    }

    [HttpGet("[action]/{id}")]
    public Task<ResponseState<SponsorModel>> Get(long id)
    {
        return TryCatch(async () =>
        {
            SponsorModel data = new SponsorModel
            {
                SponsorLoans = new List<AgentLoanModel>(),
                IsActive = true
            };
            if (id != 0L)
            {
                User user = await _sponsorService.GetById(id, "AgentLoans", "AgentMetaData");
                if (user == null)
                {
                    return Error<SponsorModel>(null, "اطلاعات مورد نظر یافت نشد");
                }
                data = new SponsorModel(user)
                {
                    Password = string.Empty,
                    ConfirmPassword = string.Empty
                };
            }
            return Success(data);
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<long>> Create([FromForm] SponsorModel model)
    {
        return TryCatch(async () =>
        {
            Customer customer = await _customerService.GetById(_currentUserService.CustomerId.Value);
            model.UserName = GenerateUserName(customer.Code, model);
            if (await _userManager.FindByUserName(model.UserName) != null)
            {
                return Error(0L, "نام کاربری وارد شده تکراری است.");
            }
            await _userManager.GetById(_currentUserService.UserId.Value);
            User entity = new User
            {
                Id = model.Id,
                UserName = model.UserName,
                FirstName = model.FirstName,
                Address = model.Address,
                Email = model.Email,
                PostalCode = model.PostalCode,
                LastLoginDate = model.LastLoginDate,
                Mobile = model.Mobile,
                Phone = model.Phone,
                BusinessName = model.BusinessName,
                RegDate = DateTime.Now,
                IsActive = model.IsActive,
                BankAccountsJson = model.BankAccountsJson,
                City = model.City,
                SmsActiveForMembers = model.SmsActiveForMembers,
                AgentMetaData = ((model.MetaData != null) ? new AgentMetaData
                {
                    CustomerId = customer.Id,
                    SiteUrl = model.MetaData.SiteUrl,
                    AgentType = model.MetaData.SponsorType,
                    IsOnline = model.MetaData.IsOnline,
                    IsPhysical = model.MetaData.IsPhysical,
                    LastAdminChangeDateTime = DateTime.Now,
                    CanConvertRequestToLoan = model.MetaData.CanConvertRequestToLoan
                } : null)
            };
            entity.CustomerId = _currentUserService.CustomerId.Value;
            entity.UserRoleNames = new List<string> { "Sponsor" };
            PasswordHasher<User> passwordHasher = new PasswordHasher<User>();
            string passwordHash = passwordHasher.HashPassword(entity, model.Password);
            entity.PasswordHash = passwordHash;
            IdentityResult identityResult = await _userManager.Insert(entity, model.Password, entity.UserRoleNames);
            if (identityResult.Succeeded)
            {
                List<AgentLoan> agentLoans = ((model.SponsorLoans?.Any() ?? false) ? model.SponsorLoans.Select((AgentLoanModel x) =>
                {
                    AgentLoan agentLoan = new AgentLoan
                    {
                        Id = x.Id,
                        AgentId = entity.Id,
                        CustomerId = _currentUserService.CustomerId.Value,
                        LoanId = x.LoanId,
                        Percent = x.Percent,
                        DefaultCheckoutDays = x.DefaultCheckoutDays,
                        ContactId = x.Contact?.Id,
                        ContactPercent = x.ContactPercent,
                        ProductPercent = x.ProductPercent,
                        ProductCheckoutDays = x.ProductCheckoutDays
                    };
                    List<OtherPercent> otherPercents = x.OtherPercents;
                    agentLoan.OtherPercentsJson = ((otherPercents != null && otherPercents.Any()) ? JsonSerializer.Serialize(x.OtherPercents) : null);
                    return agentLoan;
                }).ToList() : new List<AgentLoan>());
                await _agentLoanService.Modify(agentLoans, entity.Id);
                return Success(entity.Id, "عملیات با موفقیت انجام شد.");
            }
            return Error(500L, string.Join(".", identityResult.Errors.Select((IdentityError x) => x.Description)));
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<long>> Edit([FromForm] SponsorModel model)
    {
        return TryCatch(async () =>
        {
            User currentUser = await _userManager.GetById(_currentUserService.UserId.Value);
            Customer customer = await _customerService.GetById(_currentUserService.CustomerId.Value);
            model.UserName = GenerateUserName(customer.Code, model);
            User user = await _userManager.FindByUserName(model.UserName);
            if (user != null && user.Id != model.Id)
            {
                return Error(0L, "نام کاربری وارد شده تکراری است.");
            }
            User entity = await _sponsorService.GetById(model.Id, "AgentMetaData");
            if (model.MetaData != null)
            {
                if (entity.AgentMetaData == null)
                {
                    entity.AgentMetaData = new AgentMetaData();
                }
                entity.AgentMetaData.CustomerId = customer.Id;
                entity.AgentMetaData.SiteUrl = model.MetaData.SiteUrl;
                entity.AgentMetaData.IsOnline = model.MetaData.IsOnline;
                entity.AgentMetaData.IsPhysical = model.MetaData.IsPhysical;
                entity.AgentMetaData.CheckedBy = currentUser?.FullName;
                entity.AgentMetaData.AgentType = model.MetaData.SponsorType;
                entity.AgentMetaData.CanConvertRequestToLoan = model.MetaData.CanConvertRequestToLoan;
            }
            entity.Id = model.Id;
            entity.UserName = model.UserName;
            entity.FirstName = model.FirstName;
            entity.LastName = model.LastName;
            entity.LastLoginDate = model.LastLoginDate;
            entity.BusinessName = model.BusinessName;
            entity.Mobile = model.Mobile;
            entity.Phone = model.Phone;
            entity.Address = model.Address;
            entity.Email = model.Email;
            entity.PostalCode = model.PostalCode;
            entity.IsActive = model.IsActive;
            entity.CustomerId = _currentUserService.CustomerId.Value;
            entity.BankAccountsJson = model.BankAccountsJson;
            entity.City = model.City;
            entity.SmsActiveForMembers = model.SmsActiveForMembers;
            if (!string.IsNullOrEmpty(model.Password))
            {
                PasswordHasher<User> passwordHasher = new PasswordHasher<User>();
                string passwordHash = passwordHasher.HashPassword(entity, model.Password);
                entity.PasswordHash = passwordHash;
            }
            try
            {
                await _userManager.UpdateAsync(entity);
                List<AgentLoan> agentLoans = ((model.SponsorLoans?.Any() ?? false) ? model.SponsorLoans.Select((AgentLoanModel x) =>
                {
                    AgentLoan agentLoan = new AgentLoan
                    {
                        Id = x.Id,
                        AgentId = x.AgentId,
                        CustomerId = _currentUserService.CustomerId.Value,
                        LoanId = x.LoanId,
                        Percent = x.Percent,
                        DefaultCheckoutDays = x.DefaultCheckoutDays,
                        ContactId = x.Contact?.Id,
                        ContactPercent = x.ContactPercent,
                        ProductPercent = x.ProductPercent,
                        ProductCheckoutDays = x.ProductCheckoutDays
                    };
                    List<OtherPercent> otherPercents = x.OtherPercents;
                    agentLoan.OtherPercentsJson = ((otherPercents != null && otherPercents.Any()) ? JsonSerializer.Serialize(x.OtherPercents) : null);
                    return agentLoan;
                }).ToList() : new List<AgentLoan>());
                await _agentLoanService.Modify(agentLoans, entity.Id);
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
            if ((await _userLoanRequestService.GetByCondition((UserLoanRequest x) => x.SponsorId == (long?)id, false)).Any())
            {
                return Error("این اسپانسر دارای وام می باشد و قابل حذف  نمی باشد. برای حذف اسپانسر ابتدا درخواست وام های ثبت شده را حذف نمایید.");
            }
            User user = await _sponsorService.GetById(id);
            if (user == null)
            {
                return Error("امکان حذف وجود ندارد. لطفا بعدا دوباره تلاش نمایید");
            }
            await _sponsorService.Delete(user);
            return Success();
        });
    }

    private string GenerateUserName(int customerCode, SponsorModel model)
    {
        if (_appConfig.IsCloud)
        {
            return $"{customerCode}-s-{model.Mobile}";
        }
        return "s" + model.Mobile;
    }
}
