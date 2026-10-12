using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Core.Configuration;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Services.Users;
using LoanSystem.Site.Areas.Admin.Models;
using LoanSystem.Site.Areas.Agent.Controllers;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Services.Models.DynamicQueryModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LoanSystem.Site.Areas.Admin.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Admin,Agent")]
public class CustomerController : BaseAgentController
{
	private readonly IMemberService _memberService;

	private readonly ICurrentUserService _currentUserService;

	private readonly UserManager _userManager;

	private readonly IUserLoanService _userLoanService;

	private readonly ICustomerService _customerService;

	private readonly IAppSettingsService _appSettingsService;

	private readonly IAppSmsService _appSmsService;

	private readonly IntegratedSmsSenderService _integratedSmsSenderService;

	private readonly IntegratedEmailSenderService _integratedEmailSenderService;

	private readonly AppConfig _appConfig;

	public CustomerController(ILogger<MemberController> logger, IOptions<AppConfig> appConfig, IMemberService memberService, ICurrentUserService currentUserService, IUserLoanService userLoanService, UserManager userManager, ICustomerService customerService, IAppSettingsService appSettingsService, IAppSmsService appSmsService, IntegratedSmsSenderService integratedSmsSenderService, IntegratedEmailSenderService integratedEmailSenderService)
		: base(logger)
	{
		_memberService = memberService;
		_currentUserService = currentUserService;
		_appConfig = appConfig.Value;
		_userLoanService = userLoanService;
		_userManager = userManager;
		_customerService = customerService;
		_appSettingsService = appSettingsService;
		_appSmsService = appSmsService;
		_integratedSmsSenderService = integratedSmsSenderService;
		_integratedEmailSenderService = integratedEmailSenderService;
	}

	[HttpPost("[action]")]
	public Task<ResponseState<MemberListResultModel>> DynamicList([FromBody] DynamicQueryModel query)
	{
		return TryCatch(async () =>
		{
			List<UserLoan> userLoans = new List<UserLoan>();
			userLoans = await _userLoanService.GetAllUserLoans("UserLoanPayments");
			List<UserLoanPayment> userPayments = userLoans.SelectMany((UserLoan x) => x.UserLoanPayments).ToList();
			List<long> memberIdsFromUserLoans = (from x in userLoans
				where x.AgentId == _currentUserService.UserId
				select x.UserId).ToList();
			IQueryable<User> memberQuery = (await _memberService.AsQueryable()).Where((User x) => x.AgentId == _currentUserService.UserId || memberIdsFromUserLoans.Contains(x.Id));
			int countAll = await memberQuery.CountAsync();
			if (!string.IsNullOrWhiteSpace(query.Search))
			{
				memberQuery = memberQuery.Where((User x) => x.NationalCode.Contains(query.Search) || x.FirstName.Contains(query.Search) || x.LastName.Contains(query.Search) || x.Mobile.Contains(query.Search) || x.PhoneNumber.Contains(query.Search) || string.Concat(x.FirstName + " ", x.LastName).Contains(query.Search));
			}
			memberQuery = memberQuery.OrderByDescending((User x) => x.Id).Skip((query.Page - 1) * query.ItemsPerPage).Take(query.ItemsPerPage);
			List<Member3Model> members = (from x in await memberQuery.ToListAsync()
				select new Member3Model
				{
					Id = x.Id,
					UserName = x.UserName,
					FullName = x.FullName,
					FirstName = x.FirstName,
					LastName = x.LastName,
					Mobile = x.Mobile,
					NationalCode = x.NationalCode,
					TotalSavedMoney = x.TotalSavedMoney,
					RegDate2 = x.RegDate2,
					LoanCount = userLoans.Count((UserLoan ul) => ul.UserId == x.Id),
					TotalLoans = userLoans.Where((UserLoan ul) => ul.UserId == x.Id).Sum((UserLoan ul) => ul.PayableAmount),
					TotalRemain = userLoans.Where((UserLoan ul) => ul.UserId == x.Id).Sum((UserLoan ul) => ul.PayableAmount) - userPayments.Where((UserLoanPayment up) => up.UserLoan.UserId == x.Id).Sum((UserLoanPayment up) => up.SumAmount)
				} into x
				orderby x.Id descending
				select x).ToList();
			MemberListResultModel data = new MemberListResultModel
			{
				Members = members,
				CountAll = countAll,
				CountActives = 0
			};
			return Success(data);
		});
	}

