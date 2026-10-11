using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoanSystem.Core.Configuration;
using LoanSystem.Core.Exceptions;
using LoanSystem.Data;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Models;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Data.Enums;
using LoanSystemAvizhe.Services.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Spm.Services.Infrastructure;

namespace LoanSystem.Services.Base;

public class UserPaymentService : BaseService, IUserPaymentService
{
	private readonly IAppSmsService _appSmsService;

	private readonly IntegratedSmsSenderService _integratedSmsSenderService;

	private readonly IntegratedEmailSenderService _integratedEmailSenderService;

	private readonly IAppSettingsService _appSettingsService;

	private readonly AppConfig _appConfig;

	private AppSettingsModel _appSettings;

	private readonly IReliabilityService _reliabilityService;

	public UserPaymentService(AppDbContext context, ICurrentUserService currentUserService, IntegratedSmsSenderService integratedSmsSenderService, IntegratedEmailSenderService integratedEmailSenderService, IAppSmsService appSmsService, IOptions<AppConfig> appConfig, IReliabilityService reliabilityService, IAppSettingsService appSettingsService)
		: base(context, currentUserService)
	{
		_integratedSmsSenderService = integratedSmsSenderService;
		_integratedEmailSenderService = integratedEmailSenderService;
		_appSmsService = appSmsService;
		_reliabilityService = reliabilityService;
		_appSettingsService = appSettingsService;
		_appConfig = appConfig.Value;
	}

	private async Task<AppSettingsModel> GetAppSettings()
	{
		if (_appSettings != null)
		{
			return _appSettings;
		}
		AppSettings appSettings = await _appSettingsService.GetAppSettings();
		if (appSettings != null)
		{
			_appSettings = new AppSettingsModel(appSettings);
		}
		return _appSettings;
	}

	public Task<List<GroupedUserPaymentReportModel>> GetGroupedOnMethodByDateAsync(DateTime startDate, DateTime endDate)
	{
		return _context.UserLoanPayments.Where((UserLoanPayment x) => x.SaveDate.Date >= startDate && x.SaveDate.Date <= endDate && x.UserLoan.User.CustomerId == _currentUserService.CustomerId && x.SumAmount > 0m).GroupBy((UserLoanPayment x) => x.PaymentMethod, (PaymentMethods key, IEnumerable<UserLoanPayment> x) => new GroupedUserPaymentReportModel
		{
			PaymentMethod = key,
			Count = x.Count(),
			Sum = x.Sum((UserLoanPayment s) => s.SumAmount)
		}).ToListAsync();
	}

	public Task<List<UserLoanPayment>> GetByCondition(Expression<Func<UserLoanPayment, bool>> condition, params string[] includs)
	{
		IQueryable<UserLoanPayment> source = Table<UserLoanPayment>();
		foreach (string navigationPropertyPath in includs)
		{
			source = source.Include(navigationPropertyPath);
		}
		condition = condition.AND((UserLoanPayment x) => x.UserLoan.User.CustomerId == _currentUserService.CustomerId);
		source = source.Where(condition);
		return source.ToListAsync();
	}

	public Task<List<UserLoanPayment>> GetAllUserPayments(bool includeDetails = false)
	{
		return (from x in Table<UserLoanPayment>().Include((UserLoanPayment x) => x.UserLoan).Include((UserLoanPayment x) => x.UserLoan).ThenInclude((UserLoan x) => x.Loan)
				.Include((UserLoanPayment x) => x.UserLoan)
				.ThenInclude((UserLoan x) => x.User)
			where x.UserLoan.User.CustomerId == _currentUserService.CustomerId
			select x).AsSplitQuery().ToListAsync();
	}

	public Task<UserLoanPayment> GetById(Guid entityId, params string[] includs)
	{
		return Table<UserLoanPayment>().Include((UserLoanPayment x) => x.UserLoan).ThenInclude((UserLoan x) => x.User).Include((UserLoanPayment x) => x.UserLoan)
			.ThenInclude((UserLoan x) => x.Loan)
			.Include((UserLoanPayment x) => x.UserLoanPaymentOthers)
			.FirstOrDefaultAsync((UserLoanPayment x) => x.Id == entityId);
	}

	public Task<UserLoanPayment> GetByCode(int code, params string[] includs)
	{
		IQueryable<UserLoanPayment> source = Table<UserLoanPayment>();
		foreach (string navigationPropertyPath in includs)
		{
			source = source.Include(navigationPropertyPath);
		}
		return source.Include((UserLoanPayment x) => x.UserLoan).ThenInclude((UserLoan x) => x.User).Include((UserLoanPayment x) => x.UserLoan)
			.ThenInclude((UserLoan x) => x.Loan)
			.FirstOrDefaultAsync((UserLoanPayment x) => x.Code == code);
	}

