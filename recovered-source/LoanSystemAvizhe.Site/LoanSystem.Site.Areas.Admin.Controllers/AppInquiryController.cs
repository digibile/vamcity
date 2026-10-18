using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Core.Exceptions;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Areas.Admin.Models;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Data.Enums;
using LoanSystemAvizhe.Services.Inquiry.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoanSystem.Site.Areas.Admin.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Admin")]
public class AppInquiryController : BaseAdminController
{
    private readonly IAppInquiryService _appInquiryService;

    private readonly IntegratedInquiryService _integratedInquiryService;

    public AppInquiryController(ILogger<AppInquiryController> logger, IAppInquiryService appInquiryService, IntegratedInquiryService integratedInquiryService)
        : base(logger)
    {
        _appInquiryService = appInquiryService;
        _integratedInquiryService = integratedInquiryService;
    }

    [HttpGet("[action]")]
    public Task<ResponseState<List<AppInquiryModel>>> List(DateTime? start, DateTime? end)
    {
        return TryCatch(async () =>
        {
            List<AppInquiryModel> data = (from x in await _appInquiryService.GetByCondition((AppInquiry x) => (((DateTime?)start).HasValue ? (x.Date.Date >= ((DateTime?)start).Value.Date) : true) && (((DateTime?)end).HasValue ? (x.Date.Date <= ((DateTime?)end).Value.Date) : true), false)
                                          select new AppInquiryModel(x) into x
                                          orderby x.Date descending
                                          select x).ToList();
            return Success(data);
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<List<AppInquiryModel>>> ListByIds([FromBody] IdList list)
    {
        return TryCatch(async () =>
        {
            List<AppInquiryModel> data = (from x in await _appInquiryService.GetByCondition((AppInquiry x) => list.Ids.Contains(x.Id), false)
                                          select new AppInquiryModel(x) into x
                                          orderby x.Date descending
                                          select x).ToList();
            return Success(data);
        });
    }

    [HttpGet("[action]/{id}")]
    public Task<ResponseState<AppInquiryModel>> Get(Guid id)
    {
        return TryCatch(async () =>
        {
            AppInquiry appInquiry = await _appInquiryService.GetById(id);
            if (appInquiry == null)
            {
                return Error<AppInquiryModel>(null, "اطلاعات مورد نظر یافت نشد.");
            }
            AppInquiryModel data = new AppInquiryModel(appInquiry);
            return Success(data);
        });
    }

    [HttpGet("[action]/{id}")]
    public Task<ResponseState<List<AppInquiryModel>>> ListByMemberId(long id)
    {
        return TryCatch(async () =>
        {
            List<AppInquiryModel> data = (from x in await _appInquiryService.GetByCondition((AppInquiry x) => x.MemberId == (long?)id, false)
                                          select new AppInquiryModel(x) into x
                                          orderby x.Date descending
                                          select x).ToList();
            return Success(data);
        });
    }

    [HttpGet("[action]/{id}/{type}")]
    public Task<ResponseState<List<AppInquiryModel>>> ListByMemberId(long id, AppInquiryType type)
    {
        return TryCatch(async () =>
        {
            List<AppInquiryModel> data = (from x in await _appInquiryService.GetByCondition((AppInquiry x) => x.MemberId == (long?)id && (int)x.AppInquiryType == (int)type, false)
                                          select new AppInquiryModel(x) into x
                                          orderby x.Date descending
                                          select x).ToList();
            return Success(data);
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<AppInquiryModel>> InquiryRequest([FromBody] InquiryRequestModel inquiryRequestModel)
    {
        return TryCatch(async () =>
        {
            try
            {
                AppInquiry appInquiry = await _integratedInquiryService.InquiryRequest(inquiryRequestModel);
                return Success(new AppInquiryModel(appInquiry), "عملیات با موفقیت انجام شد.");
            }
            catch (CustomException ex)
            {
                return Error<AppInquiryModel>(null, ex.Message);
            }
        });
    }
}