	[HttpGet("[action]")]
	public Task<ResponseState<List<Member3Model>>> List()
	{
		return TryCatch(async () =>
		{
			List<UserLoan> userLoans = new List<UserLoan>();
			userLoans = await _userLoanService.GetAllUserLoans("UserLoanPayments");
			List<UserLoanPayment> userPayments = userLoans.SelectMany((UserLoan x) => x.UserLoanPayments).ToList();
			List<long> memberIdsFromUserLoans = (from x in userLoans
				where x.AgentId == _currentUserService.UserId
				select x.UserId).ToList();
			List<User> source = (await _memberService.GetAllMembers()).Where((User x) => x.AgentId == _currentUserService.UserId || memberIdsFromUserLoans.Contains(x.Id)).ToList();
			List<Member3Model> data = (from x in source
				select new Member3Model
				{
					Id = x.Id,
					UserName = x.UserName,
					FullName = x.FullName,
					FirstName = x.FirstName,
					LastName = x.LastName,
					Mobile = x.Mobile,
					NationalCode = x.NationalCode,
					TotalSavedMoney = x.TotalSavedMoney,
					RegDate2 = x.RegDate2,
					LoanCount = userLoans.Count((UserLoan ul) => ul.UserId == x.Id),
					TotalLoans = userLoans.Where((UserLoan ul) => ul.UserId == x.Id).Sum((UserLoan ul) => ul.PayableAmount),
					TotalRemain = userLoans.Where((UserLoan ul) => ul.UserId == x.Id).Sum((UserLoan ul) => ul.PayableAmount) - userPayments.Where((UserLoanPayment up) => up.UserLoan.UserId == x.Id).Sum((UserLoanPayment up) => up.SumAmount)
				} into x
				orderby x.Id descending
				select x).ToList();
			return Success(data);
		});
	}

	[HttpGet("[action]/{id}/{includeLoans?}")]
	public Task<ResponseState<MemberModel>> Get(long id, bool includeLoans)
	{
		return TryCatch(async () =>
		{
			MemberModel model = new MemberModel();
			if (id != 0L)
			{
				User user = await _memberService.GetById(id);
				if (user == null)
				{
					return Error<MemberModel>(null, "اطلاعات مورد نظر یافت نشد");
				}
				model.Id = user.Id;
				model.FirstName = user.FirstName;
				model.LastName = user.LastName;
				model.Gender = user.Gender;
				model.Credit = user.Credit;
				model.LastLoginDate = user.LastLoginDate;
				model.Mobile = user.Mobile;
				model.MobileVerified = user.MobileVerified;
				model.Phone = user.Phone;
				model.Address = user.Address;
				model.PostalCode = user.PostalCode;
				model.NationalCode = user.NationalCode;
				model.RegDate2 = user.RegDate2;
				model.BirthDate = user.BirthDate;
				model.Password = "@#$@#$xd2X#$%XXF%^e5";
				model.ConfirmPassword = "@#$@#$xd2X#$%XXF%^e5";
				model.Familiar1FirstName = user.Familiar1FirstName;
				model.Familiar1LastName = user.Familiar1LastName;
				model.Familiar1Mobile = user.Familiar1Mobile;
				model.Familiar2FirstName = user.Familiar2FirstName;
				model.Familiar2LastName = user.Familiar2LastName;
				model.Familiar2Mobile = user.Familiar2Mobile;
				model.FatherName = user.FatherName;
				model.UserName = user.UserName;
				model.City = user.City;
				model.BankAccountsJson = user.BankAccountsJson;
				model.AppSmsList = (await _appSmsService.GetByCondition((AppSms x) => x.EntityType == "User" && x.Tag == ((long)id).ToString(), false)).Select((AppSms x) => new AppSmsModel(x)).ToList();
				if (includeLoans)
				{
					List<UserLoan> source = await _userLoanService.GetByUserId(id, "UserLoanPayments");
					List<UserLoanPayment> source2 = source.SelectMany((UserLoan x) => x.UserLoanPayments).ToList();
					model.LoanCount = source.Count((UserLoan ul) => ul.UserId == id);
					model.TotalLoans = source.Where((UserLoan ul) => ul.UserId == id).Sum((UserLoan ul) => ul.Amount);
					model.TotalRemain = source.Where((UserLoan ul) => ul.UserId == id).Sum((UserLoan ul) => ul.PayableAmount) - source2.Where((UserLoanPayment up) => up.UserLoan.UserId == id).Sum((UserLoanPayment up) => up.SumAmount);
				}
			}
			return Success(model);
		});
	}

	[HttpGet("[action]/{nationalCode}")]
	public Task<ResponseState<MemberModel>> GetByNationalCode(string nationalCode)
	{
		return TryCatch(async () =>
		{
			MemberModel model = new MemberModel();
			User user = await _memberService.GetByNationalCode(nationalCode);
			if (user == null)
			{
				return Success(model);
			}
			model.Id = user.Id;
			model.FirstName = user.FirstName;
			model.LastName = user.LastName;
			return Success(model);
		});
	}

