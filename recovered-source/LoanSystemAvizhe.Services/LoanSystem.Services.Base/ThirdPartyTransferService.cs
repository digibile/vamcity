using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using LoanSystem.Core;
using LoanSystem.Data;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Models;
using LoanSystem.Services.Models.Landa;
using LoanSystem.Services.Users;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Data.Enums;
using LoanSystemAvizhe.Services.Models.Digibile;
using LoanSystemAvizhe.Services.Models.DynamicQueryModel;
using LoanSystemAvizhe.Services.ThirdParty;
using Microsoft.EntityFrameworkCore;
using NikanBase.Services.Security;
using Spm.Services.Infrastructure;

namespace LoanSystem.Services.Base;

public class ThirdPartyTransferService : BaseService, IThirdPartyTransferService
{
    private readonly IMemberService _memberService;

    private readonly LandaService _landaService;

    private readonly DigibileService _digibileService;

    private readonly UserManager _userManager;

    private readonly IUserLoanService _userLoanService;

    private readonly IUserPaymentService _userPaymentService;

    private readonly IThirdPartySettingsService _thirdPartySettingsService;

    private readonly IEncryptionService _encryptionService;

    public ThirdPartyTransferService(IMemberService memberService, AppDbContext context, ICurrentUserService currentUserService, IThirdPartySettingsService thirdPartySettingsService, LandaService landaService, UserManager userManager, IUserLoanService userLoanService, IUserPaymentService userPaymentService, DigibileService digibileService, IEncryptionService encryptionService)
        : base(context, currentUserService)
    {
        _memberService = memberService;
        _landaService = landaService;
        _userManager = userManager;
        _userLoanService = userLoanService;
        _thirdPartySettingsService = thirdPartySettingsService;
        _userPaymentService = userPaymentService;
        _digibileService = digibileService;
        _encryptionService = encryptionService;
    }

    public async Task<Guid?> GetLandaId(string entityId, string entityType)
    {
        ThirdPartyTransfer thirdPartyTransfer = await (from x in Table<ThirdPartyTransfer>()
                                                       where x.EntityId == entityId && x.EntityType == entityType
                                                       select x).FirstOrDefaultAsync();
        if (thirdPartyTransfer == null)
        {
            return null;
        }
        ThirdPartyTransferModel thirdPartyTransferModel = new ThirdPartyTransferModel(thirdPartyTransfer);
        return thirdPartyTransferModel.LandaTransfer.LandaId;
    }

    public Task<List<ThirdPartyTransfer>> GetThirdPartyTransfers(string entityId, string entityType, ThirdPart? thirdParty)
    {
        IQueryable<ThirdPartyTransfer> source = Table<ThirdPartyTransfer>();
        if (!string.IsNullOrEmpty(entityId))
        {
            source = source.Where((ThirdPartyTransfer x) => x.EntityId == entityId);
        }
        if (!string.IsNullOrEmpty(entityType))
        {
            source = source.Where((ThirdPartyTransfer x) => x.EntityType == entityType);
        }
        if (thirdParty.HasValue)
        {
            switch (thirdParty)
            {
                case ThirdPart.Landa:
                    source = source.Where((ThirdPartyTransfer x) => !string.IsNullOrEmpty(x.LandaJson));
                    break;
                case ThirdPart.Digibile:
                    source = source.Where((ThirdPartyTransfer x) => !string.IsNullOrEmpty(x.DigibileJson));
                    break;
                case ThirdPart.Nikan:
                    source = source.Where((ThirdPartyTransfer x) => !string.IsNullOrEmpty(x.NikanJson));
                    break;
            }
        }
        return source.ToListAsync();
    }

