using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Ganss.Xss;
using LoanSystem.Core;
using LoanSystem.Core.Exceptions;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Services.Users;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Data.Enums;
using LoanSystemAvizhe.Services.Facades;
using LoanSystemAvizhe.Services.Models.RefahBeta;
using LoanSystemAvizhe.Services.ThirdParty;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LoanSystem.Site.Areas.Agent.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Agent")]
public class UserLoanRequestController : BaseAgentController
{
	private readonly IAgentLoanService _agentLoanService;

	private readonly ILoanService _loanService;

	private readonly IUserLoanRequestService _userLoanRequestService;

	private readonly IUserLoanService _userLoanService;

	private readonly ITransactionService _transactionService;

	private readonly ISubAgentTransactionService _subAgentTransactionService;

	private readonly IAppContext _appContext;

	private readonly ICurrentUserService _currentUserService;

	private readonly IUserLoanRequestAuthorizationItemService _userLoanRequestAuthorizationItemService;

	private readonly ILoanRequestStatusService _loanRequestStatusService;

	private readonly UserManager _userManager;

	private readonly IUserLoanFacade _userLoanFacade;

	private readonly IAppSettingsService _appSettingsService;

	private readonly IServiceProvider _serviceProvider;

	private readonly IMemberAuthorizationItemService _memberAuthorizationItemService;

	private readonly IntegratedSmsSenderService _integratedSmsSenderService;

	private readonly IntegratedEmailSenderService _integratedEmailSenderService;

	public UserLoanRequestController(ILogger<UserLoanRequestController> logger, IUserLoanRequestService userLoanRequestService, IUserLoanService userLoanService, ILoanService loanService, IAppContext appContext, ICurrentUserService currentUserService, IAgentLoanService agentLoanService, ITransactionService transactionService, IUserLoanRequestAuthorizationItemService userLoanRequestAuthorizationItemService, ISubAgentTransactionService subAgentTransactionService, ILoanRequestStatusService loanRequestStatusService, IntegratedSmsSenderService integratedSmsSenderService, IntegratedEmailSenderService integratedEmailSenderService, UserManager userManager, IUserLoanFacade userLoanFacade, IAppSettingsService appSettingsService, IServiceProvider serviceProvider, IMemberAuthorizationItemService memberAuthorizationItemService)
		: base(logger)
	{
		_userLoanRequestService = userLoanRequestService;
		_appContext = appContext;
		_currentUserService = currentUserService;
		_loanService = loanService;
		_userLoanRequestAuthorizationItemService = userLoanRequestAuthorizationItemService;
		_agentLoanService = agentLoanService;
		_userLoanService = userLoanService;
		_transactionService = transactionService;
		_subAgentTransactionService = subAgentTransactionService;
		_loanRequestStatusService = loanRequestStatusService;
		_integratedSmsSenderService = integratedSmsSenderService;
		_integratedEmailSenderService = integratedEmailSenderService;
		_userManager = userManager;
		_userLoanFacade = userLoanFacade;
		_appSettingsService = appSettingsService;
		_serviceProvider = serviceProvider;
		_memberAuthorizationItemService = memberAuthorizationItemService;
	}

	[HttpGet("[action]/{id}")]
	public Task<ResponseState<UserLoanRequestModel>> Get(Guid id)
	{
		return TryCatch(async () =>
		{
			if (_appContext.User == null)
			{
				return Error<UserLoanRequestModel>(null, "کاربر یافت نشد");
			}
			if (id == Guid.Empty)
			{
				return Success(new UserLoanRequestModel());
			}
			UserLoanRequest userLoanRequest = await _userLoanRequestService.GetById(id, "Loan", "User", "Status", "AgentStatus");
			if (userLoanRequest == null)
			{
				return Error<UserLoanRequestModel>(null, "اطلاعات مورد نظر یافت نشد");
			}
			if (userLoanRequest.AgentId != _appContext.User.Id)
			{
				return Error<UserLoanRequestModel>(null, "دسترسی به این درخواست برای شما مجاز نمی باشد.");
			}
			UserLoanRequestModel data = new UserLoanRequestModel(userLoanRequest);
			return Success(data);
		});
	}

