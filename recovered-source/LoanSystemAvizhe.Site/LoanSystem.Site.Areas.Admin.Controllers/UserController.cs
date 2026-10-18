using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LoanSystem.Site.Areas.Admin.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Admin")]
public class UserController : BaseAdminController
{
    private readonly UserManager _userManager;

    private readonly ICurrentUserService _currentUserService;

    private readonly ICustomerService _customerService;

    private readonly AppConfig _appConfig;

    public UserController(ILogger<UserController> logger, IOptions<AppConfig> appConfig, UserManager userManager, ICurrentUserService currentUserService, ICustomerService customerService)
        : base(logger)
    {
        _userManager = userManager;
        _appConfig = appConfig.Value;
        _customerService = customerService;
        _currentUserService = currentUserService;
    }

    [HttpGet("[action]")]
    public Task<ResponseState<List<UserModel>>> List()
    {
        return TryCatch(async () =>
        {
            List<User> source = (await _userManager.GetUsersInRoleAsync("Admin")).ToList();
            List<UserModel> data = source.Select((User x) => new UserModel(x)).ToList();
            return Success(data);
        });
    }

    [HttpGet("[action]/{id}")]
    public Task<ResponseState<UserModel>> Get(long id)
    {
        return TryCatch(async () =>
        {
            UserModel userModel = new UserModel();
            if (id != 0L)
            {
                User user = await _userManager.GetById(id);
                if (user == null)
                {
                    return Error<UserModel>(null, "کاربر مورد نظر یافت نشد");
                }
                userModel = new UserModel(user)
                {
                    Password = null,
                    ConfirmPassword = null,
                    Claims = (await _userManager.GetClaimsAsync(user)).Select((Claim x) => new UserClaimModel
                    {
                        Type = x.Type,
                        Value = x.Value
                    }).ToList()
                };
            }
            return Success(userModel);
        });
    }

    [HttpGet("[action]/{id}")]
    public Task<ResponseState<List<UserClaimModel>>> GetClaims(long id)
    {
        return TryCatch(async () =>
        {
            List<UserClaimModel> userClaimModels = new List<UserClaimModel>();
            if (id != 0L)
            {
                User user = await _userManager.GetById(id);
                if (user != null)
                {
                    userClaimModels = (await _userManager.GetClaimsAsync(user)).Select((Claim x) => new UserClaimModel
                    {
                        Type = x.Type,
                        Value = x.Value
                    }).ToList();
                }
            }
            return Success(userClaimModels);
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<long>> Create([FromBody] UserModel model)
    {
        return TryCatch(async () =>
        {
            if (await _userManager.GetAllAsQueryable().FirstOrDefaultAsync((User x) => x.Email == model.Email) != null)
            {
                return Error(0L, "ایمیل وارد شده تکراری است.");
            }
            Customer customer = await _customerService.GetById(_currentUserService.CustomerId.Value);
            model.UserName = GenerateUserName(customer.Code, model);
            if (await _userManager.FindByUserName(model.UserName) != null)
            {
                return Error(0L, "نام کاربری وارد شده تکراری است.");
            }
            if (model.Password == "@#$@#$xd2X#$%XXF%^e5")
            {
                return Error(0L, "کلمه عبور وارد شده مجاز نمی باشد.");
            }
            User user = new User
            {
                Id = model.Id,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                Mobile = model.Mobile,
                UserName = model.UserName,
                IsActive = model.IsActive
            };
            user.CustomerId = _currentUserService.CustomerId.Value;
            user.UserRoleNames = new List<string> { "Admin" };
            PasswordHasher<User> passwordHasher = new PasswordHasher<User>();
            string passwordHash = passwordHasher.HashPassword(user, model.Password);
            user.PasswordHash = passwordHash;
            IdentityResult identityResult = await _userManager.Insert(user, model.Password, user.UserRoleNames, model.Claims?.Select((UserClaimModel x) => new Claim(x.Type, x.Value)).ToList());
            return identityResult.Errors.Any() ? Error(0L, string.Join(',', identityResult.Errors.Select((IdentityError x) => x.Description).ToArray())) : Success(user.Id, "عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<long>> Edit([FromBody] UserModel model)
    {
        return TryCatch(async () =>
        {
            if (model.Id == 0L && string.IsNullOrEmpty(model.Password))
            {
                return Error(0L, "کلمه عبور الزامی است.");
            }
            if (await _userManager.GetAllAsQueryable().FirstOrDefaultAsync((User x) => x.Email == model.Email && x.Id != model.Id) != null)
            {
                return Error(0L, "ایمیل وارد شده تکراری است.");
            }
            Customer customer = await _customerService.GetById(_currentUserService.CustomerId.Value);
            if (!model.UserName.StartsWith($"{customer.Code}-") && model.UserName != customer.Mobile)
            {
                model.UserName = GenerateUserName(customer.Code, model);
            }
            User user = await _userManager.FindByUserName(model.UserName);
            if (user != null && model.Id != user.Id)
            {
                return Error(0L, "نام کاربری وارد شده تکراری است.");
            }
            User user2 = await _userManager.GetById(model.Id);
            user2.Id = model.Id;
            user2.FirstName = model.FirstName;
            user2.LastName = model.LastName;
            user2.UserName = model.UserName;
            user2.Email = model.Email;
            user2.Mobile = model.Mobile;
            user2.IsActive = model.IsActive;
            if (!string.IsNullOrEmpty(model.Password))
            {
                PasswordHasher<User> passwordHasher = new PasswordHasher<User>();
                string passwordHash = passwordHasher.HashPassword(user2, model.Password);
                user2.PasswordHash = passwordHash;
            }
            await _userManager.UpdateAsync(user2);
            IList<Claim> currentClaims = await _userManager.GetClaimsAsync(user2);
            List<Claim> shouldRemove = currentClaims.Where((Claim x) => !model.Claims.Select((UserClaimModel s) => s.Value).ToList().Contains(x.Value)).ToList();
            IEnumerable<Claim> claims = from x in model.Claims
                                        where !currentClaims.Select((Claim s) => s.Value).ToList().Contains(x.Value)
                                        select new Claim(x.Type, x.Value);
            await _userManager.AddClaimsAsync(user2, claims);
            await _userManager.RemoveClaimsAsync(user2, shouldRemove);
            return Success(user2.Id, "عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]/{id}")]
    public Task<DefaultResponseState> Delete(long id)
    {
        return TryCatch(async () =>
        {
            User user = await _userManager.GetById(id);
            if (user == null)
            {
                return Error("امکان حذف وجود ندارد. لطفا بعدا دوباره تلاش نمایید");
            }
            await _userManager.DeleteAsync(user);
            return Success();
        });
    }

    [HttpGet("[action]")]
    public Task<ResponseState<List<UserClaimModel>>> ClaimsList()
    {
        return TryCatch(async () =>
        {
            List<UserClaimModel> claims = _userManager.GetClaims();
            return Success(claims);
        });
    }

    private string GenerateUserName(int customerCode, UserModel model)
    {
        if (_appConfig.IsCloud)
        {
            return $"{customerCode}-{model.UserName}";
        }
        return model.UserName;
    }
}
