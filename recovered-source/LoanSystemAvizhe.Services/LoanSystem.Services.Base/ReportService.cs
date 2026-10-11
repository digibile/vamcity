using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Core;
using LoanSystem.Data;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Models;
using LoanSystem.Services.Users;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Data.Enums;
using Microsoft.EntityFrameworkCore;
using SharpCompress;
using Spm.Services.Infrastructure;

namespace LoanSystem.Services.Base;

public class ReportService : BaseService, IReportService
{
	private readonly IUserPaymentService _userPaymentService;

	private readonly IUserSavedMoneyService _userSavedMoneyService;

	private readonly IAppSettingsService _appSettingsService;

	private readonly UserManager _userManager;

	private readonly IUserLoanRequestService _userLoanRequestService;

	private readonly IUserLoanService _userLoanService;

	private readonly IMemberService _memberService;

	private readonly IMemberPurchaseService _memberPurchaseService;

	private readonly ITicketService _ticketService;

	private readonly ITransactionService _transactionService;

	public ReportService(AppDbContext context, IUserLoanRequestService userLoanRequestService, IUserLoanService userLoanService, IMemberService memberService, IMemberPurchaseService memberPurchaseService, ITicketService ticketService, ITransactionService transactionService, IUserPaymentService userPaymentService, UserManager userManager, ICurrentUserService currentUserService, IUserSavedMoneyService userSavedMoneyService, IAppSettingsService appSettingsService)
		: base(context, currentUserService)
	{
		_userPaymentService = userPaymentService;
		_userLoanService = userLoanService;
		_userManager = userManager;
		_userSavedMoneyService = userSavedMoneyService;
		_appSettingsService = appSettingsService;
		_userLoanRequestService = userLoanRequestService;
		_memberService = memberService;
		_memberPurchaseService = memberPurchaseService;
		_ticketService = ticketService;
		_transactionService = transactionService;
	}

	public async Task<List<dynamic>> CheckoutThisMonth(DateTime? dateTime = null)
	{
		if (!dateTime.HasValue)
		{
			dateTime = await (from x in _context.UserLoanPayments.Include((UserLoanPayment x) => x.UserLoan).ThenInclude((UserLoan x) => x.User)
				where x.UserLoan.User.CustomerId == _currentUserService.CustomerId
				orderby x.TargetMonth descending
				select x.TargetMonth).FirstOrDefaultAsync();
		}
		if (!dateTime.HasValue)
		{
			dateTime = DateTime.Now;
		}
		List<Guid> userLoanIds = await (from x in _context.UserLoanPayments.Include((UserLoanPayment x) => x.UserLoan).ThenInclude((UserLoan x) => x.User)
			where x.UserLoan.User.CustomerId == _currentUserService.CustomerId && x.TargetMonth.Month == ((DateTime?)dateTime).Value.Month
			select x.UserLoanId).ToListAsync();
		return await (from x in _context.UserLoans.Include((UserLoan x) => x.Loan).Include((UserLoan x) => x.User)
			where x.User.CustomerId == _currentUserService.CustomerId && x.Remain == 0m && userLoanIds.Contains(x.Id)
			select new
			{
				FirstName = x.User.FirstName,
				LastName = x.User.LastName,
				LoanTitle = x.Loan.Title,
				LoanAmount = x.Amount,
				StartDate = x.StartDate.ToPersianDate(),
				InstallmentsCount = x.Month,
				Remain = x.Remain
			}).Cast<object>().ToListAsync();
	}

	public async Task<List<dynamic>> CheckoutedMembers()
	{
		List<object> result = new List<object>();
		List<UserLoan> source = await (from x in _context.UserLoans.AsNoTracking().Include((UserLoan x) => x.Loan).Include((UserLoan x) => x.User)
			where x.User.CustomerId == _currentUserService.CustomerId
			select x).ToListAsync();
		List<User> users = (from x in source
			where x.Remain > 0m && x.IsCheckouted != true
			select x.User).ToList();
		foreach (User user in (from x in source
			where !users.Contains(x.User, new UserComparer())
			select x.User).Distinct(new UserComparer()).ToList())
		{
			DateTime? dateTime = source.Where((UserLoan x) => x.UserId == user.Id).Last()?.CheckoutDate;
			result.Add(new
			{
				FirstName = user.FirstName,
				LastName = user.LastName,
				Mobile = user.Mobile,
				LoanCount = source.Count((UserLoan x) => x.UserId == user.Id),
				LoanSum = source.Where((UserLoan x) => x.UserId == user.Id).Sum((UserLoan x) => x.Amount),
				TotalSavedMoney = user.TotalSavedMoney,
				CheckoutDate = ((dateTime.HasValue && dateTime.Value.Year != 1900) ? dateTime.ToPersianDate() : "")
			});
		}
		return result;
	}

	public async Task<List<dynamic>> LatestMembers()
	{
		List<UserLoan> userLoans = await _userLoanService.GetAllUserLoans();
		List<UserLoanPayment> userPayments = await _userPaymentService.GetAllUserPayments();
		return (from x in (from x in (await _userManager.GetUsersInRoleAsync("Member")).Where((User x) =>
				{
					Guid? customerId = x.CustomerId;
					Guid? customerId2 = _currentUserService.CustomerId;
					if (customerId.HasValue != customerId2.HasValue)
					{
						return false;
					}
					return !customerId.HasValue || customerId.GetValueOrDefault() == customerId2.GetValueOrDefault();
				}).ToList()
				orderby x.RegDate descending
				select x).Take(20)
			select new
			{
				Id = x.Id,
				FirstName = x.FirstName,
				LastName = x.LastName,
				NationalCode = x.NationalCode,
				RegDate = x.RegDate.ToPersianDate(),
				TotalSavedMoney = x.TotalSavedMoney,
				LoanCount = userLoans.Count((UserLoan ul) => ul.UserId == x.Id),
				TotalLoans = userLoans.Where((UserLoan ul) => ul.UserId == x.Id).Sum((UserLoan ul) => ul.Amount),
				TotalRemain = userLoans.Where((UserLoan ul) => ul.UserId == x.Id).Sum((UserLoan ul) => ul.Amount) - userPayments.Where((UserLoanPayment up) => up.UserLoan.UserId == x.Id).Sum((UserLoanPayment up) => up.SumAmount)
			}).Cast<object>().ToList();
	}

	public async Task<List<dynamic>> RemainInstallmentCount()
	{
		return await (from x in _context.UserLoans.Include((UserLoan x) => x.Loan).Include((UserLoan x) => x.User)
			where x.User.CustomerId == _currentUserService.CustomerId && !x.IsCanceled
			orderby x.Remain / x.Installment descending
			select new
			{
				FirstName = x.User.FirstName,
				LastName = x.User.LastName,
				LoanTitle = x.Loan.Title,
				LoanAmount = x.Amount,
				StartDate = x.StartDate.ToPersianDate(),
				InstallmentsCount = x.Month,
				Remain = x.Remain,
				RemainCount = Math.Round(x.Remain / x.Installment)
			}).Cast<object>().ToListAsync();
	}