	[HttpGet("[action]")]
	public Task<ResponseState<List<UserLoanRequestModel>>> List(long? agentId, DateTime? start, DateTime? end)
	{
		return TryCatch(async () =>
		{
			if (_appContext.User == null && !agentId.HasValue)
			{
				return Error<List<UserLoanRequestModel>>(null, "کاربر یافت نشد");
			}
			if (!agentId.HasValue)
			{
				agentId = _appContext.User.Id;
			}
			List<UserLoanRequest> entities = await _userLoanRequestService.GetByCondition((UserLoanRequest x) => (((DateTime?)start).HasValue ? (x.RequestDate.Date >= ((DateTime?)start).Value.Date) : true) && (((DateTime?)end).HasValue ? (x.RequestDate.Date <= ((DateTime?)end).Value.Date) : true) && x.AgentId == agentId, false, "User", "Loan", "Status", "AgentStatus");
			List<UserLoan> userLoans = await _userLoanService.GetByCondition((UserLoan x) => x.UserLoanRequestId.HasValue && entities.Select((UserLoanRequest e) => e.Id).ToList().Contains((Guid)x.UserLoanRequestId), "Loan");
			List<Transaction> payments = await _transactionService.GetByCondition((Transaction x) => x.UserLoanId.HasValue && x.AgentId.HasValue && userLoans.Select((UserLoan ul) => ul.Id).ToList().Contains((Guid)x.UserLoanId));
			List<SubAgentTransaction> subAgentPayments = await _subAgentTransactionService.GetByCondition((SubAgentTransaction x) => x.UserLoanId.HasValue && userLoans.Select((UserLoan ul) => ul.Id).ToList().Contains((Guid)x.UserLoanId));
			List<UserLoanRequestModel> data = (from x in entities.Select((UserLoanRequest x) =>
				{
					UserLoanRequestModel ulr = new UserLoanRequestModel(x);
					ulr.UserLoan = (userLoans.Any((UserLoan ul) => ul.UserLoanRequestId == x.Id) ? new UserLoanModel(userLoans.FirstOrDefault((UserLoan ul) => ul.UserLoanRequestId == x.Id)) : null);
					if (ulr.UserLoan != null)
					{
						ulr.UserLoan.SumSponsorPayments = payments.Where((Transaction pay) => pay.UserLoanId == ulr.UserLoan.Id).Sum((Transaction p) => p.Amount);
						ulr.UserLoan.SumParentAgentPayments = subAgentPayments.Where((SubAgentTransaction pay) => pay.UserLoanId == ulr.UserLoan.Id).Sum((SubAgentTransaction p) => p.Amount);
					}
					return ulr;
				})
				orderby x.RequestDate descending
				select x).ToList();
			return Success(data);
		});
	}

	[HttpPost("[action]")]
	public Task<ResponseState<UserLoanRequestModel>> Create([FromBody] UserLoanRequestModel model)
	{
		foreach (KeyValuePair<string, ModelStateEntry> item in ModelState)
		{
			string key = item.Key;
			if (key.Contains("MemberModel") || key.Contains("LoanModel"))
			{
				ModelState[key].Errors.Clear();
			}
		}
		return TryCatch(async () =>
		{
			List<LoanRequestStatus> statuses = await _loanRequestStatusService.GetAllLoanRequestStatuses("SmsPattern");
			User user = await _userManager.GetById(model.UserId);
			Loan loan = await (from x in _loanService.GetAllLoansAsQueryable()
				where x.Id == model.LoanModel.Id
				select x).Include((Loan x) => x.AgentLoanAuthorizationItems).ThenInclude((AgentLoanAuthorizationItem x) => x.AuthorizationItem).FirstOrDefaultAsync();
			string text = await CreateUserLoanRequest_Validation(model, user, loan);
			if (!string.IsNullOrWhiteSpace(text))
			{
				return Error<UserLoanRequestModel>(null, text);
			}
			UserLoanRequest entity = await CreateUserLoanRequest_CreateEntity(user, model, loan, statuses);
			text = await CreateUserLoanRequest_RefahBeta(user, entity, model, loan, statuses);
			if (!string.IsNullOrWhiteSpace(text))
			{
				return Error<UserLoanRequestModel>(null, text);
			}
			await CreateUserLoanRequest_Sms(entity, statuses);
			UserLoanRequestModel data = new UserLoanRequestModel(entity)
			{
				Status = model.Status
			};
			return Success(data, "عملیات با موفقیت انجام شد.");
		});
	}