	public Task<List<UserLoanPayment>> GetPaymentsByUserLoanId(Guid userLoanId, bool includeMemberPurchases, bool includePaymentRequest)
	{
		IQueryable<UserLoanPayment> source = Table<UserLoanPayment>();
		source = source.Include((UserLoanPayment x) => x.UserLoanPaymentOthers).Include((UserLoanPayment x) => x.UserLoan).ThenInclude((UserLoan x) => x.Loan);
		if (includeMemberPurchases)
		{
			source = source.Include((UserLoanPayment x) => x.MemberPurchases);
		}
		if (includePaymentRequest)
		{
			source = source.Include((UserLoanPayment x) => x.UserLoanPaymentRequests);
		}
		return source.Where((UserLoanPayment x) => x.UserLoan.Id == userLoanId).ToListAsync();
	}

	public Task<List<LoanPaymentByMonthModel>> GetListByTargetMonth(DateTime start, DateTime end)
	{
		return (from x in Table<UserLoanPayment>()
			where x.TargetMonth >= start && x.TargetMonth <= end
			select new LoanPaymentByMonthModel
			{
				Id = x.Id,
				Installment = x.SumAmount,
				LoanTitle = x.UserLoan.Loan.Title,
				MemberFullName = x.UserLoan.User.FullName,
				NationalCode = x.UserLoan.User.NationalCode,
				Remain = x.UserLoan.Remain,
				TargetMonth = x.TargetMonth,
				Total = x.UserLoan.Amount
			}).ToListAsync();
	}

	public Task<List<UserLoanPayment>> GetPaymentsByUserId(long userId)
	{
		return (from x in Table<UserLoanPayment>()
			where x.UserLoan.UserId == userId
			select x).ToListAsync();
	}

	public async Task<Dictionary<long, List<UserLoanPayment>>> GetPaymentsByUserIdsAsDictionary(List<long> userIds)
	{
		IQueryable<UserLoanPayment> source = Table<UserLoanPayment>();
		if (userIds?.Any() ?? false)
		{
			source = source.Where((UserLoanPayment x) => userIds.Contains(x.UserLoan.UserId));
		}
		return (from x in await source.Include((UserLoanPayment x) => x.UserLoan).ThenInclude((UserLoan x) => x.User).AsNoTracking()
				.ToListAsync()
			group x by x.UserLoan.User.Id).ToDictionary((IGrouping<long, UserLoanPayment> x) => x.Key, (IGrouping<long, UserLoanPayment> val) => val.ToList());
	}

	public async Task<Task> Insert(UserLoanPayment entity)
	{
		Insert<UserLoanPayment>(entity);
		UserLoan userLoan = _context.UserLoans.Include((UserLoan x) => x.Loan).Include((UserLoan x) => x.UserLoanPayments).FirstOrDefault((UserLoan x) => x.Id == entity.UserLoanId);
		userLoan.Paid = userLoan.UserLoanPayments.Sum((UserLoanPayment x) => x.SumAmount);
		userLoan.Paid += entity.SumAmount;
		UserLoanModel userLoanModel = new UserLoanModel(userLoan)
		{
			LoanPayments = userLoan.UserLoanPayments.Select((UserLoanPayment x) => new LoanPaymentModel(x)).ToList()
		};
		await SetPaymentsDelays(userLoanModel.LoanPayments);
		userLoan.IsCheckouted = userLoanModel.Remain == 0m;
		UserLoan userLoan2 = userLoan;
		DateTime? checkoutDate;
		if (userLoan.CheckoutDate.HasValue)
		{
			checkoutDate = userLoan.CheckoutDate;
		}
		else
		{
			checkoutDate = ((userLoanModel.Remain == 0m) ? new DateTime?(DateTime.Now) : ((DateTime?)null));
		}
		userLoan2.CheckoutDate = checkoutDate;
		return Save();
	}

