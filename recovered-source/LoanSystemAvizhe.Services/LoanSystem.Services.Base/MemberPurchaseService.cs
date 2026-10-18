using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LoanSystem.Core.Exceptions;
using LoanSystem.Data;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Models;
using LoanSystem.Services.Models.IPGParsian;
using LoanSystem.Services.Models.NextPay;
using LoanSystem.Services.Models.Sep;
using LoanSystem.Services.Models.Sepehr;
using LoanSystem.Services.Models.ZarinPal;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Data.Enums;
using LoanSystemAvizhe.Services.Models.IranValidity;
using LoanSystemAvizhe.Services.Models.Sadad;
using LoanSystemAvizhe.Services.ThirdParty;
using LoanSystemAvizhe.Site.Areas.Member.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Spm.Services.Infrastructure;

namespace LoanSystem.Services.Base;

public class MemberPurchaseService : BaseService, IMemberPurchaseService
{
    private readonly ILogger<MemberPurchaseService> _logger;

    private readonly IUserPaymentService _userPaymentService;

    private readonly ZarinPalService _zarinPalService;

    private readonly NextPayService _nextPayService;

    private readonly SepService _sepService;

    private readonly SadadService _sadadService;

    private readonly SepehrService _sepehrService;

    private readonly ICustomerService _customerService;

    private readonly IpgParsianService _ipgParsianService;

    private readonly IOptions<ZarinPalConfig> _zarinPalConfig;

    private readonly IOptions<SepConfig> _sepConfig;

    private readonly IOptions<SepehrConfig> _sepehrConfig;

    private readonly IOptions<SadadConfig> _sadadConfig;

    private readonly IOptions<NextPayConfig> _nextPayConfig;

    private readonly IOptions<IPGParsianConfig> _ipgParsianConfig;

    private readonly IAppSettingsService _appSettingsService;

    private readonly IUserLoanService _userLoanService;

    private readonly IMemberService _memberService;

    private readonly IAgentFactorService _agentFactorService;

    private Customer _customer;

    public MemberPurchaseService(ILogger<MemberPurchaseService> logger, AppDbContext context, ICurrentUserService currentUserService, IUserPaymentService userPaymentService, ZarinPalService zarinPalService, NextPayService nextPayService, SepService sepService, SadadService sadadService, SepehrService sepehrService, ICustomerService customerService, IOptions<SepConfig> sepConfig, IOptions<SepehrConfig> sepehrConfig, IOptions<ZarinPalConfig> zarinPalConfig, IOptions<NextPayConfig> nextPayConfig, IOptions<IPGParsianConfig> ipgParsianConfig, IOptions<SadadConfig> sadadConfig, IpgParsianService ipgParsianService, IMemberService memberService, IAgentFactorService agentFactorService, IAppSettingsService appSettingsService, IUserLoanService userLoanService)
        : base(context, currentUserService)
    {
        _logger = logger;
        _userPaymentService = userPaymentService;
        _zarinPalService = zarinPalService;
        _sepService = sepService;
        _sepehrConfig = sepehrConfig;
        _sadadService = sadadService;
        _sepehrService = sepehrService;
        _customerService = customerService;
        _nextPayService = nextPayService;
        _sepConfig = sepConfig;
        _sadadConfig = sadadConfig;
        _nextPayConfig = nextPayConfig;
        _zarinPalConfig = zarinPalConfig;
        _memberService = memberService;
        _agentFactorService = agentFactorService;
        _userPaymentService = userPaymentService;
        _appSettingsService = appSettingsService;
        _userLoanService = userLoanService;
        _appSettingsService = appSettingsService;
        _ipgParsianConfig = ipgParsianConfig;
        _ipgParsianService = ipgParsianService;
    }

    public Task<List<GroupedPurchaseReportModel>> GetGroupedOnTypeByDateAsync(DateTime startDate, DateTime endDate)
    {
        return _context.MemberPurchases.Where((MemberPurchase x) => x.StartDate.Date >= startDate && x.StartDate.Date <= endDate && x.CustomerId == _currentUserService.CustomerId && x.IsPaymentSuccess).GroupBy((MemberPurchase x) => x.MemberPurchaseType, (MemberPurchaseTypes key, IEnumerable<MemberPurchase> x) => new GroupedPurchaseReportModel
        {
            Type = key,
            Count = x.Count(),
            Sum = x.Sum((MemberPurchase s) => s.Amount)
        }).ToListAsync();
    }

    public void DeleteByUserPaymentId(Guid userPaymentId)
    {
        List<MemberPurchase> entities = _context.MemberPurchases.Where((MemberPurchase x) => x.UserLoanPaymentId == userPaymentId).ToList();
        _context.RemoveRange(entities);
    }

    public IQueryable<MemberPurchase> GetAllAsQueryable()
    {
        return Table<MemberPurchase>();
    }