    public async Task<(int, List<ThirdPartyTransferModel>)> GetNotSentMembers(DynamicQueryForThirdPartyModel dynamicQuery)
    {
        IQueryable<ThirdPartyTransfer> query = Table<ThirdPartyTransfer>();
        if (dynamicQuery.ThirdParty == ThirdPart.Landa)
        {
            query = query.Where((ThirdPartyTransfer x) => !string.IsNullOrEmpty(x.LandaJson));
        }
        if (dynamicQuery.ThirdParty == ThirdPart.Digibile)
        {
            query = query.Where((ThirdPartyTransfer x) => !string.IsNullOrEmpty(x.DigibileJson));
        }
        List<long> userIds = (await _userLoanService.GetAllUserLoans()).Select((UserLoan x) => x.UserId).ToList();
        IQueryable<User> usersQuery = (await _memberService.AsQueryable()).Where((User x) => userIds.Contains(x.Id) && !query.Select((ThirdPartyTransfer t) => t.EntityId).Any((string a) => x.Id.ToString() == a));
        if (!string.IsNullOrEmpty(dynamicQuery.EntityId))
        {
            usersQuery = usersQuery.Where((User x) => x.Id == long.Parse(dynamicQuery.EntityId));
        }
        else if (!string.IsNullOrWhiteSpace(dynamicQuery.Search))
        {
            usersQuery = usersQuery.Where((User x) => x.NationalCode.Contains(dynamicQuery.Search) || x.FirstName.Contains(dynamicQuery.Search) || x.LastName.Contains(dynamicQuery.Search) || x.Mobile.Contains(dynamicQuery.Search) || x.PhoneNumber.Contains(dynamicQuery.Search) || string.Concat(x.FirstName + " ", x.LastName).Contains(dynamicQuery.Search));
        }
        int countAll = await usersQuery.CountAsync();
        usersQuery = usersQuery.OrderByDescending((User x) => x.Id).Skip((dynamicQuery.Page - 1) * dynamicQuery.ItemsPerPage).Take(dynamicQuery.ItemsPerPage);
        List<ThirdPartyTransferModel> item = (await usersQuery.AsNoTracking().ToListAsync()).Select((User user) => new ThirdPartyTransferModel
        {
            Id = Guid.NewGuid(),
            EntityId = user.Id.ToString(),
            EntityType = "User",
            EntityTag = user.FullName + ", کد ملی (" + user.NationalCode + ")"
        }).ToList();
        return (countAll, item);
    }

    public async Task<(int, List<ThirdPartyTransferModel>)> GetNotSentLoans(DynamicQueryForThirdPartyModel dynamicQuery)
    {
        IQueryable<ThirdPartyTransfer> query = Table<ThirdPartyTransfer>();
        if (dynamicQuery.ThirdParty == ThirdPart.Landa)
        {
            query = query.Where((ThirdPartyTransfer x) => !string.IsNullOrEmpty(x.LandaJson));
        }
        if (dynamicQuery.ThirdParty == ThirdPart.Digibile)
        {
            query = query.Where((ThirdPartyTransfer x) => !string.IsNullOrEmpty(x.DigibileJson));
        }
        IQueryable<UserLoan> userLoansQuery = from x in _context.UserLoans.AsNoTracking().Include((UserLoan x) => x.Loan).Include((UserLoan x) => x.User)
                                              where !query.Select((ThirdPartyTransfer t) => t.EntityId).Any((string a) => ((object)x.Id).ToString() == a)
                                              select x;
        if (!string.IsNullOrEmpty(dynamicQuery.EntityId))
        {
            userLoansQuery = userLoansQuery.Where((UserLoan x) => x.Id == Guid.Parse(dynamicQuery.EntityId));
        }
        else if (!string.IsNullOrWhiteSpace(dynamicQuery.Search))
        {
            userLoansQuery = userLoansQuery.Where((UserLoan x) => x.Code.ToString() == dynamicQuery.Search || x.Installment.ToString() == dynamicQuery.Search || x.Amount.ToString() == dynamicQuery.Search || x.Loan.Title.Contains(dynamicQuery.Search));
        }
        int countAll = await userLoansQuery.CountAsync();
        userLoansQuery = userLoansQuery.OrderByDescending((UserLoan x) => x.Id).Skip((dynamicQuery.Page - 1) * dynamicQuery.ItemsPerPage).Take(dynamicQuery.ItemsPerPage);
        List<ThirdPartyTransferModel> item = (await userLoansQuery.AsNoTracking().ToListAsync()).Select((UserLoan userLoan) => new ThirdPartyTransferModel
        {
            Id = Guid.NewGuid(),
            EntityId = userLoan.Id.ToString(),
            EntityType = "UserLoan",
            EntityTag = string.Format("شماره وام: {0}, عنوان وام: [{1}], متعلق به: [{2}], مبلغ [{3}]", userLoan.Code, userLoan.Loan.Title, userLoan.User.FullName, userLoan.Amount.ToString("N0"))
        }).ToList();
        return (countAll, item);
    }

