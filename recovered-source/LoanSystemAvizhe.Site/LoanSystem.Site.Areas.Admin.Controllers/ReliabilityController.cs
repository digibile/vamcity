using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Areas.Admin.Models;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoanSystem.Site.Areas.Admin.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Admin")]
public class ReliabilityController : BaseAdminController
{
    private readonly IReliabilityService _ReliabilityService;

    private readonly IUserPaymentService _userPaymentService;

    private readonly ICurrentUserService _currentUserService;

    public ReliabilityController(ILogger<ReliabilityController> logger, IReliabilityService ReliabilityService, IUserPaymentService userPaymentService, ICurrentUserService currentUserService)
        : base(logger)
    {
        _currentUserService = currentUserService;
        _ReliabilityService = ReliabilityService;
        _userPaymentService = userPaymentService;
    }

    [HttpGet("[action]/{id}")]
    public Task<ResponseState<ReliabilityModel>> Get(Guid id)
    {
        return TryCatch(async () =>
        {
            Reliability reliability = await _ReliabilityService.GetById(id);
            return Success(new ReliabilityModel(reliability));
        });
    }

    [HttpGet("[action]")]
    public Task<ResponseState<List<ReliabilityModel>>> List()
    {
        return TryCatch(async () =>
        {
            List<ReliabilityModel> data = (await _ReliabilityService.GetAll(null)).Select((Reliability x) => new ReliabilityModel(x)).ToList();
            return Success(data);
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<Guid>> Create([FromBody] ReliabilityModel model)
    {
        return TryCatch(async () =>
        {
            List<Reliability> list = await _ReliabilityService.GetByCondition((Reliability x) => x.Title == model.Title);
            if (list != null && list.Any())
            {
                return Error("عنوان وارد شده تکراری است.");
            }
            Reliability entity = new Reliability
            {
                Id = Guid.Empty,
                IsNewLoanLock = model.IsNewLoanLock,
                Color = model.Color,
                Title = model.Title,
                From = model.From,
                To = model.To,
                Count = model.Count,
                RowType = model.RowType
            };
            await _ReliabilityService.Insert(entity);
            return Success(entity.Id, "عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<Guid>> Edit([FromBody] ReliabilityModel model)
    {
        return TryCatch(async () =>
        {
            List<Reliability> list = await _ReliabilityService.GetByCondition((Reliability x) => x.Id != model.Id && x.Title == model.Title);
            if (list != null && list.Any())
            {
                return Error("عنوان وارد شده تکراری است.");
            }
            Reliability entity = await _ReliabilityService.GetById(model.Id);
            entity.Title = model.Title;
            entity.IsNewLoanLock = model.IsNewLoanLock;
            entity.Color = model.Color;
            entity.From = model.From;
            entity.To = model.To;
            entity.Count = model.Count;
            entity.RowType = model.RowType;
            await _ReliabilityService.Update(entity);
            return Success(entity.Id, "عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]")]
    public Task<DefaultResponseState> SetMembersReliability([FromBody] LongIdList model)
    {
        return TryCatch(async () =>
        {
            Dictionary<long, List<UserLoanPayment>> member_payments_dic = await _userPaymentService.GetPaymentsByUserIdsAsDictionary(model.Ids);
            _ReliabilityService.SetUserPaymentServie(_userPaymentService);
            await _ReliabilityService.SetMembersReliability(model.Ids, member_payments_dic);
            return Success("عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]/{memberId}/{reliabilityId}")]
    public Task<DefaultResponseState> SetMembersReliabilityManualy(long memberId, Guid reliabilityId)
    {
        return TryCatch(async () =>
        {
            await _ReliabilityService.SetMemberReliability(memberId, reliabilityId);
            return Success("عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]/{id}")]
    public Task<DefaultResponseState> Delete(Guid id)
    {
        return TryCatch(async () =>
        {
            Reliability reliability = await _ReliabilityService.GetById(id);
            if (reliability == null)
            {
                return Error("امکان حذف وجود ندارد. لطفا بعدا دوباره تلاش نمایید");
            }
            await _ReliabilityService.Delete(reliability);
            return Success();
        });
    }
}