    public Task<List<MemberPurchase>> GetAll(Guid? customerId, params string[] includs)
    {
        IQueryable<MemberPurchase> source = Table<MemberPurchase>();
        foreach (string navigationPropertyPath in includs)
        {
            source = source.Include(navigationPropertyPath);
        }
        if (customerId.HasValue)
        {
            source = source.Where((MemberPurchase x) => x.CustomerId == customerId);
        }
        return source.ToListAsync();
    }

    public Task<MemberPurchase> GetByUserLoanPaymentId(Guid userLoanPaymentId)
    {
        return Table<MemberPurchase>().FirstOrDefaultAsync((MemberPurchase x) => x.UserLoanPaymentId == userLoanPaymentId);
    }

    public Task<MemberPurchase> GetById(Guid entityId, params string[] includs)
    {
        return GetById<MemberPurchase>(entityId, includs);
    }

    public Task<MemberPurchase> GetByCode(int code)
    {
        return _context.MemberPurchases.FirstOrDefaultAsync((MemberPurchase x) => x.Code == code);
    }

    public Task<MemberPurchase> GetByZarinPalAuthority(string authority)
    {
        return _context.MemberPurchases.FromSqlRaw("Select * from MemberPurchases where ZarinPalJson is not null and ZarinPalJson != '' and JSON_VALUE(ZarinPalJson, '$.Authority') = '" + authority + "'").FirstOrDefaultAsync();
    }

    public Task<MemberPurchase> GetBySepResNum(string resNum)
    {
        return _context.MemberPurchases.FromSqlRaw("Select * from MemberPurchases where SepJson is not null and SepJson != '' and JSON_VALUE(SepJson, '$.ResNum') = '" + resNum + "'").FirstOrDefaultAsync();
    }

    public Task<MemberPurchase> GetBySepehrDigitalReceiptNum(string digitalReceipt)
    {
        return _context.MemberPurchases.FromSqlRaw("Select * from MemberPurchases where SepehrJson is not null and SepehrJson != '' and JSON_VALUE(SepehrJson, '$.DigitalReceipt') = '" + digitalReceipt + "'").FirstOrDefaultAsync();
    }

    public Task<MemberPurchase> GetByIPGRRN(long rrn)
    {
        return _context.MemberPurchases.FromSqlRaw($"Select * from MemberPurchases where IPGParsianJson is not null and IPGParsianJson != '' and JSON_VALUE(IPGParsianJson, '$.RRN') = '{rrn}'").FirstOrDefaultAsync();
    }

    public Task<MemberPurchase> GetBySepRefNum(string refNum)
    {
        return _context.MemberPurchases.FromSqlRaw("Select * from MemberPurchases where SepJson is not null and SepJson != '' and JSON_VALUE(SepJson, '$.RefNum') = '" + refNum + "'").FirstOrDefaultAsync();
    }

    public Task<MemberPurchase> GetBySadadRetrivalRefNo(string retrivalRefNo)
    {
        return _context.MemberPurchases.FromSqlRaw("Select * from MemberPurchases where SadadJson is not null and SadadJson != '' and JSON_VALUE(SadadJson, '$.RetrivalRefNo') = '" + retrivalRefNo + "'").FirstOrDefaultAsync();
    }

    public Task<MemberPurchase> GetByNextPayTransId(string transId)
    {
        return _context.MemberPurchases.FromSqlRaw("Select * from MemberPurchases where NextPayJson is not null and NextPayJson != '' and JSON_VALUE(NextPayJson, '$.TransId') = '" + transId + "'").FirstOrDefaultAsync();
    }