    public async Task<(int, List<ThirdPartyTransferModel>)> GetNotSentLoanPayments(DynamicQueryForThirdPartyModel dynamicQuery)
    {
        IQueryable<ThirdPartyTransfer> query = Table<ThirdPartyTransfer>();
        if (dynamicQuery.ThirdParty == ThirdPart.Landa)
        {
            query = query.Where((ThirdPartyTransfer x) => !string.IsNullOrEmpty(x.LandaJson));
        }
        if (dynamicQuery.ThirdParty == ThirdPart.Digibile)
        {
            query = query.Where((ThirdPartyTransfer x) => !string.IsNullOrEmpty(x.DigibileJson));
        }
        IQueryable<UserLoanPayment> userLoanPaymentsQuery = from x in _context.UserLoanPayments.AsNoTracking().Include((UserLoanPayment x) => x.UserLoan).ThenInclude((UserLoan x) => x.Loan)
                                                            where !query.Select((ThirdPartyTransfer t) => t.EntityId).Any((string a) => ((object)x.Id).ToString() == a)
                                                            select x;
        if (!string.IsNullOrEmpty(dynamicQuery.EntityId))
        {
            userLoanPaymentsQuery = userLoanPaymentsQuery.Where((UserLoanPayment x) => x.Id == Guid.Parse(dynamicQuery.EntityId));
        }
        else if (!string.IsNullOrWhiteSpace(dynamicQuery.Search))
        {
            userLoanPaymentsQuery = userLoanPaymentsQuery.Where((UserLoanPayment x) => x.Code.ToString() == dynamicQuery.Search || x.UserLoan.Code.ToString() == dynamicQuery.Search || x.SumAmount.ToString() == dynamicQuery.Search || x.UserLoan.Loan.Title.Contains(dynamicQuery.Search));
        }
        int countAll = await userLoanPaymentsQuery.CountAsync();
        userLoanPaymentsQuery = userLoanPaymentsQuery.OrderByDescending((UserLoanPayment x) => x.Id).Skip((dynamicQuery.Page - 1) * dynamicQuery.ItemsPerPage).Take(dynamicQuery.ItemsPerPage);
        List<ThirdPartyTransferModel> item = (await userLoanPaymentsQuery.AsNoTracking().ToListAsync()).Select((UserLoanPayment userLoanPayment) => new ThirdPartyTransferModel
        {
            Id = Guid.NewGuid(),
            EntityId = userLoanPayment.Id.ToString(),
            EntityType = "UserLoanPayment",
            EntityTag = string.Format("عنوان وام [{0} - شماره وام: ({1})], شماره قسط [{2}], مبلغ قسط [{3}], مبلغ واریز [{4}]", userLoanPayment.UserLoan.Loan.Title, userLoanPayment.UserLoan.Code, userLoanPayment.InstallmentNumber, userLoanPayment.InstallmentAmount.ToString("N0"), userLoanPayment.SumAmount.ToString("N0"))
        }).ToList();
        return (countAll, item);
    }