	public async Task<List<dynamic>> AllMembers()
	{
		return (from x in (await _userManager.GetUsersInRoleAsync("Member")).Where((User x) =>
			{
				Guid? customerId = x.CustomerId;
				Guid? customerId2 = _currentUserService.CustomerId;
				if (customerId.HasValue != customerId2.HasValue)
				{
					return false;
				}
				return !customerId.HasValue || customerId.GetValueOrDefault() == customerId2.GetValueOrDefault();
			}).ToList()
			select new { x.Id, x.FirstName, x.LastName, x.NationalCode, x.Gender, x.Mobile, x.TotalSavedMoney }).Cast<object>().ToList();
	}

	public async Task<List<dynamic>> MemberLoansByDate(DateTime? start, DateTime? end, bool? isCheckout)
	{
		IQueryable<UserLoan> source = (from x in _context.UserLoans.Include((UserLoan x) => x.Loan).Include((UserLoan x) => x.User).ThenInclude((User x) => x.Reliability)
				.Include((UserLoan x) => x.Agent)
				.Include((UserLoan x) => x.UserLoanPayments)
				.ThenInclude((UserLoanPayment x) => x.UserLoanPaymentOthers)
			where x.User.CustomerId == _currentUserService.CustomerId
			select x).AsNoTracking().AsQueryable();
		if (start.HasValue)
		{
			source = source.Where((UserLoan x) => x.StartDate >= start);
		}
		if (end.HasValue)
		{
			source = source.Where((UserLoan x) => x.StartDate <= end);
		}
		if (isCheckout == true)
		{
			source = source.Where((UserLoan x) => x.IsCheckouted == (bool?)true);
		}
		List<UserLoanModel> models = (await source.OrderBy((UserLoan x) => x.StartDate).ToListAsync()).Select((UserLoan x) => new UserLoanModel(x)
		{
			LoanPayments = x.UserLoanPayments.Select((UserLoanPayment p) => new LoanPaymentModel(p)).ToList(),
			PaidCount = x.UserLoanPayments.Count
		}).ToList();
		foreach (UserLoanModel item in models)
		{
			await _userPaymentService.SetPaymentsDelays(item.LoanPayments);
		}
		await _userLoanService.SetAppSms(models, ignoreText: false);
		return models.Select((UserLoanModel x) =>
		{
			List<LoanPaymentModel> loanPayments = x.LoanPayments;
			return new
			{
				Id = x.Id,
				AgentBusinessName = x.AgentModel?.BusinessName,
				MemberId = x.MemberModel.Id,
				FullName = x.MemberModel.FullName,
				Mobile = x.MemberModel.Mobile,
				UserName = x.MemberModel.UserName,
				NationalCode = x.MemberModel.NationalCode,
				Reliability = x.MemberModel.Reliability,
				LoanId = x.LoanModel.Id,
				Title = x.LoanModel.Title,
				Code = x.Code,
				Amount = x.Amount,
				Installment = x.Installment,
				Month = x.Month,
				IsCheckouted = x.IsCheckouted,
				CheckoutDate = x.CheckoutDate.ToPersianDate(),
				Paid = x.Paid,
				Remain = x.Remain,
				Delays = x.Delays,
				Discount = x.Discount,
				StartDate = x.StartDate.ToPersianDate(),
				PaidCount = ((x.IsCheckouted?.ToString() == "True") ? x.Month : loanPayments.Count((LoanPaymentModel loanPaymentModel) => loanPaymentModel.SumAmount > 0m)),
				UnpaidCount = ((!(x.IsCheckouted?.ToString() == "True")) ? loanPayments.Count((LoanPaymentModel c) => c.SumAmount == 0m) : 0),
				AppSmsList = x.AppSmsList
			};
		}).Cast<object>().ToList();
	}

	public async Task<List<dynamic>> SponsoredLoans(DateTime? start, DateTime? end)
	{
		List<Transaction> transactions = (from x in _context.Withdraws.AsNoTracking()
			where x.SponsoredLoanId != null && x.CustomerId == _currentUserService.CustomerId
			select x).ToList();
		IQueryable<UserLoan> source = from x in _context.UserLoans.Include((UserLoan x) => x.Loan).Include((UserLoan x) => x.User).AsNoTracking()
			where x.Loan.IsSponsored && x.User.CustomerId == _currentUserService.CustomerId
			select x;
		if (start.HasValue)
		{
			source = source.Where((UserLoan x) => x.StartDate.Date >= ((DateTime?)start).Value.Date);
		}
		if (end.HasValue)
		{
			source = source.Where((UserLoan x) => x.StartDate.Date <= ((DateTime?)end).Value.Date);
		}
		List<UserLoan> entities = await source.ToListAsync();
		List<Guid> sponsoredLoanIds = entities.Select((UserLoan x) => x.LoanId).Distinct().ToList();
		List<Loan> list = await (from x in _context.Loans.AsNoTracking()
			where sponsoredLoanIds.Contains(x.Id)
			select x).ToListAsync();
		List<object> list2 = new List<object>();
		foreach (Loan sl in list)
		{
			decimal num = entities.Where((UserLoan x) => x.LoanId == sl.Id).Sum((UserLoan x) => x.Amount);
			decimal num2 = transactions.Where((Transaction x) => x.SponsoredLoanId == sl.Id).Sum((Transaction x) => x.Amount);
			list2.Add(new
			{
				Id = sl.Id,
				Title = sl.Title,
				Count = entities.Count((UserLoan x) => x.LoanId == sl.Id),
				Sum = num,
				Paid = entities.Where((UserLoan x) => x.LoanId == sl.Id).Sum((UserLoan x) => x.Paid),
				SponsorePaid = num2,
				remain = num - num2
			});
		}
		return list2;
	}

	public async Task<List<TransactionModel>> SponsoredPaids(DateTime? start, DateTime? end, Guid loanId)
	{
		IQueryable<Transaction> source = from x in _context.Withdraws.AsNoTracking()
			where x.SponsoredLoanId == loanId && x.CustomerId == _currentUserService.CustomerId
			select x;
		if (start.HasValue)
		{
			source = source.Where((Transaction x) => x.SaveDate.Date >= ((DateTime?)start).Value.Date);
		}
		if (end.HasValue)
		{
			source = source.Where((Transaction x) => x.SaveDate.Date <= ((DateTime?)end).Value.Date);
		}
		return (await source.ToListAsync()).Select((Transaction x) => new TransactionModel(x)).ToList();
	}