	[HttpGet("[action]/{data}")]
	public Task<ResponseState<List<Member4Model>>> GetForFactor(string data)
	{
		return TryCatch(async () =>
		{
			if (string.IsNullOrWhiteSpace(data))
			{
				return Error<List<Member4Model>>(null, "هیچ مقداری جهت جستجو وارد نشده است.");
			}
			new List<Member4Model>();
			List<Member4Model> data2 = (await _memberService.GetByCondition((User x) => x.Mobile == data || x.NationalCode == data || x.FirstName == data || x.LastName == data || string.Concat(x.FirstName + " ", x.LastName) == data || x.BankAccountsJson.Contains(data))).Select((User x) => new Member4Model(x)).ToList();
			return Success(data2);
		});
	}

	[HttpPost("[action]")]
	public Task<ResponseState<long>> Create([FromBody] MemberModel model)
	{
		return TryCatch(async () =>
		{
			Customer customer = await _customerService.GetById(_currentUserService.CustomerId.Value);
			model.UserName = GenerateUserName(customer.Code, model);
			if (await _userManager.FindByUserName(model.UserName) != null)
			{
				return Error(0L, "نام کاربری وارد شده تکراری است.");
			}
			User user = await _memberService.GetByNationalCode(model.NationalCode);
			if (user != null && user.CustomerId == customer.Id)
			{
				return Error(0L, "کد ملی وارد شده تکراری است.");
			}
			user = await _memberService.GetByMobile(model.Mobile);
			if (user != null && user.CustomerId == customer.Id)
			{
				return Error(0L, "شماره موبایل وارد شده تکراری است.");
			}
			AppSettings appSettings = await _appSettingsService.GetAppSettings();
			User currentUser = await _userManager.GetById(_currentUserService.UserId.Value);
			bool mobileVerified = false;
			if (currentUser?.SmsActiveForMembers ?? false)
			{
				int num;
				if (appSettings == null)
				{
					num = 0;
				}
				else
				{
					_ = appSettings.DefaultSmsPanel;
					num = 1;
				}
				if (num != 0 && appSettings.RegisterOtpSmsPatternId.HasValue)
				{
					if (string.IsNullOrEmpty(model.MobileOtp))
					{
						return Error(0L, "کد تاییدیه موبایل وارد نشده است.");
					}
					List<AppSms> list = await _appSmsService.GetByCondition((AppSms x) => x.To == model.Mobile && (int)x.AppSmsType == 8 && x.Date >= DateTime.Now.AddMinutes(-2.0) && x.Tag == model.MobileOtp, false);
					if (list == null || !list.Any())
					{
						return Error(0L, "کد تاییدیه موبایل معتبر نمی باشد.");
					}
					mobileVerified = true;
				}
			}
			User entity = new User
			{
				Id = model.Id,
				UserName = model.UserName,
				FirstName = model.FirstName,
				LastName = model.LastName,
				Address = model.Address,
				Gender = model.Gender,
				PostalCode = model.PostalCode,
				LastLoginDate = model.LastLoginDate,
				Mobile = model.Mobile,
				Phone = model.Phone,
				NationalCode = model.NationalCode,
				RegDate = DateTime.Now,
				RegDate2 = model.RegDate2,
				BirthDate = model.BirthDate,
				Familiar1FirstName = model.Familiar1FirstName,
				Familiar1LastName = model.Familiar1LastName,
				Familiar1Mobile = model.Familiar1Mobile,
				Familiar2FirstName = model.Familiar1FirstName,
				Familiar2LastName = model.Familiar2LastName,
				Familiar2Mobile = model.Familiar2Mobile,
				FatherName = model.FatherName,
				City = model.City,
				BankAccountsJson = model.BankAccountsJson,
				IsActive = true,
				AgentId = _currentUserService.UserId,
				MobileVerified = mobileVerified
			};
			entity.CustomerId = _currentUserService.CustomerId.Value;
			entity.UserRoleNames = new List<string> { "Member" };
			PasswordHasher<User> passwordHasher = new PasswordHasher<User>();
			string passwordHash = passwordHasher.HashPassword(entity, (!string.IsNullOrEmpty(model.Password)) ? model.Password : model.NationalCode);
			entity.PasswordHash = passwordHash;
			IdentityResult identityResult = await _userManager.Insert(entity, model.NationalCode, entity.UserRoleNames);
			if (identityResult.Succeeded)
			{
				if (model.SendSms)
				{
					await _integratedSmsSenderService.TrySendRegister(new LoginRegisterModel
					{
						Date = DateTime.Now,
						Email = entity.Email,
						Mobile = entity.Mobile,
						User_FullName = entity.FullName,
						User_Id = entity.Id,
						User_UserName = entity.UserName,
						Agent_BusinessName = currentUser.BusinessName
					}, _currentUserService.CustomerId.Value);
					await _integratedEmailSenderService.TrySendRegister(new LoginRegisterModel
					{
						Date = DateTime.Now,
						Email = entity.Email,
						Mobile = entity.Mobile,
						User_FullName = entity.FullName,
						User_Id = entity.Id,
						User_UserName = entity.UserName,
						Agent_BusinessName = currentUser.BusinessName
					}, _currentUserService.CustomerId.Value);
				}
				return Success(entity.Id, "عملیات با موفقیت انجام شد.");
			}
			return Error(500L, string.Join(".", identityResult.Errors.Select((IdentityError x) => x.Description)));
		});
	}