    public async Task<Dictionary<string, dynamic>> SendMember(List<string> ids, ThirdPart thirdParty, List<GetCustomerResponse> currentLandaCustomers = null, Dictionary<string, string> extraData = null, List<ShowUserResponse> currentDigibileCustomers = null)
    {
        JsonSerializerOptions JsonSetting = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        List<User> users = await _memberService.GetByCondition((User x) => ids.Contains(x.Id.ToString()));
        User user = ((!_currentUserService.UserId.HasValue) ? null : (await _userManager.GetById(_currentUserService.UserId.Value)));
        User currentUser = user;
        Dictionary<string, object> summery = new Dictionary<string, object>();
        if (thirdParty == ThirdPart.Landa)
        {
            if (currentLandaCustomers == null && (ids?.Any() ?? false))
            {
                currentLandaCustomers = await _landaService.GetAllCustomers();
            }
            if (currentLandaCustomers == null)
            {
                currentLandaCustomers = new List<GetCustomerResponse>();
            }
        }
        if (thirdParty == ThirdPart.Digibile)
        {
            if (currentDigibileCustomers == null && (users?.Any() ?? false))
            {
                List<string> national_ids = users.Select((User x) => x.NationalCode).ToList();
                currentDigibileCustomers = (await _digibileService.ShowUsers(new ShowUsers
                {
                    national_ids = national_ids
                })).result?.users;
            }
            if (currentDigibileCustomers == null)
            {
                currentDigibileCustomers = new List<ShowUserResponse>();
            }
        }
        foreach (string userId in ids)
        {
            try
            {
                long.TryParse(userId, out var uId);
                User user2 = users.FirstOrDefault((User x) => x.Id == uId);
                if (user2 == null)
                {
                    summery.TryAdd(userId, new
                    {
                        error = true,
                        value = "مشتری یافت نشد"
                    });
                    continue;
                }
                if (thirdParty != ThirdPart.Landa)
                {
                    goto IL_0c75;
                }
                ThirdPartyTransfer test1 = (await GetThirdPartyTransfers(userId, "User", null))?.FirstOrDefault();
                if (!string.IsNullOrEmpty(test1?.LandaJson))
                {
                    summery.TryAdd(userId, new
                    {
                        error = false,
                        value = test1.LandaJson
                    });
                    continue;
                }
                GetCustomerResponse test2 = currentLandaCustomers.FirstOrDefault((GetCustomerResponse x) => x.code == user2.NationalCode.ToString());
                if (test2 != null)
                {
                    if (test1 != null)
                    {
                        test1.LandaJson = JsonSerializer.Serialize(test2, JsonSetting);
                        await Update(test1);
                        summery.TryAdd(userId, new
                        {
                            error = false,
                            value = test1.LandaJson
                        });
                        continue;
                    }
                    await Insert(new ThirdPartyTransfer
                    {
                        CustomerId = _currentUserService.CustomerId.Value,
                        EntityId = userId,
                        EntityType = "User",
                        EntityTag = user2.FullName + ", کد ملی (" + user2.NationalCode + ")",
                        LandaJson = JsonSerializer.Serialize(new LandaTransferModel
                        {
                            CreatedDateTime = DateTime.Now.ToPersianDateTime(),
                            Creator = (currentUser?.FullName ?? "نامشخص"),
                            LandaCode = test2.code,
                            LandaId = test2.id,
                            LandaName = test2.name
                        }, JsonSetting)
                    });
                    summery.TryAdd(userId, new
                    {
                        error = true,
                        value = JsonSerializer.Serialize(test2)
                    });
                    goto IL_0c75;
                }
                LandaResponse<CreateCustomerResponse> landaResponse = await _landaService.CreateCustomer(new CreateCustomer
                {
                    name = user2.FirstName + " " + user2.LastName,
                    aliasName = "",
                    code = user2.NationalCode,
                    comment = "",
                    customerContactMobiles = new List<object>
                    {
                        new
                        {
                            value = user2.Mobile,
                            comment = ""
                        }
                    },
                    customerAddresses = new List<object>
                    {
                        new
                        {
                            address = user2.Address,
                            postalCode = user2.PostalCode
                        }
                    },
                    customerContactEmails = new List<object>
                    {
                        new
                        {
                            value = (user2.Email ?? ""),
                            comment = ""
                        }
                    },
                    person = new Person
                    {
                        gender = 101,
                        lastName = user2.LastName,
                        name = user2.FirstName
                    },
                    typeId = 172,
                    unitId = Guid.Parse("1e9ff747-ce38-4204-91ae-b80d7f4e7cb1")
                });
                if (landaResponse.code != 1)
                {
                    summery.TryAdd(userId, new
                    {
                        error = true,
                        value = landaResponse.message
                    });
                    continue;
                }
                if (test1 != null)
                {
                    test1.LandaJson = JsonSerializer.Serialize(new LandaTransferModel
                    {
                        CreatedDateTime = DateTime.Now.ToPersianDateTime(),
                        Creator = (currentUser?.FullName ?? "نامشخص"),
                        LandaCode = landaResponse.data.code,
                        LandaId = landaResponse.data.id,
                        LandaName = landaResponse.data.name
                    }, JsonSetting);
                    await Update(test1);
                    summery.TryAdd(userId, new
                    {
                        error = false,
                        value = test1.LandaJson
                    });
                }
                else
                {
                    summery.TryAdd(userId, new
                    {
                        error = false,
                        value = JsonSerializer.Serialize(landaResponse.data)
                    });
                    await Insert(new ThirdPartyTransfer
                    {
                        CustomerId = _currentUserService.CustomerId.Value,
                        EntityId = userId,
                        EntityType = "User",
                        EntityTag = user2.FullName + ", کد ملی (" + user2.NationalCode + ")",
                        LandaJson = JsonSerializer.Serialize(new LandaTransferModel
                        {
                            CreatedDateTime = DateTime.Now.ToPersianDateTime(),
                            Creator = (currentUser?.FullName ?? "نامشخص"),
                            LandaCode = landaResponse.data.code,
                            LandaId = landaResponse.data.id,
                            LandaName = landaResponse.data.name
                        }, JsonSetting)
                    });
                }
                goto IL_0c75;
            IL_0c75:
                if (thirdParty != ThirdPart.Digibile)
                {
                    continue;
                }
                test1 = (await GetThirdPartyTransfers(userId, "User", null))?.FirstOrDefault();
                ShowUserResponse test3 = currentDigibileCustomers.FirstOrDefault((ShowUserResponse x) => x.user_natID == user2.NationalCode.ToString());
                if (test3 != null)
                {
                    string value = "";
                    if (extraData == null || !extraData.TryGetValue(userId, out value))
                    {
                        summery.TryAdd(userId, new
                        {
                            error = true,
                            value = "در حال حاضر آپدیت کاربر امکان پذیر نمی باشد (کلمه عبور کاربر در دسترس نمی باشد)"
                        });
                        continue;
                    }
                    DigibileBaseResponse<UpdateUserResponse> digibileBaseResponse = await _digibileService.UpdateUser(new UpdateUser
                    {
                        user_cell = user2.Mobile,
                        user_fullname = user2.FirstName + " " + user2.LastName,
                        user_natID = user2.NationalCode,
                        user_password = value
                    });
                    if (digibileBaseResponse.success != "true")
                    {
                        summery.TryAdd(userId, new
                        {
                            error = true,
                            value = digibileBaseResponse.message
                        });
                    }
                    else if (test1 != null)
                    {
                        test1.DigibileJson = JsonSerializer.Serialize(new DigibileTransferModel
                        {
                            CreatedDateTime = DateTime.Now.ToPersianDateTime(),
                            Creator = (currentUser?.FullName ?? "نامشخص"),
                            DigibileId = test3.user_id
                        }, JsonSetting);
                        await Update(test1);
                        summery.TryAdd(userId, new
                        {
                            error = false,
                            value = test1.DigibileJson
                        });
                    }
                    else
                    {
                        await Insert(new ThirdPartyTransfer
                        {
                            CustomerId = _currentUserService.CustomerId.Value,
                            EntityId = userId,
                            EntityType = "User",
                            EntityTag = user2.FullName + ", کد ملی (" + user2.NationalCode + ")",
                            DigibileJson = JsonSerializer.Serialize(new DigibileTransferModel
                            {
                                CreatedDateTime = DateTime.Now.ToPersianDateTime(),
                                Creator = (currentUser?.FullName ?? "نامشخص"),
                                DigibileId = test3.user_id
                            }, JsonSetting)
                        });
                        summery.TryAdd(userId, new
                        {
                            error = true,
                            value = JsonSerializer.Serialize(test3)
                        });
                    }
                    continue;
                }
                string value2 = "";
                if (extraData == null || !extraData.TryGetValue(userId, out value2))
                {
                    summery.TryAdd(userId, new
                    {
                        error = true,
                        value = "در حال حاضر ارسال اطلاعات امکان پذیر نمی باشد (کلمه عبور کاربر در دسترس نمی باشد)."
                    });
                    continue;
                }
                DigibileBaseResponse<RegisterUserResponse> digibileBaseResponse2 = await _digibileService.RegisterUser(new RegisterUser
                {
                    user_address = user2.Address,
                    user_cell = user2.Mobile,
                    user_email = user2.Email,
                    user_fullname = user2.FirstName + " " + user2.LastName,
                    user_natID = user2.NationalCode,
                    user_password = value2,
                    user_zipcode = user2.PostalCode
                });
                if (digibileBaseResponse2.success != "true")
                {
                    summery.TryAdd(userId, new
                    {
                        error = true,
                        value = digibileBaseResponse2.message
                    });
                }
                else if (test1 != null)
                {
                    test1.DigibileJson = JsonSerializer.Serialize(new DigibileTransferModel
                    {
                        CreatedDateTime = DateTime.Now.ToPersianDateTime(),
                        Creator = (currentUser?.FullName ?? "نامشخص"),
                        DigibileId = digibileBaseResponse2.result.user_id.ToString()
                    }, JsonSetting);
                    await Update(test1);
                    summery.TryAdd(userId, new
                    {
                        error = false,
                        value = test1.DigibileJson
                    });
                }
                else
                {
                    summery.TryAdd(userId, new
                    {
                        error = false,
                        value = JsonSerializer.Serialize(digibileBaseResponse2)
                    });
                    await Insert(new ThirdPartyTransfer
                    {
                        CustomerId = _currentUserService.CustomerId.Value,
                        EntityId = userId,
                        EntityType = "User",
                        EntityTag = user2.FullName + ", کد ملی (" + user2.NationalCode + ")",
                        DigibileJson = JsonSerializer.Serialize(new DigibileTransferModel
                        {
                            CreatedDateTime = DateTime.Now.ToPersianDateTime(),
                            Creator = (currentUser?.FullName ?? "نامشخص"),
                            DigibileId = digibileBaseResponse2.result.user_id.ToString()
                        }, JsonSetting)
                    });
                }
            }
            catch (Exception ex)
            {
                summery.TryAdd(userId, new
                {
                    error = true,
                    value = ex.Message
                });
            }
        }
        return summery;
    }

