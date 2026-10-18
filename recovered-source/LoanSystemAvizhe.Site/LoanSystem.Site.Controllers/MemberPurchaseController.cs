using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LoanSystem.Core.Configuration;
using LoanSystem.Core.Exceptions;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Services.Models.IPGParsian;
using LoanSystem.Services.Models.NextPay;
using LoanSystem.Services.Models.Sep;
using LoanSystem.Services.Models.Sepehr;
using LoanSystem.Services.Models.ZarinPal;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Data.Enums;
using LoanSystemAvizhe.Services.Models.Sadad;
using LoanSystemAvizhe.Services.ThirdParty;
using LoanSystemAvizhe.Site.Areas.Member.Models;
using LoanSystemAvizhe.Site.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NikanBase.Services.Security;

namespace LoanSystem.Site.Controllers;

[Route("[controller]")]
public class MemberPurchaseController : BaseDefaultController
{
    private readonly IMemberPurchaseService _memberPurchaseService;

    private readonly ZarinPalService _zarinPalService;

    private readonly SepService _sepService;

    private readonly SepehrService _sepehrService;

    private readonly SadadService _sadadService;

    private readonly IpgParsianService _parsianService;

    private readonly NextPayService _nextPayService;

    private readonly IOptions<ZarinPalConfig> _zarinPalConfig;

    private readonly IOptions<AppConfig> _appConfig;

    private readonly IAgentFactorService _agentFactorService;

    private readonly IUserPaymentService _userPaymentService;

    private readonly ICurrentUserService _currentUserService;

    private readonly ICustomerService _customerService;

    private readonly IAppSettingsService _appSettingsService;

    private readonly IEncryptionService _encryptionService;

    private readonly IMemberService _memberService;

    private readonly IUserLoanService _userLoanService;

    public MemberPurchaseController(ILogger<PurchaseController> logger, IMemberPurchaseService memberPurchaseService, ZarinPalService zarinPalService, SepService sepService, SepehrService sepehrService, SadadService sadadService, IpgParsianService parsianService, NextPayService nextPayService, IOptions<ZarinPalConfig> zarinPalConfig, IOptions<AppConfig> appConfig, IUserPaymentService userPaymentService, ICustomerService customerService, ICurrentUserService currentUserService, IAppSettingsService appSettingsService, IEncryptionService encryptionService, IMemberService memberService, IUserLoanService userLoanService, IAgentFactorService agentFactorService)
        : base(logger)
    {
        _currentUserService = currentUserService;
        _memberPurchaseService = memberPurchaseService;
        _zarinPalService = zarinPalService;
        _zarinPalConfig = zarinPalConfig;
        _sepService = sepService;
        _sepehrService = sepehrService;
        _sadadService = sadadService;
        _nextPayService = nextPayService;
        _userPaymentService = userPaymentService;
        _customerService = customerService;
        _parsianService = parsianService;
        _appConfig = appConfig;
        _appSettingsService = appSettingsService;
        _encryptionService = encryptionService;
        _memberService = memberService;
        _userLoanService = userLoanService;
        _agentFactorService = agentFactorService;
    }

