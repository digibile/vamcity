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
[Authorize(Roles = "Admin")]
public class MemberController : BaseAdminController
{
	private readonly IMemberService _memberService;

	private readonly IAgentService _agentService;

	private readonly ICurrentUserService _currentUserService;

	private readonly UserManager _userManager;

	private readonly IUserLoanService _userLoanService;

	private readonly IUserPaymentService _userPaymentService;

	private readonly ICustomerService _customerService;

	private readonly ICustomerMarketerService _customerMarketerService;

	private readonly IThirdPartyTransferService _thirdPartyTransferService;

	private readonly IThirdPersonAssuranceAuthorizationItemService _thirdPersonAssuranceAuthorizationItemService;

	private readonly IAppSettingsService _appSettingsService;

	private readonly IAppSmsService _appSmsService;

	private readonly IntegratedSmsSenderService _integratedSmsSenderService;

	private readonly IntegratedEmailSenderService _integratedEmailSenderService;

	private readonly AppConfig _appConfig;

	public MemberController(ILogger<MemberController> logger, IOptions<AppConfig> appConfig, IMemberService memberService, IAgentService agentService, ICurrentUserService currentUserService, IUserLoanService userLoanService, IUserPaymentService userPaymentService, UserManager userManager, ICustomerService customerService, ICustomerMarketerService customerMarketerService, IThirdPartyTransferService thirdPartyTransferService, IThirdPersonAssuranceAuthorizationItemService thirdPersonAssuranceAuthorizationItemService, IAppSettingsService appSettingsService, IAppSmsService appSmsService, IntegratedSmsSenderService integratedSmsSenderService, IntegratedEmailSenderService integratedEmailSenderService)
		: base(logger)
	{
		_memberService = memberService;
		_agentService = agentService;
		_currentUserService = currentUserService;
		_appConfig = appConfig.Value;
		_userLoanService = userLoanService;
		_userPaymentService = userPaymentService;
		_userManager = userManager;
		_customerService = customerService;
		_customerMarketerService = customerMarketerService;
		_thirdPartyTransferService = thirdPartyTransferService;
		_thirdPersonAssuranceAuthorizationItemService = thirdPersonAssuranceAuthorizationItemService;
		_appSettingsService = appSettingsService;
		_appSmsService = appSmsService;
		_integratedSmsSenderService = integratedSmsSenderService;
		_integratedEmailSenderService = integratedEmailSenderService;
	}