    public async Task<Dictionary<string, dynamic>> SendInvoice(List<string> ids, ThirdPart thirdParty)
    {
        JsonSerializerOptions JsonSetting = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        User currentUser = await _userManager.GetById(_currentUserService.UserId.Value);
        Dictionary<string, object> summery = new Dictionary<string, object>();
        List<UserLoan> userLoans = new List<UserLoan>();
        List<GetCustomerResponse> currentLandaCustomers = new List<GetCustomerResponse>();
        if ((ids?.Any() ?? false) && thirdParty == ThirdPart.Landa)
        {
            userLoans = await _userLoanService.GetByCondition((UserLoan x) => ids.Contains(((object)x.Id).ToString()), "UserLoanPayments", "Loan", "User");
            List<string> userIds = userLoans.Select((UserLoan x) => x.UserId.ToString()).ToList();
            if (await Table<ThirdPartyTransfer>().AnyAsync((ThirdPartyTransfer x) => userIds.Contains(x.EntityId) && string.IsNullOrEmpty(x.LandaJson)))
            {
                currentLandaCustomers = await _landaService.GetAllCustomers();
            }
        }
        foreach (string userLoanId in ids)
        {
            try
            {
                Guid.TryParse(userLoanId, out var uId);
                UserLoan userLoan = userLoans.Find((UserLoan x) => x.Id == uId);
                if (userLoan == null)
                {
                    summery.TryAdd(userLoanId, new
                    {
                        error = true,
                        value = "وام مشتری یافت نشد"
                    });
                    continue;
                }
                if (thirdParty != ThirdPart.Landa)
                {
                    goto IL_0b4f;
                }
                ThirdPartyTransfer test1 = (await GetThirdPartyTransfers(userLoanId, "UserLoan", null))?.FirstOrDefault();
                if (!string.IsNullOrEmpty(test1?.LandaJson))
                {
                    summery.TryAdd(userLoanId, new
                    {
                        error = true,
                        value = test1.LandaJson
                    });
                    continue;
                }
                Guid customerId = await PrepareCustomer(userLoan.UserId, thirdParty, currentLandaCustomers);
                List<InvoiceItem> invoiceItems = await PrepareInvoiceItems(userLoan, thirdParty, await PrepareProduct(userLoan, thirdParty));
                List<PaymentItem> paymentItems = await PreparePayments(userLoan.UserLoanPayments.ToList(), thirdParty);
                LandaResponse<CreateInvoiceResponse> landaResponse = await _landaService.CreateInvoice(new CreateInvoice
                {
                    customerId = customerId,
                    date = userLoan.StartDate.ToString("yyyy-MM-dd"),
                    docNo = userLoan.Code.ToString(),
                    invoiceItems = invoiceItems,
                    paymentItems = paymentItems,
                    typeId = Guid.Parse("b39ae284-f90f-42fe-b94a-b5f2ba9179cd"),
                    summary = $"ارسال شده از سیستم مدیریت اقساط, شماره وام: {userLoan.Code}, تعداد اقساط: {userLoan.Month}"
                });
                if (landaResponse.code != 1)
                {
                    summery.TryAdd(userLoanId, new
                    {
                        error = true,
                        value = landaResponse.message
                    });
                    continue;
                }
                if (test1 != null)
                {
                    test1.LandaJson = JsonSerializer.Serialize(landaResponse.data, JsonSetting);
                    await Update(test1);
                    summery.TryAdd(userLoanId, new
                    {
                        error = true,
                        value = test1.LandaJson
                    });
                }
                else
                {
                    summery.TryAdd(userLoanId, new
                    {
                        error = true,
                        value = JsonSerializer.Serialize(landaResponse.data)
                    });
                    await Insert(new ThirdPartyTransfer
                    {
                        CustomerId = _currentUserService.CustomerId.Value,
                        EntityId = userLoanId,
                        EntityType = "UserLoan",
                        EntityTag = string.Format("شماره وام: {0}, عنوان وام: [{1}], متعلق به: [{2}], مبلغ [{3}]", userLoan.Code, userLoan.Loan.Title, userLoan.User.FullName, userLoan.Amount.ToString("N0")),
                        LandaJson = JsonSerializer.Serialize(new LandaTransferModel
                        {
                            CreatedDateTime = DateTime.Now.ToPersianDateTime(),
                            Creator = (currentUser?.FullName ?? "نامشخص"),
                            LandaCode = landaResponse.data.no.ToString(),
                            LandaId = landaResponse.data.id
                        }, JsonSetting)
                    });
                }
                goto IL_0b4f;
            IL_0b4f:
                if (thirdParty == ThirdPart.Digibile)
                {
                    summery.TryAdd(userLoanId, new
                    {
                        error = true,
                        value = "ماژول ارتباطی وام سیتی پیاده سازی نشده است."
                    });
                }
                if (thirdParty == ThirdPart.Nikan)
                {
                    summery.TryAdd(userLoanId, new
                    {
                        error = true,
                        value = "ماژول ارتباطی نیکان پیاده سازی نشده است."
                    });
                }
            }
            catch (Exception ex)
            {
                summery.TryAdd(userLoanId, new
                {
                    error = true,
                    value = ex.Message
                });
            }
        }
        return summery;
    }

