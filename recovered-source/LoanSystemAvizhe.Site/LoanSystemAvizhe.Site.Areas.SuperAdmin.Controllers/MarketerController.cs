using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Services.Users;
using LoanSystem.Site.Areas.SuperAdmin.Controllers;
using LoanSystem.Site.Helpers.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoanSystemAvizhe.Site.Areas.SuperAdmin.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "SuperAdmin")]
public class MarketerController : BaseSuperAdminController
{
    private readonly IMarketerService _marketerService;

    private readonly IAffiliateCommissionService _affiliateCommissionService;

    private readonly UserManager _userManager;

    public MarketerController(ILogger<MarketerController> logger, IMarketerService marketerService, UserManager userManager, IAffiliateCommissionService affiliateCommissionService)
        : base(logger)
    {
        _marketerService = marketerService;
        _userManager = userManager;
        _affiliateCommissionService = affiliateCommissionService;
    }

    [HttpGet("[action]")]
    public Task<ResponseState<List<MarketerModel>>> List()
    {
        return TryCatch(async () =>
        {
            List<AffiliateCommission> commissions = await _affiliateCommissionService.GetAll();
            List<MarketerModel> list = (await _marketerService.GetAllMarketers()).Select((User x) => new MarketerModel(x)).ToList();
            list.ForEach((MarketerModel marketer) =>
            {
                marketer.TotalCommission = commissions.Where((AffiliateCommission c) => c.UserId == marketer.Id).Sum((AffiliateCommission c) => c.Amount);
                marketer.TotalPaids = commissions.Where((AffiliateCommission c) => c.UserId == marketer.Id).Sum((AffiliateCommission c) => c.PaidToUser);
            });
            return Success(list);
        });
    }

    [HttpGet("[action]/{id}")]
    public Task<ResponseState<MarketerModel>> Get(long id)
    {
        return TryCatch(async () =>
        {
            MarketerModel data = new MarketerModel();
            if (id != 0L)
            {
                User user = await _marketerService.GetById(id);
                if (user == null)
                {
                    return Error<MarketerModel>(null, "اطلاعات مورد نظر یافت نشد");
                }
                data = new MarketerModel(user)
                {
                    Password = "@#$@#$xd2X#$%XXF%^e5",
                    ConfirmPassword = "@#$@#$xd2X#$%XXF%^e5"
                };
            }
            return Success(data);
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<long>> Create([FromBody] MarketerModel model)
    {
        return TryCatch(async () =>
        {
            model.UserName = model.Email;
            if (await _userManager.FindByUserName(model.UserName) != null)
            {
                return Error(0L, "نام کاربری وارد شده تکراری است.");
            }
            if (await _userManager.FindByAffiliateCode(model.AffiliateCode) != null)
            {
                return Error(0L, "کد معرفی وارد شده تکراری است.");
            }
            User entity = new User
            {
                Id = model.Id,
                UserName = model.UserName,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Gender = model.Gender,
                LastLoginDate = model.LastLoginDate,
                Mobile = model.Mobile,
                NationalCode = model.NationalCode,
                RegDate = DateTime.Now,
                IsActive = true,
                Email = model.Email,
                AffiliateCode = model.AffiliateCode,
                CommissionPercent = model.CommissionPercent
            };
            entity.UserRoleNames = new List<string> { "Marketer" };
            PasswordHasher<User> passwordHasher = new PasswordHasher<User>();
            string passwordHash = passwordHasher.HashPassword(entity, (!string.IsNullOrEmpty(model.Password)) ? model.Password : model.NationalCode);
            entity.PasswordHash = passwordHash;
            IdentityResult identityResult = await _userManager.Insert(entity, model.ConfirmPassword, entity.UserRoleNames);
            return identityResult.Succeeded ? Success(entity.Id, "عملیات با موفقیت انجام شد.") : Error(0L, string.Join(".", identityResult.Errors.Select((IdentityError x) => x.Description)));
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<long>> Edit([FromBody] MarketerModel model)
    {
        return TryCatch(async () =>
        {
            model.UserName = model.Email;
            User user = await _userManager.FindByUserName(model.UserName);
            if (user != null && user.Id != model.Id)
            {
                return Error(0L, "نام کاربری وارد شده تکراری است.");
            }
            user = await _userManager.FindByAffiliateCode(model.AffiliateCode);
            if (user != null && user.Id != model.Id)
            {
                return Error(0L, "کد معرفی وارد شده تکراری است.");
            }
            User entity = await _marketerService.GetById(model.Id);
            entity.Id = model.Id;
            entity.UserName = model.UserName;
            entity.FirstName = model.FirstName;
            entity.LastName = model.LastName;
            entity.Gender = model.Gender;
            entity.LastLoginDate = model.LastLoginDate;
            entity.Mobile = model.Mobile;
            entity.AffiliateCode = model.AffiliateCode;
            entity.CommissionPercent = model.CommissionPercent;
            entity.NationalCode = model.NationalCode;
            entity.Email = model.Email;
            if (model.Password != "@#$@#$xd2X#$%XXF%^e5")
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
            User user = await _marketerService.GetById(id);
            if (user == null)
            {
                return Error("امکان حذف وجود ندارد. لطفا بعدا دوباره تلاش نمایید");
            }
            await _marketerService.Delete(user);
            return Success();
        });
    }
}
