using System.Threading.Tasks;
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

namespace LoanSystem.Site.Areas.Member.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Member,Admin,Agent")]
public class UserController : BaseMemberController
{
    private readonly IAppContext _appContext;

    private readonly ICurrentUserService _currentUserService;

    private readonly IThirdPartyTransferService _thirdPartyTransferService;

    private readonly IAppSettingsService _appSettingsService;

    private readonly UserManager _userManager;

    public UserController(ILogger<UserController> logger, UserManager userManager, IAppContext appContext, ICurrentUserService currentUserService, IThirdPartyTransferService thirdPartyTransferService, IAppSettingsService appSettingsService)
        : base(logger)
    {
        _userManager = userManager;
        _appContext = appContext;
        _currentUserService = currentUserService;
        _thirdPartyTransferService = thirdPartyTransferService;
        _appSettingsService = appSettingsService;
    }

    [HttpGet("[action]")]
    [Authorize(Roles = "Member")]
    public Task<ResponseState<UserModel>> Get()
    {
        return TryCatch(async () =>
        {
            if (_appContext.User == null)
            {
                return Error<UserModel>(null, "کاربر یافت نشد");
            }
            UserModel data = new UserModel();
            if (_appContext.User.Id != 0L)
            {
                User user = await _userManager.GetById(_appContext.User.Id);
                if (user == null)
                {
                    return Error<UserModel>(null, "کاربر مورد نظر یافت نشد");
                }
                data = new UserModel(user)
                {
                    Password = "",
                    ConfirmPassword = ""
                };
            }
            return Success(data);
        });
    }

    [HttpGet("[action]/{userId?}")]
    public Task<ResponseState<decimal>> LoadUserCredit(long? userId)
    {
        return TryCatch(async () =>
        {
            if (_currentUserService.IsInRole("Member") && userId.HasValue && _currentUserService.UserId != userId)
            {
                return Error(0m, "دسترسی غیرمجاز");
            }
            long? num = userId ?? _appContext.User?.Id;
            if (!num.HasValue)
            {
                return Error(0m);
            }
            if (!_currentUserService.IsInRole("Admin") && !_currentUserService.IsInRole("Agent") && userId.HasValue && userId.Value != _appContext.User?.Id)
            {
                return Error(0m);
            }
            User user = await _userManager.GetById(num.Value);
            return (user == null) ? Error(0m) : Success(user.Credit);
        });
    }

    [HttpPost("[action]")]
    [Authorize(Roles = "Member")]
    public Task<ResponseState<long>> Edit([FromBody] UserModel model)
    {
        return TryCatch(async () =>
        {
            if (!(await _appSettingsService.GetAppSettings()).MemberCanChangeProfile)
            {
                return Error(0L, "در حال حاضر امکان ویرایش پروفایل کاربری غیر فعال است.");
            }
            model.Id = _appContext.User.Id;
            User user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null && model.Id != user.Id)
            {
                return Error(0L, "ایمیل وارد شده تکراری است.");
            }
            user = await _userManager.FindByUserName(model.UserName);
            if (user != null && model.Id != user.Id)
            {
                return Error(0L, "نام کاربری وارد شده تکراری است.");
            }
            User user2 = await _userManager.GetById(model.Id);
            user2.Id = model.Id;
            user2.FirstName = model.FirstName;
            user2.LastName = model.LastName;
            user2.Email = model.Email;
            user2.FatherName = model.FatherName;
            user2.Phone = model.Phone;
            user2.Address = model.Address;
            user2.PostalCode = model.PostalCode;
            user2.City = model.City;
            user2.BankAccountsJson = model.BankAccountsJson;
            if (!string.IsNullOrEmpty(model.Password))
            {
                PasswordHasher<User> passwordHasher = new PasswordHasher<User>();
                string passwordHash = passwordHasher.HashPassword(user2, model.Password);
                user2.PasswordHash = passwordHash;
            }
            await _userManager.UpdateAsync(user2);
            try
            {
                await _thirdPartyTransferService.RegisterOrUpdateDigibileMember(user2.Id.ToString(), model.Password);
            }
            catch
            {
            }
            return Success(user2.Id, "عملیات با موفقیت انجام شد.");
        });
    }
}