	public async Task<List<dynamic>> SponsoredUserLoansByLoanId(DateTime? start, DateTime? end, Guid loanId)
	{
		IQueryable<UserLoan> source = from x in _context.UserLoans.Include((UserLoan x) => x.Loan).Include((UserLoan x) => x.User).AsNoTracking()
			where x.Loan.IsSponsored && x.LoanId == loanId && x.User.CustomerId == _currentUserService.CustomerId
			select x;
		if (start.HasValue)
		{
			source = source.Where((UserLoan x) => x.StartDate >= start);
		}
		if (end.HasValue)
		{
			source = source.Where((UserLoan x) => x.StartDate <= end);
		}
		List<UserLoan> list = await source.OrderBy((UserLoan x) => x.StartDate).ToListAsync();
		List<object> list2 = new List<object>();
		foreach (UserLoan item in list)
		{
			list2.Add(new
			{
				Id = item.Id,
				MemberId = item.UserId,
				FullName = item.User.FullName,
				Mobile = item.User.Mobile,
				NationalCode = item.User.NationalCode,
				Title = item.Loan.Title,
				Code = item.Code,
				Amount = item.Amount,
				Installment = item.Installment,
				Month = item.Month,
				IsCheckouted = item.IsCheckouted,
				CheckoutDate = item.CheckoutDate.ToPersianDate(),
				Paid = item.Paid,
				Remain = item.Remain,
				StartDate = item.StartDate.ToPersianDate()
			});
		}
		return list2;
	}

	public async Task<List<dynamic>> InstallmentsByMonth(DateTime? start, DateTime? end)
	{
		IQueryable<UserLoanPayment> source = (from x in _context.UserLoanPayments.Include((UserLoanPayment x) => x.UserLoan).ThenInclude((UserLoan x) => x.User).ThenInclude((User x) => x.Reliability)
				.Include((UserLoanPayment x) => x.UserLoan)
				.ThenInclude((UserLoan x) => x.Agent)
				.Include((UserLoanPayment x) => x.UserLoan)
				.ThenInclude((UserLoan x) => x.Loan)
				.AsNoTracking()
			where x.UserLoan.User.CustomerId == _currentUserService.CustomerId
			select x).AsQueryable();
		if (start.HasValue)
		{
			source = source.Where((UserLoanPayment x) => x.TargetMonth.Date >= ((DateTime?)start).Value.Date);
		}
		if (end.HasValue)
		{
			source = source.Where((UserLoanPayment x) => x.TargetMonth.Date <= ((DateTime?)end).Value.Date);
		}
		List<LoanPaymentModel> models = (await source.OrderByDescending((UserLoanPayment x) => x.TargetMonth).ToListAsync()).Select((UserLoanPayment x) => new LoanPaymentModel(x)).ToList();
		await _userPaymentService.SetAppSms(models, ignoreText: true);
		return models.Select((LoanPaymentModel x) => new
		{
			Id = x.Id,
			LoanPayment = x,
			UserLoan = x.UserLoanModel
		}).Cast<object>().ToList();
	}

	public async Task<List<dynamic>> PaidInstallmentsByMonth(DateTime? start, DateTime? end)
	{
		IQueryable<UserLoanPayment> source = (from x in _context.UserLoanPayments.Include((UserLoanPayment x) => x.UserLoan).ThenInclude((UserLoan x) => x.User).ThenInclude((User x) => x.Reliability)
				.Include((UserLoanPayment x) => x.UserLoan)
				.ThenInclude((UserLoan x) => x.Agent)
				.Include((UserLoanPayment x) => x.UserLoan)
				.ThenInclude((UserLoan x) => x.Loan)
				.AsNoTracking()
			where x.Amount > 0m && x.SaveDate != DateTime.MinValue && x.UserLoan.User.CustomerId == _currentUserService.CustomerId
			select x).AsQueryable();
		if (start.HasValue)
		{
			source = source.Where((UserLoanPayment x) => x.SaveDate.Date >= ((DateTime?)start).Value.Date);
		}
		if (end.HasValue)
		{
			source = source.Where((UserLoanPayment x) => x.SaveDate.Date <= ((DateTime?)end).Value.Date);
		}
		List<LoanPaymentModel> models = (await source.AsSplitQuery().ToListAsync()).Select((UserLoanPayment x) => new LoanPaymentModel(x)).ToList();
		IQueryable<UserLoanPaymentOther> source2 = (from x in _context.UserLoanPaymentOthers.Include((UserLoanPaymentOther x) => x.UserLoanPayment).ThenInclude((UserLoanPayment x) => x.UserLoan).ThenInclude((UserLoan x) => x.User)
				.ThenInclude((User x) => x.Reliability)
				.Include((UserLoanPaymentOther x) => x.UserLoanPayment)
				.ThenInclude((UserLoanPayment x) => x.UserLoan)
				.ThenInclude((UserLoan x) => x.Agent)
				.Include((UserLoanPaymentOther x) => x.UserLoanPayment)
				.ThenInclude((UserLoanPayment x) => x.UserLoan)
				.ThenInclude((UserLoan x) => x.Loan)
				.AsNoTracking()
			where x.Amount > 0m && x.SaveDate != DateTime.MinValue && x.UserLoanPayment.UserLoan.User.CustomerId == _currentUserService.CustomerId
			select x).AsQueryable();
		if (start.HasValue)
		{
			source2 = source2.Where((UserLoanPaymentOther x) => x.SaveDate.Date >= ((DateTime?)start).Value.Date);
		}
		if (end.HasValue)
		{
			source2 = source2.Where((UserLoanPaymentOther x) => x.SaveDate.Date <= ((DateTime?)end).Value.Date);
		}
		List<UserLoanPaymentOther> source3 = await source2.AsSplitQuery().ToListAsync();
		if (source3.Any())
		{
			models.AddRange(source3.Select((UserLoanPaymentOther other) => new LoanPaymentModel
			{
				Amount = other.Amount,
				Code = other.UserLoanPayment.Code,
				DelayDay = other.UserLoanPayment.DelayDay,
				DelayPrice = other.UserLoanPayment.DelayPrice,
				Description = other.Description,
				GoldRate = other.UserLoanPayment.GoldRate,
				GoldWeight = other.UserLoanPayment.GoldWeight,
				InstallmentAmount = other.UserLoanPayment.InstallmentAmount,
				InstallmentNumber = other.UserLoanPayment.InstallmentNumber,
				PaymentMethod = other.UserLoanPayment.PaymentMethod,
				SaveDate = other.SaveDate,
				SavedBy = other.SavedBy,
				IsGoldCheckouted = other.IsGoldCheckouted,
				Id = other.Id,
				TargetMonth = other.UserLoanPayment.TargetMonth,
				UserLoanModel = new UserLoanModel(other.UserLoanPayment.UserLoan)
			}));
		}
		models = models.OrderByDescending((LoanPaymentModel o) => o.SaveDate).ToList();
		return models.Select((LoanPaymentModel x) => new
		{
			Id = x.Id,
			LoanPayment = x,
			UserLoan = x.UserLoanModel
		}).Cast<object>().ToList();
	}