	[HttpPost("[action]")]
	public Task<ResponseState<UserLoanRequestModel>> Edit([FromBody] UserLoanRequestModel model)
	{
		return TryCatch(async () =>
		{
			UserLoanRequest entity = await _userLoanRequestService.GetById(model.Id, "Status", "AgentStatus");
			if (entity == null)
			{
				return Error<UserLoanRequestModel>(null, "اطلاعات مورد نظر یافت نشد.");
			}
			if (!entity.Status.Changable)
			{
				return Error<UserLoanRequestModel>(null, "در وضعیتی که درخواست قرار دارد اجازه ویرایش مجاز نمی باشد.");
			}
			if (entity.AgentId != _appContext.User.Id)
			{
				return Error<UserLoanRequestModel>(null, "دسترسی به این درخواست برای شما مجاز نمی باشد.");
			}
			if (entity.LoanId != model.LoanModel.Id && model.LoanModel.Disable)
			{
				return Error<UserLoanRequestModel>(null, (!string.IsNullOrEmpty(model.LoanModel.DisableText)) ? model.LoanModel.DisableText : "این نوع وام در حال حاضر غیرفعال می باشد.");
			}
			entity.Amount = model.Amount;
			entity.InstallmentCount = model.InstallmentCount;
			entity.MonthGap = model.MonthGap;
			entity.LoanId = model.LoanModel.Id;
			entity.Comment = model.Comment;
			entity.Installment = model.Installment;
			entity.Products = new HtmlSanitizer().Sanitize(model.Products);
			entity.ChargeType = model.ChargeType;
			await _userLoanRequestService.Update(entity);
			return Success(new UserLoanRequestModel(entity), "عملیات با موفقیت انجام شد.");
		});
	}

