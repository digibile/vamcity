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
using LoanSystemAvizhe.Data.Enums;
using LoanSystemAvizhe.Site.Areas.Member.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoanSystem.Site.Areas.Agent.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Agent")]
public class AgentFactorController : BaseAgentController
{
    private readonly IAgentFactorService _agentFactorService;

    private readonly IWithdrawAgentFactorService _withdrawAgentFactorService;

    private readonly IAppContext _appContext;

    private readonly IAppSmsService _appSmsService;

    private readonly UserManager _userManager;

    private readonly ICurrentUserService _currentUserService;

    private readonly IntegratedSmsSenderService _integratedSmsSenderService;

    private readonly IntegratedEmailSenderService _integratedEmailSenderService;

    private readonly IAppSettingsService _appSettingsService;

    private readonly IMemberPurchaseService _memberPurchaseService;

    private readonly IAgentService _agentService;

    public AgentFactorController(ILogger<AgentFactorController> logger, IAgentFactorService agentFactorService, IWithdrawAgentFactorService withdrawAgentFactorService, IAppContext appContext, IAppSmsService appSmsService, UserManager userManager, ICurrentUserService currentUserService, IntegratedSmsSenderService integratedSmsSenderService, IntegratedEmailSenderService integratedEmailSenderService, IAppSettingsService appSettingsService, IMemberPurchaseService memberPurchaseService, IAgentService agentService)
        : base(logger)
    {
        _agentFactorService = agentFactorService;
        _withdrawAgentFactorService = withdrawAgentFactorService;
        _appContext = appContext;
        _appSmsService = appSmsService;
        _userManager = userManager;
        _currentUserService = currentUserService;
        _integratedSmsSenderService = integratedSmsSenderService;
        _integratedEmailSenderService = integratedEmailSenderService;
        _appSettingsService = appSettingsService;
        _memberPurchaseService = memberPurchaseService;
        _agentService = agentService;
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
            AgentFactor agentFactor = await _agentFactorService.GetById(id, "Member", "MemberPurchase", "AgentFactorItems");
            if (agentFactor == null)
            {
                return Error<AgentFactorModel>(null, "اطلاعات مورد نظر یافت نشد");
            }
            if (agentFactor.AgentId != _appContext.User.Id)
            {
                return Error<AgentFactorModel>(null, "دسترسی به این فاکتور برای شما مجاز نمی باشد.");
            }
            AgentFactorModel data = new AgentFactorModel(agentFactor);
            return Success(data);
        });
    }

    [HttpGet("[action]")]
    public Task<ResponseState<List<AgentFactorModel>>> List(long? memberId, DateTime? start, DateTime? end)
    {
        return TryCatch(async () =>
        {
            long agentId = _currentUserService.UserId.Value;
            List<AgentFactorModel> data = (from x in await _agentFactorService.GetByCondition((AgentFactor x) => (((DateTime?)start).HasValue ? (x.CreateAt.Date >= ((DateTime?)start).Value.Date) : true) && (((DateTime?)end).HasValue ? (x.CreateAt.Date <= ((DateTime?)end).Value.Date) : true) && x.AgentId == (long?)agentId && (((long?)memberId).HasValue ? (x.MemberId == ((long?)memberId).Value) : true), false, "Member", "Agent", "MemberPurchase", "AgentFactorItems", "WithdrawAgentFactors.Withdraw")
                                           orderby x.CreateAt descending
                                           select new AgentFactorModel(x)).ToList();
            return Success(data);
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<AgentFactorModel>> Create([FromBody] AgentFactorModel model)
    {
        return TryCatch(async () =>
        {
            if (_appContext.User == null)
            {
                return Error<AgentFactorModel>(null, "کاربر یافت نشد");
            }
            User agent = await _userManager.GetById(_currentUserService.UserId.Value);
            User member = await _userManager.FindByIdAsync(model.Member.Id.ToString());
            if (member.CustomerId != agent.CustomerId)
            {
                return Error<AgentFactorModel>(null, "مجوز دسترسی به این عملیات برای شما وجود ندارد.");
            }
            if (model.CheckoutType == AgentFactorCheckoutTypes.کیفـپول)
            {
                string text = await ValidateWalletPayment(model, member);
                if (!string.IsNullOrEmpty(text))
                {
                    return Error<AgentFactorModel>(null, text);
                }
            }
            AgentFactor entity = new AgentFactor
            {
                AgentId = _currentUserService.UserId,
                Amount = model.Amount,
                UserCode = new HtmlSanitizer().Sanitize(model.UserCode),
                CreateAt = DateTime.Now,
                Description = new HtmlSanitizer().Sanitize(model.Description),
                MemberId = member.Id,
                CheckoutType = model.CheckoutType,
                AgentFactorOnlinePaymentMethod = model.AgentFactorOnlinePaymentMethod,
                CustomerId = _currentUserService.CustomerId.Value,
                AgentFactorItems = model.AgentFactorItems?.Select((AgentFactorItemModel x) => new AgentFactorItem
                {
                    Count = x.Count,
                    CustomerId = _currentUserService.CustomerId.Value,
                    Fee = x.Fee,
                    Payable = x.Payable,
                    Product = x.Product
                }).ToList()
            };
            await _agentFactorService.Insert(entity);
            if (model.CheckoutType == AgentFactorCheckoutTypes.کیفـپول)
            {
                try
                {
                    member.Credit -= entity.Amount;
                    await _userManager.UpdateAsync(member);
                    AgentFactorModel agentFactorModel = new AgentFactorModel(entity);
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
                catch (Exception ex3)
                {
                    _ = ex3;
                    await _agentFactorService.Delete(entity);
                    return Error<AgentFactorModel>(null, "در به روز رسانی مانده کیف پول خطایی رخ داده است.");
                }
            }
            model = new AgentFactorModel(entity);
            return Success(model, "عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<MemberPurchaseCreateResult>> OnlinePayment([FromBody] AgentFactorModel model)
    {
        return TryCatch(async () =>
        {
            if (_appContext.User == null)
            {
                return Error<MemberPurchaseCreateResult>(null, "کاربر یافت نشد");
            }
            AgentFactor entity = await _agentFactorService.GetById(model.Id, "Agent", "Member");
            if (entity == null)
            {
                return Error<MemberPurchaseCreateResult>(null, "صورتحساب یافت نشد.");
            }
            User agent = await _agentService.GetById(_currentUserService.UserId.Value, "AgentMetaData");
            User member = await _userManager.FindByIdAsync(model.Member.Id.ToString());
            if (member.CustomerId != agent.CustomerId)
            {
                return Error<MemberPurchaseCreateResult>(null, "مجوز دسترسی به این عملیات برای شما وجود ندارد.");
            }
            if (model.CheckoutType != AgentFactorCheckoutTypes.آنلاین)
            {
                return Error<MemberPurchaseCreateResult>(null, "نحوه پرداخت صورتحساب به صورت درگاه بانکی نمی باشد.");
            }
            string text = await ValidateOnlinePayment(model, agent);
            if (!string.IsNullOrEmpty(text))
            {
                return Error<MemberPurchaseCreateResult>(null, text);
            }
            MemberPurchaseCreateResult res = null;
            try
            {
                if (model.AgentFactorOnlinePaymentMethod == AgentFactorOnlinePaymentMethod.gateway)
                {
                    MemberPurchaseModel model2 = new MemberPurchaseModel
                    {
                        AgentFactor = new AgentFactorModel
                        {
                            Id = entity.Id
                        },
                        Amount = entity.Amount,
                        CreatorUser = new User2Model
                        {
                            Id = _currentUserService.UserId.Value
                        },
                        User = new User2Model
                        {
                            Id = member.Id
                        },
                        MemberPurchaseType = MemberPurchaseTypes.AgentFactor
                    };
                    res = await _memberPurchaseService.Create(model2);
                }
                if (model.AgentFactorOnlinePaymentMethod == AgentFactorOnlinePaymentMethod.sms)
                {
                    await _integratedSmsSenderService.SendAgentFactorPaymentLink(new AgentFactorModel(entity));
                }
                entity.AgentFactorOnlinePaymentMethod = model.AgentFactorOnlinePaymentMethod;
                await _agentFactorService.Update(entity);
            }
            catch (Exception)
            {
                if (model.AgentFactorOnlinePaymentMethod == AgentFactorOnlinePaymentMethod.gateway)
                {
                    return Error<MemberPurchaseCreateResult>(null, "در هدایت به درگاه خطایی رخ داده است");
                }
                if (model.AgentFactorOnlinePaymentMethod == AgentFactorOnlinePaymentMethod.sms)
                {
                    return Error<MemberPurchaseCreateResult>(null, "در حال حاضر امکان ارسال پیامک وجود ندارد.");
                }
            }
            return Success(res, "عملیات با موفقیت انجام شد.");
        });
    }

    private async Task<string> ValidateOnlinePayment(AgentFactorModel entity, User agent)
    {
        if (entity.MemberPurchase != null || (entity.Id != Guid.Empty && entity.CheckoutType != AgentFactorCheckoutTypes.آنلاین))
        {
            return "صورتحساب قبلا پرداخت شده است.";
        }
        AppSettings appSettings = await _appSettingsService.GetAppSettings();
        AgentMetaData agentMetaData = agent.AgentMetaData;
        decimal num = ((agentMetaData != null && !agentMetaData.UseDefaultCommission) ? agent.AgentMetaData.AgentFactorCommission : appSettings.AgentFactorCommission);
        AgentMetaData agentMetaData2 = agent.AgentMetaData;
        decimal val = ((agentMetaData2 != null && !agentMetaData2.UseDefaultCommission) ? agent.AgentMetaData.MaxAgentFactorCommission : appSettings.MaxAgentFactorCommission);
        decimal val2 = entity.Amount * num / 100m;
        val2 = Math.Min(val, val2);
        if (val2 != entity.Commission)
        {
            return "کارمزد محاسبه شده با کارمزدی ارسال مطابقت ندارد.";
        }
        return string.Empty;
    }

    private async Task<string> ValidateWalletPayment(AgentFactorModel model, User member)
    {
        if (member.Credit < model.Amount)
        {
            return "اعتبار کیف پول مشتری کافی نمی باشد.";
        }
        if (member.WalletIsLock)
        {
            return "در حال حاضر کیف پول مشتری غیرفعال است و امکان تراکنش کیف پول وجود ندارد.";
        }
        if (model.CheckoutType == AgentFactorCheckoutTypes.کیفـپول)
        {
            if (string.IsNullOrEmpty(model.MobileOtp))
            {
                return "کد تائیدیه برداشت از کیف پول وارد نشده است.";
            }
            List<AppSms> list = await _appSmsService.GetByCondition((AppSms x) => x.To == model.Member.Mobile && (int)x.AppSmsType == 16 && x.Date >= DateTime.Now.AddMinutes(-2.0) && x.Tag == $"{model.MobileOtp},{model.Amount}", false);
            if (list == null || !list.Any())
            {
                return "کد تائیدیه برداشت از کیف پول معتبر نمی باشد.";
            }
        }
        return string.Empty;
    }

    [HttpGet("[action]/{transactionId}")]
    public Task<ResponseState<List<AgentFactorModel>>> ListByTransactionId(Guid transactionId)
    {
        return TryCatch(async () =>
        {
            List<WithdrawAgentFactor> withdraws = await _withdrawAgentFactorService.GetByCondition((WithdrawAgentFactor x) => x.WithdrawId.Equals(transactionId), false);
            List<AgentFactorModel> data = (await _agentFactorService.GetByCondition((AgentFactor x) => withdraws.Select((WithdrawAgentFactor w) => w.AgentFactorId).ToList().Contains(x.Id) && x.AgentId == _currentUserService.UserId, false, "MemberPurchase", "WithdrawAgentFactors.Withdraw")).Select((AgentFactor x) => new AgentFactorModel(x)).ToList();
            return Success(data);
        });
    }
}