	public async Task<List<dynamic>> OrganizationDueDated(DateTime? start, DateTime? end)
	{
		IQueryable<UserLoanPayment> source = (from x in _context.UserLoanPayments.Include((UserLoanPayment x) => x.UserLoan).ThenInclude((UserLoan x) => x.User).Include((UserLoanPayment x) => x.UserLoan)
				.ThenInclude((UserLoan x) => x.Loan)
				.Include((UserLoanPayment x) => x.UserLoan)
				.ThenInclude((UserLoan x) => x.Assurances)
				.ThenInclude((Assurance x) => x.Organization)
				.AsNoTracking()
			where x.SumAmount == 0m && x.UserLoan.Assurances.Any((Assurance ass) => (int)ass.AssuranceType == 1 || (int)ass.AssuranceType == 2) == true && x.UserLoan.User.CustomerId == _currentUserService.CustomerId
			select x).AsQueryable();
		if (start.HasValue)
		{
			source = source.Where((UserLoanPayment x) => x.TargetMonth.Date >= ((DateTime?)start).Value.Date);
		}
		if (end.HasValue)
		{
			source = source.Where((UserLoanPayment x) => x.TargetMonth.Date <= ((DateTime?)end).Value.Date);
		}
		List<UserLoanPayment> list = await source.ToListAsync();
		List<dynamic> result = new List<object>();
		foreach (UserLoanPayment item in list)
		{
			item.UserLoan.Assurances.ForEach((Assurance ass) =>
			{
				if (ass.AssuranceType == AssuranceType.SelfOrganization || ass.AssuranceType == AssuranceType.OtherOrganization)
				{
					result.Add(new
					{
						UserLoanId = item.UserLoanId,
						UserLoanCode = item.UserLoan.Code,
						UserLoanAmount = item.UserLoan.Amount,
						UserLoanPayableAmount = item.UserLoan.PayableAmount,
						LoanTitle = item.UserLoan.Loan.Title,
						MemberId = item.UserLoan.User.Id,
						MemberFullName = item.UserLoan.User.FullName,
						MemberNationalCode = item.UserLoan.User.NationalCode,
						MemberMobile = item.UserLoan.User.Mobile,
						MemberUserName = item.UserLoan.User.UserName,
						TargetMonth = item.TargetMonth.ToPersianDate(),
						InstallmentAmount = item.InstallmentAmount,
						InstallmentNumber = item.InstallmentNumber,
						AssuranceType = ass.AssuranceType,
						AssurancePersonFullName = ((ass.AssuranceType == AssuranceType.OtherOrganization) ? ass.PersonFullName : item.UserLoan.User.FullName),
						AssurancePersonNationalCode = ((ass.AssuranceType == AssuranceType.OtherOrganization) ? ass.PersonNationalCode : item.UserLoan.User.NationalCode),
						AssurancePersonalCode = ass.PersonalCode,
						OrganizationName = ass.Organization.Name,
						OrganizationId = ass.Organization.Id
					});
				}
			});
		}
		result = (from x in result
			orderby x.OrganizationName, x.MemberId
			select x).ToList();
		return result;
	}

	public async Task<List<dynamic>> Assurances(DateTime? start, DateTime? end, long memberId)
	{
		IQueryable<Assurance> source = _context.Assurances.Include((Assurance x) => x.UserLoan).ThenInclude((UserLoan x) => x.User).Include((Assurance x) => x.UserLoan)
			.ThenInclude((UserLoan x) => x.Loan)
			.Include((Assurance x) => x.Organization)
			.Include((Assurance x) => x.AssuranceChequeItems)
			.AsNoTracking()
			.AsQueryable();
		if (memberId != 0L)
		{
			source = source.Where((Assurance x) => x.UserLoan.UserId == memberId);
		}
		if (start.HasValue)
		{
			source = source.Where((Assurance x) => x.UserLoan.StartDate >= ((DateTime?)start).Value.Date);
		}
		if (end.HasValue)
		{
			source = source.Where((Assurance x) => x.UserLoan.StartDate <= ((DateTime?)end).Value.Date);
		}
		try
		{
			List<Assurance> list = await source.ToListAsync();
			List<object> list2 = new List<object>();
			foreach (Assurance item in list)
			{
				if (item.AssuranceType == AssuranceType.Cheque)
				{
					foreach (AssuranceChequeItem assuranceChequeItem in item.AssuranceChequeItems)
					{
						list2.Add(new AssuranceReportModel
						{
							Cheque = new AssuranceChequeItemModel(assuranceChequeItem),
							UserLoanId = item.UserLoanId,
							UserLoanCode = item.UserLoan.Code,
							UserLoanAmount = item.UserLoan.Amount,
							UserLoanPayableAmount = item.UserLoan.PayableAmount,
							LoanTitle = item.UserLoan.Loan.Title,
							MemberId = item.UserLoan.User.Id,
							MemberFullName = item.UserLoan.User.FullName,
							MemberNationalCode = item.UserLoan.User.NationalCode,
							MemberMobile = item.UserLoan.User.Mobile,
							MemberUserName = item.UserLoan.User.UserName,
							InstallmentAmount = item.UserLoan.Installment,
							AssuranceType = item.AssuranceType,
							IsReturned = item.IsReturned,
							AssurancePersonFullName = item.PersonFullName,
							AssurancePersonNationalCode = item.PersonNationalCode,
							AssurancePersonalCode = "",
							OrganizationName = "",
							OrganizationId = null,
							Description = item.Description
						});
					}
				}
				else
				{
					list2.Add(new AssuranceReportModel
					{
						UserLoanId = item.UserLoanId,
						UserLoanCode = item.UserLoan.Code,
						UserLoanAmount = item.UserLoan.Amount,
						UserLoanPayableAmount = item.UserLoan.PayableAmount,
						LoanTitle = item.UserLoan.Loan.Title,
						MemberId = item.UserLoan.User.Id,
						MemberFullName = item.UserLoan.User.FullName,
						MemberNationalCode = item.UserLoan.User.NationalCode,
						MemberMobile = item.UserLoan.User.Mobile,
						MemberUserName = item.UserLoan.User.UserName,
						InstallmentAmount = item.UserLoan.Installment,
						AssuranceType = item.AssuranceType,
						IsReturned = item.IsReturned,
						AssurancePersonFullName = ((item.AssuranceType == AssuranceType.OtherOrganization) ? item.PersonFullName : ((item.AssuranceType == AssuranceType.SelfOrganization) ? item.UserLoan.User.FullName : "")),
						AssurancePersonNationalCode = ((item.AssuranceType == AssuranceType.OtherOrganization) ? item.PersonNationalCode : ((item.AssuranceType == AssuranceType.SelfOrganization) ? item.UserLoan.User.NationalCode : "")),
						AssurancePersonalCode = item.PersonalCode,
						OrganizationName = item.Organization?.Name,
						OrganizationId = item.Organization?.Id,
						GoldWeight = item.GoldWeight,
						CoinType = item.CoinType,
						Description = item.Description
					});
				}
			}
			return (from x in list2
				orderby x.OrganizationName, x.MemberId
				select x).ToList();
		}
		catch (Exception)
		{
			throw;
		}
	}