	[HttpPost("[action]")]
	public Task<ResponseState<LoanRequestStatus>> Submit([FromBody] UserLoanRequestModel model)
	{
		return TryCatch(async () =>
		{
			UserLoanRequest entity = await _userLoanRequestService.GetById(model.Id, "UserLoanRequestAuthorizationItems.AuthorizationItem", "Status");
			if (entity == null)
			{
				return Error<LoanRequestStatus>(null, "اطلاعات مورد نظر یافت نشد.");
			}
			if (!entity.Status.Changable)
			{
				return Error<LoanRequestStatus>(null, "در وضعیتی که درخواست قرار دارد اجازه ویرایش مجاز نمی باشد.");
			}
			if (entity.UserLoanRequestAuthorizationItems.Any((UserLoanRequestAuthorizationItem x) => x.AuthorizationItem.SystemType == AuthorizationItemSystemType.MemberAuthorization) && (await _memberAuthorizationItemService.SyncAndGetMemberAuthorizationsItems(entity.UserId)).Any((MemberAuthorizationItemModel x) => x.Status != LoanRequestAuthorizationStatus.تایید_شده_توسط_مدیر))
			{
				return Error<LoanRequestStatus>(null, "احراز هویت تکمیل نشده است.");
			}
			List<LoanRequestStatus> source = await _loanRequestStatusService.GetAllLoanRequestStatuses();
			new LoanRequestStatus();
			LoanRequestStatus newStatus = ((entity.Status.SystemRequestStatus != SystemRequestStatus.مدارک_ناقص_تکمیل_مدارک) ? source.First((LoanRequestStatus x) => x.SystemRequestStatus == SystemRequestStatus.در_انتظار_بررسی_کارشناس) : source.First((LoanRequestStatus x) => x.SystemRequestStatus == SystemRequestStatus.بررسی_مجدد_کارشناس));
			entity.StatusId = newStatus.Id;
			LoanRequestStatus loanRequestStatus = source.First((LoanRequestStatus x) => x.SystemAgentRequestStatus == SystemAgentRequestStatus.در_انتظار_بررسی);
			entity.StatusId = newStatus.Id;
			entity.AgentStatusId = loanRequestStatus.Id;
			await _userLoanRequestService.Update(entity);
			foreach (UserLoanRequestAuthorizationItem item in entity.UserLoanRequestAuthorizationItems.Where((UserLoanRequestAuthorizationItem x) => x.Status == LoanRequestAuthorizationStatus.ثبت_شده_توسط_کاربر))
			{
				item.Status = LoanRequestAuthorizationStatus.در_انتطار_بررسی_کارشناس;
				await _userLoanRequestAuthorizationItemService.Update(item);
			}
			return Success(newStatus, "عملیات با موفقیت انجام شد.");
		});
	}

	[HttpPost("[action]/{id}")]
	public Task<DefaultResponseState> Delete(Guid id)
	{
		return TryCatch(async () =>
		{
			UserLoanRequest userLoanRequest = await _userLoanRequestService.GetById(id, "Status");
			if (!userLoanRequest.Status.Changable)
			{
				return Error("در وضعیتی که درخواست قرار دارد اجازه حذف مجاز نمی باشد.");
			}
			if (userLoanRequest == null)
			{
				return Error("امکان حذف وجود ندارد. لطفا بعدا دوباره تلاش نمایید");
			}
			await _userLoanRequestService.Delete(userLoanRequest);
			return Success();
		});
	}

	[HttpGet("[action]")]
	public Task<ResponseState<List<LoanModel>>> LoansList()
	{
		return TryCatch(async () =>
		{
			List<AgentLoan> agentLoans = await _agentLoanService.GetByCondition((AgentLoan x) => (long?)x.AgentId == _currentUserService.UserId);
			List<LoanModel> data = (await (from x in _loanService.GetAllLoansAsQueryable().Include((Loan x) => x.AgentLoanAuthorizationItems).ThenInclude((AgentLoanAuthorizationItem x) => x.AuthorizationItem)
				where agentLoans.Select((AgentLoan agentLoan) => agentLoan.LoanId).Contains(x.Id)
				select x).ToListAsync()).Select((Loan x) => new LoanModel(x)).ToList();
			return Success(data);
		});
	}