	[HttpPost("[action]")]
	public Task<ResponseState<MemberListResultModel>> DynamicListLight([FromBody] DynamicQueryModel query)
	{
		return TryCatch(async () =>
		{
			IQueryable<User> memberQuery = await _memberService.AsQueryable();
			int countAll = await memberQuery.CountAsync();
			if (!string.IsNullOrWhiteSpace(query.Search))
			{
				memberQuery = memberQuery.Where((User x) => x.NationalCode.Contains(query.Search) || x.FirstName.Contains(query.Search) || x.LastName.Contains(query.Search) || x.Mobile.Contains(query.Search) || x.PhoneNumber.Contains(query.Search) || x.UserCode.Contains(query.Search) || string.Concat(x.FirstName + " ", x.LastName).Contains(query.Search));
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
					MobileVerified = x.MobileVerified
				} into x
				orderby x.Id descending
				select x).ToList();
			MemberListResultModel data = new MemberListResultModel
			{
				Members = members,
				CountAll = countAll
			};
			return Success(data);
		});
	}

	[HttpPost("[action]")]
	public Task<ResponseState<MemberListResultModel>> DynamicList([FromBody] DynamicQueryModel query)
	{
		return TryCatch(async () =>
		{
			int countActives = 0;
			List<User> agents = await _agentService.GetAllAgents();
			List<User> marketers = await _customerMarketerService.GetAllMarketers();
			IQueryable<User> memberQuery = await _memberService.AsQueryable();
			int countAll = await memberQuery.CountAsync();
			if (!query.All)
			{
				List<long> memberIdsWithActiveLoan = await (from x in _userLoanService.GetAllAsQueryable().AsNoTracking()
					where x.IsCheckouted == null || (x.IsCheckouted == false && x.User.CustomerId == _currentUserService.CustomerId)
					select x.UserId).ToListAsync();
				countActives = memberIdsWithActiveLoan.Distinct().Count();
				memberQuery = memberQuery.Where((User x) => memberIdsWithActiveLoan.Contains(x.Id));
			}
			if (!string.IsNullOrWhiteSpace(query.Search))
			{
				decimal installment = 0m;
				decimal.TryParse(query.Search, out installment);
				List<long> memberIds = (await _userLoanService.GetByCondition((UserLoan x) => x.Code.ToString() == query.Search || ((installment != 0m) ? (x.Installment == installment) : false))).Select((UserLoan x) => x.UserId).ToList();
				memberQuery = memberQuery.Where((User x) => x.NationalCode.Contains(query.Search) || x.FirstName.Contains(query.Search) || x.LastName.Contains(query.Search) || x.Mobile.Contains(query.Search) || x.PhoneNumber.Contains(query.Search) || x.UserCode.Contains(query.Search) || string.Concat(x.FirstName + " ", x.LastName).Contains(query.Search) || memberIds.Contains(x.Id));
			}
			else if (!string.IsNullOrWhiteSpace(query.UserLoanCode))
			{
				List<long> memberIds2 = (await _userLoanService.GetByCondition((UserLoan x) => x.Code.ToString() == query.UserLoanCode)).Select((UserLoan x) => x.UserId).ToList();
				memberQuery = memberQuery.Where((User x) => memberIds2.Contains(x.Id));
			}
			memberQuery = (from x in memberQuery.Include((User x) => x.Reliability)
				orderby x.Id descending
				select x).Skip((query.Page - 1) * query.ItemsPerPage).Take(query.ItemsPerPage);
			List<User> entities = await memberQuery.ToListAsync();
			List<long> entityIds = entities.Select((User x) => x.Id).ToList();
			List<UserLoanModel> userLoanModels = (await _userLoanService.GetByCondition((UserLoan x) => entityIds.Contains(x.UserId) && !x.IsCanceled, "UserLoanPayments.UserLoanPaymentOthers", "Loan")).Select((UserLoan x) => new UserLoanModel(x)
			{
				LoanPayments = x.UserLoanPayments.Select((UserLoanPayment ulp) => new LoanPaymentModel(ulp)).ToList()
			}).ToList();
			List<LoanPaymentModel> loanPayments = userLoanModels.SelectMany((UserLoanModel x) => x.LoanPayments).ToList();
			await _userPaymentService.SetPaymentsDelays(loanPayments);
			List<AppSms> appSms = await _appSmsService.GetByCondition((AppSms x) => x.EntityType == "User" && !string.IsNullOrEmpty(x.Tag) && entities.Select((User user) => user.Id).ToList().Contains(Convert.ToInt64(x.Tag)), false);
			List<Member3Model> members = (from member in entities
				select new Member3Model
				{
					Id = member.Id,
					UserCode = member.UserCode,
					UserName = member.UserName,
					FullName = member.FullName,
					FirstName = member.FirstName,
					LastName = member.LastName,
					Mobile = member.Mobile,
					MobileVerified = member.MobileVerified,
					NationalCode = member.NationalCode,
					TotalSavedMoney = member.TotalSavedMoney,
					RegDate2 = member.RegDate2,
					AgentName = agents.FirstOrDefault((User ag) => ag.Id == member.AgentId)?.FullName,
					AgentId = agents.FirstOrDefault((User ag) => ag.Id == member.AgentId)?.Id,
					MarketerName = marketers.FirstOrDefault((User mrk) => mrk.Id == member.CustomerMarketerId)?.BusinessName,
					LoanCount = userLoanModels.Count((UserLoanModel ul) => ul.MemberId == member.Id),
					TotalLoans = userLoanModels.Where((UserLoanModel ul) => ul.MemberId == member.Id).Sum((UserLoanModel ul) => ul.PayableAmount),
					TotalRemain = userLoanModels.Where((UserLoanModel ul) => ul.MemberId == member.Id).Sum((UserLoanModel ul) => ul.Remain),
					SpreadInstallments = string.Join(',', (from x in userLoanModels
						where x.MemberId == member.Id
						select x.Installment).ToList()),
					SpreadLoanCodes = string.Join(',', (from x in userLoanModels
						where x.MemberId == member.Id
						select x.Code).ToList()),
					AppSmsList = (from x in appSms
						where x.Tag == member.Id.ToString()
						select new AppSmsModel(x)).ToList(),
					Reliability = ((member.Reliability != null) ? new ReliabilityModel(member.Reliability) : null)
				} into x
				orderby x.Id descending
				select x).ToList();
			MemberListResultModel data = new MemberListResultModel
			{
				Members = members,
				CountAll = countAll,
				CountActives = countActives
			};
			return Success(data);
		});
	}

	[HttpGet("[action]/{all}")]
	public Task<ResponseState<MemberListResultModel>> List(bool all)
	{
		return TryCatch(async () =>
		{
			List<User> agents = await _agentService.GetAllAgents();
			List<User> marketers = await _customerMarketerService.GetAllMarketers();
			List<UserLoan> userLoans = new List<UserLoan>();
			userLoans = await _userLoanService.GetAllUserLoans("UserLoanPayments");
			List<UserLoanPayment> userPayments = userLoans.SelectMany((UserLoan x) => x.UserLoanPayments).ToList();
			List<User> list = await _memberService.GetAllMembers();
			int count = list.Count;
			DateTime dateTime = (from x in userPayments
				orderby x.TargetMonth descending
				select x.TargetMonth).FirstOrDefault();
			if (dateTime == DateTime.MinValue)
			{
				_ = DateTime.Now;
			}
			if (!all)
			{
				List<long> members = (from x in userLoans
					where !x.IsCheckouted.HasValue || !x.IsCheckouted.Value
					select x.UserId).ToList();
				list = list.Where((User x) => members.Contains(x.Id)).ToList();
			}
			List<Member3Model> members2 = (from x in list
				select new Member3Model
				{
					Id = x.Id,
					UserName = x.UserName,
					FullName = x.FullName,
					FirstName = x.FirstName,
					LastName = x.LastName,
					Mobile = x.Mobile,
					MobileVerified = x.MobileVerified,
					NationalCode = x.NationalCode,
					TotalSavedMoney = x.TotalSavedMoney,
					RegDate2 = x.RegDate2,
					AgentName = agents.FirstOrDefault((User ag) => ag.Id == x.AgentId)?.FullName,
					MarketerName = marketers.FirstOrDefault((User mrk) => mrk.Id == x.CustomerMarketerId)?.BusinessName,
					LoanCount = userLoans.Count((UserLoan ul) => ul.UserId == x.Id),
					TotalLoans = userLoans.Where((UserLoan ul) => ul.UserId == x.Id).Sum((UserLoan ul) => ul.PayableAmount),
					TotalRemain = userLoans.Where((UserLoan ul) => ul.UserId == x.Id).Sum((UserLoan ul) => ul.PayableAmount) - userPayments.Where((UserLoanPayment up) => up.UserLoan.UserId == x.Id).Sum((UserLoanPayment up) => up.SumAmount),
					SpreadInstallments = string.Join(',', (from userLoan in userLoans
						where userLoan.UserId == x.Id
						select userLoan.Installment).ToList()),
					SpreadLoanCodes = string.Join(',', (from userLoan in userLoans
						where userLoan.UserId == x.Id
						select userLoan.Code).ToList())
				} into x
				orderby x.Id descending
				select x).ToList();
			MemberListResultModel data = new MemberListResultModel
			{
				Members = members2,
				CountAll = count
			};
			return Success(data);
		});
	}

	[HttpGet("[action]/{id}")]
	public Task<ResponseState<List<Member3Model>>> ListByMarketerId(long id)
	{
		return TryCatch(async () =>
		{
			List<Member3Model> data = (from x in await _memberService.GetByCustomerMarketerId(id)
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
					RegDate2 = x.RegDate2
				} into x
				orderby x.Id descending
				select x).ToList();
			return Success(data);
		});
	}

	[HttpGet("[action]/{search}")]
	public Task<ResponseState<List<Member2Model>>> Search(string search)
	{
		return TryCatch(async () =>
		{
			List<Member2Model> data = (await _memberService.SearchMembers(search)).Select((User x) => new Member2Model
			{
				Id = x.Id,
				Mobile = x.Mobile,
				NationalCode = x.NationalCode,
				FullName = x.FullName
			}).ToList();
			return Success(data);
		});
	}

	[HttpGet("[action]/{data}")]
	public Task<ResponseState<List<SearchMemberByNationalCodeResultModel>>> Search2(string data)
	{
		return TryCatch(async () =>
		{
			if (string.IsNullOrWhiteSpace(data))
			{
				return Error<List<SearchMemberByNationalCodeResultModel>>(null, "هیچ مقداری جهت جستجو وارد نشده است.");
			}
			new List<SearchMemberByNationalCodeResultModel>();
			List<User> members = await _memberService.GetByCondition((User x) => x.Mobile.Contains(data) || x.NationalCode.Contains(data) || x.FirstName == data || x.LastName == data || string.Concat(x.FirstName + " ", x.LastName) == data || x.BankAccountsJson.Contains(data));
			List<User> list = members;
			if (list == null || !list.Any())
			{
				return Error<List<SearchMemberByNationalCodeResultModel>>(null, "مشتری یافت نشد");
			}
			members = members.Take(10).ToList();
			List<UserLoan> userLoans = await _userLoanService.GetByCondition((UserLoan x) => members.Select((User user) => user.Id).ToList().Contains(x.UserId), "Loan", "UserLoanPayments.UserLoanPaymentOthers");
			List<SearchMemberByNationalCodeResultModel> data2 = members.Select((User member) => new SearchMemberByNationalCodeResultModel
			{
				Member = new Member2Model(member),
				UserLoans = (from x in userLoans
					where x.UserId == member.Id
					select new UserLoanModel(x)
					{
						LoanPayments = x.UserLoanPayments.Select((UserLoanPayment p) => new LoanPaymentModel(p)).ToList()
					}).ToList()
			}).ToList();
			return Success(data2);
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
				User user = await _memberService.GetById(id, "Agent", "CustomerMarketer", "Reliability");
				if (user == null)
				{
					return Error<MemberModel>(null, "اطلاعات مورد نظر یافت نشد");
				}
				model = new MemberModel(user)
				{
					Password = null,
					ConfirmPassword = ""
				};
				if (includeLoans)
				{
					List<UserLoan> source = await _userLoanService.GetByUserId(id, "UserLoanPayments");
					List<UserLoanPayment> source2 = source.SelectMany((UserLoan x) => x.UserLoanPayments).ToList();
					model.LoanCount = source.Count((UserLoan ul) => ul.UserId == id);
					model.TotalLoans = source.Where((UserLoan ul) => ul.UserId == id).Sum((UserLoan ul) => ul.Amount);
					model.TotalRemain = source.Where((UserLoan ul) => ul.UserId == id).Sum((UserLoan ul) => ul.PayableAmount) - source2.Where((UserLoanPayment up) => up.UserLoan.UserId == id).Sum((UserLoanPayment up) => up.SumAmount);
				}
				model.AppSmsList = (await _appSmsService.GetByCondition((AppSms x) => x.EntityType == "User" && x.Tag == ((long)id).ToString(), false)).Select((AppSms x) => new AppSmsModel(x)).ToList();
				MemberModel memberModel = model;
				memberModel.ThirdPersonAssuranceCount = await _thirdPersonAssuranceAuthorizationItemService.GetCountByCondition((ThirdPersonAssuranceAuthorizationItem x) => x.UserId == (long?)id);
			}
			return Success(model);
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
			if (!string.IsNullOrWhiteSpace(model.UserCode))
			{
				user = await _memberService.GetByUserCode(model.UserCode);
				if (user != null && user.CustomerId == customer.Id)
				{
					return Error(0L, "کد مشتری وارد شده تکراری است.");
				}
			}
			AppSettings appSettings = await _appSettingsService.GetAppSettings();
			if (appSettings?.OtpRequiredInManualMemberRegistration ?? false)
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
			}
			User entity = new User
			{
				Id = model.Id,
				UserName = model.UserName,
				FirstName = model.FirstName,
				LastName = model.LastName,
				Address = model.Address,
				PostalCode = model.PostalCode,
				Gender = model.Gender,
				LastLoginDate = model.LastLoginDate,
				Mobile = model.Mobile,
				MobileVerified = (appSettings?.OtpRequiredInManualMemberRegistration ?? false),
				Email = model.Email,
				BirthDate = model.BirthDate,
				Phone = model.Phone,
				NationalCode = model.NationalCode,
				RegDate = DateTime.Now,
				RegDate2 = model.RegDate2,
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
				AgentId = model.AgentId,
				UserCode = model.UserCode
			};
			entity.CustomerId = _currentUserService.CustomerId.Value;
			entity.UserRoleNames = new List<string> { "Member" };
			PasswordHasher<User> passwordHasher = new PasswordHasher<User>();
			string passwordHash = passwordHasher.HashPassword(entity, (!string.IsNullOrEmpty(model.Password)) ? model.Password : model.NationalCode);
			entity.PasswordHash = passwordHash;
			IdentityResult identityResult = await _userManager.Insert(entity, (!string.IsNullOrEmpty(model.Password)) ? model.Password : model.NationalCode, entity.UserRoleNames);
			if (identityResult.Succeeded)
			{
				try
				{
					await _thirdPartyTransferService.RegisterOrUpdateDigibileMember(entity.Id.ToString(), model.Password ?? model.NationalCode);
				}
				catch
				{
				}
				if (model.SendSms)
				{
					string agent_businessName = "";
					if (model.AgentId.HasValue)
					{
						agent_businessName = (await _userManager.GetById(model.AgentId.Value))?.BusinessName;
					}
					await _integratedSmsSenderService.TrySendRegister(new LoginRegisterModel
					{
						Date = DateTime.Now,
						Email = entity.Email,
						Mobile = entity.Mobile,
						User_FullName = entity.FullName,
						User_Id = entity.Id,
						User_UserName = entity.UserName,
						Agent_BusinessName = agent_businessName
					}, _currentUserService.CustomerId.Value);
					await _integratedEmailSenderService.TrySendRegister(new LoginRegisterModel
					{
						Date = DateTime.Now,
						Email = entity.Email,
						Mobile = entity.Mobile,
						User_FullName = entity.FullName,
						User_Id = entity.Id,
						User_UserName = entity.UserName,
						Agent_BusinessName = agent_businessName
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
			if (!string.IsNullOrWhiteSpace(model.UserCode))
			{
				user = await _memberService.GetByUserCode(model.UserCode);
				if (user != null && user.Id != model.Id && user.CustomerId == customer.Id)
				{
					return Error(0L, "کد مشتری وارد شده تکراری است.");
				}
			}
			User entity = await _memberService.GetById(model.Id);
			if (!entity.MobileVerified || entity.Mobile != model.Mobile)
			{
				AppSettings appSettings = await _appSettingsService.GetAppSettings();
				if (appSettings != null && appSettings.OtpRequiredInManualMemberRegistration)
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
			entity.Email = model.Email;
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
			entity.CustomerId = _currentUserService.CustomerId.Value;
			entity.City = model.City;
			entity.BankAccountsJson = model.BankAccountsJson;
			entity.AgentId = model.AgentId;
			entity.UserCode = model.UserCode;
			if (!string.IsNullOrEmpty(model.Password))
			{
				PasswordHasher<User> passwordHasher = new PasswordHasher<User>();
				string passwordHash = passwordHasher.HashPassword(entity, model.Password);
				entity.PasswordHash = passwordHash;
			}
			await _userManager.UpdateAsync(entity);
			try
			{
				await _thirdPartyTransferService.RegisterOrUpdateDigibileMember(entity.Id.ToString(), model.Password);
			}
			catch
			{
			}
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

	[HttpPost("[action]")]
	public Task<DefaultResponseState> ManageWalletLock(long id, bool isLock)
	{
		return TryCatch(async () =>
		{
			User user = await _memberService.GetById(id);
			if (user == null)
			{
				return Error("مشتری یافت نشد.");
			}
			user.WalletIsLock = isLock;
			await _memberService.Update(user);
			return Success();
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
			model.NationalCode = user.NationalCode;
			return Success(model);
		});
	}

	[HttpPost("[action]")]
	public Task<ResponseState<long>> CreateAsThirdPerson([FromBody] MemberModel model)
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
			if (string.IsNullOrEmpty(model.MobileOtp))
			{
				return Error(0L, "کد تاییدیه موبایل وارد نشده است.");
			}
			List<AppSms> list = await _appSmsService.GetByCondition((AppSms x) => x.To == model.Mobile && (int)x.AppSmsType == 8 && x.Date >= DateTime.Now.AddMinutes(-2.0) && x.Tag == model.MobileOtp, false);
			if (list == null || !list.Any())
			{
				return Error(0L, "کد تاییدیه موبایل معتبر نمی باشد.");
			}
			User entity = new User
			{
				Id = model.Id,
				UserName = model.UserName,
				FirstName = model.FirstName,
				LastName = model.LastName,
				Address = model.Address,
				PostalCode = model.PostalCode,
				Gender = model.Gender,
				LastLoginDate = model.LastLoginDate,
				Mobile = model.Mobile,
				MobileVerified = true,
				Email = model.Email,
				BirthDate = model.BirthDate,
				Phone = model.Phone,
				NationalCode = model.NationalCode,
				RegDate = DateTime.Now,
				RegDate2 = model.RegDate2,
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
				AgentId = model.AgentId,
				UserCode = model.UserCode
			};
			entity.CustomerId = _currentUserService.CustomerId.Value;
			entity.UserRoleNames = new List<string> { "Member" };
			PasswordHasher<User> passwordHasher = new PasswordHasher<User>();
			string passwordHash = passwordHasher.HashPassword(entity, (!string.IsNullOrEmpty(model.Password)) ? model.Password : model.NationalCode);
			entity.PasswordHash = passwordHash;
			IdentityResult identityResult = await _userManager.Insert(entity, (!string.IsNullOrEmpty(model.Password)) ? model.Password : model.NationalCode, entity.UserRoleNames);
			return identityResult.Succeeded ? Success(entity.Id, "عملیات با موفقیت انجام شد.") : Error(500L, string.Join(".", identityResult.Errors.Select((IdentityError x) => x.Description)));
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