	public List<dynamic> SaveMoniesByMonth(DateTime? start, DateTime? end)
	{
		IQueryable<UserSavedMoney> source = from x in _context.UserSavedMonies.AsNoTracking().Include((UserSavedMoney x) => x.Member)
			where x.Member.CustomerId == _currentUserService.CustomerId
			select x;
		if (start.HasValue)
		{
			source = source.Where((UserSavedMoney x) => x.TargetMonth >= start);
		}
		if (end.HasValue)
		{
			source = source.Where((UserSavedMoney x) => x.TargetMonth <= end);
		}
		List<object> source2 = (from x in (from x in source
				where x.TargetMonth != DateTime.MinValue && x.FileMonth != null
				orderby x.TargetMonth descending
				group x by x.TargetMonth into x
				select new
				{
					Key = x.Key,
					value = x.Sum((UserSavedMoney c) => c.Amount)
				}).ToList()
			select new
			{
				month = x.Key.ToPersianDateObject().Year + "/" + x.Key.ToPersianDateObject().Month,
				value = x.value
			}).Cast<object>().ToList();
		List<object> source3 = (from x in (from x in source
				where x.TargetMonth != DateTime.MinValue && x.FileMonth == null
				orderby x.TargetMonth descending
				group x by x.TargetMonth into x
				select new
				{
					Key = x.Key,
					value = x.Sum((UserSavedMoney c) => c.Amount)
				}).ToList()
			select new
			{
				month = x.Key.ToPersianDateObject().Year + "/" + x.Key.ToPersianDateObject().Month,
				value = x.value
			}).Cast<object>().ToList();
		List<string> list = new List<string>();
		list.AddRange(source2.Select((dynamic x) => (string)x.month).ToList());
		list.AddRange(source3.Select((dynamic x) => (string)x.month).ToList());
		List<string> list2 = list.Distinct().ToList();
		List<object> list3 = new List<object>();
		foreach (string month in list2)
		{
			list3.Add(new
			{
				month = month,
				valueByFile = (object)(source2.FirstOrDefault((dynamic x) => x.month == month)?.value ?? ((object)0)),
				valueNoFile = (object)(source3.FirstOrDefault((dynamic x) => x.month == month)?.value ?? ((object)0)),
				value = (int)(source2.FirstOrDefault((dynamic x) => x.month == month)?.value ?? ((object)0)) + (int)(source3.FirstOrDefault((dynamic x) => x.month == month)?.value ?? ((object)0))
			});
		}
		return list3;
	}

	public async Task<dynamic> MonthlyBalance(string pDate, int saveType)
	{
		pDate = pDate.Replace("-", "/");
		string persianDate = pDate + "/01";
		DateTime start = persianDate.ToGregorianDateTime().Value;
		PersianDate persianDate2 = start.ToPersianDateObject();
		string persianDate3;
		if (persianDate2.Month <= 6)
		{
			persianDate3 = pDate + "/31";
		}
		else
		{
			persianDate3 = ((persianDate2.Month != 12) ? (pDate + "/30") : (pDate + "/29"));
		}
		DateTime end = persianDate3.ToGregorianDateTime().Value;
		IQueryable<UserSavedMoney> source = (from x in _context.UserSavedMonies.AsNoTracking().Include((UserSavedMoney x) => x.Member)
			where x.Member.CustomerId == _currentUserService.CustomerId
			select x).AsQueryable();
		switch (saveType)
		{
		case 1:
			source = source.Where((UserSavedMoney x) => !string.IsNullOrEmpty(x.FileMonth));
			break;
		case 2:
			source = source.Where((UserSavedMoney x) => string.IsNullOrEmpty(x.FileMonth));
			break;
		}
		List<UserSavedMoney> savedMonies = await source.ToListAsync();
		var source2 = from x in _context.UserLoanPayments.AsNoTracking().Include((UserLoanPayment x) => x.UserLoan).ThenInclude((UserLoan x) => x.User)
				.Include((UserLoanPayment x) => x.UserLoan)
				.ThenInclude((UserLoan x) => x.Loan)
			where x.UserLoan.User.CustomerId == _currentUserService.CustomerId && !x.UserLoan.IsCanceled
			select new { x.UserLoanId, x.UserLoan, x.Id, x.SumAmount, x.TargetMonth, x.FileMonth };
		switch (saveType)
		{
		case 1:
			source2 = source2.Where(x => !string.IsNullOrEmpty(x.FileMonth));
			break;
		case 2:
			source2 = source2.Where(x => string.IsNullOrEmpty(x.FileMonth));
			break;
		}
		var payments = await source2.ToListAsync();
		List<UserLoan> userLoans = (from x in payments
			where x.TargetMonth >= start && x.TargetMonth <= end
			group x by x.UserLoanId into x
			select x.FirstOrDefault().UserLoan).ToList();
		List<object> items = new List<object>();
		foreach (UserLoan userLoan in userLoans.OrderBy((UserLoan x) => x.UserId))
		{
			items.Add(new
			{
				FullName = userLoan.User?.FullName,
				SavedMoney = savedMonies.Where((UserSavedMoney x) => x.TargetMonth >= start && x.TargetMonth <= end && x.MemberId == userLoan.UserId).Sum((UserSavedMoney x) => x.Amount),
				SavedMoneyTotal = savedMonies.Where((UserSavedMoney x) => x.TargetMonth <= end && x.MemberId == userLoan.UserId).Sum((UserSavedMoney x) => x.Amount),
				LoanTitle = userLoan.Loan?.Title,
				LoanAmount = userLoan.Amount,
				Payments = payments.Where(x => x.UserLoanId == userLoan.Id && x.TargetMonth >= start && x.TargetMonth <= end).Sum(x => x.SumAmount),
				PaymentsTotal = payments.Where(x => x.TargetMonth <= end && x.UserLoanId == userLoan.Id).Sum(x => x.SumAmount),
				Remain = userLoan.Amount - payments.Where(x => x.TargetMonth <= end && x.UserLoanId == userLoan.Id).Sum(x => x.SumAmount)
			});
		}
		foreach (User member in (from x in (from x in savedMonies
				where x.TargetMonth >= start && x.TargetMonth <= end
				select x.Member).ToList()
			where !userLoans.Select((UserLoan ul) => ul.UserId).ToList().Contains(x.Id)
			select x).ToList())
		{
			items.Add(new
			{
				FullName = member.FullName,
				SavedMoney = savedMonies.Where((UserSavedMoney x) => x.TargetMonth >= start && x.TargetMonth <= end && x.MemberId == member.Id).Sum((UserSavedMoney x) => x.Amount),
				SavedMoneyTotal = savedMonies.Where((UserSavedMoney x) => x.TargetMonth <= end && x.MemberId == member.Id).Sum((UserSavedMoney x) => x.Amount),
				LoanTitle = "",
				LoanAmount = 0,
				Payments = 0,
				PaymentsTotal = 0,
				Remain = 0
			});
		}
		decimal monthPayments = payments.Where(x => x.TargetMonth >= start && x.TargetMonth <= end).Sum(x => x.SumAmount);
		decimal monthSavedMonies = savedMonies.Where((UserSavedMoney x) => x.TargetMonth >= start && x.TargetMonth <= end).Sum((UserSavedMoney x) => x.Amount);
		decimal totalSavedMonies = savedMonies.Where((UserSavedMoney x) => x.TargetMonth <= end).Sum((UserSavedMoney x) => x.Amount);
		decimal num = await (from x in _context.UserLoans.Include((UserLoan x) => x.User).AsNoTracking()
			where x.StartDate <= end && x.User.CustomerId == _currentUserService.CustomerId && !x.IsCanceled
			select x).SumAsync((UserLoan x) => x.Amount);
		decimal num2 = payments.Where(x => x.UserLoan.StartDate <= end && x.TargetMonth <= end).Sum(x => x.SumAmount);
		decimal num3 = num - num2;
		decimal balance = totalSavedMonies - (monthPayments + monthSavedMonies + num3);
		return new
		{
			items = items,
			balance = balance,
			monthPayments = monthPayments,
			monthSavedMonies = monthSavedMonies,
			totalUnpaid = num3,
			totalSavedMonies = totalSavedMonies
		};
	}

