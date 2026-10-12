using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoanSystem.Core.Configuration;
using LoanSystem.Data;
using LoanSystem.Data.Entities;
using LoanSystemAvizhe.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Spm.Services.Infrastructure;

namespace LoanSystem.Services.Base;

public class AppSettingsService : BaseService, IAppSettingsService
{
	private readonly AppConfig _appConfig;

	public AppSettingsService(AppDbContext context, ICurrentUserService currentUserService, IOptions<AppConfig> appConfig)
		: base(context, currentUserService)
	{
		_appConfig = appConfig.Value;
	}

	public async Task SyncChargeSmsCredit(Guid? customerId = null)
	{
		if (_appConfig.IsCloud)
		{
			if (!customerId.HasValue)
			{
				customerId = _currentUserService.CustomerId.Value;
			}
			decimal sumSmsChargePurchase = await _context.Purchases.Where((Purchase x) => x.CustomerId == customerId && (int)x.ProductType == 2 && x.IsPaymentSuccess).SumAsync((Purchase x) => x.Amount);
			decimal sumSms = (await _context.AppSms.Where((AppSms x) => x.CustomerId == customerId && (int)x.Status != 3).ToListAsync()).Where((AppSms x) => x.SmsPanel.ToString().ToLower().Contains("jet")).Sum((AppSms x) => x.Price);
			(await _context.AppSettings.FirstOrDefaultAsync((AppSettings x) => x.CustomerId == customerId)).SmsCredit = (int)(sumSmsChargePurchase - sumSms);
			await _context.SaveChangesAsync();
		}
	}

	public async Task SyncChargeInquiryCredit(Guid? customerId = null)
	{
		if (_appConfig.IsCloud)
		{
			if (!customerId.HasValue)
			{
				customerId = _currentUserService.CustomerId.Value;
			}
			decimal sumInquiryChargePurchase = await _context.Purchases.Where((Purchase x) => x.CustomerId == customerId && (int)x.ProductType == 3 && x.IsPaymentSuccess).SumAsync((Purchase x) => x.Amount);
			decimal sum = await _context.AppInquirys.Where((AppInquiry x) => x.CustomerId == customerId && x.IsSuccess).SumAsync((AppInquiry x) => x.Price);
			(await _context.AppSettings.FirstOrDefaultAsync((AppSettings x) => x.CustomerId == customerId)).InquiryCredit = (int)(sumInquiryChargePurchase - sum);
			await _context.SaveChangesAsync();
		}
	}

	public Task<List<AppSettings>> GetByCondition(Expression<Func<AppSettings, bool>> condition, bool ignoreGlobalFilter, params string[] includs)
	{
		IQueryable<AppSettings> source = Table<AppSettings>();
		if (ignoreGlobalFilter)
		{
			source = source.IgnoreQueryFilters();
		}
		foreach (string navigationPropertyPath in includs)
		{
			source = source.Include(navigationPropertyPath);
		}
		source = source.Where(condition);
		return source.ToListAsync();
	}

	public Task<AppSettings> GetAppSettings()
	{
		if (!_currentUserService.CustomerId.HasValue)
		{
			return Task.FromResult<AppSettings>(null);
		}
		return GetAppSettings(_currentUserService.CustomerId.Value);
	}

	public Task<AppSettings> GetAppSettings(Guid customerId)
	{
		IQueryable<AppSettings> query = Table<AppSettings>();
		if (_currentUserService.IsInRole("SuperAdmin"))
		{
			query = query.IgnoreQueryFilters();
		}
		else if (!_currentUserService.CustomerId.HasValue || _currentUserService.CustomerId.Value != customerId)
		{
			throw new UnauthorizedAccessException("You cannot access another customer's application settings.");
		}

		return query
			.Include(x => x.NewUserLoanSmsPattern)
			.Include(x => x.ThanksPaymentSmsPattern)
			.Include(x => x.DueDateRememberSmsPattern)
			.Include(x => x.OverDueDateSmsPattern)
			.Include(x => x.BeforeDueDateSmsPattern)
			.Include(x => x.LoginSmsPattern)
			.Include(x => x.LoginEmailPattern)
			.Include(x => x.RegisterSmsPattern)
			.Include(x => x.RegisterOtpSmsPattern)
			.Include(x => x.WalletChargeSmsPattern)
			.Include(x => x.NewOrderSmsPattern)
			.Include(x => x.NewOrderEmailPattern)
			.Include(x => x.OverDueDateEmailPattern)
			.Include(x => x.RegisterEmailPattern)
			.Include(x => x.RegisterOtpEmailPattern)
			.Include(x => x.ThanksPaymentEmailPattern)
			.Include(x => x.WalletChargeEmailPattern)
			.Include(x => x.DueDateRememberEmailPattern)
			.Include(x => x.BeforeDueDateEmailPattern)
			.Include(x => x.WalletWithdrawOtpSmsPattern)
			.Include(x => x.WalletWithdrawNotificationSmsPattern)
			.Include(x => x.AgentFactorPaymentLinkSmsPattern)
			.Include(x => x.DigitalSignLinkSmsPattern)
			.Include(x => x.DigitalSignCompleteSmsPattern)
			.Include(x => x.DigitalSignOtpSmsPattern)
			.Include(x => x.ThirdPersonDigitalSignLinkSmsPattern)
			.Include(x => x.ThirdPersonDigitalSignCompleteSmsPattern)
			.Include(x => x.ThirdPersonDigitalSignOtpSmsPattern)
			.AsSplitQuery()
			.FirstOrDefaultAsync(x => x.CustomerId == customerId);
	}

	public Task Update(AppSettings entity)
	{
		if (_currentUserService.CustomerId.HasValue)
		{
			entity.CustomerId = _currentUserService.CustomerId.Value;
		}
		else if (!_currentUserService.IsInRole("SuperAdmin") || entity.CustomerId == Guid.Empty)
		{
			throw new UnauthorizedAccessException("A customer must be selected before saving application settings.");
		}
		if (entity.Id == Guid.Empty)
		{
			Insert(entity);
		}
		else
		{
			Update(entity, true);
		}
		return Save();
	}

	public Task Insert(AppSettings entity)
	{
		Insert<AppSettings>(entity);
		return Save();
	}
}
