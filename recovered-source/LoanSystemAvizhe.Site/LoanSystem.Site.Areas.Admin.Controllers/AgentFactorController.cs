using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ganss.Xss;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Services.Users;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Services.FileHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoanSystem.Site.Areas.Admin.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Admin")]
public class AgentFactorController : BaseAdminController
{
    private readonly IWithdrawAgentFactorService _withdrawAgentFactorService;

    private readonly IAgentFactorService _agentFactorService;

    private readonly IAppContext _appContext;

    private readonly IAppSmsService _appSmsService;

    private readonly UserManager _userManager;

    private readonly ICurrentUserService _currentUserService;

    private readonly IntegratedSmsSenderService _integratedSmsSenderService;

    private readonly IntegratedEmailSenderService _integratedEmailSenderService;

    private readonly IFileHelper _fileHelper;

    public AgentFactorController(ILogger<AgentFactorController> logger, IWithdrawAgentFactorService withdrawAgentFactorService, IAgentFactorService agentFactorService, IAppContext appContext, IAppSmsService appSmsService, UserManager userManager, ICurrentUserService currentUserService, IntegratedSmsSenderService integratedSmsSenderService, IntegratedEmailSenderService integratedEmailSenderService, IFileHelper fileHelper)
        : base(logger)
    {
        _withdrawAgentFactorService = withdrawAgentFactorService;
        _agentFactorService = agentFactorService;
        _appContext = appContext;
        _appSmsService = appSmsService;
        _userManager = userManager;
        _currentUserService = currentUserService;
        _integratedSmsSenderService = integratedSmsSenderService;
        _integratedEmailSenderService = integratedEmailSenderService;
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
            AgentFactor agentFactor = await _agentFactorService.GetById(id, "Member", "MemberPurchase", "AgentFactorItems", "Agent.AgentMetaData");
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
    public Task<ResponseState<List<AgentFactorModel>>> List(long? memberId, long? agentId, DateTime? start, DateTime? end)
    {
        return TryCatch(async () =>
        {
            List<AgentFactorModel> data = (from x in await _agentFactorService.GetByCondition((AgentFactor x) => (((DateTime?)start).HasValue ? (x.CreateAt.Date >= ((DateTime?)start).Value.Date) : true) && (((DateTime?)end).HasValue ? (x.CreateAt.Date <= ((DateTime?)end).Value.Date) : true) && (((long?)memberId).HasValue ? (x.MemberId == ((long?)memberId).Value) : true) && (((long?)agentId).HasValue ? (x.AgentId == (long?)((long?)agentId).Value) : true), false, "Member", "Agent", "MemberPurchase", "AgentFactorItems", "WithdrawAgentFactors.Withdraw")
                                           orderby x.CreateAt descending
                                           select new AgentFactorModel(x)).ToList();
            return Success(data);
        });
    }

    [HttpGet("[action]/{transactionId}")]
    public Task<ResponseState<List<AgentFactorModel>>> ListByTransactionId(Guid transactionId)
    {
        return TryCatch(async () =>
        {
            List<WithdrawAgentFactor> withdraws = await _withdrawAgentFactorService.GetByCondition((WithdrawAgentFactor x) => x.WithdrawId.Equals(transactionId), false);
            List<AgentFactorModel> data = (await _agentFactorService.GetByCondition((AgentFactor x) => withdraws.Select((WithdrawAgentFactor w) => w.AgentFactorId).ToList().Contains(x.Id), false, "MemberPurchase", "WithdrawAgentFactors.Withdraw")).Select((AgentFactor x) => new AgentFactorModel(x)).ToList();
            return Success(data);
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<AgentFactorModel>> Create([FromBody] AgentFactorModel model)
    {
        return TryCatch(async () =>
        {
            if (model.Agent == null)
            {
                return Error<AgentFactorModel>(null, "نماینده تعیین نشده است");
            }
            if (model.Member == null)
            {
                return Error<AgentFactorModel>(null, "مشتری تعیین نشده است");
            }
            User agent = await _userManager.GetById(model.Agent.Id);
            User member = await _userManager.FindByIdAsync(model.Member.Id.ToString());
            if (member.CustomerId != _currentUserService.CustomerId || agent.CustomerId != _currentUserService.CustomerId)
            {
                return Error<AgentFactorModel>(null, "مجوز دسترسی به این عملیات برای شما وجود ندارد.");
            }
            if (member.Credit < model.Amount)
            {
                return Error<AgentFactorModel>(null, "اعتبار کیف پول مشتری کافی نمی باشد.");
            }
            if (member.WalletIsLock)
            {
                return Error<AgentFactorModel>(null, "در حال حاضر کیف پول مشتری غیرفعال است و امکان تراکنش کیف پول وجود ندارد.");
            }
            AgentFactor entity = new AgentFactor
            {
                AgentId = model.Agent.Id,
                Amount = model.Amount,
                UserCode = new HtmlSanitizer().Sanitize(model.UserCode),
                CreateAt = model.CreateAt,
                Description = new HtmlSanitizer().Sanitize(model.Description),
                MemberId = member.Id,
                CheckoutType = model.CheckoutType,
                CustomerId = _currentUserService.CustomerId.Value
            };
            await _agentFactorService.Insert(entity);
            member.Credit -= model.Amount;
            await _userManager.UpdateAsync(member);
            model = new AgentFactorModel(entity);
            AgentFactorModel agentFactorModel = new AgentFactorModel(entity);
            try
            {
                try
                {
                    await _integratedSmsSenderService.SendWalletWithdrawNotification(agentFactorModel);
                }
                catch (Exception)
                {
                }
                try
                {
                    await _integratedEmailSenderService.SendWalletWithdrawNotification(agentFactorModel);
                }
                catch (Exception)
                {
                }
            }
            catch (Exception)
            {
            }
            return Success(model, "عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<AgentFactorModel>> Edit([FromBody] AgentFactorModel model)
    {
        return TryCatch(async () =>
        {
            AgentFactor entity = await _agentFactorService.GetById(model.Id, "Member", "Agent");
            if (entity == null)
            {
                return Error<AgentFactorModel>(null, "اطلاعات مورد نظر یافت نشد.");
            }
            if (entity.Member.CustomerId != _currentUserService.CustomerId || entity.Agent.CustomerId != _currentUserService.CustomerId)
            {
                return Error<AgentFactorModel>(null, "مجوز دسترسی به این عملیات برای شما وجود ندارد.");
            }
            if (model.Amount > entity.Amount && entity.Member.Credit < model.Amount - entity.Amount)
            {
                return Error<AgentFactorModel>(null, "اعتبار کیف پول مشتری کافی نمی باشد.");
            }
            if (entity.Member.WalletIsLock)
            {
                return Error<AgentFactorModel>(null, "در حال حاضر کیف پول مشتری غیرفعال است و امکان تراکنش کیف پول وجود ندارد.");
            }
            entity.Amount = model.Amount;
            entity.UserCode = new HtmlSanitizer().Sanitize(model.UserCode);
            entity.Description = new HtmlSanitizer().Sanitize(model.Description);
            entity.CheckoutType = model.CheckoutType;
            await _agentFactorService.Update(entity);
            if (entity.Amount != model.Amount)
            {
                entity.Member.Credit = ((model.Amount > entity.Amount) ? (entity.Member.Credit - (model.Amount - entity.Amount)) : (entity.Member.Credit + (entity.Amount - model.Amount)));
                await _userManager.UpdateAsync(entity.Member);
            }
            model = new AgentFactorModel(entity);
            AgentFactorModel agentFactorModel = new AgentFactorModel(entity);
            try
            {
                try
                {
                    await _integratedSmsSenderService.SendWalletWithdrawNotification(agentFactorModel);
                }
                catch (Exception)
                {
                }
                try
                {
                    await _integratedEmailSenderService.SendWalletWithdrawNotification(agentFactorModel);
                }
                catch (Exception)
                {
                }
            }
            catch (Exception)
            {
            }
            return Success(new AgentFactorModel(entity), "عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]/{id}/{mobileOtp}")]
    public Task<DefaultResponseState> Delete(Guid id, string mobileOtp)
    {
        return TryCatch(async () =>
        {
            AgentFactor entity = await _agentFactorService.GetById(id, "Member");
            if (entity == null)
            {
                return Error("اطلاعات مورد نظر یافت نشد.");
            }
            if (entity.Member.CustomerId != _currentUserService.CustomerId || entity.Agent.CustomerId != _currentUserService.CustomerId)
            {
                return Error("مجوز دسترسی به این عملیات برای شما وجود ندارد.");
            }
            await _agentFactorService.Delete(entity);
            entity.Member.Credit += entity.Amount;
            await _userManager.UpdateAsync(entity.Member);
            return Success();
        });
    }
}