	public async Task<List<string>> GetMonths()
	{
		List<string> months = new List<string>();
		List<DateTime> dates = new List<DateTime>();
		List<DateTime> list = dates;
		list.AddRange((await _userPaymentService.GetAllUserPayments()).Select((UserLoanPayment x) => x.TargetMonth).ToList());
		list = dates;
		list.AddRange((await _userSavedMoneyService.GetAll()).Select((UserSavedMoney x) => x.TargetMonth).ToList());
		dates = (from x in dates.Where((DateTime x) => x != DateTime.MinValue).Distinct()
			orderby x
			select x).ToList();
		foreach (DateTime item in dates)
		{
			PersianDate persianDate = item.ToPersianDateObject();
			months.Add($"{persianDate.Year}-{persianDate.Month}");
		}
		return months;
	}

	public async Task<OverDueInstallmentsModel> GetOverDueInstallments(long? agentId = null)
	{
		return await _userPaymentService.GetOverDueInstallments(loadAppSms: true, agentId);
	}

	public async Task<EmailReportModel> GetDailyReport(AppSettingsModel appSettingsModel)
	{
		PersianDate persianDate = DateTime.Now.ToPersianDateObject();
		bool weeklyMode = persianDate.DayOfWeek == DayOfWeek.Friday;
		bool monthlyMode = (persianDate.Month <= 6 && persianDate.Day == 31) || (persianDate.Month > 6 && persianDate.Day == 30) || (persianDate.Month == 12 && persianDate.Day == 29);
		DateTime today = DateTime.Now;
		DateTime startDate_week = today.AddDays(-7.0);
		DateTime startDate_month = today.AddDays(persianDate.Day * -1);
		EmailReportModel emailReportModel = new EmailReportModel();
		EmailPropsReportModel daily = emailReportModel.Daily;
		daily.LoanRequestCount = await _userLoanRequestService.GetRequestCountByDateAsync(today, today);
		daily = emailReportModel.Daily;
		daily.DueDatedLoanPaymentCount = await _userPaymentService.GetDueDatedCountByDateAsync(today, today);
		daily = emailReportModel.Daily;
		daily.OverDueDatedLoanPaymentCount = await _userPaymentService.GetOverDueDatedCountByDateAsync(today, today);
		daily = emailReportModel.Daily;
		daily.LoanCount = await _userLoanService.GetLoanCountByDateAsync(today, today);
		daily = emailReportModel.Daily;
		daily.MemberCount = (await _memberService.AsQueryable()).Count((User x) => x.RegDate2 == today);
		List<GroupedTicketReportModel> source = await _ticketService.GetGroupedCountByDateAsync(today, today);
		emailReportModel.Daily.Ticket = source.Sum((GroupedTicketReportModel x) => x.Count);
		emailReportModel.Daily.ClosedTicket = source.FirstOrDefault((GroupedTicketReportModel x) => x.Status == TicketStatus.بسته_شده)?.Count ?? 0;
		emailReportModel.Daily.RepliedTicket = source.FirstOrDefault((GroupedTicketReportModel x) => x.Status == TicketStatus.پاسخ_داده_شده)?.Count ?? 0;
		List<GroupedPurchaseReportModel> source2 = await _memberPurchaseService.GetGroupedOnTypeByDateAsync(today, today);
		emailReportModel.Daily.OnlinePayment = source2.FirstOrDefault((GroupedPurchaseReportModel x) => x.Type == MemberPurchaseTypes.LoanPayment)?.Count ?? 0;
		emailReportModel.Daily.WalletCharge = source2.FirstOrDefault((GroupedPurchaseReportModel x) => x.Type == MemberPurchaseTypes.WalletCredit)?.Count ?? 0;
		emailReportModel.Daily.OnlinePaymentPrice = source2.FirstOrDefault((GroupedPurchaseReportModel x) => x.Type == MemberPurchaseTypes.LoanPayment)?.Sum ?? 0m;
		emailReportModel.Daily.WalletChargePrice = source2.FirstOrDefault((GroupedPurchaseReportModel x) => x.Type == MemberPurchaseTypes.WalletCredit)?.Sum ?? 0m;
		List<GroupedUserPaymentReportModel> source3 = await _userPaymentService.GetGroupedOnMethodByDateAsync(today, today);
		emailReportModel.Daily.CashPayment = source3.FirstOrDefault((GroupedUserPaymentReportModel x) => x.PaymentMethod == PaymentMethods.Cash)?.Count ?? 0;
		emailReportModel.Daily.CashPaymentPrice = source3.FirstOrDefault((GroupedUserPaymentReportModel x) => x.PaymentMethod == PaymentMethods.Cash)?.Sum ?? 0m;
		emailReportModel.Daily.BankPayment = source3.FirstOrDefault((GroupedUserPaymentReportModel x) => x.PaymentMethod == PaymentMethods.Bank)?.Count ?? 0;
		emailReportModel.Daily.BankPaymentPrice = source3.FirstOrDefault((GroupedUserPaymentReportModel x) => x.PaymentMethod == PaymentMethods.Bank)?.Sum ?? 0m;
		GroupedTransactionReportModel groupedTransactionReportModel = await _transactionService.GetCountAndSumAgentReportByDateAsync(today, today);
		emailReportModel.Daily.AgentTransaction = groupedTransactionReportModel.Count;
		emailReportModel.Daily.AgentTransactionPrice = groupedTransactionReportModel.Sum;
		daily = emailReportModel.Daily;
		daily.SponsorDueDate = (await _userLoanRequestService.GetByCondition((UserLoanRequest x) => x.StatusId != null && Enumerable.Contains(appSettingsModel.SponsoreDueDateStatuses, ((object)x.StatusId).ToString()) && x.SponsoreDueDate != DateTime.MinValue && today == x.SponsoreDueDate.Date, false)).Count;
		if (weeklyMode)
		{
			daily = emailReportModel.Weekly;
			daily.LoanRequestCount = await _userLoanRequestService.GetRequestCountByDateAsync(startDate_week, today);
			daily = emailReportModel.Weekly;
			daily.DueDatedLoanPaymentCount = await _userPaymentService.GetDueDatedCountByDateAsync(startDate_week, today);
			daily = emailReportModel.Weekly;
			daily.OverDueDatedLoanPaymentCount = await _userPaymentService.GetOverDueDatedCountByDateAsync(startDate_week, today);
			daily = emailReportModel.Weekly;
			daily.LoanCount = await _userLoanService.GetLoanCountByDateAsync(startDate_week, today);
			daily = emailReportModel.Weekly;
			daily.MemberCount = (await _memberService.AsQueryable()).Count((User x) => x.RegDate2 >= startDate_week && x.RegDate2 <= today);
			List<GroupedTicketReportModel> source4 = await _ticketService.GetGroupedCountByDateAsync(startDate_week, today);
			emailReportModel.Weekly.Ticket = source4.Sum((GroupedTicketReportModel x) => x.Count);
			emailReportModel.Weekly.ClosedTicket = source4.FirstOrDefault((GroupedTicketReportModel x) => x.Status == TicketStatus.بسته_شده)?.Count ?? 0;
			emailReportModel.Weekly.RepliedTicket = source4.FirstOrDefault((GroupedTicketReportModel x) => x.Status == TicketStatus.پاسخ_داده_شده)?.Count ?? 0;
			List<GroupedPurchaseReportModel> source5 = await _memberPurchaseService.GetGroupedOnTypeByDateAsync(startDate_week, today);
			emailReportModel.Weekly.OnlinePayment = source5.FirstOrDefault((GroupedPurchaseReportModel x) => x.Type == MemberPurchaseTypes.LoanPayment)?.Count ?? 0;
			emailReportModel.Weekly.WalletCharge = source5.FirstOrDefault((GroupedPurchaseReportModel x) => x.Type == MemberPurchaseTypes.WalletCredit)?.Count ?? 0;
			emailReportModel.Weekly.OnlinePaymentPrice = source5.FirstOrDefault((GroupedPurchaseReportModel x) => x.Type == MemberPurchaseTypes.LoanPayment)?.Sum ?? 0m;
			emailReportModel.Weekly.WalletChargePrice = source5.FirstOrDefault((GroupedPurchaseReportModel x) => x.Type == MemberPurchaseTypes.WalletCredit)?.Sum ?? 0m;
			List<GroupedUserPaymentReportModel> source6 = await _userPaymentService.GetGroupedOnMethodByDateAsync(startDate_week, today);
			emailReportModel.Weekly.CashPayment = source6.FirstOrDefault((GroupedUserPaymentReportModel x) => x.PaymentMethod == PaymentMethods.Cash)?.Count ?? 0;
			emailReportModel.Weekly.CashPaymentPrice = source6.FirstOrDefault((GroupedUserPaymentReportModel x) => x.PaymentMethod == PaymentMethods.Cash)?.Sum ?? 0m;
			emailReportModel.Weekly.BankPayment = source6.FirstOrDefault((GroupedUserPaymentReportModel x) => x.PaymentMethod == PaymentMethods.Bank)?.Count ?? 0;
			emailReportModel.Weekly.BankPaymentPrice = source6.FirstOrDefault((GroupedUserPaymentReportModel x) => x.PaymentMethod == PaymentMethods.Bank)?.Sum ?? 0m;
			GroupedTransactionReportModel groupedTransactionReportModel2 = await _transactionService.GetCountAndSumAgentReportByDateAsync(startDate_week, today);
			emailReportModel.Weekly.AgentTransaction = groupedTransactionReportModel2.Count;
			emailReportModel.Weekly.AgentTransactionPrice = groupedTransactionReportModel2.Sum;
			daily = emailReportModel.Weekly;
			daily.SponsorDueDate = (await _userLoanRequestService.GetByCondition((UserLoanRequest x) => x.StatusId != null && Enumerable.Contains(appSettingsModel.SponsoreDueDateStatuses, ((object)x.StatusId).ToString()) && x.SponsoreDueDate != DateTime.MinValue && x.SponsoreDueDate.Date >= startDate_week && x.SponsoreDueDate.Date <= today, false)).Count;
		}
		if (monthlyMode)
		{
			daily = emailReportModel.Monthly;
			daily.LoanRequestCount = await _userLoanRequestService.GetRequestCountByDateAsync(startDate_month, today);
			daily = emailReportModel.Monthly;
			daily.DueDatedLoanPaymentCount = await _userPaymentService.GetDueDatedCountByDateAsync(startDate_month, today);
			daily = emailReportModel.Monthly;
			daily.OverDueDatedLoanPaymentCount = await _userPaymentService.GetOverDueDatedCountByDateAsync(startDate_month, today);
			daily = emailReportModel.Monthly;
			daily.LoanCount = await _userLoanService.GetLoanCountByDateAsync(startDate_month, today);
			daily = emailReportModel.Monthly;
			daily.MemberCount = (await _memberService.AsQueryable()).Count((User x) => x.RegDate2 >= startDate_month && x.RegDate2 <= today);
			List<GroupedTicketReportModel> source7 = await _ticketService.GetGroupedCountByDateAsync(startDate_month, today);
			emailReportModel.Monthly.Ticket = source7.Sum((GroupedTicketReportModel x) => x.Count);
			emailReportModel.Monthly.ClosedTicket = source7.FirstOrDefault((GroupedTicketReportModel x) => x.Status == TicketStatus.بسته_شده)?.Count ?? 0;
			emailReportModel.Monthly.RepliedTicket = source7.FirstOrDefault((GroupedTicketReportModel x) => x.Status == TicketStatus.پاسخ_داده_شده)?.Count ?? 0;
			List<GroupedPurchaseReportModel> source8 = await _memberPurchaseService.GetGroupedOnTypeByDateAsync(startDate_month, today);
			emailReportModel.Monthly.OnlinePayment = source8.FirstOrDefault((GroupedPurchaseReportModel x) => x.Type == MemberPurchaseTypes.LoanPayment)?.Count ?? 0;
			emailReportModel.Monthly.WalletCharge = source8.FirstOrDefault((GroupedPurchaseReportModel x) => x.Type == MemberPurchaseTypes.WalletCredit)?.Count ?? 0;
			emailReportModel.Monthly.OnlinePaymentPrice = source8.FirstOrDefault((GroupedPurchaseReportModel x) => x.Type == MemberPurchaseTypes.LoanPayment)?.Sum ?? 0m;
			emailReportModel.Monthly.WalletChargePrice = source8.FirstOrDefault((GroupedPurchaseReportModel x) => x.Type == MemberPurchaseTypes.WalletCredit)?.Sum ?? 0m;
			List<GroupedUserPaymentReportModel> source9 = await _userPaymentService.GetGroupedOnMethodByDateAsync(startDate_month, today);
			emailReportModel.Monthly.CashPayment = source9.FirstOrDefault((GroupedUserPaymentReportModel x) => x.PaymentMethod == PaymentMethods.Cash)?.Count ?? 0;
			emailReportModel.Monthly.CashPaymentPrice = source9.FirstOrDefault((GroupedUserPaymentReportModel x) => x.PaymentMethod == PaymentMethods.Cash)?.Sum ?? 0m;
			emailReportModel.Monthly.BankPayment = source9.FirstOrDefault((GroupedUserPaymentReportModel x) => x.PaymentMethod == PaymentMethods.Bank)?.Count ?? 0;
			emailReportModel.Monthly.BankPaymentPrice = source9.FirstOrDefault((GroupedUserPaymentReportModel x) => x.PaymentMethod == PaymentMethods.Bank)?.Sum ?? 0m;
			GroupedTransactionReportModel groupedTransactionReportModel3 = await _transactionService.GetCountAndSumAgentReportByDateAsync(startDate_month, today);
			emailReportModel.Monthly.AgentTransaction = groupedTransactionReportModel3.Count;
			emailReportModel.Monthly.AgentTransactionPrice = groupedTransactionReportModel3.Sum;
			daily = emailReportModel.Monthly;
			daily.SponsorDueDate = (await _userLoanRequestService.GetByCondition((UserLoanRequest x) => x.StatusId != null && Enumerable.Contains(appSettingsModel.SponsoreDueDateStatuses, ((object)x.StatusId).ToString()) && x.SponsoreDueDate != DateTime.MinValue && x.SponsoreDueDate.Date >= startDate_month && x.SponsoreDueDate.Date <= today, false)).Count;
		}
		return emailReportModel;
	}

