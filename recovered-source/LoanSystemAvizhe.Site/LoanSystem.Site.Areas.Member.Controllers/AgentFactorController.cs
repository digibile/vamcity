using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Services.FileHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoanSystem.Site.Areas.Member.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Member,Admin,Agent")]
public class AgentFactorController : BaseMemberController
{
    private readonly IAgentFactorService _agentFactorService;

    private readonly IAppContext _appContext;

    private readonly ICurrentUserService _currentUserService;

    private readonly IFileHelper _fileHelper;

    public AgentFactorController(ILogger<AgentFactorController> logger, IAgentFactorService agentFactorService, IAppContext appContext, ICurrentUserService currentUserService, IFileHelper fileHelper)
        : base(logger)
    {
        _agentFactorService = agentFactorService;
        _appContext = appContext;
        _currentUserService = currentUserService;
        _fileHelper = fileHelper;
    }

    [HttpGet("[action]/{id}")]
    public Task<ResponseState<AgentFactorModel>> Get(Guid id)
    {
        return TryCatch(async () =>
        {
            if (_appContext.User == null)
            {
                return Error<AgentFactorModel>(null, "کاربر یافت نشد");
            }
            if (id == Guid.Empty)
            {
                return Success(new AgentFactorModel());
            }
            AgentFactor agentFactor = await _agentFactorService.GetById(id, "Member", "Agent", "AgentFactorItems", "Agent.AgentMetaData");
            if (agentFactor == null)
            {
                return Error<AgentFactorModel>(null, "اطلاعات مورد نظر یافت نشد");
            }
            if (_currentUserService.IsInRole("Agent") && agentFactor.AgentId != _appContext.User.Id)
            {
                return Error<AgentFactorModel>(null, "دسترسی به این فاکتور برای شما مجاز نمی باشد.");
            }
            if (_currentUserService.IsInRole("Member") && agentFactor.MemberId != _appContext.User.Id)
            {
                return Error<AgentFactorModel>(null, "دسترسی به این فاکتور برای شما مجاز نمی باشد.");
            }
            AgentFactorModel agentFactorModel = new AgentFactorModel(agentFactor);
            agentFactorModel.Agent.FillImageBase64Props(_fileHelper, _currentUserService.CustomerId.Value);
            string thumbnailBase = agentFactorModel.Agent.MetaData?.ThumbnailBase64;
            agentFactorModel.Agent.MetaData = new AgentMetaDataModel
            {
                ThumbnailBase64 = thumbnailBase
            };
            return Success(agentFactorModel);
        });
    }

    [HttpGet("[action]")]
    public Task<ResponseState<List<AgentFactorModel>>> List(long? memberId, DateTime? start, DateTime? end)
    {
        return TryCatch(async () =>
        {
            memberId = _currentUserService.UserId.Value;
            List<AgentFactorModel> data = (from x in await _agentFactorService.GetByCondition((AgentFactor x) => (((DateTime?)start).HasValue ? (x.CreateAt.Date >= ((DateTime?)start).Value.Date) : true) && (((DateTime?)end).HasValue ? (x.CreateAt.Date <= ((DateTime?)end).Value.Date) : true) && x.MemberId == ((long?)memberId).Value, false, "Member", "Agent", "AgentFactorItems")
                                           orderby x.CreateAt descending
                                           select new AgentFactorModel(x)).ToList();
            return Success(data);
        });
    }
}