	[HttpPost("[action]")]
	public Task<ResponseState<long>> Edit([FromBody] MemberModel model)
	{
		return TryCatch(async () =>
		{
			Customer customer = await _customerService.GetById(_currentUserService.CustomerId.Value);
			model.UserName = GenerateUserName(customer.Code, model);
			User user = await _userManager.FindByUserName(model.UserName);
			if (user != null && user.Id != model.Id)
			{
				return Error(0L, "نام کاربری وارد شده تکراری است.");
			}
			user = await _memberService.GetByNationalCode(model.NationalCode);
			if (user != null && user.Id != model.Id && user.CustomerId == customer.Id)
			{
				return Error(0L, "کد ملی وارد شده تکراری است.");
			}
			User entity = await _memberService.GetById(model.Id);
			if (!entity.MobileVerified || entity.Mobile != model.Mobile)
			{
				User currentUser = await _userManager.GetById(_currentUserService.UserId.Value);
				AppSettings appSettings = await _appSettingsService.GetAppSettings();
				if ((currentUser?.SmsActiveForMembers ?? false) && appSettings != null)
				{
					_ = appSettings.DefaultSmsPanel;
					if (string.IsNullOrEmpty(model.MobileOtp))
					{
						return Error(0L, "کد تاییدیه موبایل وارد نشده است.");
					}
					List<AppSms> list = await _appSmsService.GetByCondition((AppSms x) => x.To == model.Mobile && (int)x.AppSmsType == 8 && x.Date >= DateTime.Now.AddMinutes(-2.0) && x.Tag == model.MobileOtp, false);
					if (list == null || !list.Any())
					{
						return Error(0L, "کد تاییدیه موبایل معتبر نمی باشد.");
					}
					entity.MobileVerified = true;
				}
			}
			entity.Id = model.Id;
			entity.UserName = model.UserName;
			entity.FirstName = model.FirstName;
			entity.LastName = model.LastName;
			entity.Gender = model.Gender;
			entity.LastLoginDate = model.LastLoginDate;
			entity.Mobile = model.Mobile;
			entity.Phone = model.Phone;
			entity.Address = model.Address;
			entity.PostalCode = model.PostalCode;
			entity.NationalCode = model.NationalCode;
			entity.RegDate2 = model.RegDate2;
			entity.BirthDate = model.BirthDate;
			entity.Familiar1FirstName = model.Familiar1FirstName;
			entity.Familiar1LastName = model.Familiar1LastName;
			entity.Familiar1Mobile = model.Familiar1Mobile;
			entity.Familiar2FirstName = model.Familiar2FirstName;
			entity.Familiar2LastName = model.Familiar2LastName;
			entity.Familiar2Mobile = model.Familiar2Mobile;
			entity.FatherName = model.FatherName;
			entity.City = model.City;
			entity.BankAccountsJson = model.BankAccountsJson;
			entity.CustomerId = _currentUserService.CustomerId.Value;
			if (model.Password != "@#$@#$xd2X#$%XXF%^e5")
			{
				PasswordHasher<User> passwordHasher = new PasswordHasher<User>();
				string passwordHash = passwordHasher.HashPassword(entity, model.Password);
				entity.PasswordHash = passwordHash;
			}
			await _userManager.UpdateAsync(entity);
			return Success(entity.Id, "عملیات با موفقیت انجام شد.");
		});
	}

	[HttpPost("[action]/{id}")]
	public Task<DefaultResponseState> Delete(long id)
	{
		return TryCatch(async () =>
		{
			if ((await _userLoanService.GetByUserId(id)).Any())
			{
				return Error("این کاربر دارای وام می باشد و قابل حذف  نمی باشد. برای حذف کاربر ابتدا وام های ثبت شده را حذف نمایید.");
			}
			User user = await _memberService.GetById(id);
			if (user == null)
			{
				return Error("امکان حذف وجود ندارد. لطفا بعدا دوباره تلاش نمایید");
			}
			await _memberService.Delete(user);
			return Success();
		});
	}

	private string GenerateUserName(int customerCode, MemberModel model)
	{
		if (_appConfig.IsCloud)
		{
			return $"{customerCode}-{model.NationalCode}";
		}
		return model.NationalCode;
	}
}