	public async Task<List<dynamic>> LoanStatistics(DateTime? start, DateTime? end)
	{
		_context.Loans.Where((Loan x) => x.CustomerId == _currentUserService.CustomerId).Load();
		IQueryable<UserLoan> query = _context.UserLoans.Where((UserLoan x) => x.Loan.CustomerId == _currentUserService.CustomerId && !x.IsCanceled).AsQueryable();
		if (start.HasValue)
		{
			query = query.Where((UserLoan x) => x.StartDate.Date >= ((DateTime?)start).Value.Date);
		}
		if (end.HasValue)
		{
			query = query.Where((UserLoan x) => x.StartDate.Date <= ((DateTime?)end).Value.Date);
		}
		_context.UserLoanPayments.Where((UserLoanPayment x) => query.Select((UserLoan q) => q.Id).Contains(x.UserLoanId)).Load();
		byte percent = (await _appSettingsService.GetAppSettings())?.PercentOfConsideringAsPaid ?? 50;
		List<object> result = new List<object>();
		try
		{
			List<UserLoan> userLoanEntities = await query.ToListAsync();
			List<Loan> list = _context.Loans.ToList();
			foreach (Loan loan in list)
			{
				List<UserLoan> userLoans = userLoanEntities.Where((UserLoan x) => x.LoanId == loan.Id).ToList();
				List<UserLoanModel> models = userLoans.Select((UserLoan x) => new UserLoanModel(x)
				{
					LoanPayments = x.UserLoanPayments.Select((UserLoanPayment p) => new LoanPaymentModel(p)).ToList()
				}).ToList();
				foreach (UserLoanModel item in models)
				{
					await _userPaymentService.SetPaymentsDelays(item.LoanPayments);
				}
				decimal num = userLoans.Sum((UserLoan x) => x.PayableAmount);
				decimal num2 = models.Sum((UserLoanModel x) => x.Delays) + num - models.Sum((UserLoanModel x) => x.Discount);
				decimal num3 = userLoans.SelectMany((UserLoan x) => x.UserLoanPayments).Sum((UserLoanPayment x) => x.SumAmount);
				int overDueCount = (from x in userLoans.SelectMany((UserLoan x) => x.UserLoanPayments)
					where DateTime.Now.Date >= x.TargetMonth.Date && (!x.UserLoan.IsCheckouted.HasValue || !x.UserLoan.IsCheckouted.Value) && (x.SumAmount == 0m || x.SumAmount < x.InstallmentAmount * (decimal)percent / 100m)
					select x).Count();
				decimal overDueAmount = (from x in userLoans.SelectMany((UserLoan x) => x.UserLoanPayments)
					where DateTime.Now.Date >= x.TargetMonth.Date && (!x.UserLoan.IsCheckouted.HasValue || !x.UserLoan.IsCheckouted.Value) && (x.SumAmount == 0m || x.SumAmount < x.InstallmentAmount * (decimal)percent / 100m)
					select x).Sum((UserLoanPayment x) => x.InstallmentAmount);
				result.Add(new
				{
					LoanTitle = loan.Title,
					MemberCount = userLoans.Select((UserLoan x) => x.UserId).Distinct().Count(),
					TotalLoans = userLoans.Sum((UserLoan x) => x.Amount),
					TotalProfitLoans = num,
					TotalProfitDelayLoans = num2,
					TotalPaid = num3,
					TotalUnpaid = num2 - num3,
					TotalNonDueDated = (from x in userLoans.Where((UserLoan x) => !x.IsCheckouted.HasValue || !x.IsCheckouted.Value).SelectMany((UserLoan x) => x.UserLoanPayments)
						where x.TargetMonth.Date > DateTime.Now.Date && x.SumAmount == 0m
						select x).Sum((UserLoanPayment x) => x.InstallmentAmount),
					AllCheckoutedCount = userLoans.Where((UserLoan x) => x.IsCheckouted == true).Count(),
					AllLoansCount = userLoans.Count(),
					OverDueCount = overDueCount,
					OverDueAmount = overDueAmount
				});
			}
			return result;
		}
		catch (Exception)
		{
			throw;
		}
	}
}