    public async Task<MemberPurchaseCreateResult> Create(MemberPurchaseModel model)
    {
        _customer = await _customerService.GetById(_currentUserService.CustomerId.Value);
        AppSettings appSettings = await _appSettingsService.GetAppSettings();
        if (!appSettings.IsOnlinePaymentActive || (appSettings.OnlinePaymentGateway == OnlinePaymentGateways.ZarinPal && string.IsNullOrEmpty(appSettings.ZarinpalMerchantCode)) || (appSettings.OnlinePaymentGateway == OnlinePaymentGateways.JetAqsaat && string.IsNullOrEmpty(appSettings.Shaba)) || (appSettings.OnlinePaymentGateway == OnlinePaymentGateways.Sep && string.IsNullOrEmpty(appSettings.SepTerminalId)) || (appSettings.OnlinePaymentGateway == OnlinePaymentGateways.Sepehr && appSettings.SepehrTerminalId == 0L) || (appSettings.OnlinePaymentGateway == OnlinePaymentGateways.NextPay && string.IsNullOrEmpty(appSettings.NextPayApiKey)) || (appSettings.OnlinePaymentGateway == OnlinePaymentGateways.IPGParsian && string.IsNullOrEmpty(appSettings.IPGParsianLoginAccount)) || (appSettings.OnlinePaymentGateway == OnlinePaymentGateways.Sadad && (string.IsNullOrEmpty(appSettings.SadadTerminalId) || string.IsNullOrEmpty(appSettings.SadadMerchantId))))
        {
            throw new CustomException("تنظیمات درگاه پرداخت آنلاین توسط مدیر انجام نشده است، پرداخت آنلاین پس از فعالسازی و تنظیم توسط مدیر میسر می شود");
        }
        if (model.MemberPurchaseType == MemberPurchaseTypes.AgentFactor && (appSettings.AgentFactorOnlinePaymentGateway == OnlinePaymentGateways.None || (appSettings.AgentFactorCommission == 0m && appSettings.MaxAgentFactorCommission == 0m)))
        {
            throw new CustomException("تنظیمات درگاه پرداخت آنلاین توسط مدیر انجام نشده است، پرداخت آنلاین پس از فعالسازی و تنظیم توسط مدیر میسر می شود");
        }
        string gateway = "";
        string text = "";
        string token = "";
        string terminalId = "";
        try
        {
            if (model.MemberPurchaseType == MemberPurchaseTypes.WalletCredit && !string.IsNullOrEmpty(appSettings.IranValidityConfigJson))
            {
                IranValidityConfig iranValidityConfig = JsonSerializer.Deserialize<IranValidityConfig>(appSettings.IranValidityConfigJson);
                appSettings.OnlinePaymentGateway = iranValidityConfig.PaymentGateway;
            }
            if (model.MemberPurchaseType == MemberPurchaseTypes.AgentFactor)
            {
                appSettings.OnlinePaymentGateway = appSettings.AgentFactorOnlinePaymentGateway;
            }
            switch (appSettings.OnlinePaymentGateway)
            {
                case OnlinePaymentGateways.ZarinPal:
                case OnlinePaymentGateways.JetAqsaat:
                    gateway = "zarinpal";
                    text = await ProccessByZarinPal(model, appSettings);
                    break;
                case OnlinePaymentGateways.Sep:
                    gateway = "sep";
                    (text, token) = await ProccessBySep(model, appSettings);
                    if (!string.IsNullOrEmpty(appSettings.BankGatewayRouterBaseUrl))
                    {
                        gateway = "router";
                    }
                    break;
                case OnlinePaymentGateways.Sepehr:
                    {
                        gateway = "sepehr";
                        (string, string, long) tuple4 = await ProccessBySepehr(model, appSettings);
                        text = tuple4.Item1;
                        terminalId = tuple4.Item3.ToString();
                        token = tuple4.Item2;
                        if (!string.IsNullOrEmpty(appSettings.BankGatewayRouterBaseUrl))
                        {
                            gateway = "router";
                        }
                        break;
                    }
                case OnlinePaymentGateways.IPGParsian:
                    gateway = "ipg";
                    text = await ProccessByIPGParsian(model, appSettings);
                    if (!string.IsNullOrEmpty(appSettings.BankGatewayRouterBaseUrl))
                    {
                        text = appSettings.BankGatewayRouterBaseUrl.TrimEnd('/') + "/start/ipg?url=" + text;
                        gateway = "router";
                    }
                    break;
                case OnlinePaymentGateways.Sadad:
                    {
                        gateway = "sadad";
                        (string, SadadRequestResult) tuple = await ProccessBySadad(model, appSettings);
                        (text, _) = tuple;
                        if (!string.IsNullOrEmpty(appSettings.BankGatewayRouterBaseUrl))
                        {
                            text = appSettings.BankGatewayRouterBaseUrl.TrimEnd('/') + "/start/sadad?token=" + tuple.Item2.Token;
                            gateway = "router";
                        }
                        break;
                    }
                case OnlinePaymentGateways.NextPay:
                    gateway = "nextpay";
                    text = await ProccessByNextPay(model, appSettings);
                    break;
            }
            return new MemberPurchaseCreateResult
            {
                Gateway = gateway,
                GatewayUrl = text,
                Token = token,
                TerminalId = terminalId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw;
        }
    }

    private async Task<(string, string)> ProccessBySep(MemberPurchaseModel model, AppSettings appSettings)
    {
        SepPurchase sepPurchase = new SepPurchase
        {
            TerminalId = appSettings.SepTerminalId
        };
        MemberPurchase memberPurchase = new MemberPurchase
        {
            Amount = model.Amount,
            UserId = _currentUserService.UserId.Value
        };
        MemberPurchase memberPurchase2 = memberPurchase;
        memberPurchase2.Description = await GetPurchaseDescription(model);
        memberPurchase.UserLoanPaymentId = model.UserLoanPayment?.Id;
        memberPurchase.AgentFactorId = model.AgentFactor?.Id;
        memberPurchase.PaymentStep = PaymentSteps.Start;
        memberPurchase.OnlinePaymentGateway = OnlinePaymentGateways.Sep;
        memberPurchase.StartDate = DateTime.Now;
        memberPurchase.SepJson = sepPurchase.ToString();
        memberPurchase.CustomerId = _currentUserService.CustomerId.Value;
        memberPurchase.MemberPurchaseType = model.MemberPurchaseType;
        MemberPurchase entity = memberPurchase;
        await Insert(entity);
        User user = await _memberService.GetById(entity.UserId);
        SepRequest request = new SepRequest
        {
            Action = "token",
            Amount = entity.Amount * 10m,
            RedirectUrl = (string.IsNullOrEmpty(appSettings.BankGatewayRouterBaseUrl) ? ReplaceCustomerParkDomain(_sepConfig.Value.MemberRedirectUrl) : (appSettings.BankGatewayRouterBaseUrl.TrimEnd('/') + "/callback/sep")),
            TerminalId = sepPurchase.TerminalId,
            CellNumber = user?.Mobile,
            ResNum = entity.Id.ToString()
        };
        (string, SepRequestResult) sRequest = (string.IsNullOrEmpty(appSettings.BankGatewayRouterBaseUrl) ? (await _sepService.Request(request)) : (await new BankRouterService(appSettings.BankGatewayRouterBaseUrl).SepRequest(request)));
        entity.PaymentStep = PaymentSteps.ResponsedInvokePayment;
        sepPurchase.TokenStatus = sRequest.Item2.Status;
        sepPurchase.TokenErrorCode = sRequest.Item2.ErrorCode;
        sepPurchase.TokenErrorDesc = sRequest.Item2.ErrorDesc;
        entity.SepJson = sepPurchase.ToString();
        await Update(entity);
        if (sRequest.Item2.Status != 1)
        {
            throw new Exception("در هدایت به درگاه خطایی رخ داده است." + sRequest.Item2.ErrorDesc);
        }
        sepPurchase.ResNum = request.ResNum;
        sepPurchase.Token = sRequest.Item2.Token;
        entity.SepJson = sepPurchase.ToString();
        await Update(entity);
        return (sRequest.Item1, sRequest.Item2.Token);
    }

    private async Task<(string, string, long)> ProccessBySepehr(MemberPurchaseModel model, AppSettings appSettings)
    {
        SepehrPurchase sepehrPurchase = new SepehrPurchase
        {
            TerminalId = appSettings.SepehrTerminalId
        };
        MemberPurchase memberPurchase = new MemberPurchase
        {
            Amount = model.Amount,
            UserId = _currentUserService.UserId.Value
        };
        MemberPurchase memberPurchase2 = memberPurchase;
        memberPurchase2.Description = await GetPurchaseDescription(model);
        memberPurchase.UserLoanPaymentId = model.UserLoanPayment?.Id;
        memberPurchase.AgentFactorId = model.AgentFactor?.Id;
        memberPurchase.PaymentStep = PaymentSteps.Start;
        memberPurchase.OnlinePaymentGateway = OnlinePaymentGateways.Sepehr;
        memberPurchase.StartDate = DateTime.Now;
        memberPurchase.SepehrJson = sepehrPurchase.ToString();
        memberPurchase.CustomerId = _currentUserService.CustomerId.Value;
        memberPurchase.MemberPurchaseType = model.MemberPurchaseType;
        MemberPurchase entity = memberPurchase;
        await Insert(entity);
        User user = await _memberService.GetById(entity.UserId);
        SepehrRequest request = new SepehrRequest
        {
            Amount = entity.Amount * 10m,
            CallbackUrl = (string.IsNullOrEmpty(appSettings.BankGatewayRouterBaseUrl) ? ReplaceCustomerParkDomain(_sepehrConfig.Value.MemberCallBackUrl) : (appSettings.BankGatewayRouterBaseUrl.TrimEnd('/') + "/callback/sepehr")),
            TerminalId = sepehrPurchase.TerminalId,
            Email = user?.Email,
            InvoiceId = entity.Code.ToString()
        };
        (string, SepehrRequestResult) sRequest = (string.IsNullOrEmpty(appSettings.BankGatewayRouterBaseUrl) ? (await _sepehrService.Request(request)) : (await new BankRouterService(appSettings.BankGatewayRouterBaseUrl).SepehrRequest(request)));
        entity.PaymentStep = PaymentSteps.ResponsedInvokePayment;
        sepehrPurchase.TokenStatus = sRequest.Item2.Status;
        entity.SepehrJson = sepehrPurchase.ToString();
        await Update(entity);
        if (sRequest.Item2.Status != 0)
        {
            throw new Exception("در هدایت به درگاه خطایی رخ داده است");
        }
        sepehrPurchase.AccessToken = sRequest.Item2.Accesstoken;
        entity.SepehrJson = sepehrPurchase.ToString();
        await Update(entity);
        return (sRequest.Item1, sRequest.Item2.Accesstoken, sepehrPurchase.TerminalId);
    }

    private async Task<(string, SadadRequestResult)> ProccessBySadad(MemberPurchaseModel model, AppSettings appSettings)
    {
        SadadPurchase sadadPurchase = new SadadPurchase
        {
            TerminalId = appSettings.SadadTerminalId,
            MerchantId = appSettings.SadadMerchantId
        };
        MemberPurchase memberPurchase = new MemberPurchase
        {
            Amount = model.Amount,
            UserId = _currentUserService.UserId.Value
        };
        MemberPurchase memberPurchase2 = memberPurchase;
        memberPurchase2.Description = await GetPurchaseDescription(model);
        memberPurchase.UserLoanPaymentId = model.UserLoanPayment?.Id;
        memberPurchase.AgentFactorId = model.AgentFactor?.Id;
        memberPurchase.PaymentStep = PaymentSteps.Start;
        memberPurchase.OnlinePaymentGateway = OnlinePaymentGateways.Sadad;
        memberPurchase.StartDate = DateTime.Now;
        memberPurchase.SadadJson = sadadPurchase.ToString();
        memberPurchase.CustomerId = _currentUserService.CustomerId.Value;
        memberPurchase.MemberPurchaseType = model.MemberPurchaseType;
        MemberPurchase entity = memberPurchase;
        await Insert(entity);
        User user = await _memberService.GetById(entity.UserId);
        SadadPaymentRequest request = new SadadPaymentRequest
        {
            Amount = entity.Amount * 10m,
            ReturnUrl = ReplaceCustomerParkDomain(_sadadConfig.Value.MemberRedirectUrl),
            TerminalId = sadadPurchase.TerminalId,
            UserId = long.Parse(user?.Mobile ?? "0"),
            OrderId = entity.Code,
            LocalDateTime = DateTime.Now,
            MerchantId = sadadPurchase.MerchantId,
            NationalCode = user?.NationalCode,
            MerchantKey = appSettings.SadadMerchantKey
        };
        (string, SadadRequestResult) sRequest = (string.IsNullOrEmpty(appSettings.BankGatewayRouterBaseUrl) ? (await _sadadService.Request(request)) : (await new BankRouterService(appSettings.BankGatewayRouterBaseUrl).SadadRequest(request)));
        entity.PaymentStep = PaymentSteps.ResponsedInvokePayment;
        sadadPurchase.OrderId = entity.Code;
        sadadPurchase.ResCode = sRequest.Item2.ResCode;
        sadadPurchase.Description = sRequest.Item2.Description;
        entity.SadadJson = sadadPurchase.ToString();
        await Update(entity);
        if (sRequest.Item2.ResCode != 0)
        {
            throw new Exception("در هدایت به درگاه خطایی رخ داده است");
        }
        sadadPurchase.Token = sRequest.Item2.Token;
        entity.SadadJson = sadadPurchase.ToString();
        await Update(entity);
        return sRequest;
    }

    private async Task<string> ProccessByIPGParsian(MemberPurchaseModel model, AppSettings appSettings)
    {
        IPGParsianPurchase ipgPurchase = new IPGParsianPurchase
        {
            LoginAccount = appSettings.IPGParsianLoginAccount
        };
        MemberPurchase memberPurchase = new MemberPurchase
        {
            Amount = model.Amount,
            UserId = _currentUserService.UserId.Value
        };
        MemberPurchase memberPurchase2 = memberPurchase;
        memberPurchase2.Description = await GetPurchaseDescription(model);
        memberPurchase.UserLoanPaymentId = model.UserLoanPayment?.Id;
        memberPurchase.AgentFactorId = model.AgentFactor?.Id;
        memberPurchase.PaymentStep = PaymentSteps.Start;
        memberPurchase.OnlinePaymentGateway = OnlinePaymentGateways.IPGParsian;
        memberPurchase.StartDate = DateTime.Now;
        memberPurchase.IPGParsianJson = ipgPurchase.ToString();
        memberPurchase.CustomerId = _currentUserService.CustomerId.Value;
        memberPurchase.MemberPurchaseType = model.MemberPurchaseType;
        MemberPurchase entity = memberPurchase;
        await Insert(entity);
        User user = await _memberService.GetById(entity.UserId);
        IPGParsianRequest request = new IPGParsianRequest
        {
            Amount = (long)(entity.Amount * 10m),
            CallBackUrl = (string.IsNullOrEmpty(appSettings.BankGatewayRouterBaseUrl) ? ReplaceCustomerParkDomain(_ipgParsianConfig.Value.MemberCallBackUrl) : (appSettings.BankGatewayRouterBaseUrl.TrimEnd('/') + "/callback/ipg")),
            LoginAccount = ipgPurchase.LoginAccount,
            AdditionalData = user?.Mobile,
            OrderId = entity.Code
        };
        (string, IPGParsianRequestResult) sRequest = await _ipgParsianService.Request(request);
        entity.PaymentStep = PaymentSteps.ResponsedInvokePayment;
        ipgPurchase.TokenStatus = sRequest.Item2.Status;
        ipgPurchase.TokenErrorCode = sRequest.Item2.Status;
        ipgPurchase.TokenErrorDesc = sRequest.Item2.Message;
        entity.IPGParsianJson = ipgPurchase.ToString();
        await Update(entity);
        if (sRequest.Item2.Status != 0 || sRequest.Item2.Token <= 0)
        {
            throw new Exception("در هدایت به درگاه خطایی رخ داده است");
        }
        ipgPurchase.Token = sRequest.Item2.Token;
        entity.IPGParsianJson = ipgPurchase.ToString();
        await Update(entity);
        return sRequest.Item1;
    }

    private async Task<string> ProccessByZarinPal(MemberPurchaseModel model, AppSettings appSettings)
    {
        ZarinPalPurchase zarinpalPurchase = new ZarinPalPurchase
        {
            MerchantId = ((appSettings.OnlinePaymentGateway == OnlinePaymentGateways.JetAqsaat) ? _zarinPalConfig.Value.MerchantID : appSettings.ZarinpalMerchantCode)
        };
        MemberPurchase memberPurchase = new MemberPurchase
        {
            Amount = model.Amount,
            UserId = _currentUserService.UserId.Value
        };
        MemberPurchase memberPurchase2 = memberPurchase;
        memberPurchase2.Description = await GetPurchaseDescription(model);
        memberPurchase.OnlinePaymentGateway = appSettings.OnlinePaymentGateway;
        memberPurchase.UserLoanPaymentId = model.UserLoanPayment?.Id;
        memberPurchase.AgentFactorId = model.AgentFactor?.Id;
        memberPurchase.PaymentStep = PaymentSteps.Start;
        memberPurchase.StartDate = DateTime.Now;
        memberPurchase.ZarinPalJson = zarinpalPurchase.ToString();
        memberPurchase.CustomerId = _currentUserService.CustomerId.Value;
        memberPurchase.MemberPurchaseType = model.MemberPurchaseType;
        MemberPurchase entity = memberPurchase;
        await Insert(entity);
        User user = await _memberService.GetById(entity.UserId);
        ZarinPalRequestWage zarinPalRequestWage = new ZarinPalRequestWage
        {
            amount = (entity.Amount * 10m).ToString(),
            callback_url = ReplaceCustomerParkDomain(_zarinPalConfig.Value.MemberCallbackUrl),
            description = entity.Description,
            merchant_id = zarinpalPurchase.MerchantId,
            metadata = new ZarinPalRequestMetadata
            {
                email = user?.Email,
                mobile = user?.Mobile
            }
        };
        if (appSettings.OnlinePaymentGateway == OnlinePaymentGateways.JetAqsaat)
        {
            zarinPalRequestWage.wages = new List<ZarinPalWage>
            {
                new ZarinPalWage
                {
                    amount = entity.Amount * 10m * 99m / 100m,
                    iban = "IR" + appSettings.Shaba,
                    description = $"تسهیم از مبلغ کل {entity.Amount * 10m}"
                }
            };
        }
        (string, ZarinPalRequestResult) zRequest = await _zarinPalService.Request(zarinPalRequestWage, (ZarinPalRequestMode)_zarinPalConfig.Value.Mode);
        entity.PaymentStep = PaymentSteps.ResponsedInvokePayment;
        zarinpalPurchase.AuthorityRequestStatus = zRequest.Item2.data?.code ?? 0;
        entity.ZarinPalJson = zarinpalPurchase.ToString();
        await Update(entity);
        ZarinPalRequestResultData data = zRequest.Item2.data;
        if (data == null || data.code != 100)
        {
            throw new Exception("در هدایت به درگاه خطایی رخ داده است");
        }
        zarinpalPurchase.Authority = zRequest.Item2.data.authority;
        entity.ZarinPalJson = zarinpalPurchase.ToString();
        await Update(entity);
        return zRequest.Item1;
    }

    private async Task<string> ProccessByNextPay(MemberPurchaseModel model, AppSettings appSettings)
    {
        NextPayPurchase nextPayPurchase = new NextPayPurchase
        {
            ApiKey = appSettings.NextPayApiKey
        };
        MemberPurchase memberPurchase = new MemberPurchase
        {
            Amount = model.Amount,
            UserId = _currentUserService.UserId.Value
        };
        MemberPurchase memberPurchase2 = memberPurchase;
        memberPurchase2.Description = await GetPurchaseDescription(model);
        memberPurchase.OnlinePaymentGateway = appSettings.OnlinePaymentGateway;
        memberPurchase.UserLoanPaymentId = model.UserLoanPayment?.Id;
        memberPurchase.AgentFactorId = model.AgentFactor?.Id;
        memberPurchase.PaymentStep = PaymentSteps.Start;
        memberPurchase.StartDate = DateTime.Now;
        memberPurchase.NextPayJson = nextPayPurchase.ToString();
        memberPurchase.CustomerId = _currentUserService.CustomerId.Value;
        memberPurchase.MemberPurchaseType = model.MemberPurchaseType;
        MemberPurchase entity = memberPurchase;
        await Insert(entity);
        User user = await _memberService.GetById(entity.UserId);
        NextPayRequest request = new NextPayRequest
        {
            api_key = nextPayPurchase.ApiKey,
            amount = (entity.Amount * 10m).ToString(),
            callback_url = ReplaceCustomerParkDomain(_nextPayConfig.Value.MemberCallbackUrl),
            payer_desc = entity.Description,
            customer_phone = user?.Phone,
            order_id = entity.Id.ToString(),
            payer_name = user?.FullName
        };
        (string, NextPayRequestResult) zRequest = await _nextPayService.Request(request);
        entity.PaymentStep = PaymentSteps.ResponsedInvokePayment;
        nextPayPurchase.AuthorityRequestStatus = zRequest.Item2.code;
        entity.NextPayJson = nextPayPurchase.ToString();
        await Update(entity);
        if (zRequest.Item2.code != -1)
        {
            throw new Exception("در هدایت به درگاه خطایی رخ داده است");
        }
        nextPayPurchase.TransId = zRequest.Item2.trans_id.ToString();
        entity.NextPayJson = nextPayPurchase.ToString();
        await Update(entity);
        return zRequest.Item1;
    }

    private async Task<string> GetPurchaseDescription(MemberPurchaseModel model)
    {
        switch (model.MemberPurchaseType)
        {
            case MemberPurchaseTypes.LoanPayment:
                {
                    UserLoanPayment userLoanPayment = await _userPaymentService.GetById(model.UserLoanPayment.Id, "UserLoan");
                    return $"پرداخت قسط شماره {userLoanPayment.InstallmentNumber} بابت خرید کالا به صورت اقساطی به شماره {userLoanPayment.UserLoan.Code}";
                }
            case MemberPurchaseTypes.LoanCheckout:
                return string.Format("تسویه خرید اقساطی با شماره پرونده {0}", (await _userPaymentService.GetById(model.UserLoanPayment.Id, "UserLoan")).UserLoan.Code);
            case MemberPurchaseTypes.WalletCredit:
                return "شارژ کیف پول به مبلغ " + model.Amount;
            case MemberPurchaseTypes.AgentFactor:
                return $"پرداخت صورتحساب خرید کالا به شماره {model.AgentFactor.Code}";
            default:
                return "نامشخص";
        }
    }

    public async Task ApplyPurchase(Guid purchaseId, bool sendSms = true)
    {
        MemberPurchase memberPurchase = await _context.MemberPurchases.FirstOrDefaultAsync((MemberPurchase x) => x.Id == purchaseId);
        if (memberPurchase == null)
        {
            throw new CustomException($"پرداخت آنلاینی با شناسه {purchaseId} یافت نشد.");
        }
        if (memberPurchase.PaymentStep == PaymentSteps.Finish)
        {
            throw new CustomException("پرداخت قبلا پردازش شده است.");
        }
        if (!memberPurchase.IsPaymentSuccess)
        {
            throw new CustomException("سفارش دارای پرداخت موفق نمی باشد.");
        }
        if (memberPurchase.MemberPurchaseType == MemberPurchaseTypes.LoanPayment || memberPurchase.MemberPurchaseType == MemberPurchaseTypes.LoanCheckout)
        {
            await ApplyLoanPayment(memberPurchase, sendSms);
        }
        if (memberPurchase.MemberPurchaseType == MemberPurchaseTypes.WalletCredit)
        {
            await ApplyWalletCharge(memberPurchase);
        }
        if (memberPurchase.MemberPurchaseType == MemberPurchaseTypes.AgentFactor)
        {
            await ApplyAgentFctor(memberPurchase);
        }
        memberPurchase.PaymentStep = PaymentSteps.Finish;
        await Update(memberPurchase);
    }

    private async Task ApplyWalletCharge(MemberPurchase memberPurchase)
    {
        User user = await _memberService.GetById(memberPurchase.UserId);
        user.Credit += memberPurchase.Amount;
        await _memberService.Update(user);
    }

    private async Task ApplyLoanPayment(MemberPurchase memberPurchase, bool sendSms)
    {
        if (!memberPurchase.UserLoanPaymentId.HasValue)
        {
            return;
        }
        bool canCheckout = false;
        MemberPurchaseModel memberPurchaseModel = new MemberPurchaseModel(memberPurchase);
        UserLoanPayment userLoanPayment = (await _userPaymentService.GetByCondition((UserLoanPayment x) => x.Id == (Guid)memberPurchase.UserLoanPaymentId, "UserLoan.Loan", "UserLoanPaymentOthers"))?.FirstOrDefault();
        if (userLoanPayment == null)
        {
            throw new CustomException("قسط یافت نشد");
        }
        if (memberPurchase.MemberPurchaseType == MemberPurchaseTypes.LoanCheckout)
        {
            decimal item = (await _userLoanService.GetCheckoutAmount(userLoanPayment.UserLoanId)).Item1;
            canCheckout = item <= memberPurchase.Amount;
        }
        if (userLoanPayment.Amount == 0m)
        {
            userLoanPayment.Amount = memberPurchase.Amount;
            userLoanPayment.SaveDate = DateTime.Now;
            userLoanPayment.PaymentMethod = PaymentMethods.Online;
            userLoanPayment.Description += $"درگاه پرداخت [{memberPurchaseModel.OnlinePaymentGatewayText}], شماره پیگیری [{GetRefCode(memberPurchaseModel)}]";
            UserLoanPayment userLoanPayment2 = userLoanPayment;
            userLoanPayment2.DelayDay = await _userPaymentService.CalcDelayDay(new LoanPaymentModel(userLoanPayment));
            userLoanPayment2 = userLoanPayment;
            userLoanPayment2.DelayPrice = await _userPaymentService.CalcDelayPrice(new LoanPaymentModel(userLoanPayment));
        }
        else
        {
            userLoanPayment.UserLoanPaymentOthers.Add(new UserLoanPaymentOther
            {
                Amount = memberPurchase.Amount,
                SaveDate = DateTime.Now,
                UserLoanPaymentId = userLoanPayment.Id,
                PaymentMethod = PaymentMethods.Online,
                Description = userLoanPayment.Description + $"درگاه پرداخت [{memberPurchaseModel.OnlinePaymentGatewayText}], شماره پیگیری [{GetRefCode(memberPurchaseModel)}]",
                MemberPurchaseId = memberPurchase.Id
            });
        }
        await _userPaymentService.Update(userLoanPayment, sendSms);
        if (memberPurchase.MemberPurchaseType == MemberPurchaseTypes.LoanCheckout)
        {
            UserLoan userLoan = await _userLoanService.GetById(userLoanPayment.UserLoanId);
            if (canCheckout && userLoan != null && userLoan.IsCheckouted != true)
            {
                userLoan.CheckoutDate = DateTime.Now;
                userLoan.IsCheckouted = true;
                await _userLoanService.Update(userLoan);
            }
        }
    }

    private async Task ApplyAgentFctor(MemberPurchase memberPurchase)
    {
        if (memberPurchase.AgentFactorId.HasValue)
        {
            new MemberPurchaseModel(memberPurchase);
            AgentFactor agentFactor = await _agentFactorService.GetById(memberPurchase.AgentFactorId.Value);
            agentFactor.MemberPurchaseId = memberPurchase.Id;
            _currentUserService.CustomerId = memberPurchase.CustomerId;
            AppSettings appSettings = await _appSettingsService.GetAppSettings();
            if (appSettings.AgentFactorCommission > 0m)
            {
                agentFactor.Commission = agentFactor.Amount * appSettings.AgentFactorCommission / 100m;
            }
            if (appSettings.MaxAgentFactorCommission > 0m)
            {
                agentFactor.Commission = Math.Min(appSettings.MaxAgentFactorCommission, agentFactor.Commission);
            }
            await _agentFactorService.Update(agentFactor);
        }
    }

    private object GetRefCode(MemberPurchaseModel memberPurchase)
    {
        if (memberPurchase.OnlinePaymentGateway == OnlinePaymentGateways.ZarinPal && !string.IsNullOrEmpty(memberPurchase.ZarinPalPurchase.RefId))
        {
            return memberPurchase.ZarinPalPurchase.RefId;
        }
        if (memberPurchase.OnlinePaymentGateway == OnlinePaymentGateways.Sep && !string.IsNullOrEmpty(memberPurchase.SepPurchase.RefNum))
        {
            return memberPurchase.SepPurchase.RefNum;
        }
        if (memberPurchase.OnlinePaymentGateway == OnlinePaymentGateways.NextPay && !string.IsNullOrEmpty(memberPurchase.NextPayPurchase.Shaparak_Ref_Id))
        {
            return memberPurchase.NextPayPurchase.Shaparak_Ref_Id;
        }
        if (memberPurchase.OnlinePaymentGateway == OnlinePaymentGateways.IPGParsian && memberPurchase.IPGParsianPurchase.RRN != 0L)
        {
            return memberPurchase.IPGParsianPurchase.RRN.ToString();
        }
        if (memberPurchase.OnlinePaymentGateway == OnlinePaymentGateways.Sadad && !string.IsNullOrEmpty(memberPurchase.SadadPurchase.SystemTraceNo))
        {
            return memberPurchase.SadadPurchase.SystemTraceNo;
        }
        if (memberPurchase.OnlinePaymentGateway == OnlinePaymentGateways.Sepehr)
        {
            return memberPurchase.SepehrPurchase.Rrn;
        }
        return "";
    }

    public Task Insert(MemberPurchase entity)
    {
        Insert<MemberPurchase>(entity);
        return Save();
    }

    public Task Update(MemberPurchase entity)
    {
        Update(entity, true);
        return Save();
    }

    public Task Delete(MemberPurchase entity)
    {
        Delete<MemberPurchase>(entity);
        return Save();
    }

    public string ReplaceCustomerParkDomain(string originalUrl)
    {
        if (_customer == null || string.IsNullOrEmpty(_customer.ParkDomain))
        {
            return originalUrl;
        }
        Uri uri = new Uri(originalUrl);
        return new UriBuilder(uri)
        {
            Host = (_customer.ParkDomain.Contains(":") ? _customer.ParkDomain.Split(':')[0] : _customer.ParkDomain),
            Port = (_customer.ParkDomain.Contains(":") ? int.Parse(_customer.ParkDomain.Split(':')[1]) : uri.Port)
        }.Uri.ToString();
    }
}