    public async Task<Dictionary<string, dynamic>> SendInvoicePayment(List<string> ids, ThirdPart thirdParty)
    {
        JsonSerializerOptions JsonSetting = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        User currentUser = await _userManager.GetById(_currentUserService.UserId.Value);
        Dictionary<string, object> summery = new Dictionary<string, object>();
        List<UserLoanPayment> userLoanPayments = new List<UserLoanPayment>();
        if ((ids?.Any() ?? false) && thirdParty == ThirdPart.Landa)
        {
            userLoanPayments = await _userPaymentService.GetByCondition((UserLoanPayment x) => ids.Contains(((object)x.Id).ToString()), "UserLoan.Loan");
        }
        foreach (string userLoanPaymentId in ids)
        {
            try
            {
                Guid.TryParse(userLoanPaymentId, out var uId);
                UserLoanPayment userLoanPayment = userLoanPayments.Find((UserLoanPayment x) => x.Id == uId);
                if (userLoanPayment == null)
                {
                    summery.TryAdd(userLoanPaymentId, new
                    {
                        error = true,
                        value = "قسط یافت نشد"
                    });
                    continue;
                }
                if (thirdParty != ThirdPart.Landa)
                {
                    goto IL_0814;
                }
                ThirdPartyTransfer test1 = (await GetThirdPartyTransfers(userLoanPaymentId, "UserLoanPayment", null))?.FirstOrDefault();
                if (!string.IsNullOrEmpty(test1?.LandaJson))
                {
                    summery.TryAdd(userLoanPaymentId, new
                    {
                        error = true,
                        value = test1.LandaJson
                    });
                    continue;
                }
                Guid invoiceId = await PrepareInvoice(userLoanPayment, thirdParty);
                List<PaymentItem> paymentItems = await PreparePayments(new List<UserLoanPayment> { userLoanPayment }, thirdParty);
                LandaResponse<CreateInvoicePaymentResponse> landaResponse = await _landaService.CreateInvoicePayment(new CreateInvoicePayment
                {
                    selectedInvoiceId = invoiceId,
                    date = userLoanPayment.SaveDate.ToString("yyyy-MM-dd"),
                    paymentItems = paymentItems,
                    summary = "ارسال شده از سیستم مدیریت اقساط"
                });
                if (landaResponse.code != 1)
                {
                    summery.TryAdd(userLoanPaymentId, new
                    {
                        error = true,
                        value = landaResponse.message
                    });
                    continue;
                }
                if (test1 != null)
                {
                    test1.LandaJson = JsonSerializer.Serialize(landaResponse.data, JsonSetting);
                    await Update(test1);
                    summery.TryAdd(userLoanPaymentId, new
                    {
                        error = false,
                        value = test1.LandaJson
                    });
                }
                else
                {
                    summery.TryAdd(userLoanPaymentId, new
                    {
                        error = false,
                        value = JsonSerializer.Serialize(landaResponse.data)
                    });
                    await Insert(new ThirdPartyTransfer
                    {
                        CustomerId = _currentUserService.CustomerId.Value,
                        EntityId = userLoanPaymentId,
                        EntityType = "UserLoanPayment",
                        EntityTag = string.Format("عنوان وام [{0} - شماره وام: ({1})], شماره قسط [{2}], مبلغ قسط [{3}], مبلغ واریز [{4}]", userLoanPayment.UserLoan.Loan.Title, userLoanPayment.UserLoan.Code, userLoanPayment.InstallmentNumber, userLoanPayment.InstallmentAmount.ToString("N0"), userLoanPayment.SumAmount.ToString("N0")),
                        LandaJson = JsonSerializer.Serialize(new LandaTransferModel
                        {
                            CreatedDateTime = DateTime.Now.ToPersianDateTime(),
                            Creator = (currentUser?.FullName ?? "نامشخص"),
                            LandaCode = landaResponse.data.id.ToString(),
                            LandaId = landaResponse.data.id
                        }, JsonSetting)
                    });
                }
                goto IL_0814;
            IL_0814:
                if (thirdParty == ThirdPart.Digibile)
                {
                    summery.TryAdd(userLoanPaymentId, new
                    {
                        error = true,
                        value = "ماژول ارتباطی وام سیتی پیاده سازی نشده است."
                    });
                }
                if (thirdParty == ThirdPart.Nikan)
                {
                    summery.TryAdd(userLoanPaymentId, new
                    {
                        error = true,
                        value = "ماژول ارتباطی نیکان پیاده سازی نشده است."
                    });
                }
            }
            catch (Exception ex)
            {
                summery.TryAdd(userLoanPaymentId, new
                {
                    error = true,
                    value = ex.Message
                });
            }
        }
        return summery;
    }