	private async Task<UserLoanRequest> CreateUserLoanRequest_CreateEntity(User user, UserLoanRequestModel model, Loan loan, List<LoanRequestStatus> statuses)
	{
		UserLoanRequest entity = new UserLoanRequest
		{
			Amount = model.Amount,
			UserId = model.UserId,
			AgentId = _appContext.User.Id,
			LoanId = model.LoanModel.Id,
			Comment = model.Comment,
			RequestDate = DateTime.Now,
			StatusId = (statuses.First((LoanRequestStatus x) => x.SystemRequestStatus == SystemRequestStatus.ایجاد_اولیه)?.Id ?? statuses.First().Id),
			AgentStatusId = (statuses.First((LoanRequestStatus x) => x.SystemAgentRequestStatus == SystemAgentRequestStatus.در_انتظار_بررسی)?.Id ?? statuses.First().Id),
			MonthGap = model.MonthGap,
			InstallmentCount = model.InstallmentCount,
			Installment = model.Installment,
			ChargeType = model.ChargeType,
			OtherPercent_CheckoutDays = (model.OtherPercent?.CheckoutDays ?? 0),
			OtherPercent_Percent = (model.OtherPercent?.Percent ?? 0m),
			Products = new HtmlSanitizer().Sanitize(model.Products)
		};
		LoanRequestStatus status = statuses.FirstOrDefault((LoanRequestStatus x) => x.SystemRequestStatus == SystemRequestStatus.تکمیل_مدارک) ?? statuses.First((LoanRequestStatus x) => x.StepCategory == LoanRequestStatusCategorys.تکمیل_مدارک);
		if (loan.AgentLoanAuthorizationItems?.Any() ?? false)
		{
			List<AuthorizationItemSystemType> systemTypes = (from x in loan.AgentLoanAuthorizationItems
				where x.AuthorizationItem.SystemType != AuthorizationItemSystemType.None
				select x.AuthorizationItem.SystemType).ToList();
			List<UserLoanRequestAuthorizationItem> source = new List<UserLoanRequestAuthorizationItem>();
			if (systemTypes?.Any() ?? false)
			{
				source = await _userLoanRequestAuthorizationItemService.GetByCondition((UserLoanRequestAuthorizationItem x) => systemTypes.Contains(x.AuthorizationItem.SystemType) && (int)x.Status == 20 && x.UserLoanRequest.UserId == model.UserId, "UserLoanRequest", "AuthorizationItem");
			}
			entity.UserLoanRequestAuthorizationItems = new List<UserLoanRequestAuthorizationItem>();
			foreach (AgentLoanAuthorizationItem loanAuthorizeItem in loan.AgentLoanAuthorizationItems)
			{
				UserLoanRequestAuthorizationItem userLoanRequestAuthorizationItem = new UserLoanRequestAuthorizationItem
				{
					CreateDate = DateTime.Now,
					AuthorizationItemId = loanAuthorizeItem.AuthorizationItemId,
					Status = LoanRequestAuthorizationStatus.در_انتظار_ثبت_توسط_کاربر,
					UserLoanRequestId = entity.Id,
					CustomerId = _currentUserService.CustomerId.Value
				};
				UserLoanRequestAuthorizationItem userLoanRequestAuthorizationItem2 = source.FirstOrDefault((UserLoanRequestAuthorizationItem x) => x.AuthorizationItem.SystemType == loanAuthorizeItem.AuthorizationItem.SystemType);
				if (userLoanRequestAuthorizationItem2 != null)
				{
					userLoanRequestAuthorizationItem.Status = LoanRequestAuthorizationStatus.تایید_شده_توسط_مدیر;
					userLoanRequestAuthorizationItem.Attachment = userLoanRequestAuthorizationItem2.Attachment;
					userLoanRequestAuthorizationItem.MemberDescription = userLoanRequestAuthorizationItem2.MemberDescription;
				}
				entity.UserLoanRequestAuthorizationItems.Add(userLoanRequestAuthorizationItem);
			}
		}
		else
		{
			status = statuses.FirstOrDefault((LoanRequestStatus x) => x.SystemRequestStatus == SystemRequestStatus.در_انتظار_بررسی_کارشناس) ?? status;
		}
		model.Status = new LoanRequestStatusModel(status);
		entity.StatusId = status.Id;
		await _userLoanRequestService.Insert(entity);
		return entity;
	}

