using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using LoanSystem.Core.Configuration;
using LoanSystem.Core.Exceptions;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Services.Users;
using LoanSystem.Site.Helpers.Security;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Data.Enums;
using LoanSystemAvizhe.Services.Enums;
using LoanSystemAvizhe.Services.FileHelper;
using LoanSystemAvizhe.Services.Inquiry.Models;
using LoanSystemAvizhe.Site.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace LoanSystem.Site.Controllers;

[ApiController]
[Route("[controller]")]
public class AccountController : BaseDefaultController
{
    private readonly UserManager _userManager;

    private readonly RoleManager<Role> _roleManager;

    private readonly ICustomerService _customerService;

    private readonly IAppSettingsService _appSettingsService;

    private readonly IntegratedSmsSenderService _integratedSmsSenderService;

    private readonly IntegratedEmailSenderService _integratedEmailSenderService;

    private readonly IntegratedInquiryService _integratedInquiryService;

    private readonly ISmsPatternService _smsPatternService;

    private readonly IFileHelper _fileHelper;

    private readonly ILoanRequestStatusService _loanRequestStatusService;

    private readonly ICustomerPurchasePlanService _customerPurchasePlanService;

    private readonly ICustomerMarketerService _customerMarketerService;

    private readonly ICurrentUserService _currentUserService;

    private readonly IThirdPartyTransferService _thirdPartyTransferService;

    private readonly IAppSmsService _appSmsService;

    private readonly IAppInquiryService _appInquiryService;

    private readonly IUserLoanRequestAuthorizationItemService _userLoanRequestAuthorizationItemService;

    private readonly IThirdPersonAssuranceAuthorizationItemService _thirdPersonAssuranceAuthorizationItemService;

    private readonly AppConfig _appConfig;

    public AccountController(ILogger<AccountController> logger, UserManager userManager, RoleManager<Role> roleManager, ICustomerService customerService, IAppSettingsService appSettingsService, IntegratedSmsSenderService integratedSmsSenderService, IntegratedEmailSenderService integratedEmailSenderService, IntegratedInquiryService integratedInquiryService, ISmsPatternService smsPatternService, ICustomerPurchasePlanService customerPurchasePlanService, ICustomerMarketerService customerMarketerService, ILoanRequestStatusService loanRequestStatusService, ICurrentUserService currentUserService, IOptions<AppConfig> options, IThirdPartyTransferService thirdPartyTransferService, IFileHelper fileHelper, IAppSmsService appSmsService, IAppInquiryService appInquiryService, IUserLoanRequestAuthorizationItemService userLoanRequestAuthorizationItemService, IThirdPersonAssuranceAuthorizationItemService thirdPersonAssuranceAuthorizationItemService)
        : base(logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _customerService = customerService;
        _integratedSmsSenderService = integratedSmsSenderService;
        _integratedEmailSenderService = integratedEmailSenderService;
        _integratedInquiryService = integratedInquiryService;
        _smsPatternService = smsPatternService;
        _customerPurchasePlanService = customerPurchasePlanService;
        _customerMarketerService = customerMarketerService;
        _appConfig = options.Value;
        _loanRequestStatusService = loanRequestStatusService;
        _currentUserService = currentUserService;
        _appSettingsService = appSettingsService;
        _thirdPartyTransferService = thirdPartyTransferService;
        _fileHelper = fileHelper;
        _appSmsService = appSmsService;
        _appInquiryService = appInquiryService;
        _userLoanRequestAuthorizationItemService = userLoanRequestAuthorizationItemService;
        _thirdPersonAssuranceAuthorizationItemService = thirdPersonAssuranceAuthorizationItemService;
    }