    [HttpPost("[action]")]
    public Task<ResponseState<MemberPurchaseCreateResult>> Create([FromBody] MemberPurchaseModel model)
    {
        foreach (KeyValuePair<string, ModelStateEntry> item in ModelState)
        {
            string key = item.Key;
            if (key.Contains("AgentFactor.Agent"))
            {
                ModelState[key].Errors.Clear();
                ModelState[key].ValidationState = ModelValidationState.Valid;
            }
            if (key.Contains("AgentFactor.Member"))
            {
                ModelState[key].Errors.Clear();
                ModelState[key].ValidationState = ModelValidationState.Valid;
            }
        }
        return TryCatch(async () =>
        {
            Customer customer = await _customerService.GetByCode(model.CustomerCode);
            if (customer == null)
            {
                return Error<MemberPurchaseCreateResult>(null, "در هدایت به درگاه خطایی رخ داده است");
            }
            _currentUserService.CustomerId = customer.Id;
            if (model.MemberPurchaseType == MemberPurchaseTypes.WalletCredit)
            {
                if (model.Amount < 50000m)
                {
                    return Error<MemberPurchaseCreateResult>(null, "حداقل مبلغ شارژ 50 هزار تومان می باشد.");
                }
                _currentUserService.UserId = model.User.Id;
            }
            else if (model.MemberPurchaseType == MemberPurchaseTypes.AgentFactor)
            {
                _currentUserService.UserId = model.User.Id;
            }
            else
            {
                _currentUserService.UserId = model.UserLoanPayment?.UserLoanModel?.MemberModel.Id;
            }
            new MemberPurchaseCreateResult();
            MemberPurchaseCreateResult data;
            try
            {
                data = await _memberPurchaseService.Create(model);
            }
            catch (CustomException ex)
            {
                return Error<MemberPurchaseCreateResult>(null, ex.Message);
            }
            catch (Exception)
            {
                return Error<MemberPurchaseCreateResult>(null, "در هدایت به درگاه خطایی رخ داده است");
            }
            return Success(data);
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<PreOnlinePaymentResult>> PreOnlinePayment([FromBody] PreOnlinePaymentDto model)
    {
        return TryCatch(async () =>
        {
            Customer customer = await _customerService.GetByCode(model.CustomerCode);
            if (customer == null)
            {
                return Error<PreOnlinePaymentResult>(null, "کد پذیرنده معتبر نمی باشد");
            }
            _currentUserService.CustomerId = customer.Id;
            UserLoanPayment userPayment = await _userPaymentService.GetByCode(model.LoanPaymentCode, "UserLoanPaymentOthers");
            if (userPayment == null)
            {
                return Error<PreOnlinePaymentResult>(null, "کد قسط معتبر نمی باشد");
            }
            if (userPayment.SumAmount >= userPayment.InstallmentAmount)
            {
                return Error<PreOnlinePaymentResult>(null, "قسط قبلا پرداخت شده است");
            }
            if (userPayment.UserLoan?.User.CustomerId != customer.Id)
            {
                return Error<PreOnlinePaymentResult>(null, "وام متعلق به پذیرنده نمی باشد");
            }
            PreOnlinePaymentResult result = new PreOnlinePaymentResult
            {
                UserLoanPayment = new LoanPaymentModel(userPayment)
            };
            UserLoan userLoan = await _userLoanService.GetById(userPayment.UserLoanId, "Loan", "User", "UserLoanPayments");
            UserLoanModel userLoanModel = new UserLoanModel(userLoan)
            {
                LoanPayments = userLoan.UserLoanPayments.Select((UserLoanPayment p) => new LoanPaymentModel(p)).ToList()
            };
            await _userPaymentService.SetPaymentsDelays(userLoanModel.LoanPayments);
            result.UserLoanPayment.UserLoanModel = userLoanModel;
            await _userPaymentService.SetPaymentsDelays(new List<LoanPaymentModel> { result.UserLoanPayment }, calcEarly: true);
            result.Member = userPayment.UserLoan?.User?.FullName ?? "نا مشخص";
            result.Customer = customer.Name;
            return Success(result, "عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<WalletChargeResult>> WalletCharge([FromBody] WalletChargeDto model)
    {
        return TryCatch(async () =>
        {
            if (string.IsNullOrEmpty(model.CustomerCode) || string.IsNullOrEmpty(model.MemberId))
            {
                return Error<WalletChargeResult>(null, "پارامترهای ارسالی معتبر نمی باشد.");
            }
            int.TryParse(model.CustomerCode, out var result);
            long.TryParse(model.MemberId, out var memberId);
            Customer customer = await _customerService.GetByCode(result);
            if (customer == null)
            {
                return Error<WalletChargeResult>(null, "کد پذیرنده معتبر نمی باشد");
            }
            _currentUserService.CustomerId = customer.Id;
            User user = await _memberService.GetById(memberId);
            if (user == null)
            {
                return Error<WalletChargeResult>(null, "مشتری یافت نشد.");
            }
            WalletChargeResult data = new WalletChargeResult
            {
                Member = (user.FullName ?? "نامشخص"),
                MemberId = user.Id,
                Customer = customer.Name,
                CustomerCode = customer.Code
            };
            return Success(data, "عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<PreAgentFactorPaymentResult>> PreAgentFactorPayment([FromBody] PreAgentFactorPaymentDto model)
    {
        return TryCatch(async () =>
        {
            Customer customer = await _customerService.GetByCode(model.CustomerCode);
            if (customer == null)
            {
                return Error<PreAgentFactorPaymentResult>(null, "کد پذیرنده معتبر نمی باشد");
            }
            _currentUserService.CustomerId = customer.Id;
            AgentFactor agentFactor = (await _agentFactorService.GetByCondition((AgentFactor x) => x.Code == model.AgentFactorCode, false, "Agent", "Member")).FirstOrDefault();
            if (agentFactor == null)
            {
                return Error<PreAgentFactorPaymentResult>(null, "شماره فاکتور معتبر نمی باشد");
            }
            if (agentFactor.MemberPurchaseId.HasValue)
            {
                return Error<PreAgentFactorPaymentResult>(null, "صورتحساب قبلا پرداخت شده است");
            }
            PreAgentFactorPaymentResult data = new PreAgentFactorPaymentResult
            {
                Member = (agentFactor.Member?.FullName ?? "نامشخص"),
                MemberId = (agentFactor.Member?.Id ?? 0),
                Agent = (agentFactor.Agent?.BusinessName ?? "نامشخص"),
                AgentId = (agentFactor.Agent?.Id ?? 0),
                Customer = customer.Name,
                AgentFactorId = agentFactor.Id,
                AgentFactorAmount = agentFactor.Amount,
                CustomerCode = customer.Code
            };
            return Success(data, "عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<string>> VerifyZarinPal([FromBody] ZarinPalVerify model)
    {
        return TryCatch(async () =>
        {
            MemberPurchase purchase = await _memberPurchaseService.GetByZarinPalAuthority(model.authority);
            if (purchase == null)
            {
                return Error("", "پرداخت با خطا مواجه شده است. ثبت سفارشی با اطلاعات دریافت شده در سیستم یافت نشد.");
            }
            if (purchase.IsPaymentSuccess)
            {
                return Error("", "این سفارش قبلا پردازش شده است.");
            }
            if (!_currentUserService.CustomerId.HasValue)
            {
                _currentUserService.CustomerId = purchase.CustomerId;
            }
            MemberPurchaseModel purchaseModel = new MemberPurchaseModel(purchase)
            {
                ZarinPalPurchase =
                {
                    Status = model.status
                }
            };
            purchase.ZarinPalJson = purchaseModel.ZarinPalPurchase.ToString();
            await _memberPurchaseService.Update(purchase);
            if (model.status != "OK")
            {
                return Error("", "پرداخت انجام نشده است.");
            }
            purchase.PaymentStep = PaymentSteps.InvokePaymentVerification;
            await _memberPurchaseService.Update(purchase);
            ZarinPalVerifyResult validate = await _zarinPalService.Verify(new ZarinPalVerify
            {
                amount = (int)purchase.Amount * 10,
                authority = model.authority,
                merchant_id = purchaseModel.ZarinPalPurchase.MerchantId
            }, (ZarinPalRequestMode)_zarinPalConfig.Value.Mode);
            purchaseModel.ZarinPalPurchase.RefId = validate.data.ref_id.ToString() ?? "";
            purchaseModel.ZarinPalPurchase.VerifyStatus = validate.data.code;
            purchase.ZarinPalJson = JsonSerializer.Serialize(purchaseModel.ZarinPalPurchase);
            purchase.PaymentStep = PaymentSteps.ResponsedInvokePaymentVerification;
            await _memberPurchaseService.Update(purchase);
            ZarinPalVerifyResultData data = validate.data;
            if (data != null && data.code == 101)
            {
                return Success(purchaseModel.ZarinPalPurchase.RefId, "عملیات با موفقیت انجام شد.");
            }
            ZarinPalVerifyResultData data2 = validate.data;
            if (data2 == null || data2.code != 100)
            {
                return Error("", "شماره سفارش: " + purchase.Id);
            }
            purchase.IsPaymentSuccess = true;
            await _memberPurchaseService.Update(purchase);
            try
            {
                await _memberPurchaseService.ApplyPurchase(purchase.Id);
            }
            catch (Exception ex)
            {
                return Error("", ex.Message + "شماره پیگیری: " + validate.data.ref_id);
            }
            return Success(purchaseModel.ZarinPalPurchase.RefId, "عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<string>> VerifyNextPay([FromBody] NextPayVerify model)
    {
        return TryCatch(async () =>
        {
            MemberPurchase purchase = await _memberPurchaseService.GetByNextPayTransId(model.trans_id);
            if (purchase == null)
            {
                return Error("", "پرداخت با خطا مواجه شده است. ثبت سفارشی با اطلاعات دریافت شده در سیستم یافت نشد.");
            }
            if (purchase.IsPaymentSuccess)
            {
                return Error("", "این سفارش قبلا پردازش شده است.");
            }
            if (!_currentUserService.CustomerId.HasValue)
            {
                _currentUserService.CustomerId = purchase.CustomerId;
            }
            MemberPurchaseModel purchaseModel = new MemberPurchaseModel(purchase);
            purchase.PaymentStep = PaymentSteps.InvokePaymentVerification;
            await _memberPurchaseService.Update(purchase);
            NextPayVerifyResult validate = await _nextPayService.Verify(new NextPayVerify
            {
                amount = (int)purchase.Amount * 10,
                api_key = purchaseModel.NextPayPurchase.ApiKey,
                trans_id = model.trans_id
            });
            purchaseModel.NextPayPurchase.Shaparak_Ref_Id = validate.Shaparak_Ref_Id;
            purchaseModel.NextPayPurchase.VerifyCode = validate.code;
            purchase.NextPayJson = JsonSerializer.Serialize(purchaseModel.NextPayPurchase);
            purchase.PaymentStep = PaymentSteps.ResponsedInvokePaymentVerification;
            await _memberPurchaseService.Update(purchase);
            if (validate.code != 0)
            {
                return Error("", "شماره سفارش: " + purchase.Id);
            }
            purchase.IsPaymentSuccess = true;
            await _memberPurchaseService.Update(purchase);
            try
            {
                await _memberPurchaseService.ApplyPurchase(purchase.Id);
            }
            catch (Exception ex)
            {
                return Error("", ex.Message + "شماره پیگیری: " + validate.Shaparak_Ref_Id);
            }
            return Success(purchaseModel.ZarinPalPurchase.RefId, "عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<string>> VerifySep([FromBody] SepPurchase model)
    {
        return TryCatch(async () =>
        {
            MemberPurchase purchase = await _memberPurchaseService.GetBySepResNum(model.ResNum);
            if (purchase == null)
            {
                return Error("", "پرداخت با خطا مواجه شده است. ثبت سفارشی با اطلاعات دریافت شده در سیستم یافت نشد.");
            }
            if (purchase.IsPaymentSuccess)
            {
                return Error("", "این سفارش قبلا پردازش شده است.");
            }
            if (!_currentUserService.CustomerId.HasValue)
            {
                _currentUserService.CustomerId = purchase.CustomerId;
            }
            MemberPurchaseModel purchaseModel = new MemberPurchaseModel(purchase);
            if (await _memberPurchaseService.GetBySepRefNum(model.RefNum) != null && purchaseModel.SepPurchase.ResNum != model.ResNum)
            {
                return Error("", "این رسید دیجیتال قبلا پردازش شده است.");
            }
            purchaseModel.SepPurchase.State = model.State;
            purchaseModel.SepPurchase.Status = model.Status;
            purchaseModel.SepPurchase.MID = model.MID;
            purchaseModel.SepPurchase.RefNum = model.RefNum;
            purchaseModel.SepPurchase.SecurePan = model.SecurePan;
            purchaseModel.SepPurchase.RRN = model.RRN;
            purchaseModel.SepPurchase.TraceNo = model.TraceNo;
            purchase.SepJson = purchaseModel.SepPurchase.ToString();
            await _memberPurchaseService.Update(purchase);
            if (model.Status != 2)
            {
                return Error("", "پرداخت انجام نشده است.");
            }
            purchase.PaymentStep = PaymentSteps.InvokePaymentVerification;
            await _memberPurchaseService.Update(purchase);
            AppSettings appSettings = await _appSettingsService.GetAppSettings();
            SepVerifyResult sepVerify;
            if (!string.IsNullOrEmpty(appSettings.BankGatewayRouterBaseUrl))
            {
                BankRouterService bankRouterService = new BankRouterService(appSettings.BankGatewayRouterBaseUrl);
                sepVerify = await bankRouterService.Verify(new SepVerify
                {
                    RefNum = model.RefNum,
                    TerminalNumber = long.Parse(model.TerminalId)
                });
            }
            else
            {
                sepVerify = await _sepService.Verify(new SepVerify
                {
                    RefNum = model.RefNum,
                    TerminalNumber = long.Parse(model.TerminalId)
                });
            }
            purchaseModel.SepPurchase.VerifyResultCode = sepVerify.ResultCode;
            purchaseModel.SepPurchase.VerifyResultDescription = sepVerify.ResultDescription;
            purchaseModel.SepPurchase.VerifyOrginalAmount = sepVerify.TransactionDetail.OrginalAmount;
            purchase.SepJson = JsonSerializer.Serialize(purchaseModel.SepPurchase);
            purchase.PaymentStep = PaymentSteps.ResponsedInvokePaymentVerification;
            await _memberPurchaseService.Update(purchase);
            if (sepVerify.ResultCode != 0 || (decimal)sepVerify.TransactionDetail.OrginalAmount != purchase.Amount * 10m)
            {
                return Error("", "شماره سفارش: " + purchase.Id);
            }
            purchase.IsPaymentSuccess = true;
            await _memberPurchaseService.Update(purchase);
            try
            {
                await _memberPurchaseService.ApplyPurchase(purchase.Id);
            }
            catch (Exception ex)
            {
                return Error("", ex.Message + "<br/>شماره پیگیری: " + purchaseModel.SepPurchase.TraceNo);
            }
            return Success(purchaseModel.SepPurchase.TraceNo, "عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]")]
    public async Task<IActionResult> SepehrCallBack(SepehrPurchase model)
    {
        string baseRedirectUrl = _appConfig.Value.BaseUrl + "/gateway/sepehrMemberCallback/";
        MemberPurchase purchase = await _memberPurchaseService.GetByCode(int.Parse(model.InvoiceId));
        if (purchase == null)
        {
            return RedirectPermanent(baseRedirectUrl + "?msg=پرداخت با خطا مواجه شده است. ثبت سفارشی با اطلاعات دریافت شده در سیستم یافت نشد.");
        }
        if (purchase.IsPaymentSuccess)
        {
            return RedirectPermanent(baseRedirectUrl + "?msg=این سفارش قبلا پردازش شده است.");
        }
        if (!_currentUserService.CustomerId.HasValue)
        {
            _currentUserService.CustomerId = purchase.CustomerId;
        }
        MemberPurchaseModel purchaseModel = new MemberPurchaseModel(purchase)
        {
            SepehrPurchase =
            {
                RespCode = model.RespCode,
                Respmsg = model.Respmsg,
                Amount = model.Amount,
                Hash = model.Hash,
                Rrn = model.Rrn,
                InvoiceId = model.InvoiceId
            }
        };
        purchase.SepehrJson = purchaseModel.SepehrPurchase.ToString();
        await _memberPurchaseService.Update(purchase);
        if (model.RespCode != 0)
        {
            return RedirectPermanent(baseRedirectUrl + "?msg=پرداخت انجام نشده است." + model.Respmsg);
        }
        if ((decimal)model.Amount != purchase.Amount * 10m)
        {
            return RedirectPermanent(baseRedirectUrl + "?msg=مبلغ پرداخت شده با مبلغ قابل پرداخت مغایرت دارد.");
        }
        if (!string.IsNullOrEmpty(model.Digitalreceipt) && await _memberPurchaseService.GetBySepehrDigitalReceiptNum(model.Digitalreceipt) != null)
        {
            return RedirectPermanent(baseRedirectUrl + "?msg=رسید دیجیتال تکراری است.");
        }
        purchase.PaymentStep = PaymentSteps.InvokePaymentVerification;
        await _memberPurchaseService.Update(purchase);
        SepehrVerifyResult sepehrVerify = await _sepehrService.Verify(new SepehrVerify
        {
            DigitalReceipt = model.Digitalreceipt,
            Tid = model.TerminalId
        });
        purchaseModel.SepehrPurchase.VerifyReturnId = sepehrVerify.ReturnId;
        purchaseModel.SepehrPurchase.VerifyMessage = sepehrVerify.Message;
        purchaseModel.SepehrPurchase.VerifyStatus = sepehrVerify.Status;
        purchase.SepehrJson = purchaseModel.SepehrPurchase.ToString();
        purchase.PaymentStep = PaymentSteps.ResponsedInvokePaymentVerification;
        await _memberPurchaseService.Update(purchase);
        if (sepehrVerify.Status.ToLower() != "ok" || !decimal.TryParse(sepehrVerify.ReturnId, out var result) || purchase.Amount * 10m != result)
        {
            return RedirectPermanent(baseRedirectUrl + "?msg=" + $"خطا در پرداخت: {sepehrVerify.Message}, شماره سفارش: {purchase.Code}");
        }
        purchase.IsPaymentSuccess = true;
        await _memberPurchaseService.Update(purchase);
        try
        {
            await _memberPurchaseService.ApplyPurchase(purchase.Id);
        }
        catch (Exception ex)
        {
            return RedirectPermanent(baseRedirectUrl + "?msg=" + ex.Message + "<br/>شماره پیگیری: " + purchaseModel.SepehrPurchase.Tracenumber + "<br/>شماره سفارش: " + purchase.Code);
        }
        return RedirectPermanent(baseRedirectUrl + "?msg=با تشکر، پرداخت شما با موفقیت انجام و ثبت شد.<br/>شماره سفارش: " + purchase.Code + "&success=true");
    }

    [HttpPost("[action]")]
    public async Task<IActionResult> IPGCallBack(IPGParsianPurchase model)
    {
        string baseRedirectUrl = _appConfig.Value.BaseUrl + "/gateway/ipgMemberCallback/";
        MemberPurchase purchase = await _memberPurchaseService.GetByCode(model.OrderId);
        if (purchase == null)
        {
            return RedirectPermanent(baseRedirectUrl + "?msg=پرداخت با خطا مواجه شده است. ثبت سفارشی با اطلاعات دریافت شده در سیستم یافت نشد.");
        }
        if (purchase.IsPaymentSuccess)
        {
            return RedirectPermanent(baseRedirectUrl + "?msg=این سفارش قبلا پردازش شده است.");
        }
        if (!_currentUserService.CustomerId.HasValue)
        {
            _currentUserService.CustomerId = purchase.CustomerId;
        }
        MemberPurchaseModel purchaseModel = new MemberPurchaseModel(purchase)
        {
            IPGParsianPurchase =
            {
                Status = model.Status,
                TerminalNo = model.TerminalNo,
                Amount = model.Amount,
                HashCardNumber = model.HashCardNumber,
                RRN = model.RRN
            }
        };
        purchase.IPGParsianJson = purchaseModel.IPGParsianPurchase.ToString();
        await _memberPurchaseService.Update(purchase);
        if (model.Status != 0 || model.RRN <= 0)
        {
            return RedirectPermanent(baseRedirectUrl + "?msg=پرداخت انجام نشده است.");
        }
        decimal.TryParse(model.Amount, NumberStyles.Any, CultureInfo.CurrentCulture, out var result);
        if (result != purchase.Amount * 10m)
        {
            return RedirectPermanent(baseRedirectUrl + "?msg=مبلغ پرداخت شده با مبلغ قابل پرداخت مغایرت دارد.");
        }
        purchase.PaymentStep = PaymentSteps.InvokePaymentVerification;
        await _memberPurchaseService.Update(purchase);
        if (await _memberPurchaseService.GetByIPGRRN(model.RRN) != null)
        {
            return RedirectPermanent(baseRedirectUrl + "?msg=این رسید دیجیتال قبلا پردازش شده است.");
        }
        IPGParsianVerifyResult ipgVerify = await _parsianService.Verify(new IPGParsianVerify
        {
            LoginAccount = purchaseModel.IPGParsianPurchase.LoginAccount,
            Token = purchaseModel.IPGParsianPurchase.Token
        });
        purchaseModel.IPGParsianPurchase.VerifyCardNumberMasked = ipgVerify.CardNumberMasked;
        purchaseModel.IPGParsianPurchase.VerifyStatus = ipgVerify.Status;
        purchaseModel.IPGParsianPurchase.VerifyRRN = ipgVerify.RRN;
        purchaseModel.IPGParsianPurchase.VerifyToken = ipgVerify.Token;
        purchase.IPGParsianJson = purchaseModel.IPGParsianPurchase.ToString();
        purchase.PaymentStep = PaymentSteps.ResponsedInvokePaymentVerification;
        await _memberPurchaseService.Update(purchase);
        if (ipgVerify.Status != 0)
        {
            return RedirectPermanent(baseRedirectUrl + "?msg=شماره سفارش: " + purchase.Code);
        }
        purchase.IsPaymentSuccess = true;
        await _memberPurchaseService.Update(purchase);
        try
        {
            await _memberPurchaseService.ApplyPurchase(purchase.Id);
        }
        catch (Exception ex)
        {
            return RedirectPermanent(baseRedirectUrl + "?msg=" + ex.Message + "<br/>شماره پیگیری: " + purchaseModel.IPGParsianPurchase.RRN + "<br/>شماره سفارش: " + purchase.Code);
        }
        return RedirectPermanent(baseRedirectUrl + "?msg=با تشکر، پرداخت شما با موفقیت انجام و ثبت شد.<br/>شماره سفارش: " + purchase.Code + "&success=true");
    }

    [HttpPost("[action]")]
    public async Task<IActionResult> SadadCallBack(SadadPurchase model)
    {
        string baseRedirectUrl = _appConfig.Value.BaseUrl + "/gateway/SadadMemberCallback/";
        MemberPurchase purchase = await _memberPurchaseService.GetByCode(model.OrderId);
        if (purchase == null)
        {
            return RedirectPermanent(baseRedirectUrl + "?msg=پرداخت با خطا مواجه شده است. ثبت سفارشی با اطلاعات دریافت شده در سیستم یافت نشد.");
        }
        if (purchase.IsPaymentSuccess)
        {
            return RedirectPermanent(baseRedirectUrl + "?msg=این سفارش قبلا پردازش شده است.");
        }
        if (!_currentUserService.CustomerId.HasValue)
        {
            _currentUserService.CustomerId = purchase.CustomerId;
        }
        MemberPurchaseModel purchaseModel = new MemberPurchaseModel(purchase);
        if (purchaseModel.SadadPurchase.Token != model.Token)
        {
            return RedirectPermanent(baseRedirectUrl + "?msg=اطلاعات توکن مطابقت ندارد.");
        }
        purchaseModel.SadadPurchase.ResCode = model.ResCode;
        purchase.SadadJson = purchaseModel.SadadPurchase.ToString();
        await _memberPurchaseService.Update(purchase);
        if (model.ResCode != 0)
        {
            return RedirectPermanent(baseRedirectUrl + "?msg=پرداخت انجام نشده است.");
        }
        purchase.PaymentStep = PaymentSteps.InvokePaymentVerification;
        await _memberPurchaseService.Update(purchase);
        AppSettings appSettings = await _appSettingsService.GetAppSettings();
        SadadVerifyResult sadadVerify;
        if (!string.IsNullOrEmpty(appSettings.BankGatewayRouterBaseUrl))
        {
            BankRouterService bankRouterService = new BankRouterService(appSettings.BankGatewayRouterBaseUrl);
            sadadVerify = await bankRouterService.SadadVerify(new SadadVerifyRequest
            {
                Token = purchaseModel.SadadPurchase.Token,
                MerchantKey = appSettings.SadadMerchantKey
            });
        }
        else
        {
            sadadVerify = await _sadadService.Verify(new SadadVerifyRequest
            {
                Token = purchaseModel.SadadPurchase.Token,
                MerchantKey = appSettings.SadadMerchantKey
            });
        }
        if (sadadVerify.Amount != purchase.Amount * 10m)
        {
            return RedirectPermanent(baseRedirectUrl + "?msg=مبلغ واریز شده با مبلغ سفارش مطابقت ندارد.");
        }
        if (await _memberPurchaseService.GetBySadadRetrivalRefNo(sadadVerify.RetrivalRefNo) != null)
        {
            return RedirectPermanent(baseRedirectUrl + "?msg=این رسید دیجیتال قبلا پردازش شده است.");
        }
        purchaseModel.SadadPurchase.RetrivalRefNo = sadadVerify.RetrivalRefNo;
        purchaseModel.SadadPurchase.ResCode = sadadVerify.ResCode;
        purchaseModel.SadadPurchase.CardHolderFullName = sadadVerify.CardHolderFullName;
        purchaseModel.SadadPurchase.Description = sadadVerify.Description;
        purchaseModel.SadadPurchase.SystemTraceNo = sadadVerify.SystemTraceNo;
        purchase.SadadJson = purchaseModel.SadadPurchase.ToString();
        purchase.PaymentStep = PaymentSteps.ResponsedInvokePaymentVerification;
        await _memberPurchaseService.Update(purchase);
        if (sadadVerify.ResCode != 0)
        {
            return RedirectPermanent(baseRedirectUrl + "?msg=خطا در پرداخت. شماره سفارش: " + purchase.Code + "<br/>" + sadadVerify.Description);
        }
        purchase.IsPaymentSuccess = true;
        await _memberPurchaseService.Update(purchase);
        try
        {
            await _memberPurchaseService.ApplyPurchase(purchase.Id);
        }
        catch (Exception ex)
        {
            return RedirectPermanent(baseRedirectUrl + "?msg=" + ex.Message + "<br/>شماره پیگیری: " + purchaseModel.SadadPurchase.SystemTraceNo + "<br/>شماره سفارش: " + purchase.Code);
        }
        return RedirectPermanent(baseRedirectUrl + "?msg=با تشکر، پرداخت شما با موفقیت انجام و ثبت شد.<br/>شماره سفارش: " + purchase.Code + "<br/>شماره پیگیری: " + sadadVerify.SystemTraceNo + "&success=true");
    }
}
