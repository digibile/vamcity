using System;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Core;
using LoanSystem.Core.Configuration;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Data.Enums;
using LoanSystemAvizhe.Services.Inquiry.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LoanSystem.Site.Controllers;

[ApiController]
[Route("[controller]")]
public class InquiryController : BaseDefaultController
{
    private readonly ICustomerService _customerService;

    private readonly IAppSettingsService _appSettingsService;

    private readonly IntegratedInquiryService _integratedInquiryService;

    private readonly ICurrentUserService _currentUserService;

    private readonly IAppInquiryService _appInquiryService;

    private readonly AppConfig _appConfig;

    public InquiryController(ILogger<InquiryController> logger, ICustomerService customerService, IAppSettingsService appSettingsService, IOptions<AppConfig> options, IntegratedInquiryService integratedInquiryService, ICurrentUserService currentUserService, IAppInquiryService appInquiryService)
        : base(logger)
    {
        _customerService = customerService;
        _appConfig = options.Value;
        _appSettingsService = appSettingsService;
        _integratedInquiryService = integratedInquiryService;
        _currentUserService = currentUserService;
        _appInquiryService = appInquiryService;
    }

    [HttpPost("[action]")]
    public Task<ResponseState<AppInquiryLightModel>> Shahkar(string mobile, string nationalCode, int customerCode)
    {
        return TryCatch(async () =>
        {
            string text = await SetCustomer(customerCode);
            if (!string.IsNullOrWhiteSpace(text))
            {
                return Error<AppInquiryLightModel>(null, text);
            }
            AppInquiry appInquiry = await _appInquiryService.GetInquiryByKey(mobile + "," + nationalCode, AppInquiryType.شاهکار);
            if (appInquiry == null)
            {
                appInquiry = await _integratedInquiryService.Shahkar(new ShahkarModel
                {
                    Member = new MemberModel
                    {
                        Mobile = mobile,
                        NationalCode = nationalCode
                    }
                }, AppInquiryType.شاهکار);
            }
            return Success(new AppInquiryLightModel(appInquiry), "عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<AppInquiryLightModel>> PersonInfo(string nationalCode, DateTime birthDate, int customerCode)
    {
        return TryCatch(async () =>
        {
            string text = await SetCustomer(customerCode);
            if (!string.IsNullOrWhiteSpace(text))
            {
                return Error<AppInquiryLightModel>(null, text);
            }
            AppInquiry appInquiry = await _appInquiryService.GetInquiryByKey(nationalCode + "," + birthDate.ToPersianDate(), AppInquiryType.مشخصات_هویتی);
            if (appInquiry == null)
            {
                appInquiry = await _integratedInquiryService.PersonInfo(new PersonInfoModel
                {
                    Member = new MemberModel
                    {
                        BirthDate = birthDate,
                        NationalCode = nationalCode
                    }
                }, AppInquiryType.مشخصات_هویتی);
            }
            return Success(new AppInquiryLightModel(appInquiry), "عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<AppInquiryLightModel>> PostalAddress(string postalCode, int customerCode)
    {
        return TryCatch(async () =>
        {
            string text = await SetCustomer(customerCode);
            if (!string.IsNullOrWhiteSpace(text))
            {
                return Error<AppInquiryLightModel>(null, text);
            }
            AppInquiry appInquiry = await _appInquiryService.GetInquiryByKey(postalCode ?? "", AppInquiryType.کدپستی);
            if (appInquiry == null)
            {
                appInquiry = await _integratedInquiryService.PostalAddress(new PostalAddressModel
                {
                    Member = new MemberModel
                    {
                        PostalCode = postalCode
                    }
                }, AppInquiryType.کدپستی);
            }
            return Success(new AppInquiryLightModel(appInquiry), "عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<AppInquiryLightModel>> BankCardInfo(string cardNumber, int customerCode)
    {
        return TryCatch(async () =>
        {
            string text = await SetCustomer(customerCode);
            if (!string.IsNullOrWhiteSpace(text))
            {
                return Error<AppInquiryLightModel>(null, text);
            }
            AppInquiry appInquiry = await _appInquiryService.GetInquiryByKey(cardNumber ?? "", AppInquiryType.مشخصات_کارت_بانکی);
            if (appInquiry == null)
            {
                appInquiry = await _integratedInquiryService.BankCardInfo(new BankCardInfoModel
                {
                    MemberId = null,
                    CardNumber = cardNumber
                }, AppInquiryType.مشخصات_کارت_بانکی);
            }
            return Success(new AppInquiryLightModel(appInquiry), "عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<AppInquiryLightModel>> IbanMatch(string iban, string nationalCode, DateTime birthDate, int customerCode)
    {
        return TryCatch(async () =>
        {
            string text = await SetCustomer(customerCode);
            if (!string.IsNullOrWhiteSpace(text))
            {
                return Error<AppInquiryLightModel>(null, text);
            }
            AppInquiry appInquiry = await _appInquiryService.GetInquiryByKey(iban + "," + nationalCode, AppInquiryType.مطابقت_کدملی_شبا);
            if (appInquiry == null)
            {
                appInquiry = await _integratedInquiryService.IbanMatch(new IbanMatchModel
                {
                    Member = new MemberModel
                    {
                        NationalCode = nationalCode,
                        BirthDate = birthDate
                    },
                    Iban = iban
                }, AppInquiryType.مطابقت_کدملی_شبا);
            }
            return Success(new AppInquiryLightModel(appInquiry), "عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<AppInquiryLightModel>> IbanInfo(string iban, int customerCode)
    {
        return TryCatch(async () =>
        {
            string text = await SetCustomer(customerCode);
            if (!string.IsNullOrWhiteSpace(text))
            {
                return Error<AppInquiryLightModel>(null, text);
            }
            AppInquiry appInquiry = await _appInquiryService.GetInquiryByKey(iban ?? "", AppInquiryType.مشخصات_شبا);
            if (appInquiry == null)
            {
                appInquiry = await _integratedInquiryService.IbanInfo(new IbanInfoModel
                {
                    MemberId = null,
                    Iban = iban
                }, AppInquiryType.مشخصات_شبا);
            }
            return Success(new AppInquiryLightModel(appInquiry), "عملیات با موفقیت انجام شد.");
        });
    }

    private async Task<string> SetCustomer(int customerCode)
    {
        Customer customer;
        if (_appConfig.IsCloud)
        {
            if (customerCode == 0)
            {
                return "پارامترهای مشتری معتبر نمی باشد.";
            }
            customer = await _customerService.GetByCode(customerCode);
            if (customer == null)
            {
                return "پارامترهای مشتری معتبر نمی باشد.";
            }
        }
        else
        {
            customer = (await _customerService.GetAllCustomers()).FirstOrDefault();
        }
        if (customer == null)
        {
            return "پارامترهای مشتری معتبر نمی باشد.";
        }
        _currentUserService.CustomerId = customer.Id;
        return "";
    }
}