	private async Task<string> CreateUserLoanRequest_Validation(UserLoanRequestModel model, User user, Loan loan)
	{
		if (_appContext.User == null)
		{
			return "کاربر یافت نشد";
		}
		List<UserLoanRequest> list = await _userLoanRequestService.GetByCondition((UserLoanRequest x) => (int?)x.Status.SystemRequestStatus == (int?)15 && x.UserId == model.UserId && x.LoanId == model.LoanModel.Id, false);
		if (list != null && list.Any())
		{
			return "مشتری قبلا دارای درخواست وام با 'وضعیت انتظار بررسی کارشناس' برای این نوع وام می باشد.";
		}
		if (!_userManager.CheckUserProfileCompletion(user))
		{
			return "Profile Completion Error";
		}
		if (loan.Disable)
		{
			return (!string.IsNullOrEmpty(loan.DisableText)) ? loan.DisableText : "این نوع وام در حال حاضر غیرفعال می باشد.";
		}
		if (loan.AgentLoanAuthorizationItems.Any((AgentLoanAuthorizationItem x) => x.AuthorizationItem.SystemType == AuthorizationItemSystemType.RefahBeta))
		{
			AppSettings appSettings = await _appSettingsService.GetAppSettings();
			RefahBetaConfig refahBetaConfig = ((!string.IsNullOrEmpty(appSettings.RefahBetaConfigJson)) ? JsonSerializer.Deserialize<RefahBetaConfig>(appSettings.RefahBetaConfigJson) : null);
			if (refahBetaConfig == null)
			{
				return "در حال حاضر امکان استعلام توان بازپرداخت از بانک رفاه وجود ندارد.";
			}
			if (model.RefahBetaInquiryAmount <= 0)
			{
				return "استعلام توان اقساط شما معتبر نمی باشد.";
			}
			double num = (double)(model.RefahBetaInquiryAmount * model.InstallmentCount * model.MonthGap) / (1.0 + (double)(model.InstallmentCount * model.MonthGap) * loan.ProfitPercent / 100.0);
			if (model.Amount > (decimal)num)
			{
				return "مبلغ درخواستی از حداکثر توان خرید شما بیشتر است.";
			}
		}
		return string.Empty;
	}

	private async Task<string> CreateUserLoanRequest_RefahBeta(User user, UserLoanRequest entity, UserLoanRequestModel model, Loan loan, List<LoanRequestStatus> statuses)
	{
		if (loan.AgentLoanAuthorizationItems.Any((AgentLoanAuthorizationItem x) => x.AuthorizationItem.SystemType == AuthorizationItemSystemType.RefahBeta))
		{
			try
			{
				AppSettings appSettings = await _appSettingsService.GetAppSettings();
				RefahBetaConfig refahBetaConfig = ((!string.IsNullOrEmpty(appSettings.RefahBetaConfigJson)) ? JsonSerializer.Deserialize<RefahBetaConfig>(appSettings.RefahBetaConfigJson) : null);
				ILoggerFactory requiredService = _serviceProvider.GetRequiredService<ILoggerFactory>();
				ILogger<RefahBetaService> logger = requiredService.CreateLogger<RefahBetaService>();
				RefahBetaService refahBetaService = new RefahBetaService(refahBetaConfig, logger);
				DateTime startDate = ((DateTime.Now.ToPersianDateObject().Day <= 15) ? DateTimeExtentions.GetStartEndOfMonthInPersian(DateTime.Now).Item2.ToGregorianDateTime().Value : DateTimeExtentions.GetEndOfNextPersianMonth(DateTime.Now));
				RefahBetaResult<long> refahBetaResult = await refahBetaService.ConsumeRequest(new ConsumeRequest
				{
					Title = "خرید اقساطی کالا خدمات " + entity.Products?.Replace('\r', ' ')?.Replace("\n", ""),
					StartDate = startDate,
					Amount = entity.Installment * (decimal)entity.InstallmentCount * 10m,
					NationalCode = user.NationalCode,
					NumberOfInstallments = entity.InstallmentCount,
					Otp = model.RefahOtp,
					RequestId = entity.Id,
					StartDateYearMonth = startDate.ToString("yyyyMM")
				});
				if (refahBetaResult.Status != 200)
				{
					throw new CustomException($"خطای صادره از سامانه رفاه. کد خطا: [{refahBetaResult.Status}] شرح: " + refahBetaResult.Message);
				}
				entity.RefahBetaOrderId = refahBetaResult.Data;
				entity.RefahBetaInquiryAmount = model.RefahBetaInquiryAmount;
				List<Guid> refahAuthorizationItemIds = (from x in loan.AgentLoanAuthorizationItems
					where x.AuthorizationItem.SystemType == AuthorizationItemSystemType.RefahBeta
					select x.AuthorizationItemId).ToList();
				List<UserLoanRequestAuthorizationItem> list = entity.UserLoanRequestAuthorizationItems.Where((UserLoanRequestAuthorizationItem x) => refahAuthorizationItemIds.Contains(x.AuthorizationItemId)).ToList();
				foreach (UserLoanRequestAuthorizationItem item in list)
				{
					item.MemberDescription = $"استعلام توان: {entity.RefahBetaInquiryAmount}, شماره سفارش در بانک رفاه: {entity.RefahBetaOrderId}";
					item.Status = LoanRequestAuthorizationStatus.تایید_شده_توسط_مدیر;
				}
				model.Status = new LoanRequestStatusModel(statuses.FirstOrDefault((LoanRequestStatus x) => x.SystemRequestStatus == SystemRequestStatus.رفاه_بتا_ثبت_شد_انتظار_بررسی_کارشناس));
				entity.StatusId = model.Status?.Id;
				model.AgentModel = new AgentModel(_appContext.User);
				model.AdminDescription = $"استعلام توان: {entity.RefahBetaInquiryAmount}, شماره سفارش در بانک رفاه: {entity.RefahBetaOrderId}";
				await _userLoanRequestService.Update(entity);
				model.Id = entity.Id;
				model.UserLoan = new UserLoanModel
				{
					AgentDueDate = DateTime.Now,
					DepositChargeDueDate = DateTime.Now,
					StartDate = DateTime.Now
				};
				model.MemberModel = new Member2Model(user);
				FacadeResponse<Guid> facadeResponse = await _userLoanFacade.CreateByUserLoanRequest(model);
				if (!facadeResponse.Success)
				{
					throw new CustomException(facadeResponse.Message);
				}
			}
			catch (CustomException ex)
			{
				await _userLoanRequestService.Delete(entity);
				return ex.Message;
			}
			catch (Exception ex2)
			{
				_ = ex2;
				await _userLoanRequestService.Delete(entity);
				throw;
			}
		}
		return string.Empty;
	}

