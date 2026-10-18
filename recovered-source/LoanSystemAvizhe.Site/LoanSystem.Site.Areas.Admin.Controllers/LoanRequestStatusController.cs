using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Helpers.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoanSystem.Site.Areas.Admin.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Admin")]
public class LoanRequestStatusController : BaseAdminController
{
    private readonly ILoanRequestStatusService _loanRequestStatusService;

    public LoanRequestStatusController(ILogger<LoanRequestStatusController> logger, ILoanRequestStatusService loanRequestStatusService)
        : base(logger)
    {
        _loanRequestStatusService = loanRequestStatusService;
    }

    [HttpGet("[action]")]
    public Task<ResponseState<List<LoanRequestStatusModel>>> List()
    {
        return TryCatch(async () =>
        {
            List<LoanRequestStatusModel> data = (await _loanRequestStatusService.GetAllLoanRequestStatuses("SmsPattern")).Select((LoanRequestStatus x) => new LoanRequestStatusModel(x)).ToList();
            return Success(data);
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<Guid>> Create([FromBody] LoanRequestStatusModel model)
    {
        return TryCatch(async () =>
        {
            LoanRequestStatus entity = new LoanRequestStatus
            {
                Title = model.Title,
                Changable = model.Changable,
                AllowNewRequest = model.AllowNewRequest,
                Order = model.Order,
                SmsPatternId = model.SmsPattern?.Id,
                StepCategory = model.StepCategory,
                StatusGroup = model.StatusGroup
            };
            await _loanRequestStatusService.Insert(entity);
            return Success(entity.Id, "عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<Guid>> Edit([FromBody] LoanRequestStatusModel model)
    {
        return TryCatch(async () =>
        {
            LoanRequestStatus entity = await _loanRequestStatusService.GetById(model.Id);
            if (entity == null)
            {
                return Error("آیتم یافت نشد.");
            }
            entity.Title = model.Title;
            entity.Changable = model.Changable;
            entity.AllowNewRequest = model.AllowNewRequest;
            entity.Order = model.Order;
            entity.SmsPatternId = model.SmsPattern?.Id;
            entity.StepCategory = model.StepCategory;
            entity.StatusGroup = model.StatusGroup;
            await _loanRequestStatusService.Update(entity);
            return Success(entity.Id, "عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]/{id}")]
    public Task<DefaultResponseState> Delete(Guid id)
    {
        return TryCatch(async () =>
        {
            LoanRequestStatus loanRequestStatus = await _loanRequestStatusService.GetById(id);
            if (loanRequestStatus == null)
            {
                return Error("امکان حذف وجود ندارد. لطفا بعدا دوباره تلاش نمایید");
            }
            if (loanRequestStatus.SystemRequestStatus.HasValue)
            {
                return Error("آیتم های سیستمی مجاز به حذف نمی باشند.");
            }
            await _loanRequestStatusService.Delete(loanRequestStatus);
            return Success();
        });
    }
}