	public Task Update(UserLoanPayment entity, bool sendSms = true)
	{
		return RunInTransaction(async (DbTransaction trs) =>
		{
			await PrepareOtherPayments(entity);
			entity.SumAmount = entity.Amount + (entity.UserLoanPaymentOthers?.Sum((UserLoanPaymentOther x) => x.Amount) ?? 0m);
			if (entity.UserLoanPaymentOthers?.Any() ?? false)
			{
				entity.UserLoanPaymentOthers.Clear();
			}
			Update(entity, update: true, Array.Empty<Expression<Func<UserLoanPayment, object>>>());
			await Save();
			UserLoan userLoan = _context.UserLoans.AsNoTracking().Include((UserLoan x) => x.Agent).Include((UserLoan x) => x.Loan)
				.Include((UserLoan x) => x.UserLoanPayments)
				.FirstOrDefault((UserLoan x) => x.Id == entity.UserLoanId);
			userLoan.PayableAmount = userLoan.UserLoanPayments.Sum((UserLoanPayment x) => x.InstallmentAmount);
			userLoan.Paid = userLoan.UserLoanPayments.Sum((UserLoanPayment x) => x.SumAmount);
			UserLoanModel userLoanModel = new UserLoanModel(userLoan)
			{
				LoanPayments = userLoan.UserLoanPayments.Select((UserLoanPayment x) => new LoanPaymentModel(x)).ToList()
			};
			userLoanModel.LoanPayments.First((LoanPaymentModel x) => x.Id == entity.Id).SumAmount = 0m;
			await SetPaymentsDelays(userLoanModel.LoanPayments, calcEarly: true);
			userLoanModel.IsCheckouted = false;
			if (userLoanModel.Remain != 0m)
			{
				await SetPaymentsDelays(userLoanModel.LoanPayments, calcEarly: true, checkoutModeOnLoanProfit: true);
			}
			userLoan.IsCheckouted = userLoanModel.Remain == 0m;
			UserLoan userLoan2 = userLoan;
			DateTime? checkoutDate = (userLoan.CheckoutDate.HasValue ? userLoan.CheckoutDate : ((userLoanModel.Remain == 0m) ? new DateTime?(DateTime.Now) : ((DateTime?)null)));
			userLoan2.CheckoutDate = checkoutDate;
			Update(userLoan, true);
			if (userLoan.IsCheckouted == true)
			{
				UserLoanPayment userLoanPayment = userLoan.UserLoanPayments.OrderBy((UserLoanPayment x) => x.TargetMonth).Last();
				if (entity.TargetMonth.Date != userLoanPayment.TargetMonth.Date && entity.DelayDay < 0)
				{
					entity.DelayDay = (int)(DateTime.Now.Date - userLoanPayment.TargetMonth.Date).TotalDays;
					entity.DelayPrice = userLoanModel.LoanPayments.Where((LoanPaymentModel x) => x.TargetMonth.Date >= entity.TargetMonth.Date).Sum((LoanPaymentModel x) => x.DelayPrice);
					Update(entity, update: true, Array.Empty<Expression<Func<UserLoanPayment, object>>>());
				}
			}
			await Save();
			if (sendSms)
			{
				UserLoanPayment userPayment = await GetById(entity.Id, "UserLoanPaymentOthers");
				if (userLoan.Agent == null || userLoan.Agent.SmsActiveForMembers)
				{
					try
					{
						await _integratedSmsSenderService.SendThanks(new LoanPaymentModel(userPayment));
					}
					catch
					{
					}
					try
					{
						await _integratedEmailSenderService.SendThanks(new LoanPaymentModel(userPayment));
					}
					catch
					{
					}
				}
			}
			if (_appConfig.ActiveParts.Reliability)
			{
				Dictionary<long, List<UserLoanPayment>> member_payments_dic = await GetPaymentsByUserIdsAsDictionary(new List<long> { userLoan.UserId });
				_reliabilityService.SetUserPaymentServie(this);
				await _reliabilityService.SetMembersReliability(new List<long> { userLoan.UserId }, member_payments_dic);
			}
		});
	}

