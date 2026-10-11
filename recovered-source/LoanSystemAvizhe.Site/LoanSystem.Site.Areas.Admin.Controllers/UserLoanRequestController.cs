using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LoanSystem.Core.Configuration;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Services.Users;
using LoanSystem.Site.Areas.Admin.Models;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Data.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LoanSystem.Site.Areas.Admin.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Admin")]
public class UserLoanRequestController : BaseAdminController
{
	private readonly IUserLoanRequestService _userLoanRequestService;

	private readonly ILoanRequestStatusService _loanRequestStatusService;

	private readonly IUserLoanService _userLoanService;

	private readonly ISubAgentTransactionService _subAgentTransactionService;

	private readonly ITransactionService _transactionService;

	private readonly IAppSettingsService _appSettingsService;

	private readonly IntegratedSmsSenderService _integratedSmsSenderService;

	private readonly ICurrentUserService _currentUserService;

	private readonly IAgentLoanService _agentLoanService;

	private readonly IUserLoanRequestSponsorHistoryService _userLoanRequestSponsorHistoryService;

	private readonly UserManager _userManager;

	private readonly AppConfig _appConfig;

	public UserLoanRequestController(ILogger<UserLoanRequestController> logger, IUserLoanRequestService userLoanRequestService, ILoanRequestStatusService loanRequestStatusService, IUserLoanService userLoanService, ITransactionService transactionService, IAppSettingsService appSettingsService, ISubAgentTransactionService subAgentTransactionService, ICurrentUserService currentUserService, IAgentLoanService agentLoanService, IUserLoanRequestSponsorHistoryService userLoanRequestSponsorHistoryService, UserManager userManager, IntegratedSmsSenderService integratedSmsSenderService, IOptions<AppConfig> appConfig)
		: base(logger)
	{
		_userLoanRequestService = userLoanRequestService;
		_loanRequestStatusService = loanRequestStatusService;
		_userLoanService = userLoanService;
		_transactionService = transactionService;
		_appSettingsService = appSettingsService;
		_subAgentTransactionService = subAgentTransactionService;
		_integratedSmsSenderService = integratedSmsSenderService;
		_currentUserService = currentUserService;
		_agentLoanService = agentLoanService;
		_userLoanRequestSponsorHistoryService = userLoanRequestSponsorHistoryService;
		_userManager = userManager;
		_appConfig = appConfig.Value;
	}

	[HttpGet("[action]")]
	public Task<ResponseState<List<UserLoanRequestModel>>> GetNewRequests(Guid statusId)
	{
		return TryCatch(async () =>
		{
			List<UserLoanRequestModel> data = (from x in await _userLoanRequestService.GetAllUserLoanRequests("User", "Loan")
				where x.StatusId == statusId
				select new UserLoanRequestModel(x)).ToList();
			return Success(data);
		});
	}

	[HttpGet("[action]")]
	public Task<ResponseState<List<UserLoanRequestModel>>> List(DateTime? start, DateTime? end, long? memberId)
	{
		return TryCatch(async () =>
		{
			List<UserLoanRequestModel> data = (from x in await _userLoanRequestService.GetByCondition((UserLoanRequest x) => (((DateTime?)start).HasValue ? (x.RequestDate.Date >= ((DateTime?)start).Value.Date) : true) && (((DateTime?)end).HasValue ? (x.RequestDate.Date <= ((DateTime?)end).Value.Date) : true) && (((long?)memberId).HasValue ? ((long?)x.UserId == memberId) : true), false, "User.CustomerMarketer", "Loan", "Agent", "Sponsor", "Status", "AgentStatus")
				select new UserLoanRequestModel(x) into x
				orderby x.RequestDate descending
				select x).ToList();
			return Success(data);
		});
	}