	private async Task CreateUserLoanRequest_Sms(UserLoanRequest entity, List<LoanRequestStatus> statuses)
	{
		entity.User = await _userManager.GetById(entity.UserId);
		UserLoanRequestModel userLoanRequestModel = new UserLoanRequestModel(entity);
		try
		{
			userLoanRequestModel.Status = new LoanRequestStatusModel(statuses.FirstOrDefault((LoanRequestStatus x) => x.SystemRequestStatus == SystemRequestStatus.ایجاد_اولیه));
			if (userLoanRequestModel.Status.SmsPattern != null)
			{
				try
				{
					await _integratedSmsSenderService.SendChangeLoanRequestStatus(userLoanRequestModel);
				}
				catch (Exception)
				{
				}
				try
				{
					await _integratedEmailSenderService.SendChangeLoanRequestStatus(userLoanRequestModel);
				}
				catch (Exception)
				{
				}
			}
			userLoanRequestModel.AgentStatus = new LoanRequestStatusModel(statuses.FirstOrDefault((LoanRequestStatus x) => x.SystemAgentRequestStatus == SystemAgentRequestStatus.در_انتظار_بررسی));
			if (userLoanRequestModel.AgentStatus.SmsPattern != null)
			{
				try
				{
					await _integratedSmsSenderService.SendChangeLoanRequestAgentStatus(userLoanRequestModel);
				}
				catch (Exception)
				{
				}
				try
				{
					await _integratedEmailSenderService.SendChangeLoanRequestAgentStatus(userLoanRequestModel);
					return;
				}
				catch (Exception)
				{
					return;
				}
			}
		}
		catch (Exception)
		{
		}
	}
}