	private async Task PrepareOtherPayments(UserLoanPayment entity)
	{
		List<Guid> currentOthers = await (from x in Table<UserLoanPaymentOther>()
			where x.UserLoanPaymentId == entity.Id
			select x.Id).ToListAsync();
		List<UserLoanPaymentOther> list = entity.UserLoanPaymentOthers?.Where((UserLoanPaymentOther x) => x.Id == Guid.Empty).ToList();
		List<UserLoanPaymentOther> list2 = ((entity.UserLoanPaymentOthers != null) ? (from x in currentOthers
			where !entity.UserLoanPaymentOthers.Select((UserLoanPaymentOther userLoanPaymentOther) => userLoanPaymentOther.Id).Contains(x)
			select new UserLoanPaymentOther
			{
				Id = x
			}).ToList() : new List<UserLoanPaymentOther>());
		List<UserLoanPaymentOther> list3 = ((entity.UserLoanPaymentOthers != null) ? entity.UserLoanPaymentOthers.Where((UserLoanPaymentOther x) => currentOthers.Contains(x.Id)).ToList() : new List<UserLoanPaymentOther>());
		if (list != null && list.Any())
		{
			list.ForEach((UserLoanPaymentOther x) =>
			{
				x.SaveDate = new DateTime(x.SaveDate.Year, x.SaveDate.Month, x.SaveDate.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
			});
			InsertBulk(list);
		}
		if (list2 != null && list2.Any())
		{
			Delete((IEnumerable<UserLoanPaymentOther>)list2);
		}
		if (list3 != null && list3.Any())
		{
			Update(list3, update: true, Array.Empty<Expression<Func<UserLoanPaymentOther, object>>>());
		}
	}

	public async Task Delete(UserLoanPayment entity)
	{
		entity = await Table<UserLoanPayment>().FirstOrDefaultAsync((UserLoanPayment x) => x.Id == entity.Id);
		UserLoanPayment userLoanPayment = new UserLoanPayment();
		userLoanPayment.Id = entity.Id;
		userLoanPayment.UserLoanId = entity.UserLoanId;
		userLoanPayment.InstallmentNumber = entity.InstallmentNumber;
		userLoanPayment.InstallmentAmount = entity.InstallmentAmount;
		userLoanPayment.TargetMonth = entity.TargetMonth;
		Update(userLoanPayment, update: true, Array.Empty<Expression<Func<UserLoanPayment, object>>>());
		await Save();
		UserLoan userLoan = _context.UserLoans.Include((UserLoan x) => x.Loan).Include((UserLoan x) => x.UserLoanPayments).FirstOrDefault((UserLoan x) => x.Id == entity.UserLoanId);
		userLoan.Paid = userLoan.UserLoanPayments.Sum((UserLoanPayment x) => x.SumAmount);
		UserLoanModel userLoanModel = new UserLoanModel(userLoan)
		{
			LoanPayments = userLoan.UserLoanPayments.Select((UserLoanPayment x) => new LoanPaymentModel(x)).ToList()
		};
		await SetPaymentsDelays(userLoanModel.LoanPayments, calcEarly: true);
		userLoanModel.IsCheckouted = false;
		userLoan.IsCheckouted = userLoanModel.Remain == 0m;
		UserLoan userLoan2 = userLoan;
		DateTime? checkoutDate;
		if (userLoan.CheckoutDate.HasValue)
		{
			checkoutDate = userLoan.CheckoutDate;
		}
		else
		{
			checkoutDate = ((userLoanModel.Remain == 0m) ? new DateTime?(DateTime.Now) : ((DateTime?)null));
		}
		userLoan2.CheckoutDate = checkoutDate;
		await Save();
	}

	public void DeleteByUserLoanId(Guid userLoanId)
	{
		List<UserLoanPayment> entities = _context.UserLoanPayments.Where((UserLoanPayment x) => x.UserLoanId == userLoanId).ToList();
		_context.RemoveRange(entities);
	}

	public async Task<int> CalcDelayDay(LoanPaymentModel model)
	{
		DateTime date = (await calcDesireDate()).Date;
		if (date != DateTime.MinValue)
		{
			return (int)(date - model.TargetMonth.Date).TotalDays;
		}
		return (int)(DateTime.Now.Date - model.TargetMonth.Date).TotalDays;
		async Task<DateTime> calcDesireDate()
		{
			if (model.SaveDate.Date == DateTime.MinValue)
			{
				return DateTime.MinValue;
			}
			byte b = (await GetAppSettings())?.PercentOfConsideringAsPaid ?? 50;
			decimal num = model.InstallmentAmount * (decimal)b / 100m;
			if (model.UserLoanModel?.Month == model.InstallmentNumber && DateTime.Now.Date < model.TargetMonth.Date)
			{
				num = 10m;
			}
			if (model.Amount >= num)
			{
				return model.SaveDate;
			}
			decimal amount = model.Amount;
			DateTime result = ((DateTime.Now.Date < model.TargetMonth.Date) ? model.TargetMonth.Date : DateTime.MinValue);
			if (model.LoanPaymentOthers?.Any() ?? false)
			{
				foreach (LoanPaymentOtherModel item in model.LoanPaymentOthers.OrderBy((LoanPaymentOtherModel x) => x.SaveDate))
				{
					amount += item.Amount;
					if (amount >= num)
					{
						result = item.SaveDate;
						break;
					}
				}
			}
			return result;
		}
	}

	public async Task<decimal> CalcDelayPrice(LoanPaymentModel model, bool checkoutModeOnLoanProfit = false, int? days = null)
	{
		if (!days.HasValue)
		{
			days = await CalcDelayDay(model);
		}
		if (days == 0)
		{
			return 0m;
		}
		if (model.UserLoanModel == null || model.UserLoanModel.LoanModel == null)
		{
			throw new CustomException("Loan not found.");
		}
		double num;
		if (days > 0)
		{
			num = model.UserLoanModel.LoanModel.DelayPercent;
		}
		else if (checkoutModeOnLoanProfit)
		{
			int num2 = ((model.UserLoanModel.LoanModel.LoanProfitType == LoanProfitType.Monthly) ? 30 : 365);
			decimal? customProfitPercent = model.UserLoanModel.CustomProfitPercent;
			num = ((!((customProfitPercent.GetValueOrDefault() > 0m) & customProfitPercent.HasValue)) ? (model.UserLoanModel.LoanModel.ProfitPercent / 100.0 / (double)num2) : ((double)model.UserLoanModel.CustomProfitPercent.Value / 100.0 / (double)num2));
		}
		else
		{
			num = model.UserLoanModel.LoanModel.EarlyPercent;
		}
		if ((days < 0) & checkoutModeOnLoanProfit)
		{
			days = Math.Max(-30, days.Value);
		}
		return Math.Round((decimal)Math.Abs(days.Value) * (decimal)num * ((days > 0 || !checkoutModeOnLoanProfit) ? model.InstallmentAmount : model.UserLoanModel.Amount));
	}

	public async Task SetPaymentsDelays(List<LoanPaymentModel> loanPayments, bool calcEarly = false, bool checkoutModeOnLoanProfit = false)
	{
		if (loanPayments == null || !loanPayments.Any())
		{
			return;
		}
		DateTime today = DateTime.Now;
		byte percent = (await GetAppSettings())?.PercentOfConsideringAsPaid ?? 50;
		foreach (LoanPaymentModel loanPayment in loanPayments)
		{
			if ((!(loanPayment.SumAmount != 0m) || !(loanPayment.SumAmount >= loanPayment.InstallmentAmount * (decimal)percent / 100m)) && (calcEarly || loanPayment.TargetMonth < today))
			{
				if (calcEarly && loanPayment.Amount == 0m)
				{
					loanPayment.Amount = loanPayment.InstallmentAmount;
				}
				LoanPaymentModel loanPaymentModel = loanPayment;
				loanPaymentModel.DelayDay = await CalcDelayDay(loanPayment);
				loanPaymentModel = loanPayment;
				loanPaymentModel.DelayPrice = await CalcDelayPrice(loanPayment, checkoutModeOnLoanProfit);
			}
		}
	}

	public async Task SetAppSms(List<LoanPaymentModel> loanPayments, bool ignoreText)
	{
		if (loanPayments == null || !loanPayments.Any())
		{
			return;
		}
		List<Guid> entityIds = loanPayments.Select((LoanPaymentModel x) => x.Id).ToList();
		List<AppSms> source = await _appSmsService.GetByCondition((AppSms x) => entityIds.Contains(x.EntityId) && x.EntityType == "UserLoanPayment", false);
		foreach (LoanPaymentModel loanPayment in loanPayments)
		{
			loanPayment.AppSmsList = (from x in source
				where x.EntityId == loanPayment.Id
				select new AppSmsModel(x)).ToList();
			if (ignoreText)
			{
				loanPayment.AppSmsList.ForEach((AppSmsModel x) =>
				{
					x.Text = string.Empty;
				});
			}
		}
	}

	public async Task<OverDueInstallmentsModel> GetOverDueInstallments(bool loadAppSms, long? agentId = null)
	{
		byte percent = (await GetAppSettings())?.PercentOfConsideringAsPaid ?? 50;
		List<LoanPaymentModel> models = (await (from x in _context.UserLoanPayments.Include((UserLoanPayment x) => x.UserLoan).ThenInclude((UserLoan x) => x.Loan).Include((UserLoanPayment x) => x.UserLoan)
				.ThenInclude((UserLoan x) => x.Agent)
				.Include((UserLoanPayment x) => x.UserLoan)
				.ThenInclude((UserLoan x) => x.User)
				.AsNoTracking()
			where x.UserLoan.User.CustomerId == _currentUserService.CustomerId && DateTime.Now.Date >= x.TargetMonth.Date && x.UserLoan.IsCheckouted != true && (x.SumAmount == 0m || x.SumAmount < x.InstallmentAmount * (decimal)percent / 100m) && !x.UserLoan.Loan.HasSponsore
			orderby x.TargetMonth descending
			select x).AsSplitQuery().ToListAsync()).Select((UserLoanPayment x) => new LoanPaymentModel(x)).ToList();
		if (agentId.HasValue)
		{
			models = models.Where((LoanPaymentModel x) =>
			{
				AgentModel agentModel = x.UserLoanModel.AgentModel;
				return (agentModel == null) ? (!agentId.HasValue) : (agentModel.Id == agentId);
			}).ToList();
		}
		await SetPaymentsDelays(models);
		if (loadAppSms)
		{
			await SetAppSms(models, ignoreText: true);
		}
		return new OverDueInstallmentsModel
		{
			LoanPayments = models,
			Sum = models.Sum((LoanPaymentModel x) => x.UserLoanModel.Installment),
			Count = models.Count,
			SumByDelays = models.Sum((LoanPaymentModel x) => x.UserLoanModel.Installment + x.DelayPrice)
		};
	}

	public async Task<List<LoanPaymentModel>> GetDueDatedItemsForSms()
	{
		byte percent = (await GetAppSettings())?.PercentOfConsideringAsPaid ?? 50;
		List<string> alreadySent = await (from x in _context.AppSms.AsNoTracking()
			where (int)x.Status != 3 && x.Date.Date == DateTime.Now.Date && (int)x.AppSmsType == 1
			select x.To).ToListAsync();
		return (await (from x in _context.UserLoanPayments.Include((UserLoanPayment x) => x.UserLoan).ThenInclude((UserLoan x) => x.Loan).Include((UserLoanPayment x) => x.UserLoan)
				.ThenInclude((UserLoan x) => x.User)
				.Include((UserLoanPayment x) => x.UserLoan)
				.ThenInclude((UserLoan x) => x.Agent)
				.AsNoTracking()
			where x.UserLoan.Loan.IsSmsActive && x.UserLoan.User.CustomerId == _currentUserService.CustomerId && DateTime.Now.Date == x.TargetMonth.Date && x.UserLoan.IsCheckouted != true && (x.SumAmount == 0m || x.SumAmount < x.InstallmentAmount * (decimal)percent / 100m) && !x.SentDueDateSms && !alreadySent.Contains(x.UserLoan.User.Mobile)
			orderby x.TargetMonth descending
			select x).ToListAsync()).Select((UserLoanPayment x) => new LoanPaymentModel(x)).ToList();
	}

	public async Task<List<LoanPaymentModel>> GetDueDatedItemsForEmail()
	{
		byte percent = (await GetAppSettings())?.PercentOfConsideringAsPaid ?? 50;
		List<string> alreadySent = await (from x in _context.AppEmails.AsNoTracking()
			where x.Date.Date == DateTime.Now.Date && (int)x.AppEmailType == 1
			select x.To).ToListAsync();
		return (await (from x in _context.UserLoanPayments.Include((UserLoanPayment x) => x.UserLoan).ThenInclude((UserLoan x) => x.Loan).Include((UserLoanPayment x) => x.UserLoan)
				.ThenInclude((UserLoan x) => x.User)
				.Include((UserLoanPayment x) => x.UserLoan)
				.ThenInclude((UserLoan x) => x.Agent)
				.AsNoTracking()
			where x.UserLoan.Loan.IsSmsActive && x.UserLoan.User.CustomerId == _currentUserService.CustomerId && DateTime.Now.Date == x.TargetMonth.Date && x.UserLoan.IsCheckouted != true && (x.SumAmount == 0m || x.SumAmount < x.InstallmentAmount * (decimal)percent / 100m) && x.UserLoan.User.Email != "" && !x.SentDueDateEmail && !alreadySent.Contains(x.UserLoan.User.Email)
			orderby x.TargetMonth descending
			select x).ToListAsync()).Select((UserLoanPayment x) => new LoanPaymentModel(x)).ToList();
	}

	public async Task<List<LoanPaymentModel>> GetBeforeDueDatedItemsForSms(int days)
	{
		byte percent = (await GetAppSettings())?.PercentOfConsideringAsPaid ?? 50;
		List<string> alreadySent = await (from x in _context.AppSms.AsNoTracking()
			where (int)x.Status != 3 && x.Date.Date == DateTime.Now.Date && (int)x.AppSmsType == 4
			select x.To).ToListAsync();
		return (await (from x in _context.UserLoanPayments.Include((UserLoanPayment x) => x.UserLoan).ThenInclude((UserLoan x) => x.Loan).Include((UserLoanPayment x) => x.UserLoan)
				.ThenInclude((UserLoan x) => x.User)
				.Include((UserLoanPayment x) => x.UserLoan)
				.ThenInclude((UserLoan x) => x.Agent)
				.AsNoTracking()
			where x.UserLoan.Loan.IsSmsActive && x.UserLoan.User.CustomerId == _currentUserService.CustomerId && DateTime.Now.Date == x.TargetMonth.AddDays(days * -1).Date && x.UserLoan.IsCheckouted != true && (x.SumAmount == 0m || x.SumAmount < x.InstallmentAmount * (decimal)percent / 100m) && !x.SentBeforeDueDateSms && !alreadySent.Contains(x.UserLoan.User.Mobile)
			orderby x.TargetMonth descending
			select x).ToListAsync()).Select((UserLoanPayment x) => new LoanPaymentModel(x)).ToList();
	}

	public async Task<List<LoanPaymentModel>> GetBeforeDueDatedItemsForEmail(int days)
	{
		byte percent = (await GetAppSettings())?.PercentOfConsideringAsPaid ?? 50;
		List<string> alreadySent = await (from x in _context.AppEmails.AsNoTracking()
			where x.Date.Date == DateTime.Now.Date && (int)x.AppEmailType == 4
			select x.To).ToListAsync();
		return (await (from x in _context.UserLoanPayments.Include((UserLoanPayment x) => x.UserLoan).ThenInclude((UserLoan x) => x.Loan).Include((UserLoanPayment x) => x.UserLoan)
				.ThenInclude((UserLoan x) => x.User)
				.Include((UserLoanPayment x) => x.UserLoan)
				.ThenInclude((UserLoan x) => x.Agent)
				.AsNoTracking()
			where x.UserLoan.Loan.IsSmsActive && x.UserLoan.User.CustomerId == _currentUserService.CustomerId && DateTime.Now.Date == x.TargetMonth.AddDays(days * -1).Date && x.UserLoan.IsCheckouted != true && (x.SumAmount == 0m || x.SumAmount < x.InstallmentAmount * (decimal)percent / 100m) && !x.SentBeforeDueDateEmail && x.UserLoan.User.Email != "" && !alreadySent.Contains(x.UserLoan.User.Email)
			orderby x.TargetMonth descending
			select x).ToListAsync()).Select((UserLoanPayment x) => new LoanPaymentModel(x)).ToList();
	}

	public async Task<List<LoanPaymentModel>> GetOverDueDatedItemsForSms(int daysCount, bool onlyOnce, int gapPeriod)
	{
		byte percent = (await GetAppSettings())?.PercentOfConsideringAsPaid ?? 50;
		List<string> alreadySent = await (from x in _context.AppSms.AsNoTracking()
			where (int)x.Status != 3 && x.Date.Date == DateTime.Now.Date && (int)x.AppSmsType == 3
			select x.To).ToListAsync();
		return (await (from x in _context.UserLoanPayments.Include((UserLoanPayment x) => x.UserLoan).ThenInclude((UserLoan x) => x.Loan).Include((UserLoanPayment x) => x.UserLoan)
				.ThenInclude((UserLoan x) => x.User)
				.Include((UserLoanPayment x) => x.UserLoan)
				.ThenInclude((UserLoan x) => x.Agent)
				.AsNoTracking()
			where x.UserLoan.Loan.IsSmsActive && x.UserLoan.User.CustomerId == _currentUserService.CustomerId && x.TargetMonth.Date <= DateTime.Now.AddDays(daysCount * -1).Date && x.UserLoan.IsCheckouted != true && (x.SumAmount == 0m || x.SumAmount < x.InstallmentAmount * (decimal)percent / 100m) && (x.LastOverDueSmsDate == null || (!onlyOnce && x.LastOverDueSmsDate.Value.AddDays(gapPeriod).Date <= DateTime.Now.Date)) && !alreadySent.Contains(x.UserLoan.User.Mobile)
			orderby x.TargetMonth descending
			select x).ToListAsync()).Select((UserLoanPayment x) => new LoanPaymentModel(x)).ToList();
	}

	public async Task<List<LoanPaymentModel>> GetOverDueDatedItemsForEmail(int daysCount, bool onlyOnce)
	{
		byte percent = (await GetAppSettings())?.PercentOfConsideringAsPaid ?? 50;
		List<string> alreadySent = await (from x in _context.AppEmails.AsNoTracking()
			where x.Date.Date == DateTime.Now.Date && (int)x.AppEmailType == 3
			select x.To).ToListAsync();
		return (await (from x in _context.UserLoanPayments.Include((UserLoanPayment x) => x.UserLoan).ThenInclude((UserLoan x) => x.Loan).Include((UserLoanPayment x) => x.UserLoan)
				.ThenInclude((UserLoan x) => x.User)
				.Include((UserLoanPayment x) => x.UserLoan)
				.ThenInclude((UserLoan x) => x.Agent)
				.AsNoTracking()
			where x.UserLoan.Loan.IsSmsActive && x.UserLoan.User.CustomerId == _currentUserService.CustomerId && x.TargetMonth.Date <= DateTime.Now.AddDays(daysCount * -1).Date && x.UserLoan.IsCheckouted != true && (x.SumAmount == 0m || x.SumAmount < x.InstallmentAmount * (decimal)percent / 100m) && x.UserLoan.User.Email != "" && (!x.SentOverDueDateEmail || !onlyOnce) && !alreadySent.Contains(x.UserLoan.User.Email)
			orderby x.TargetMonth descending
			select x).ToListAsync()).Select((UserLoanPayment x) => new LoanPaymentModel(x)).ToList();
	}

	public async Task<int> GetDueDatedCountByDateAsync(DateTime startDate, DateTime endDate)
	{
		return await (from x in _context.UserLoanPayments.Include((UserLoanPayment x) => x.UserLoan).ThenInclude((UserLoan x) => x.User).AsNoTracking()
			where x.UserLoan.User.CustomerId == _currentUserService.CustomerId && x.TargetMonth.Date >= ((DateTime)startDate).Date && x.TargetMonth.Date <= ((DateTime)endDate).Date
			select x).CountAsync();
	}

	public async Task<int> GetOverDueDatedCountByDateAsync(DateTime startDate, DateTime endDate)
	{
		byte percent = (await GetAppSettings())?.PercentOfConsideringAsPaid ?? 50;
		return await (from x in _context.UserLoanPayments.Include((UserLoanPayment x) => x.UserLoan).ThenInclude((UserLoan x) => x.User).AsNoTracking()
			where x.UserLoan.User.CustomerId == _currentUserService.CustomerId && x.TargetMonth.Date >= ((DateTime)startDate).Date && x.TargetMonth.Date <= ((DateTime)endDate).Date && x.UserLoan.IsCheckouted != true && (x.SumAmount == 0m || x.SumAmount < x.InstallmentAmount * (decimal)percent / 100m)
			select x).CountAsync();
	}

	public async Task MarkAsSentDueDateSms(Guid id)
	{
		UserLoanPayment userLoanPayment = await GetById(id);
		userLoanPayment.SentDueDateSms = true;
		Update(userLoanPayment, update: true, Array.Empty<Expression<Func<UserLoanPayment, object>>>());
		await Save();
	}

	public async Task MarkAsSentDueDateEmail(Guid id)
	{
		UserLoanPayment userLoanPayment = await GetById(id);
		userLoanPayment.SentDueDateEmail = true;
		Update(userLoanPayment, update: true, Array.Empty<Expression<Func<UserLoanPayment, object>>>());
		await Save();
	}

	public async Task MarkAsSentOverDueDateSms(Guid id)
	{
		UserLoanPayment userLoanPayment = await GetById(id);
		userLoanPayment.LastOverDueSmsDate = DateTime.Now;
		Update(userLoanPayment, update: true, Array.Empty<Expression<Func<UserLoanPayment, object>>>());
		await Save();
	}

	public async Task MarkAsSentOverDueDateEmail(Guid id)
	{
		UserLoanPayment userLoanPayment = await GetById(id);
		userLoanPayment.SentOverDueDateEmail = true;
		Update(userLoanPayment, update: true, Array.Empty<Expression<Func<UserLoanPayment, object>>>());
		await Save();
	}

	public async Task MarkAsSentBeforeDueDateSms(Guid id)
	{
		UserLoanPayment userLoanPayment = await GetById(id);
		userLoanPayment.SentBeforeDueDateSms = true;
		Update(userLoanPayment, update: true, Array.Empty<Expression<Func<UserLoanPayment, object>>>());
		await Save();
	}

	public async Task MarkAsSentBeforeDueDateEmail(Guid id)
	{
		UserLoanPayment userLoanPayment = await GetById(id);
		userLoanPayment.SentBeforeDueDateEmail = true;
		Update(userLoanPayment, update: true, Array.Empty<Expression<Func<UserLoanPayment, object>>>());
		await Save();
	}

	public async Task<string> ValidateLoanPayment(UserLoanPayment userLoanPayment)
	{
		string summary = string.Empty;
		if ((await GetByCondition((UserLoanPayment x) => x.UserLoanId == userLoanPayment.UserLoanId && x.InstallmentNumber < userLoanPayment.InstallmentNumber && x.SumAmount == 0m))?.Any() ?? false)
		{
			summary += "ابتدا باید سررسید های قبلی پرداخت شود.";
		}
		return summary;
	}
}
