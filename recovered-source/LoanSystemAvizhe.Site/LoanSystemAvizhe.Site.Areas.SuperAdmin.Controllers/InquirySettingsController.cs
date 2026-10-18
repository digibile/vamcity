using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Areas.SuperAdmin.Controllers;
using LoanSystem.Site.Helpers.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoanSystemAvizhe.Site.Areas.SuperAdmin.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "SuperAdmin")]
public class InquirySettingsController : BaseSuperAdminController
{
    private readonly IInquirySettingsService _inquirySettingsService;

    public InquirySettingsController(ILogger<InquirySettingsController> logger, IInquirySettingsService InquirySettingsService)
        : base(logger)
    {
        _inquirySettingsService = InquirySettingsService;
    }

    [HttpGet("[action]/{id}")]
    public Task<ResponseState<InquirySettingsModel>> Get(Guid id)
    {
        return TryCatch(async () =>
        {
            InquirySettings inquirySettings = await _inquirySettingsService.GetById(id);
            if (inquirySettings == null)
            {
                return Error<InquirySettingsModel>(null, "اطلاعات مورد نظر یافت نشد");
            }
            InquirySettingsModel data = new InquirySettingsModel(inquirySettings);
            return Success(data);
        });
    }

    [HttpGet("[action]")]
    public Task<ResponseState<List<InquirySettingsModel>>> List()
    {
        return TryCatch(async () =>
        {
            List<InquirySettingsModel> data = (await _inquirySettingsService.GetAll()).Select((InquirySettings x) => new InquirySettingsModel(x)).ToList();
            return Success(data);
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<InquirySettingsModel>> Create([FromBody] InquirySettingsModel model)
    {
        return TryCatch(async () =>
        {
            InquirySettings entity = new InquirySettings
            {
                ApiIR_IsActive = model.ApiIR_IsActive,
                ApiIR_PriceFrom = model.ApiIR_PriceFrom,
                ApiIR_PriceTo = model.ApiIR_PriceTo,
                AppInquiryType = model.AppInquiryType
            };
            await _inquirySettingsService.Insert(entity);
            return Success(model, "عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<InquirySettingsModel>> Edit([FromBody] InquirySettingsModel model)
    {
        return TryCatch(async () =>
        {
            InquirySettings entity = await _inquirySettingsService.GetById(model.Id);
            if (entity == null)
            {
                return Error<InquirySettingsModel>(null, "اطلاعات مورد نظر یافت نشد.");
            }
            entity.AppInquiryType = model.AppInquiryType;
            entity.ApiIR_IsActive = model.ApiIR_IsActive;
            entity.ApiIR_PriceTo = model.ApiIR_PriceTo;
            entity.ApiIR_PriceFrom = model.ApiIR_PriceFrom;
            await _inquirySettingsService.Update(entity);
            return Success(new InquirySettingsModel(entity), "عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]/{id}")]
    public Task<DefaultResponseState> Delete(Guid id)
    {
        return TryCatch(async () =>
        {
            InquirySettings inquirySettings = await _inquirySettingsService.GetById(id);
            if (inquirySettings == null)
            {
                return Error("اطلاعات مورد نظر یافت نشد.");
            }
            await _inquirySettingsService.Delete(inquirySettings);
            return Success();
        });
    }
}
