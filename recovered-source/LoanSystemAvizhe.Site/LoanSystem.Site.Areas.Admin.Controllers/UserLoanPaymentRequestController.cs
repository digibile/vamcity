using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Services.FileHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;

namespace LoanSystem.Site.Areas.Admin.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Admin")]
public class UserLoanPaymentRequestController : BaseAdminController
{
    private readonly IUserLoanPaymentRequestService _userLoanPaymentRequestService;

    private readonly IFileHelper _fileHelper;

    private readonly ICurrentUserService _currentUserService;

    public UserLoanPaymentRequestController(ILogger<UserLoanPaymentRequestController> logger, IUserLoanPaymentRequestService userLoanPaymentRequestService, IFileHelper fileHelper, ICurrentUserService currentUserService)
        : base(logger)
    {
        _userLoanPaymentRequestService = userLoanPaymentRequestService;
        _currentUserService = currentUserService;
        _fileHelper = fileHelper;
    }

    [HttpGet("[action]/{status?}")]
    public Task<ResponseState<List<UserLoanPaymentRequestModel>>> List(UserLoanPaymentRequestStatus? status)
    {
        return TryCatch(async () =>
        {
            Expression<Func<UserLoanPaymentRequest, bool>> condition = null;
            if (status.HasValue)
            {
                condition = (UserLoanPaymentRequest x) => (int?)x.Status == (int?)status;
            }
            List<UserLoanPaymentRequestModel> data = (from x in await _userLoanPaymentRequestService.GetAll(include: true, condition)
                                                      select new UserLoanPaymentRequestModel(x) into x
                                                      orderby x.Status
                                                      select x).ToList();
            return Success(data);
        });
    }

    [HttpGet("[action]/{id}")]
    public Task<ResponseState<UserLoanPaymentRequestModel>> Get(Guid id)
    {
        return TryCatch(async () =>
        {
            if (id == Guid.Empty)
            {
                return Success(new UserLoanPaymentRequestModel());
            }
            UserLoanPaymentRequest userLoanPaymentRequest = await _userLoanPaymentRequestService.GetById(id, include: true);
            if (userLoanPaymentRequest == null)
            {
                return Error<UserLoanPaymentRequestModel>(null, "اطلاعات مورد نظر یافت نشد.");
            }
            UserLoanPaymentRequestModel userLoanPaymentRequestModel = new UserLoanPaymentRequestModel(userLoanPaymentRequest);
            userLoanPaymentRequestModel.FillImageBase64Prop(_fileHelper, _currentUserService.CustomerId.Value);
            return Success(userLoanPaymentRequestModel);
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<Guid>> Edit(UserLoanPaymentRequestModel model)
    {
        foreach (KeyValuePair<string, ModelStateEntry> item in ModelState)
        {
            string key = item.Key;
            if (key.Contains("AdminAcceptor"))
            {
                ModelState[key].ValidationState = ModelValidationState.Valid;
                ModelState[key].Errors.Clear();
            }
        }
        return TryCatch(async () =>
        {
            UserLoanPaymentRequest entity = await _userLoanPaymentRequestService.GetById(model.Id, include: true);
            if (entity == null)
            {
                return Error(Guid.Empty, "اطلاعات مورد نظر یافت نشد.");
            }
            entity.PaymentMethod = model.PaymentMethod;
            entity.AdminDescription = model.AdminDescription;
            if (!entity.AffectOnMainPayment)
            {
                entity.AffectOnMainPayment = model.AffectOnMainPayment;
            }
            if (entity.Status != model.Status)
            {
                entity.AdminAcceptDate = DateTime.Now;
                entity.AdminAcceptorId = _currentUserService.UserId;
                entity.Status = model.Status;
            }
            await _userLoanPaymentRequestService.Update(entity, model.SendSms);
            return Success(entity.Id, "عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]/{id}")]
    public Task<DefaultResponseState> Delete(Guid id)
    {
        return TryCatch(async () =>
        {
            UserLoanPaymentRequest userLoanPaymentRequest = await _userLoanPaymentRequestService.GetById(id, include: false);
            if (userLoanPaymentRequest == null)
            {
                return Error("امکان حذف وجود ندارد. لطفا بعدا دوباره تلاش نمایید");
            }
            await _userLoanPaymentRequestService.Delete(userLoanPaymentRequest);
            return Success();
        });
    }
}