    public Task<ThirdPartyTransfer> GetById(Guid id, params string[] includs)
    {
        return GetById<ThirdPartyTransfer>(id, includs);
    }

    public Task Insert(ThirdPartyTransfer entity)
    {
        entity.CustomerId = _currentUserService.CustomerId.Value;
        Insert<ThirdPartyTransfer>(entity);
        return Save();
    }

    public Task Update(ThirdPartyTransfer entity)
    {
        entity.CustomerId = _currentUserService.CustomerId.Value;
        Update(entity, true);
        return Save();
    }

    public Task Delete(ThirdPartyTransfer entity)
    {
        Delete<ThirdPartyTransfer>(entity);
        return Save();
    }

    public async Task RegisterOrUpdateDigibileMember(string id, string password)
    {
        ThirdPartySettings thirdPartySettings = await _thirdPartySettingsService.GetThirdPartySettings();
        if (thirdPartySettings == null || string.IsNullOrEmpty(password))
        {
            return;
        }
        ThirdPartySettingsModel thirdPartySettingsModel = new ThirdPartySettingsModel(thirdPartySettings);
        if (thirdPartySettingsModel.DigibileSettings.IsActive)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary.Add(id, _encryptionService.DigibileSimpleEncrypt(password, "76hd"));
            if (thirdPartySettingsModel.DigibileSettings?.IsActive ?? false)
            {
                await SendMember(new List<string> { id }, ThirdPart.Digibile, null, dictionary);
            }
        }
    }

    private async Task<Guid> PrepareInvoice(UserLoanPayment userLoanPayment, ThirdPart thirdParty)
    {
        Guid? guid = Guid.Empty;
        switch (thirdParty)
        {
            case ThirdPart.Landa:
                guid = await GetLandaId(userLoanPayment.UserLoanId.ToString(), "UserLoan");
                if (!guid.HasValue || guid == Guid.Empty)
                {
                    throw new Exception("اطلاعات وام به عنوان فاکتور به لاندا ارسال نشده است.");
                }
                break;
        }
        return guid.Value;
    }

    private async Task<Guid> PrepareCustomer(long userId, ThirdPart thirdParty, List<GetCustomerResponse> currentLandaCustomers)
    {
        (await SendMember(new List<string> { userId.ToString() }, thirdParty, currentLandaCustomers)).TryGetValue(userId.ToString(), out dynamic value);
        if (value?.error != false)
        {
            throw new Exception(string.Format("خطا در اطلاعات مشتری: {0}", (object?)(value?.value ?? "نامشخص")));
        }
        dynamic val = JsonSerializer.Deserialize<LandaTransferModel>(value.value);
        return val.LandaId;
    }

    private async Task<string> PrepareProduct(UserLoan userLoan, ThirdPart thirdParty)
    {
        ThirdPartySettingsModel thirdPartySettingsModel = new ThirdPartySettingsModel(await _thirdPartySettingsService.GetThirdPartySettings());
        string code = default;
        if (thirdPartySettingsModel == null || thirdPartySettingsModel.LandaSettings?.ProductMap?.TryGetValue(userLoan.LoanId, out code) != true)
        {
            throw new Exception("کد کالای معادل این وام تعیین نشده است.");
        }
        return code;
    }

    private async Task<List<PaymentItem>> PreparePayments(List<UserLoanPayment> userLoanPayments, ThirdPart thirdParty)
    {
        ThirdPartySettingsModel thirdPartySettingsModel = new ThirdPartySettingsModel(await _thirdPartySettingsService.GetThirdPartySettings());
        List<PaymentItem> list = new List<PaymentItem>();
        foreach (UserLoanPayment item in userLoanPayments.Where((UserLoanPayment x) => x.SumAmount > 0m))
        {
            if (item.SumAmount == 0m)
            {
                throw new Exception("مبلغ پرداختی قسط نمی تواند صفر باشد.");
            }
            if ((item.PaymentMethod == PaymentMethods.Online || item.PaymentMethod == PaymentMethods.Bank) && (thirdPartySettingsModel == null || thirdPartySettingsModel.LandaSettings?.BankId.HasValue != true))
            {
                throw new Exception("شناسه بانک تعیین نشده است.");
            }
            if (item.PaymentMethod == PaymentMethods.Cash && (thirdPartySettingsModel == null || thirdPartySettingsModel.LandaSettings?.CashId.HasValue != true))
            {
                throw new Exception("شناسه صندوق تعیین نشده است.");
            }
            list.Add(new PaymentItem
            {
                amount = item.SumAmount * 10m,
                bankAccountId = ((item.PaymentMethod != PaymentMethods.Online && item.PaymentMethod != PaymentMethods.Bank) ? "" : thirdPartySettingsModel?.LandaSettings?.BankId.ToString()),
                cashId = ((item.PaymentMethod == PaymentMethods.Cash) ? thirdPartySettingsModel?.LandaSettings?.CashId.ToString() : ""),
                typeId = ((item.PaymentMethod == PaymentMethods.Cash) ? 1071 : 1073)
            });
        }
        return list;
    }

    private async Task<List<InvoiceItem>> PrepareInvoiceItems(UserLoan userLoan, ThirdPart thirdParty, string productCode)
    {
        new ThirdPartySettingsModel(await _thirdPartySettingsService.GetThirdPartySettings());
        return new List<InvoiceItem>
        {
            new InvoiceItem
            {
                price = userLoan.Amount * 10m,
                quantity = 1m,
                productCode = productCode
            }
        };
    }
}