	[HttpGet("[action]")]
	public Task<ResponseState<List<LoanRequestStatusModel>>> StatusList()
	{
		return TryCatch(async () =>
		{
			List<LoanRequestStatusModel> data = (await _loanRequestStatusService.GetAllLoanRequestStatuses()).Select((LoanRequestStatus x) => new LoanRequestStatusModel(x)).ToList();
			return Success(data);
		});
	}

	[HttpGet("[action]/{agentId}/{start?}/{end?}")]
	public Task<ResponseState<List<UserLoanRequestModel>>> ListByAgentId(long agentId, DateTime? start, DateTime? end)
	{
		return TryCatch(async () =>
		{
			if (agentId == 0L)
			{
				return Error<List<UserLoanRequestModel>>(null, "کاربر یافت نشد");
			}
			List<UserLoanRequest> entities = await _userLoanRequestService.GetByCondition((UserLoanRequest x) => (((DateTime?)start).HasValue ? (x.RequestDate.Date >= ((DateTime?)start).Value.Date) : true) && (((DateTime?)end).HasValue ? (x.RequestDate.Date <= ((DateTime?)end).Value.Date) : true) && x.AgentId == (long?)agentId, false, "User", "Loan", "Status", "AgentStatus");
			List<UserLoan> userLoans = await _userLoanService.GetByCondition((UserLoan x) => x.UserLoanRequestId.HasValue && entities.Select((UserLoanRequest e) => e.Id).ToList().Contains((Guid)x.UserLoanRequestId), "Loan", "Agent");
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

	[HttpGet("[action]/{id}")]
	public Task<ResponseState<UserLoanRequestModel>> Get(Guid id)
	{
		return TryCatch(async () =>
		{
			if (id == Guid.Empty)
			{
				return Success(new UserLoanRequestModel());
			}
			UserLoanRequest entity = await _userLoanRequestService.GetById(id, "User.Reliability", "Loan", "Status", "AgentStatus", "Agent.AgentLoans", "Agent.Agent", "Sponsor");
			if (entity == null)
			{
				return Error<UserLoanRequestModel>(null, "اطلاعات مورد نظر یافت نشد");
			}
			UserLoanRequestModel model = new UserLoanRequestModel(entity);
			List<UserLoan> list = await _userLoanService.GetByUserLoanRequestId(id);
			if (list != null && list.Any())
			{
				model.UserLoan = new UserLoanModel(list.First());
			}
			if (string.IsNullOrEmpty(entity.LockedBy))
			{
				User user = await _userManager.GetById(_currentUserService.UserId.Value);
				LockedByModel value = new LockedByModel
				{
					Date = DateTime.Now,
					UserId = _currentUserService.UserId.Value,
					User_FullName = user.FullName
				};
				entity.LockedBy = JsonSerializer.Serialize(value);
				await _userLoanRequestService.Update(entity);
			}
			if (_appConfig.ActiveParts.Sponsor)
			{
				Loan2Model loanModel = model.LoanModel;
				loanModel.AssignedToSponsor = await _agentLoanService.CheckLoanAssignedToSponsor(model.LoanModel.Id);
				UserLoanRequestModel userLoanRequestModel = model;
				userLoanRequestModel.SponsorHistoryCount = await _userLoanRequestSponsorHistoryService.GetHistoryCount(model.Id);
			}
			return Success(model);
		});
	}

	[HttpPost("[action]")]
	public Task<ResponseState<Guid>> Edit([FromBody] UserLoanRequestModel model)
	{
		return TryCatch(async () =>
		{
			UserLoanRequest entity = await _userLoanRequestService.GetById(model.Id, "Status", "AgentStatus");
			if (entity == null)
			{
				return Error(Guid.Empty, "اطلاعات مورد نظر یافت نشد.");
			}
			Guid? guid = entity.Status?.Id;
			Guid? guid2 = model.Status?.Id;
			if (guid.HasValue == guid2.HasValue && guid.HasValue)
			{
				_ = guid.GetValueOrDefault() != guid2.GetValueOrDefault();
			}
			guid2 = entity.AgentStatus?.Id;
			guid = model.AgentStatus?.Id;
			if (guid2.HasValue == guid.HasValue && guid2.HasValue)
			{
				_ = guid2.GetValueOrDefault() != guid.GetValueOrDefault();
			}
			entity.Amount = model.Amount;
			entity.LoanId = model.LoanModel.Id;
			entity.UserId = model.MemberModel.Id;
			entity.Comment = model.Comment;
			entity.AdminDescription = model.AdminDescription;
			entity.AdminDescriptionPrivate = model.AdminDescriptionPrivate;
			entity.StatusId = model.Status.Id;
			entity.AgentStatusId = model.AgentStatus?.Id;
			entity.IsPhysicalDocsDelivered = model.IsPhysicalDocsDelivered;
			entity.Month = model.Month;
			entity.Year = model.Year;
			entity.SponsoreDueDate = model.SponsoreDueDate;
			entity.SponsoreAmount = model.SponsoreAmount;
			entity.LockedBy = string.Empty;
			await _userLoanRequestService.Update(entity);
			return Success(entity.Id, "عملیات با موفقیت انجام شد.");
		});
	}

	[HttpPost("[action]")]
	public Task<ResponseState<Guid>> SubmitSponsor([FromBody] SubmitSponsorModel model)
	{
		return TryCatch(async () =>
		{
			UserLoanRequest entity = await _userLoanRequestService.GetById(model.UserLoanRequestId);
			if (entity == null)
			{
				return Error(Guid.Empty, "اطلاعات مورد نظر یافت نشد.");
			}
			if (entity.UserLoanSponsorStatus != UserLoanSponsorStatuses.در_انتظار_بررسی)
			{
				return Error(Guid.Empty, "به دلیل تغییر وضعیت توسط اسپانسر، امکان تغییر اسپانسر وجود ندارد.");
			}
			entity.SponsorId = model.SponsorId;
			await _userLoanRequestService.Update(entity);
			User user = null;
			if (model.SponsorId.HasValue)
			{
				user = await _userManager.GetById(model.SponsorId.Value);
			}
			UserLoanRequestSponsorHistory entity2 = new UserLoanRequestSponsorHistory
			{
				DateTime = DateTime.Now,
				UserId = _currentUserService.UserId.Value,
				UserLoanRequestId = model.UserLoanRequestId,
				Subject = ((!model.SponsorId.HasValue) ? "حذف اسپانسر" : "تعیین اسپانسر"),
				Description = ((!model.SponsorId.HasValue) ? "اسپانسر حذف شد." : ("اسپانسر تعیین شد: [" + user?.BusinessName + "]"))
			};
			await _userLoanRequestSponsorHistoryService.Insert(entity2);
			return Success(entity.Id, "عملیات با موفقیت انجام شد.");
		});
	}

	[HttpPost("[action]")]
	public Task<DefaultResponseState> ChangeAgentStatusBulk([FromBody] ChangeLoanRequestStatusBulkModel model)
	{
		return TryCatch(async () =>
		{
			foreach (UserLoanRequest item in await _userLoanRequestService.GetByCondition((UserLoanRequest x) => model.Ids.Contains(x.Id), false, "AgentStatus.SmsPattern", "Agent", "User"))
			{
				bool agentStatusChanged = item.AgentStatusId != model.AgentStatusId;
				item.AgentStatusId = model.AgentStatusId;
				await _userLoanRequestService.Update(item);
				if (agentStatusChanged && item.AgentStatusId.HasValue && item.Agent != null && item.AgentStatus.SmsPattern != null)
				{
					UserLoanRequestModel userLoanRequestModel = new UserLoanRequestModel(item);
					await _integratedSmsSenderService.SendChangeLoanRequestAgentStatus(userLoanRequestModel);
				}
			}
			return Success("عملیات با موفقیت انجام شد.");
		});
	}

	[HttpPost("[action]")]
	public Task<DefaultResponseState> ChangeStatusBulk([FromBody] ChangeLoanRequestStatusBulkModel model)
	{
		return TryCatch(async () =>
		{
			foreach (UserLoanRequest item in await _userLoanRequestService.GetByCondition((UserLoanRequest x) => model.Ids.Contains(x.Id), true))
			{
				item.StatusId = model.StatusId;
				await _userLoanRequestService.Update(item);
			}
			return Success("عملیات با موفقیت انجام شد.");
		});
	}

	[HttpPost("[action]/{id}")]
	public Task<DefaultResponseState> Delete(Guid id)
	{
		return TryCatch(async () =>
		{
			UserLoanRequest userLoanRequest = await _userLoanRequestService.GetById(id);
			if (userLoanRequest == null)
			{
				return Error("امکان حذف وجود ندارد. لطفا بعدا دوباره تلاش نمایید");
			}
			try
			{
				await _userLoanRequestService.Delete(userLoanRequest);
			}
			catch (Exception)
			{
				throw;
			}
			return Success();
		});
	}

	[HttpGet("[action]")]
	public Task<ResponseState<List<UserLoanRequestModel>>> GetDueDatedRequestsBySponsore()
	{
		return TryCatch(async () =>
		{
			AppSettingsModel appSettingsModel = new AppSettingsModel(await _appSettingsService.GetAppSettings());
			List<UserLoanRequestModel> data = (await _userLoanRequestService.GetByCondition((UserLoanRequest x) => x.StatusId != null && Enumerable.Contains(appSettingsModel.SponsoreDueDateStatuses, ((object)x.StatusId).ToString()) && x.SponsoreDueDate != DateTime.MinValue && DateTime.Now >= x.SponsoreDueDate, false, "User", "Loan", "Status")).Select((UserLoanRequest x) => new UserLoanRequestModel(x)).ToList();
			return Success(data);
		});
	}

	[HttpGet("[action]")]
	public Task<ResponseState<List<UserLoanRequestSponsorHistoryModel>>> GetSponsorHistory(Guid userLoanRequestId)
	{
		return TryCatch(async () =>
		{
			List<UserLoanRequestSponsorHistoryModel> data = (from x in await _userLoanRequestSponsorHistoryService.GetByUserLoanRequestId(userLoanRequestId, "User")
				select new UserLoanRequestSponsorHistoryModel(x) into x
				orderby x.DateTime descending
				select x).ToList();
			return Success(data);
		});
	}

	[HttpPost("[action]")]
	public Task<DefaultResponseState> ChangeLockState([FromBody] ManageLoanRequestLockStateModel model)
	{
		return TryCatch(async () =>
		{
			UserLoanRequest loanRequest = await _userLoanRequestService.GetById(model.UserLoanRequestId);
			if (loanRequest == null)
			{
				return Error("درخواست جهت قفل گذاری یافت نشد.");
			}
			if (model.Lock)
			{
				if (!string.IsNullOrEmpty(loanRequest.LockedBy))
				{
					return Success();
				}
				User user = await _userManager.GetById(_currentUserService.UserId.Value);
				LockedByModel value = new LockedByModel
				{
					Date = DateTime.Now,
					UserId = _currentUserService.UserId.Value,
					User_FullName = user.FullName
				};
				loanRequest.LockedBy = JsonSerializer.Serialize(value);
				await _userLoanRequestService.Update(loanRequest);
			}
			else
			{
				UserLoanRequestModel userLoanRequestModel = new UserLoanRequestModel(loanRequest);
				if (userLoanRequestModel.LockedBy?.UserId == _currentUserService.UserId)
				{
					loanRequest.LockedBy = string.Empty;
					await _userLoanRequestService.Update(loanRequest);
				}
			}
			return Success();
		});
	}
}