    [HttpPost("[action]")]
    public Task<ResponseState<string>> Register([FromForm] RegisterModel model)
    {
        return TryCatch(async () =>
        {
            if (!CheckMobileFormatValidity(model.Mobile))
            {
                return Error("", "شماره موبایل وارد شده معتبر نمی باشد.");
            }
            Customer c = await _customerService.GetByMobile(model.Mobile);
            if (c != null)
            {
                if (c.FirstLoginDone)
                {
                    return Error("", "با این شماره موبایل قبلا ثبت نام انجام شده است");
                }
                if (model.SendSms)
                {
                    await _integratedSmsSenderService.SendRegisterationCode(model.Mobile, c.InitPassword);
                }
                return Success(c.InitPassword, "");
            }
            if (await _userManager.FindByUserName(model.Mobile) != null)
            {
                return Error("", "با این شماره موبایل قبلا کاربر در سیستم شده است");
            }
            Guid customerId = Guid.NewGuid();
            CustomerPurchasePlan customerPurchasePlan = new CustomerPurchasePlan
            {
                CustomerId = customerId,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(10.0),
                MaxLoanCount = 10,
                RemainLoanCount = 10
            };
            Customer customer = new Customer
            {
                Id = customerId,
                Email = "",
                Mobile = model.Mobile,
                Name = model.Name,
                RegDate = DateTime.Now,
                InitPassword = new Random().Next(10000, 99999).ToString(),
                AffCode = model.AffCode,
                ExpireDate = customerPurchasePlan.EndDate
            };
            await _customerService.Insert(customer);
            await _customerPurchasePlanService.Insert(customerPurchasePlan);
            customer.ExpireDate = customerPurchasePlan.EndDate;
            SmsPattern entity = new SmsPattern
            {
                CustomerId = customer.Id,
                IsSystem = true,
                Pattern = "مشتری گرامی {نام مشتری}؛ سررسید قسط به مبلغ {مبلغ قسط} فرا رسیده است، لطفا نسبت به پرداخت به موقع اقدام فرمایید.\r\n {نام فروشگاه}",
                Title = "پیامک یادآور سررسید",
                Status = SmsPatternStatus.Approved,
                Category = SmsPatternCategory.قسط,
                IsPatternBase = false,
                SmsPatternPanel = SmsPanel.MelliPayamak,
                PatternBaseCode = "356784"
            };
            SmsPattern overdueSms = new SmsPattern
            {
                CustomerId = customer.Id,
                IsSystem = true,
                Pattern = "مشتری گرامی {نام مشتری}؛ سررسید قسط به مبلغ {مبلغ مانده قسط} معوق شده است، لطفا در اسرع وقت نسبت به پرداخت اقدام فرمایید.\r\n {نام فروشگاه}",
                Title = "پیامک یادآور قسط معوق",
                Status = SmsPatternStatus.Approved,
                Category = SmsPatternCategory.قسط,
                IsPatternBase = false,
                SmsPatternPanel = SmsPanel.MelliPayamak,
                PatternBaseCode = "356783"
            };
            SmsPattern thanksSms = new SmsPattern
            {
                CustomerId = customer.Id,
                IsSystem = true,
                Pattern = "قسط به مبلغ {مبلغ آخرین واریز} دریافت شد. مانده وام {مانده وام}.\r\nبا تشکر {نام فروشگاه}",
                Title = "تشکر از پرداخت قسط",
                Status = SmsPatternStatus.Approved,
                Category = SmsPatternCategory.قسط,
                IsPatternBase = false,
                SmsPatternPanel = SmsPanel.MelliPayamak,
                PatternBaseCode = "356782"
            };
            SmsPattern beforeDueSms = new SmsPattern
            {
                CustomerId = customer.Id,
                IsSystem = true,
                Pattern = "درود {نام مشتری} گرامی؛ سررسید قسط شماره {شماره قسط} به مبلغ {مبلغ قسط} طی 2 روز آینده فرا خواهد رسید، لطفا نسبت به پرداخت به موقع اقدام فرمایید.",
                Title = "یادآوری پیش از موعد",
                Status = SmsPatternStatus.Approved,
                Category = SmsPatternCategory.قسط,
                IsPatternBase = false,
                SmsPatternPanel = SmsPanel.MelliPayamak,
                PatternBaseCode = "356549"
            };
            await _smsPatternService.Insert(entity, customer.Id);
            await _smsPatternService.Insert(overdueSms, customer.Id);
            await _smsPatternService.Insert(thanksSms, customer.Id);
            await _smsPatternService.Insert(beforeDueSms, customer.Id);
            List<LoanRequestStatus> entities = new List<LoanRequestStatus>
            {
                new LoanRequestStatus
                {
                    Title = "ایجاد اولیه",
                    StepCategory = LoanRequestStatusCategorys.قبل_از_تکمیل_مدارک,
                    Changable = true,
                    Order = 0,
                    CustomerId = customerId,
                    SmsPatternId = null,
                    SystemRequestStatus = SystemRequestStatus.ایجاد_اولیه
                },
                new LoanRequestStatus
                {
                    Title = "تکمیل مدارک",
                    StepCategory = LoanRequestStatusCategorys.تکمیل_مدارک,
                    Changable = true,
                    Order = 2,
                    CustomerId = customerId,
                    SmsPatternId = null,
                    SystemRequestStatus = SystemRequestStatus.تکمیل_مدارک
                },
                new LoanRequestStatus
                {
                    Title = "مدارک ناقص تکمیل مدارک",
                    StepCategory = LoanRequestStatusCategorys.تکمیل_مدارک,
                    Changable = true,
                    Order = 4,
                    CustomerId = customerId,
                    SmsPatternId = null,
                    SystemRequestStatus = SystemRequestStatus.مدارک_ناقص_تکمیل_مدارک
                },
                new LoanRequestStatus
                {
                    Title = "در انتظار بررسی کارشناس",
                    StepCategory = LoanRequestStatusCategorys.بعد_از_تکمیل_مدارک,
                    Changable = false,
                    Order = 6,
                    CustomerId = customerId,
                    SmsPatternId = null,
                    SystemRequestStatus = SystemRequestStatus.در_انتظار_بررسی_کارشناس
                },
                new LoanRequestStatus
                {
                    Title = "نیاز به پیگیری",
                    StepCategory = LoanRequestStatusCategorys.بعد_از_تکمیل_مدارک,
                    Changable = false,
                    Order = 8,
                    CustomerId = customerId,
                    SmsPatternId = null,
                    SystemRequestStatus = SystemRequestStatus.نیاز_به_پیگیری
                },
                new LoanRequestStatus
                {
                    Title = "بررسی مجدد کارشناس",
                    StepCategory = LoanRequestStatusCategorys.تکمیل_مدارک,
                    Changable = false,
                    Order = 9,
                    CustomerId = customer.Id,
                    SmsPatternId = null,
                    SystemRequestStatus = SystemRequestStatus.بررسی_مجدد_کارشناس
                },
                new LoanRequestStatus
                {
                    Title = "رد شده",
                    StepCategory = LoanRequestStatusCategorys.بعد_از_تکمیل_مدارک,
                    Changable = false,
                    Order = 10,
                    CustomerId = customerId,
                    SmsPatternId = null,
                    SystemRequestStatus = SystemRequestStatus.رد_شده,
                    AllowNewRequest = true
                },
                new LoanRequestStatus
                {
                    Title = "اولویت بندی شده",
                    StepCategory = LoanRequestStatusCategorys.بعد_از_تکمیل_مدارک,
                    Changable = false,
                    Order = 12,
                    CustomerId = customerId,
                    SmsPatternId = null,
                    SystemRequestStatus = SystemRequestStatus.اولویت_بندی_شده
                },
                new LoanRequestStatus
                {
                    Title = "در انتظار تسویه با نماینده",
                    StepCategory = LoanRequestStatusCategorys.بعد_از_تکمیل_مدارک,
                    Changable = false,
                    Order = 13,
                    CustomerId = customerId,
                    SmsPatternId = null,
                    SystemRequestStatus = SystemRequestStatus.در_انتظار_تسویه_با_نماینده,
                    AllowNewRequest = true
                },
                new LoanRequestStatus
                {
                    Title = "پرداخت شده",
                    StepCategory = LoanRequestStatusCategorys.بعد_از_تکمیل_مدارک,
                    Changable = false,
                    Order = 14,
                    CustomerId = customerId,
                    SmsPatternId = null,
                    SystemRequestStatus = SystemRequestStatus.پرداخت_شده,
                    AllowNewRequest = true
                },
                new LoanRequestStatus
                {
                    Title = "بتا ثبت شده انتظار بررسی کارشناس",
                    StepCategory = LoanRequestStatusCategorys.بعد_از_تکمیل_مدارک,
                    Changable = false,
                    Order = 16,
                    CustomerId = customer.Id,
                    SmsPatternId = null,
                    SystemRequestStatus = SystemRequestStatus.رفاه_بتا_ثبت_شد_انتظار_بررسی_کارشناس,
                    AllowNewRequest = true
                }
            };
            await _loanRequestStatusService.InsertBatch(entities);
            List<LoanRequestStatus> entities2 = new List<LoanRequestStatus>
            {
                new LoanRequestStatus
                {
                    Title = "بدون وضعیت",
                    StepCategory = LoanRequestStatusCategorys.قبل_از_تکمیل_مدارک,
                    StatusGroup = StatusGroup.نماینده,
                    Changable = true,
                    Order = 0,
                    CustomerId = customer.Id,
                    SmsPatternId = null,
                    SystemAgentRequestStatus = SystemAgentRequestStatus.بدون_وضعیت,
                    AllowNewRequest = true
                },
                new LoanRequestStatus
                {
                    Title = "در انتظار بررسی",
                    StepCategory = LoanRequestStatusCategorys.بعد_از_تکمیل_مدارک,
                    StatusGroup = StatusGroup.نماینده,
                    Changable = true,
                    Order = 2,
                    CustomerId = customer.Id,
                    SmsPatternId = null,
                    SystemAgentRequestStatus = SystemAgentRequestStatus.در_انتظار_بررسی
                },
                new LoanRequestStatus
                {
                    Title = "نقص مدارک",
                    StepCategory = LoanRequestStatusCategorys.تکمیل_مدارک,
                    StatusGroup = StatusGroup.نماینده,
                    Changable = true,
                    Order = 4,
                    CustomerId = customer.Id,
                    SmsPatternId = null,
                    SystemAgentRequestStatus = SystemAgentRequestStatus.نقص_مدارک
                },
                new LoanRequestStatus
                {
                    Title = "ارسال به تامین کننده",
                    StepCategory = LoanRequestStatusCategorys.قبل_از_تکمیل_مدارک,
                    StatusGroup = StatusGroup.نماینده,
                    Changable = true,
                    Order = 6,
                    CustomerId = customer.Id,
                    SmsPatternId = null,
                    SystemAgentRequestStatus = SystemAgentRequestStatus.ارسال_به_تامین_کننده
                },
                new LoanRequestStatus
                {
                    Title = "اعطا شده",
                    StepCategory = LoanRequestStatusCategorys.بعد_از_تکمیل_مدارک,
                    StatusGroup = StatusGroup.نماینده,
                    Changable = true,
                    Order = 8,
                    CustomerId = customer.Id,
                    SmsPatternId = null,
                    SystemAgentRequestStatus = SystemAgentRequestStatus.اعطاـشده,
                    AllowNewRequest = true
                },
                new LoanRequestStatus
                {
                    Title = "در انتظار تسویه",
                    StepCategory = LoanRequestStatusCategorys.بعد_از_تکمیل_مدارک,
                    StatusGroup = StatusGroup.نماینده,
                    Changable = true,
                    Order = 10,
                    CustomerId = customer.Id,
                    SmsPatternId = null,
                    SystemAgentRequestStatus = SystemAgentRequestStatus.در_انتظار_تسویه,
                    AllowNewRequest = true
                },
                new LoanRequestStatus
                {
                    Title = "تسویه شده",
                    StepCategory = LoanRequestStatusCategorys.قبل_از_تکمیل_مدارک,
                    StatusGroup = StatusGroup.نماینده,
                    Changable = true,
                    Order = 12,
                    CustomerId = customer.Id,
                    SmsPatternId = null,
                    SystemAgentRequestStatus = SystemAgentRequestStatus.تسویه_شده,
                    AllowNewRequest = true
                },
                new LoanRequestStatus
                {
                    Title = "رد شده",
                    StepCategory = LoanRequestStatusCategorys.بعد_از_تکمیل_مدارک,
                    StatusGroup = StatusGroup.نماینده,
                    Changable = true,
                    Order = 14,
                    CustomerId = customer.Id,
                    SmsPatternId = null,
                    SystemAgentRequestStatus = SystemAgentRequestStatus.رد_شده,
                    AllowNewRequest = true
                }
            };
            await _loanRequestStatusService.InsertBatch(entities2);
            await _appSettingsService.Insert(new AppSettings
            {
                CustomerId = customerId
            });
            User user = new User
            {
                FirstName = "مدیر",
                LastName = "صندوق",
                Email = model.Mobile + "@mail.com",
                UserName = model.Mobile,
                IsActive = true,
                CustomerId = customer.Id,
                RegDate2 = DateTime.Now
            };
            user.UserRoleNames = new List<string> { "Admin" };
            PasswordHasher<User> passwordHasher = new PasswordHasher<User>();
            string passwordHash = passwordHasher.HashPassword(user, customer.InitPassword);
            user.PasswordHash = passwordHash;
            List<Claim> list = new List<Claim>();
            foreach (object value in Enum.GetValues(typeof(UserPermissionClaims)))
            {
                list.Add(new Claim("Permission", value.ToString()));
            }
            IdentityResult identityResult = await _userManager.Insert(user, customer.InitPassword, user.UserRoleNames, list);
            if (identityResult.Errors.Any())
            {
                return Error("", string.Join(',', identityResult.Errors.Select((IdentityError x) => x.Description).ToArray()));
            }
            if (model.SendSms)
            {
                await _integratedSmsSenderService.SendRegisterationCode(model.Mobile, customer.InitPassword);
            }
            return Success(customer.InitPassword, "عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<dynamic>> PreRegisterAndSendOTP(MemberRegisterDto model)
    {
        return TryCatch(async () =>
        {
            if (!CheckMobileFormatValidity(model.Mobile))
            {
                return Error((object)0, "شماره موبایل وارد شده معتبر نمی باشد.");
            }
            string userName = model.NationalCode;
            Customer customer;
            if (_appConfig.IsCloud)
            {
                if (model.CustomerCode == 0)
                {
                    return Error((object)0, "پارامترهای مشتری معتبر نمی باشد.");
                }
                customer = await _customerService.GetByCode(model.CustomerCode);
                if (customer == null)
                {
                    return Error((object)0, "پارامترهای مشتری معتبر نمی باشد.");
                }
                userName = $"{customer.Code}-{model.NationalCode}";
            }
            else
            {
                customer = (await _customerService.GetAllCustomers()).FirstOrDefault();
            }
            if (customer == null)
            {
                return Error((object)0, "پارامترهای مشتری معتبر نمی باشد.");
            }
            _currentUserService.CustomerId = customer.Id;
            AppSettings appSettings = await _appSettingsService.GetAppSettings();
            if (appSettings.IsMemberRegistrationDisabled)
            {
                return Error((object)0, "در حال حاضر امکان ثبت نام کاربر غیر فعال است.");
            }
            User user = await _userManager.FindByUserName(userName);
            if (user != null && user.RegistrationCompleted)
            {
                return Error((object)0, "با این نام کاربری قبلا ثبت نام انجام شده است");
            }
            if ((appSettings?.VerifyMobileByNationalCodeInRegister ?? false) && (await _integratedInquiryService.Shahkar(new ShahkarModel
            {
                Member = new MemberModel
                {
                    NationalCode = model.NationalCode,
                    Mobile = model.Mobile
                }
            }, AppInquiryType.شاهکار)).Result != "تطابق دارد")
            {
                return Error((object)0, "مالکیت شماره موبایل با کد ملی مطابقت ندارد.");
            }
            if (user == null)
            {
                user = new User();
            }
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Mobile = model.Mobile;
            user.NationalCode = model.NationalCode;
            user.IsActive = true;
            user.UserName = userName;
            user.CustomerId = customer.Id;
            user.RegDate2 = DateTime.Now;
            user.Otp = new Random().Next(10000, 99999);
            user.UserRoleNames = new List<string> { "Member" };
            PasswordHasher<User> hasher = new PasswordHasher<User>();
            string passwordHash = hasher.HashPassword(user, model.Password);
            user.PasswordHash = passwordHash;
            if (!string.IsNullOrEmpty(model.MrkCode))
            {
                User user2 = await _customerMarketerService.GetByAffliateCode(model.MrkCode, customer.Id);
                if (user2 != null)
                {
                    user.CustomerMarketerId = user2.Id;
                }
            }
            if (!string.IsNullOrEmpty(model.AgnCode) && long.TryParse(model.AgnCode, out var agentId))
            {
                User user3 = await _userManager.GetById(agentId, ignoreQueryFilter: true);
                if (user3 != null && user3.IsActive)
                {
                    user.AgentId = agentId;
                }
            }
            if (user.Id == 0L)
            {
                IdentityResult identityResult = await _userManager.Insert(user, user.PasswordHash, user.UserRoleNames);
                if (identityResult.Errors.Any())
                {
                    return Error<object>(null, string.Join(',', identityResult.Errors.Select((IdentityError x) => x.Description).ToArray()));
                }
                user.PasswordHash = hasher.HashPassword(user, model.Password);
                await _userManager.UpdateAsync(user);
            }
            else
            {
                IdentityResult identityResult2 = await _userManager.Update(user);
                if (identityResult2.Errors.Any())
                {
                    return Error<object>(null, string.Join(',', identityResult2.Errors.Select((IdentityError x) => x.Description).ToArray()));
                }
            }
            bool isOtpActive = false;
            try
            {
                if (appSettings?.RegisterOtpSmsPattern == null)
                {
                    await _integratedSmsSenderService.TrySendRegister(new LoginRegisterModel
                    {
                        Date = DateTime.Now,
                        Mobile = user.Mobile,
                        User_FullName = user.FullName,
                        User_UserName = user.UserName,
                        User_Id = user.Id
                    }, user.CustomerId.Value);
                    await _integratedEmailSenderService.TrySendRegister(new LoginRegisterModel
                    {
                        Date = DateTime.Now,
                        Email = user.Email,
                        User_FullName = user.FullName,
                        User_UserName = user.UserName,
                        User_Id = user.Id
                    }, user.CustomerId.Value);
                }
                else
                {
                    isOtpActive = true;
                    await _integratedSmsSenderService.SendMemberRegisterationCode(model.Mobile, user.Otp.ToString(), customer.Id);
                }
            }
            catch (CustomException)
            {
                return Error<object>(null, "در حال حاضر امکان ارسال کد تائیدیه وجود ندارد.");
            }
            try
            {
                await _thirdPartyTransferService.RegisterOrUpdateDigibileMember(user.Id.ToString(), model.Password);
            }
            catch
            {
            }
            return Success((object)new
            {
                isOtpActive = isOtpActive,
                userId = user.Id
            }, "عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<PreCheckRegistrationResult>> PreCheckRegistration(string nationalCode, int customerCode)
    {
        return TryCatch(async () =>
        {
            string userName = nationalCode;
            Customer customer;
            if (_appConfig.IsCloud)
            {
                if (customerCode == 0)
                {
                    return Error<PreCheckRegistrationResult>(null, "پارامترهای مشتری معتبر نمی باشد.");
                }
                customer = await _customerService.GetByCode(customerCode);
                if (customer == null)
                {
                    return Error<PreCheckRegistrationResult>(null, "پارامترهای مشتری معتبر نمی باشد.");
                }
                userName = $"{customer.Code}-{nationalCode}";
            }
            else
            {
                customer = (await _customerService.GetAllCustomers()).FirstOrDefault();
            }
            if (customer == null)
            {
                return Error<PreCheckRegistrationResult>(null, "پارامترهای مشتری معتبر نمی باشد.");
            }
            _currentUserService.CustomerId = customer.Id;
            if ((await _appSettingsService.GetAppSettings()).IsMemberRegistrationDisabled)
            {
                return Error<PreCheckRegistrationResult>(null, "در حال حاضر امکان ثبت نام کاربر غیر فعال است.");
            }
            return (await _userManager.FindByUserName(userName) != null) ? Error<PreCheckRegistrationResult>(null, "با این کد ملی قبلا ثبت نام انجام شده است") : Success(new PreCheckRegistrationResult
            {
                Username = userName,
                CustomerId = customer.Id
            }, "عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<dynamic>> StepFormRegister(MemberRegisterDto model)
    {
        return TryCatch(async () =>
        {
            if (!CheckMobileFormatValidity(model.Mobile))
            {
                return Error((object)0, "شماره موبایل وارد شده معتبر نمی باشد.");
            }
            ResponseState<PreCheckRegistrationResult> preCheck = await PreCheckRegistration(model.NationalCode, model.CustomerCode);
            if (!preCheck.Success)
            {
                return Error((object)0, preCheck.Message);
            }
            AppSettings appSettings = await _appSettingsService.GetAppSettings();
            Guid customerId = preCheck.Data.CustomerId;
            User user = new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Mobile = model.Mobile,
                Address = model.Address,
                PostalCode = model.PostalCode,
                City = model.City,
                FatherName = model.FatherName,
                BirthDate = model.BirthDate,
                MobileVerified = appSettings.RegisterOtpSmsPatternId.HasValue,
                NationalCode = model.NationalCode,
                BankAccountsJson = model.BankAccountsJson,
                Familiar1FirstName = model.Familiar1LastName,
                Familiar1LastName = model.Familiar1LastName,
                Familiar1Mobile = model.Familiar1Mobile,
                Familiar2FirstName = model.Familiar2FirstName,
                Familiar2LastName = model.Familiar2LastName,
                Familiar2Mobile = model.Familiar2Mobile,
                IsActive = true,
                UserName = preCheck.Data.Username,
                CustomerId = customerId,
                RegDate2 = DateTime.Now,
                UserRoleNames = new List<string> { "Member" }
            };
            PasswordHasher<User> hasher = new PasswordHasher<User>();
            string password = (string.IsNullOrWhiteSpace(model.Password) ? new Random().Next(100000, 999999).ToString() : model.Password);
            string passwordHash = hasher.HashPassword(user, password);
            user.PasswordHash = passwordHash;
            if (!string.IsNullOrEmpty(model.MrkCode))
            {
                User user2 = await _customerMarketerService.GetByAffliateCode(model.MrkCode, customerId);
                if (user2 != null)
                {
                    user.CustomerMarketerId = user2.Id;
                }
            }
            if (!string.IsNullOrEmpty(model.AgnCode) && long.TryParse(model.AgnCode, out var agentId))
            {
                if (_currentUserService.UserId.HasValue && _currentUserService.UserId != agentId)
                {
                    return Error((object)0, "عدم تطابق شناسه نماینده");
                }
                User user3 = await _userManager.GetById(agentId, ignoreQueryFilter: true);
                if (user3 != null && user3.IsActive)
                {
                    user.AgentId = agentId;
                }
            }
            IdentityResult identityResult = await _userManager.Insert(user, user.PasswordHash, user.UserRoleNames);
            if (identityResult.Errors.Any())
            {
                return Error<object>(null, string.Join(',', identityResult.Errors.Select((IdentityError x) => x.Description).ToArray()));
            }
            user.PasswordHash = hasher.HashPassword(user, password);
            user.RegistrationCompleted = true;
            await _userManager.UpdateAsync(user);
            await _appInquiryService.UpdateMember(model.InquiryIds, user.Id);
            await _integratedSmsSenderService.TrySendRegister(new LoginRegisterModel
            {
                Date = DateTime.Now,
                Mobile = user.Mobile,
                User_FullName = user.FullName,
                User_UserName = user.UserName,
                User_Id = user.Id
            }, user.CustomerId.Value);
            await _integratedEmailSenderService.TrySendRegister(new LoginRegisterModel
            {
                Date = DateTime.Now,
                Email = user.Email,
                User_FullName = user.FullName,
                User_UserName = user.UserName,
                User_Id = user.Id
            }, user.CustomerId.Value);
            try
            {
                await _thirdPartyTransferService.RegisterOrUpdateDigibileMember(user.Id.ToString(), model.Password);
            }
            catch
            {
            }
            return Success((object)new
            {
                username = user.UserName,
                password = password,
                Id = user.Id
            }, "عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]")]
    public Task<DefaultResponseState> VerifySmsCode(VerifySmsCodeDto model)
    {
        return TryCatch(async () =>
        {
            User user = await _userManager.GetById(model.UserId, ignoreQueryFilter: true);
            if (user == null)
            {
                return Error("کاربر یافت نشد.");
            }
            if (user.Otp != model.Code)
            {
                return Error("کد تائیدیه صحیح نمی باشد.");
            }
            user.RegistrationCompleted = true;
            user.MobileVerified = true;
            user.IsActive = true;
            IdentityResult identityResult = await _userManager.Update(user);
            if (identityResult.Errors.Any())
            {
                return Error(string.Join(',', identityResult.Errors.Select((IdentityError x) => x.Description).ToArray()));
            }
            await _integratedSmsSenderService.TrySendRegister(new LoginRegisterModel
            {
                Date = DateTime.Now,
                Mobile = user.Mobile,
                User_FullName = user.FullName,
                User_UserName = user.UserName,
                User_Id = user.Id
            }, user.CustomerId.Value);
            await _integratedEmailSenderService.TrySendRegister(new LoginRegisterModel
            {
                Date = DateTime.Now,
                Email = user.Email,
                User_FullName = user.FullName,
                User_UserName = user.UserName,
                User_Id = user.Id
            }, user.CustomerId.Value);
            return Success("عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]/{username}")]
    public Task<ResponseState<string>> Forgot(string username)
    {
        return TryCatch(async () =>
        {
            if (string.IsNullOrEmpty(username))
            {
                return Error("", "نام کاربری خالی است.");
            }
            User user = await _userManager.FindByUserName(username);
            if (user == null)
            {
                return Error("", "نام کاربری معتبر نمی باشد.");
            }
            if (string.IsNullOrEmpty(user.Mobile))
            {
                return Error("", "سرور قادر به ارسال پیامک به کاربر نمی باشد. شماره موبایلی برای این نام کاربری ثبت نشده است.");
            }
            PasswordHasher<User> passwordHasher = new PasswordHasher<User>();
            string newPassword = new Random().Next(10000, 99999).ToString();
            string passwordHash = passwordHasher.HashPassword(user, newPassword);
            user.PasswordHash = passwordHash;
            try
            {
                if (!user.CustomerId.HasValue)
                {
                    await _integratedSmsSenderService.SendRegisterationCode(user.Mobile, newPassword);
                }
                else
                {
                    await _integratedSmsSenderService.SendMemberRegisterationCode(user.Mobile, newPassword, user.CustomerId.Value, insertAppSms: true);
                }
                await _userManager.UpdateAsync(user);
            }
            catch (Exception)
            {
                return Error("", "خطا در ارسال پیامک.");
            }
            try
            {
                await _thirdPartyTransferService.RegisterOrUpdateDigibileMember(user.Id.ToString(), newPassword);
            }
            catch
            {
            }
            return Success("", "عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]/{mobile}")]
    public Task<DefaultResponseState> SendOTP(string mobile, int customerCode)
    {
        return TryCatch(async () =>
        {
            if (!_currentUserService.CustomerId.HasValue)
            {
                Customer customer;
                if (_appConfig.IsCloud)
                {
                    if (customerCode == 0)
                    {
                        return Error("پارامترهای مشتری معتبر نمی باشد.");
                    }
                    customer = await _customerService.GetByCode(customerCode);
                    if (customer == null)
                    {
                        return Error("پارامترهای مشتری معتبر نمی باشد.");
                    }
                }
                else
                {
                    customer = (await _customerService.GetAllCustomers()).FirstOrDefault();
                }
                if (customer == null)
                {
                    return Error("پارامترهای مشتری معتبر نمی باشد.");
                }
                _currentUserService.CustomerId = customer.Id;
            }
            if (!CheckMobileFormatValidity(mobile))
            {
                return Error("شماره موبایل وارد شده معتبر نمی باشد.");
            }
            if ((await _appSmsService.GetByCondition((AppSms x) => x.To == mobile && (int)x.AppSmsType == 8 && x.Date >= DateTime.Now.AddMinutes(-2.0), false)).Any())
            {
                return Error("از آخرین ارسال باید حداقل 2 دقیقه گذشته باشد.");
            }
            try
            {
                int num = new Random().Next(10000, 99999);
                await _integratedSmsSenderService.SendMemberRegisterationCode(mobile, num.ToString(), _currentUserService.CustomerId.Value, insertAppSms: true);
            }
            catch (CustomException)
            {
                return Error("در حال حاضر امکان ارسال کد تائیدیه وجود ندارد.");
            }
            return Success("عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]")]
    public Task<DefaultResponseState> SendLoginOTP(LoginOTP model)
    {
        return TryCatch(async () =>
        {
            if (!CheckMobileFormatValidity(model.Mobile))
            {
                return Error("شماره موبایل وارد شده معتبر نمی باشد.");
            }
            Customer customer;
            if (_appConfig.IsCloud)
            {
                if (model.CustomerCode == 0)
                {
                    return Error("پارامترهای مشتری معتبر نمی باشد.");
                }
                customer = await _customerService.GetByCode(model.CustomerCode);
                if (customer == null)
                {
                    return Error("پارامترهای مشتری معتبر نمی باشد.");
                }
            }
            else
            {
                customer = (await _customerService.GetAllCustomers()).FirstOrDefault();
            }
            if (customer == null)
            {
                return Error("پارامترهای مشتری معتبر نمی باشد.");
            }
            _currentUserService.CustomerId = customer.Id;
            if ((await _appSmsService.GetByCondition((AppSms x) => x.To == model.Mobile && (int)x.AppSmsType == 12 && x.Date >= DateTime.Now.AddMinutes(-2.0), false)).Any())
            {
                return Error("از آخرین ارسال باید حداقل 2 دقیقه گذشته باشد.");
            }
            List<User> list = await _userManager.GetByMobile(model.Mobile);
            if (list == null || !list.Any())
            {
                return Error("کاربری متعلق به این شماره موبایل یافت نشد.");
            }
            try
            {
                int num = new Random().Next(10000, 99999);
                await _integratedSmsSenderService.SendLoginOTPCode(model.Mobile, num.ToString(), _currentUserService.CustomerId.Value, insertAppSms: true);
            }
            catch (CustomException)
            {
                return Error("در حال حاضر امکان ارسال کد تائیدیه وجود ندارد.");
            }
            return Success("عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]")]
    public Task<DefaultResponseState> SendWalletWithdrawOTP(AgentFactorModel model)
    {
        return TryCatch(async () =>
        {
            if (model.Member == null)
            {
                return Error("اطلاعات مشتری یافت نشد.");
            }
            if (!CheckMobileFormatValidity(model.Member.Mobile))
            {
                return Error("شماره موبایل وارد شده معتبر نمی باشد.");
            }
            if ((await _appSmsService.GetByCondition((AppSms x) => x.To == model.Member.Mobile && (int)x.AppSmsType == 16 && x.Date >= DateTime.Now.AddMinutes(-2.0), false)).Any())
            {
                return Error("از آخرین ارسال باید حداقل 2 دقیقه گذشته باشد.");
            }
            try
            {
                int num = new Random().Next(10000, 99999);
                await _integratedSmsSenderService.SendWalletWithdrawOtp(model, $"{num.ToString()},{model.Amount}");
            }
            catch (CustomException)
            {
                return Error("در حال حاضر امکان ارسال کد تائیدیه وجود ندارد.");
            }
            return Success("عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]/{mobile}")]
    public Task<DefaultResponseState> SendDigitalSignOTP(string mobile, int customerCode, Guid id)
    {
        return TryCatch(async () =>
        {
            if (!CheckMobileFormatValidity(mobile))
            {
                return Error("شماره موبایل وارد شده معتبر نمی باشد.");
            }
            if ((await _appSmsService.GetByCondition((AppSms x) => x.To == mobile && (int)x.AppSmsType == 18 && x.Date >= DateTime.Now.AddMinutes(-2.0), false)).Any())
            {
                return Error("از آخرین ارسال باید حداقل 2 دقیقه گذشته باشد.");
            }
            Guid? customerId = _currentUserService.CustomerId;
            if (customerCode != 0)
            {
                customerId = (await _customerService.GetByCode(customerCode))?.Id;
            }
            if (!customerId.HasValue)
            {
                return Error("پارامترهای مشتری معتبر نمی باشد.");
            }
            _currentUserService.CustomerId = customerId;
            try
            {
                UserLoanRequestAuthorizationItem userLoanRequestAuthorizationItem = (await _userLoanRequestAuthorizationItemService.GetByCondition((UserLoanRequestAuthorizationItem x) => x.Id == id, "UserLoanRequest.User", "UserLoanRequest.Agent", "UserLoanRequest.Loan", "AuthorizationItem")).FirstOrDefault();
                if (userLoanRequestAuthorizationItem == null)
                {
                    return Error("آیتم قرارداد یافت نشد.");
                }
                int num = new Random().Next(10000, 99999);
                await _integratedSmsSenderService.SendDigitalSignOtp(mobile, num.ToString(), new UserLoanRequestAuthorizationItemModel(userLoanRequestAuthorizationItem), customerId.Value, insertAppSms: true);
            }
            catch (CustomException)
            {
                return Error("در حال حاضر امکان ارسال کد تائیدیه وجود ندارد.");
            }
            return Success("عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]/{mobile}")]
    public Task<DefaultResponseState> SendThirdPersonDigitalSignOTP(string mobile, int customerCode, Guid id)
    {
        return TryCatch(async () =>
        {
            if (!CheckMobileFormatValidity(mobile))
            {
                return Error("شماره موبایل وارد شده معتبر نمی باشد.");
            }
            if ((await _appSmsService.GetByCondition((AppSms x) => x.To == mobile && (int)x.AppSmsType == 24 && x.Date >= DateTime.Now.AddMinutes(-2.0), false)).Any())
            {
                return Error("از آخرین ارسال باید حداقل 2 دقیقه گذشته باشد.");
            }
            Guid? customerId = _currentUserService.CustomerId;
            if (customerCode != 0)
            {
                customerId = (await _customerService.GetByCode(customerCode))?.Id;
            }
            if (!customerId.HasValue)
            {
                return Error("پارامترهای مشتری معتبر نمی باشد.");
            }
            _currentUserService.CustomerId = customerId;
            try
            {
                ThirdPersonAssuranceAuthorizationItem thirdPersonAssuranceAuthorizationItem = await _thirdPersonAssuranceAuthorizationItemService.GetByIdIncludeRelations(id);
                if (thirdPersonAssuranceAuthorizationItem == null)
                {
                    return Error("آیتم قرارداد یافت نشد.");
                }
                int num = new Random().Next(10000, 99999);
                await _integratedSmsSenderService.SendThirdPersonDigitalSignOtp(mobile, num.ToString(), new ThirdPersonAssuranceAuthorizationItemModel(thirdPersonAssuranceAuthorizationItem), customerId.Value, insertAppSms: true);
            }
            catch (CustomException)
            {
                return Error("در حال حاضر امکان ارسال کد تائیدیه وجود ندارد.");
            }
            return Success("عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]/{domain}")]
    public Task<ResponseState<int>> GetCustomerCodeByDomain(string domain)
    {
        return TryCatch(async () =>
        {
            if (string.IsNullOrEmpty(domain))
            {
                return Error(0, "پارامتر نامعتبر");
            }
            Customer customer = await _customerService.GetByDomain(domain);
            return (customer == null) ? Error(0, "اطلاعات مورد نظر یافت نشد.") : Success(customer.Code, "عملیات با موفقیت انجام شد.");
        });
    }

    private bool CheckMobileFormatValidity(string mobile)
    {
        if (string.IsNullOrEmpty(mobile))
        {
            return false;
        }
        long result2;
        if (!mobile.StartsWith("0"))
        {
            if (mobile.Length != 10 || !long.TryParse(mobile, out var _))
            {
                return false;
            }
        }
        else if (mobile.Length != 11 || !long.TryParse(mobile, out result2))
        {
            return false;
        }
        return true;
    }

    [HttpPost("[action]")]
    public Task<ResponseState<JwtToken>> Token([FromForm] GetApiTokenModel model)
    {
        return TryCatch(async () =>
        {
            AppSettings appSettings = (await _appSettingsService.GetByCondition((AppSettings x) => x.ApiKey == model.ApiKey && x.ApiPassword == model.Password, true))?.FirstOrDefault();
            if (appSettings == null)
            {
                return Error<JwtToken>(null, "کلید رابط یا کلمه عبور معتبر نمی باشد.");
            }
            Customer customer = await _customerService.GetById(appSettings.CustomerId);
            if (customer == null)
            {
                return Error<JwtToken>(null, "مشتری یافت نشد.");
            }
            _currentUserService.CustomerId = customer.Id;
            User user = (await _userManager.GetUsersInRoleAsync("AppApiUser"))?.FirstOrDefault();
            if (user == null)
            {
                return Error<JwtToken>(null, "کاربر api یافت نشد.");
            }
            user = await _userManager.FindByUserName(user.UserName);
            User user2 = user;
            user2.UserRoleNames = (await _userManager.GetRolesAsync(user)) as List<string>;
            JwtTokenService jwtTokenService = new JwtTokenService(_userManager, _roleManager);
            ClaimsIdentity claimIdentity = await jwtTokenService.GetUserClaims(user);
            TokenProviderOptions options = new TokenProviderOptions
            {
                Path = "/api/token",
                Audience = "api-users",
                Issuer = "http://xxx.com",
                Expiration = TimeSpan.FromDays(1.0),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_appConfig.JwtSecretKey)), "HS256")
            };
            JwtToken jwtToken = await jwtTokenService.GetJwtToken(claimIdentity, options);
            jwtToken.UserData = null;
            return Success(jwtToken, "عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<OTPLoginResult>> OTPToken([FromForm] LoginOTP model)
    {
        return TryCatch(async () =>
        {
            if (string.IsNullOrWhiteSpace(model.OTPCode))
            {
                return Error<OTPLoginResult>(null, "رمز پیامکی وارد نشده است.");
            }
            Customer customer;
            if (_appConfig.IsCloud)
            {
                if (model.CustomerCode == 0)
                {
                    return Error<OTPLoginResult>(null, "پارامترهای مشتری معتبر نمی باشد.");
                }
                customer = await _customerService.GetByCode(model.CustomerCode);
                if (customer == null)
                {
                    return Error<OTPLoginResult>(null, "پارامترهای مشتری معتبر نمی باشد.");
                }
            }
            else
            {
                customer = (await _customerService.GetAllCustomers()).FirstOrDefault();
            }
            if (customer == null)
            {
                return Error<OTPLoginResult>(null, "پارامترهای مشتری معتبر نمی باشد.");
            }
            _currentUserService.CustomerId = customer.Id;
            List<AppSms> list = await _appSmsService.GetByCondition((AppSms x) => x.To == model.Mobile && (int)x.AppSmsType == (int)model.AppSmsType && x.Date >= DateTime.Now.AddMinutes((!string.IsNullOrWhiteSpace(model.Role)) ? (-4) : (-2)) && x.Tag == model.OTPCode, false);
            if (list == null || !list.Any())
            {
                return Error<OTPLoginResult>(null, "رمز وارد شده معتبر نمی باشد.");
            }
            List<User> users = await _userManager.GetByMobile(model.Mobile);
            if (users == null || !users.Any())
            {
                return Error<OTPLoginResult>(null, "کاربر یافت نشد.");
            }
            foreach (User item in users)
            {
                User user = item;
                user.UserRoleNames = (await _userManager.GetRolesAsync(item)) as List<string>;
            }
            List<string> list2 = users.SelectMany((User x) => x.UserRoleNames).Distinct().ToList();
            if (list2.Count() > 1 && !list2.Contains(model.Role))
            {
                return Success(new OTPLoginResult
                {
                    Roles = list2
                });
            }
            User user2 = ((users.Count == 1 || (users.Count > 1 && string.IsNullOrWhiteSpace(model.Role))) ? users.First() : users.FirstOrDefault((User x) => x.UserRoleNames.Contains(model.Role) && x.IsActive));
            if (user2 == null)
            {
                return Error<OTPLoginResult>(null, "عدم امکان ورود به سیستم. کاربر یافت نشد.");
            }
            if (!user2.IsActive)
            {
                return Error<OTPLoginResult>(null, "حساب کاربری شما غیر فعال شده است");
            }
            JwtTokenService jwtTokenService = new JwtTokenService(_userManager, _roleManager);
            ClaimsIdentity claimIdentity = await jwtTokenService.GetUserClaims(user2);
            user2.LastLoginDate = DateTime.Now;
            await _userManager.UpdateAsync(user2);
            await _integratedSmsSenderService.TrySendLogin(new LoginRegisterModel
            {
                Date = DateTime.Now,
                Mobile = user2.Mobile,
                User_FullName = user2.FullName,
                User_UserName = user2.UserName
            }, user2.CustomerId.Value);
            await _integratedEmailSenderService.TrySendLogin(new LoginRegisterModel
            {
                Date = DateTime.Now,
                Email = user2.Email,
                User_FullName = user2.FullName,
                User_UserName = user2.UserName
            }, user2.CustomerId.Value);
            TokenProviderOptions options = new TokenProviderOptions
            {
                Path = "/api/token",
                Audience = "api-users",
                Issuer = "http://xxx.com",
                Expiration = TimeSpan.FromDays(1.0),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_appConfig.JwtSecretKey)), "HS256")
            };
            JwtToken jwtToken = await jwtTokenService.GetJwtToken(claimIdentity, options);
            return Success(new OTPLoginResult
            {
                JwtToken = jwtToken
            }, "عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<dynamic>> PreAgentRegisterAndSendOTP([FromForm] AgentRegisterDto model)
    {
        return TryCatch(async () =>
        {
            if (!CheckMobileFormatValidity(model.Mobile))
            {
                return Error((object)0, "شماره موبایل وارد شده معتبر نمی باشد.");
            }
            string userName = "n" + model.Mobile;
            Customer customer;
            if (_appConfig.IsCloud)
            {
                if (model.CustomerCode == 0)
                {
                    return Error((object)0, "پارامترهای مشتری معتبر نمی باشد.");
                }
                customer = await _customerService.GetByCode(model.CustomerCode);
                if (customer == null)
                {
                    return Error((object)0, "پارامترهای مشتری معتبر نمی باشد.");
                }
                userName = $"{customer.Code}-n-{model.Mobile}";
            }
            else
            {
                customer = (await _customerService.GetAllCustomers()).FirstOrDefault();
            }
            if (customer == null)
            {
                return Error((object)0, "پارامترهای مشتری معتبر نمی باشد.");
            }
            _currentUserService.CustomerId = customer.Id;
            User user = await _userManager.FindByUserName(userName);
            if (user != null && user.RegistrationCompleted)
            {
                return Error((object)0, "با این نام کاربری قبلا ثبت نام انجام شده است");
            }
            AgentMetaData agentMetaData = null;
            if (user == null)
            {
                user = new User();
                if (model.MetaData != null)
                {
                    agentMetaData = new AgentMetaData
                    {
                        AgentCategoryActivities = model.MetaData.AgentCategoryActivities,
                        CustomerId = customer.Id,
                        ShenaseMelli = model.MetaData.ShenaseMelli,
                        ShomareSabt = model.MetaData.ShomareSabt,
                        SignaturesOwnerNames = model.MetaData.SignaturesOwnerNames,
                        Status = AgentStatus.Pending,
                        CompanyName = model.MetaData.CompanyName,
                        AgentType = model.MetaData.AgentType,
                        IsOnline = model.MetaData.IsOnline,
                        IsPhysical = model.MetaData.IsPhysical,
                        SiteUrl = model.MetaData.SiteUrl
                    };
                }
            }
            else if (model.MetaData != null)
            {
                if (user.AgentMetaData == null)
                {
                    user.AgentMetaData = new AgentMetaData();
                }
                user.AgentMetaData.AgentCategoryActivities = model.MetaData.AgentCategoryActivities;
                user.AgentMetaData.CustomerId = customer.Id;
                user.AgentMetaData.ShenaseMelli = model.MetaData.ShenaseMelli;
                user.AgentMetaData.ShomareSabt = model.MetaData.ShomareSabt;
                user.AgentMetaData.SignaturesOwnerNames = model.MetaData.SignaturesOwnerNames;
                user.AgentMetaData.Status = AgentStatus.Pending;
                user.AgentMetaData.CompanyName = model.MetaData.CompanyName;
                user.AgentMetaData.AgentType = model.MetaData.AgentType;
                user.AgentMetaData.IsOnline = model.MetaData.IsOnline;
                user.AgentMetaData.IsPhysical = model.MetaData.IsPhysical;
                user.AgentMetaData.SiteUrl = model.MetaData.SiteUrl;
            }
            if (model.MetaData?.ThumbnailFile != null)
            {
                if (model.MetaData.ThumbnailFile.Length == 0L)
                {
                    return Error<object>(null, "فایل کارت ملی خالی است.");
                }
                if (!_fileHelper.CheckFileIsImage(model.MetaData.ThumbnailFile))
                {
                    return Error<object>(null, "فقط فایل با فرمت تصویر و pdf و rar مجاز می باشد.");
                }
                AgentMetaData agentMetaData2 = agentMetaData;
                agentMetaData2.ThumbnailFileName = await _fileHelper.UploadFile(model.MetaData.ThumbnailFile, FileUploadDirectory.AgentAttachment, customer.Id.ToString());
            }
            if (model.MetaData?.KartMelliFile != null)
            {
                if (model.MetaData.KartMelliFile.Length == 0L)
                {
                    return Error<object>(null, "فایل کارت ملی خالی است.");
                }
                if (!_fileHelper.CheckFileIsImageOrPdfOrRarOrZipOrXls(model.MetaData.KartMelliFile))
                {
                    return Error<object>(null, "فقط فایل با فرمت تصویر و pdf و rar مجاز می باشد.");
                }
                AgentMetaData agentMetaData2 = agentMetaData;
                agentMetaData2.KartMelliFileName = await _fileHelper.UploadFile(model.MetaData.KartMelliFile, FileUploadDirectory.AgentAttachment, customer.Id.ToString());
            }
            if (model.MetaData?.JavazKasbFile != null)
            {
                if (model.MetaData.JavazKasbFile.Length == 0L)
                {
                    return Error<object>(null, "فایل کارت ملی خالی است.");
                }
                if (!_fileHelper.CheckFileIsImageOrPdfOrRarOrZipOrXls(model.MetaData.JavazKasbFile))
                {
                    return Error<object>(null, "فقط فایل با فرمت تصویر و pdf و rar مجاز می باشد.");
                }
                AgentMetaData agentMetaData2 = agentMetaData;
                agentMetaData2.JavazKasbFileName = await _fileHelper.UploadFile(model.MetaData.JavazKasbFile, FileUploadDirectory.AgentAttachment, customer.Id.ToString());
            }
            if (model.MetaData?.KartMelliFile != null)
            {
                if (model.MetaData.KartMelliFile.Length == 0L)
                {
                    return Error<object>(null, "فایل کارت ملی خالی است.");
                }
                if (!_fileHelper.CheckFileIsImageOrPdfOrRarOrZipOrXls(model.MetaData.KartMelliFile))
                {
                    return Error<object>(null, "فقط فایل با فرمت تصویر و pdf و rar مجاز می باشد.");
                }
                AgentMetaData agentMetaData2 = agentMetaData;
                agentMetaData2.KartMelliFileName = await _fileHelper.UploadFile(model.MetaData.KartMelliFile, FileUploadDirectory.AgentAttachment, customer.Id.ToString());
            }
            if (model.MetaData?.EjarenameFile != null)
            {
                if (model.MetaData.EjarenameFile.Length == 0L)
                {
                    return Error<object>(null, "فایل اجاره نامه خالی است.");
                }
                if (!_fileHelper.CheckFileIsImageOrPdfOrRarOrZipOrXls(model.MetaData.EjarenameFile))
                {
                    return Error<object>(null, "فقط فایل با فرمت تصویر و pdf و rar مجاز می باشد.");
                }
                AgentMetaData agentMetaData2 = agentMetaData;
                agentMetaData2.EjarenameFileName = await _fileHelper.UploadFile(model.MetaData.EjarenameFile, FileUploadDirectory.AgentAttachment, customer.Id.ToString());
            }
            if (model.MetaData?.AsasnameFile != null)
            {
                if (model.MetaData.AsasnameFile.Length == 0L)
                {
                    return Error<object>(null, "فایل اساسنامه خالی است.");
                }
                if (!_fileHelper.CheckFileIsImageOrPdfOrRarOrZipOrXls(model.MetaData.AsasnameFile))
                {
                    return Error<object>(null, "فقط فایل با فرمت تصویر و pdf و rar مجاز می باشد.");
                }
                AgentMetaData agentMetaData2 = agentMetaData;
                agentMetaData2.AsasnameFileName = await _fileHelper.UploadFile(model.MetaData.AsasnameFile, FileUploadDirectory.AgentAttachment, customer.Id.ToString());
            }
            if (model.MetaData?.RoznameRasmiFile != null)
            {
                if (model.MetaData.RoznameRasmiFile.Length == 0L)
                {
                    return Error<object>(null, "فایل روزنامه رسمی خالی است.");
                }
                if (!_fileHelper.CheckFileIsImageOrPdfOrRarOrZipOrXls(model.MetaData.RoznameRasmiFile))
                {
                    return Error<object>(null, "فقط فایل با فرمت تصویر و pdf و rar مجاز می باشد.");
                }
                AgentMetaData agentMetaData2 = agentMetaData;
                agentMetaData2.RoznameRasmiFileName = await _fileHelper.UploadFile(model.MetaData.RoznameRasmiFile, FileUploadDirectory.AgentAttachment, customer.Id.ToString());
            }
            if (model.MetaData?.PledgeChequeFile != null)
            {
                if (model.MetaData.PledgeChequeFile.Length == 0L)
                {
                    return Error<object>(null, "فایل روزنامه رسمی خالی است.");
                }
                if (!_fileHelper.CheckFileIsImageOrPdfOrRarOrZipOrXls(model.MetaData.PledgeChequeFile))
                {
                    return Error<object>(null, "فقط فایل با فرمت تصویر و pdf و rar مجاز می باشد.");
                }
                AgentMetaData agentMetaData2 = agentMetaData;
                agentMetaData2.PledgeChequeFileName = await _fileHelper.UploadFile(model.MetaData.PledgeChequeFile, FileUploadDirectory.AgentAttachment, customer.Id.ToString());
            }
            user.FirstName = model.FirstName;
            user.BusinessName = model.BusinessName;
            user.Shenasname = model.Shenasname;
            user.Mobile = model.Mobile;
            user.Phone = model.Phone;
            user.Address = model.Address;
            user.Email = model.Email;
            user.City = model.City;
            user.PostalCode = model.PostalCode;
            user.NationalCode = model.NationalCode;
            user.IsActive = true;
            user.UserName = userName;
            user.CustomerId = customer.Id;
            user.RegDate2 = DateTime.Now;
            user.Otp = new Random().Next(10000, 99999);
            user.AgentMetaData = agentMetaData;
            user.UserRoleNames = new List<string> { "Agent" };
            PasswordHasher<User> hasher = new PasswordHasher<User>();
            string passwordHash = hasher.HashPassword(user, model.Password);
            user.PasswordHash = passwordHash;
            if (user.Id == 0L)
            {
                IdentityResult identityResult = await _userManager.Insert(user, user.PasswordHash, user.UserRoleNames);
                if (identityResult.Errors.Any())
                {
                    return Error<object>(null, string.Join(',', identityResult.Errors.Select((IdentityError x) => x.Description).ToArray()));
                }
                user.PasswordHash = hasher.HashPassword(user, model.Password);
                await _userManager.UpdateAsync(user);
            }
            else
            {
                IdentityResult identityResult2 = await _userManager.Update(user);
                if (identityResult2.Errors.Any())
                {
                    return Error<object>(null, string.Join(',', identityResult2.Errors.Select((IdentityError x) => x.Description).ToArray()));
                }
            }
            bool isOtpActive = false;
            try
            {
                if ((await _appSettingsService.GetAppSettings())?.RegisterOtpSmsPattern == null)
                {
                    await _integratedSmsSenderService.TrySendRegister(new LoginRegisterModel
                    {
                        Date = DateTime.Now,
                        Mobile = user.Mobile,
                        User_FullName = user.FullName,
                        User_UserName = user.UserName,
                        User_Id = user.Id
                    }, user.CustomerId.Value);
                    await _integratedEmailSenderService.TrySendRegister(new LoginRegisterModel
                    {
                        Date = DateTime.Now,
                        Email = user.Email,
                        User_FullName = user.FullName,
                        User_UserName = user.UserName,
                        User_Id = user.Id
                    }, user.CustomerId.Value);
                }
                else
                {
                    isOtpActive = true;
                    await _integratedSmsSenderService.SendMemberRegisterationCode(model.Mobile, user.Otp.ToString(), customer.Id);
                }
            }
            catch (CustomException)
            {
                return Error<object>(null, "در حال حاضر امکان ارسال کد تائیدیه وجود ندارد.");
            }
            return Success((object)new
            {
                isOtpActive = isOtpActive,
                userId = user.Id
            }, "عملیات با موفقیت انجام شد.");
        });
    }
}
