using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Core.Exceptions;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Services.FileHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoanSystem.Site.Areas.Admin.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Admin")]
public class EmzameContractAuthorizationItemController : BaseAdminController
{
    private readonly IEmzameContractAuthorizationItemService _emzameContractAuthorizationItemService;

    private readonly IUserLoanRequestAuthorizationItemService _userLoanRequestAuthorizationItemService;

    private readonly ICustomerService _customerService;

    private readonly IFileHelper _fileHelper;

    private readonly ICurrentUserService _currentUserService;

    public EmzameContractAuthorizationItemController(ILogger<EmzameContractAuthorizationItemController> logger, IEmzameContractAuthorizationItemService emzameContractAuthorizationItemService, ICurrentUserService currentUserService, IFileHelper fileHelper, IUserLoanRequestAuthorizationItemService userLoanRequestAuthorizationItemService, ICustomerService customerService)
        : base(logger)
    {
        _emzameContractAuthorizationItemService = emzameContractAuthorizationItemService;
        _currentUserService = currentUserService;
        _userLoanRequestAuthorizationItemService = userLoanRequestAuthorizationItemService;
        _customerService = customerService;
        _fileHelper = fileHelper;
    }

    [HttpGet("[action]/{id}")]
    public Task<ResponseState<EmzameContractAuthorizationItemModel>> Get(Guid id)
    {
        return TryCatch(async () =>
        {
            EmzameContractAuthorizationItemModel data = new EmzameContractAuthorizationItemModel(await _emzameContractAuthorizationItemService.GetById(id, "AuthorizationItem", "User"));
            return Success(data);
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<Guid>> Create([FromBody] EmzameContractAuthorizationItemModel model)
    {
        return TryCatch(async () =>
        {
            if (!model.UserLoanRequestAuthorizationItemId.HasValue && !model.UserLoanId.HasValue)
            {
                return Error<Guid>(default, "جهت ثبت اطلاعات امضا قرارداد باید یکی از اطلاعات درخواست وام یا پرونده وام تعیین شده باشد.");
            }
            EmzameContractAuthorizationItem newItem = new EmzameContractAuthorizationItem
            {
                AuthorizationItemId = model.AuthorizationItemId,
                UserLoanRequestAuthorizationItemId = model.UserLoanRequestAuthorizationItemId,
                UserLoanId = model.UserLoanId,
                UserId = model.User?.Id,
                CustomerId = _currentUserService.CustomerId.Value
            };
            try
            {
                await _emzameContractAuthorizationItemService.Insert(newItem);
                EmzameContractAuthorizationItem emzame_auth = await _emzameContractAuthorizationItemService.GetByIdIncludeRelations(newItem.Id);
                EmzameContractAuthorizationItemModel emzameContractAuthorizationItemModel = new EmzameContractAuthorizationItemModel(emzame_auth);
                emzameContractAuthorizationItemModel.SetUserLoanRequestAuthorizationItem(new UserLoanRequestAuthorizationItemModel(emzame_auth.UserLoanRequestAuthorizationItem));
                EmzameContractAuthorizationItem emzameContractAuthorizationItem = newItem;
                emzameContractAuthorizationItem.Comment = await _emzameContractAuthorizationItemService.ReplaceDataByTags(emzameContractAuthorizationItemModel, new Dictionary<string, string>(), emzame_auth.AuthorizationItem.Description);
                await _emzameContractAuthorizationItemService.Update(newItem);
                UserLoanRequestAuthorizationItem userLoanRequestAuthorizationItem = await _userLoanRequestAuthorizationItemService.GetById(emzame_auth.UserLoanRequestAuthorizationItemId.Value);
                userLoanRequestAuthorizationItem.EmzameContractAuthorizationItemId = newItem.Id;
                userLoanRequestAuthorizationItem.Status = LoanRequestAuthorizationStatus.ثبت_شده_توسط_کاربر;
                await _userLoanRequestAuthorizationItemService.Update(userLoanRequestAuthorizationItem);
            }
            catch (Exception)
            {
                throw;
            }
            return Success(newItem.Id, "عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<Guid>> Edit([FromBody] EmzameContractAuthorizationItemModel model)
    {
        return TryCatch(async () =>
        {
            EmzameContractAuthorizationItem entity = await _emzameContractAuthorizationItemService.GetByIdIncludeRelations(model.Id);
            if (entity == null)
            {
                return Error<Guid>(default, "آیتم قرارداد یافت نشد.");
            }
            EmzameContractAuthorizationItemModel emzameContractAuthorizationItemModel = new EmzameContractAuthorizationItemModel(entity);
            if (emzameContractAuthorizationItemModel.Contract.status == "accept")
            {
                return Error<Guid>(default, "آیتم قبلا امضا شده و اجازه تغییر ندارد.");
            }
            UserLoanRequestAuthorizationItem userLoanRequestAuthorizationItem = entity.UserLoanRequestAuthorizationItem;
            if (userLoanRequestAuthorizationItem != null && userLoanRequestAuthorizationItem.Status == LoanRequestAuthorizationStatus.تایید_شده_توسط_مدیر)
            {
                return Error<Guid>(default, "آیتم در وضعیت تایید شده است و اجازه تغییر ندارد.");
            }
            emzameContractAuthorizationItemModel.SetUserLoanRequestAuthorizationItem(new UserLoanRequestAuthorizationItemModel(entity.UserLoanRequestAuthorizationItem));
            EmzameContractAuthorizationItem emzameContractAuthorizationItem = entity;
            emzameContractAuthorizationItem.Comment = await _emzameContractAuthorizationItemService.ReplaceDataByTags(emzameContractAuthorizationItemModel, new Dictionary<string, string>(), entity.AuthorizationItem.Description);
            await _emzameContractAuthorizationItemService.Update(entity);
            return Success(entity.Id, "عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]")]
    public Task<DefaultResponseState> UpdateContractDescription([FromBody] UpdateDigitalSignDescriptionModel model)
    {
        return TryCatch(async () =>
        {
            EmzameContractAuthorizationItem entity = await _emzameContractAuthorizationItemService.GetByIdIncludeRelations(model.EmzameContractAuthorizationItemId);
            if (entity == null)
            {
                return Error("آیتم ضمانت یافت نشد.");
            }
            EmzameContractAuthorizationItemModel emzameContractAuthorizationItemModel = new EmzameContractAuthorizationItemModel(entity);
            emzameContractAuthorizationItemModel.SetUserLoanRequestAuthorizationItem(new UserLoanRequestAuthorizationItemModel(entity.UserLoanRequestAuthorizationItem));
            if (emzameContractAuthorizationItemModel.Contract?.status == "accept")
            {
                return Error("آیتم امضا شده است و اجازه تغییر ندارد.");
            }
            UserLoanRequestAuthorizationItem userLoanRequestAuthorizationItem = entity.UserLoanRequestAuthorizationItem;
            if (userLoanRequestAuthorizationItem != null && userLoanRequestAuthorizationItem.Status == LoanRequestAuthorizationStatus.تایید_شده_توسط_مدیر)
            {
                return Error("آیتم در وضعیت تایید شده است و اجازه تغییر ندارد.");
            }
            EmzameContractAuthorizationItem emzameContractAuthorizationItem = entity;
            emzameContractAuthorizationItem.Comment = await _emzameContractAuthorizationItemService.ReplaceDataByTags(emzameContractAuthorizationItemModel, model.ManualFields, entity.AuthorizationItem.Description);
            try
            {
                await _emzameContractAuthorizationItemService.Update(entity);
            }
            catch (Exception)
            {
                throw;
            }
            return Success("عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]/{id}")]
    public Task<DefaultResponseState> Delete(Guid id)
    {
        return TryCatch(async () =>
        {
            EmzameContractAuthorizationItem emzameContractAuthorizationItem = (await _emzameContractAuthorizationItemService.GetByCondition((EmzameContractAuthorizationItem x) => x.Id == id)).FirstOrDefault();
            if (emzameContractAuthorizationItem == null)
            {
                return Error("امکان حذف وجود ندارد. لطفا بعدا دوباره تلاش نمایید");
            }
            await _emzameContractAuthorizationItemService.Delete(emzameContractAuthorizationItem);
            return Success();
        });
    }

    [HttpPost("[action]/{id}")]
    public Task<DefaultResponseState> SendContractToEmzame(Guid id)
    {
        return TryCatch(async () =>
        {
            EmzameContractAuthorizationItem emzameContractAuthorizationItem = await _emzameContractAuthorizationItemService.GetById(id, "User");
            if (emzameContractAuthorizationItem == null)
            {
                return Error("آیتم قرارداد یافت نشد.");
            }
            try
            {
                EmzameContractAuthorizationItemModel mdl = new EmzameContractAuthorizationItemModel(emzameContractAuthorizationItem);
                Customer customer = await _customerService.GetById(_currentUserService.CustomerId.Value);
                await _emzameContractAuthorizationItemService.SendContractToEmzame(mdl, "قرارداد اخذ تسهیلات " + mdl.User.FullName + " از " + customer.Name, "");
            }
            catch (CustomException ex)
            {
                return Error(ex.Message);
            }
            catch (Exception)
            {
                throw;
            }
            return Success("عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]/{id}")]
    public Task<DefaultResponseState> GetContractFromEmzame(Guid id)
    {
        return TryCatch(async () =>
        {
            try
            {
                await _emzameContractAuthorizationItemService.GetContractFromEmzame(id);
            }
            catch (CustomException ex)
            {
                return Error(ex.Message);
            }
            catch (Exception)
            {
                throw;
            }
            return Success("عملیات با موفقیت انجام شد.");
        });
    }
}
