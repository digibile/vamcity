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
		return Table<AppSettings>().Include((AppSettings x) => x.NewUserLoanSmsPattern).Include((AppSettings x) => x.ThanksPaymentSmsPattern).Include((AppSettings x) => x.DueDateRememberSmsPattern)
			.Include((AppSettings x) => x.OverDueDateSmsPattern)
			.Include((AppSettings x) => x.BeforeDueDateSmsPattern)
			.Include((AppSettings x) => x.LoginSmsPattern)
			.Include((AppSettings x) => x.LoginEmailPattern)
			.Include((AppSettings x) => x.RegisterSmsPattern)
			.Include((AppSettings x) => x.RegisterOtpSmsPattern)
			.Include((AppSettings x) => x.WalletChargeSmsPattern)
			.Include((AppSettings x) => x.NewOrderSmsPattern)
			.Include((AppSettings x) => x.NewOrderEmailPattern)
			.Include((AppSettings x) => x.OverDueDateEmailPattern)
			.Include((AppSettings x) => x.RegisterEmailPattern)
			.Include((AppSettings x) => x.RegisterOtpEmailPattern)
			.Include((AppSettings x) => x.ThanksPaymentEmailPattern)
			.Include((AppSettings x) => x.WalletChargeEmailPattern)
			.Include((AppSettings x) => x.DueDateRememberEmailPattern)
			.Include((AppSettings x) => x.BeforeDueDateEmailPattern)
			.Include((AppSettings x) => x.WalletWithdrawOtpSmsPattern)
			.Include((AppSettings x) => x.WalletWithdrawNotificationSmsPattern)
			.Include((AppSettings x) => x.AgentFactorPaymentLinkSmsPattern)
			.Include((AppSettings x) => x.DigitalSignLinkSmsPattern)
			.Include((AppSettings x) => x.DigitalSignCompleteSmsPattern)
			.Include((AppSettings x) => x.DigitalSignOtpSmsPattern)
			.Include((AppSettings x) => x.ThirdPersonDigitalSignLinkSmsPattern)
			.Include((AppSettings x) => x.ThirdPersonDigitalSignCompleteSmsPattern)
			.Include((AppSettings x) => x.ThirdPersonDigitalSignOtpSmsPattern)
			.AsSplitQuery()
			.FirstOrDefaultAsync((AppSettings x) => x.CustomerId == (Guid)_currentUserService.CustomerId);
	}

	public Task<AppSettings> GetAppSettings(Guid customerId)
	{
		return Table<AppSettings>().Include((AppSettings x) => x.ThanksPaymentSmsPattern).Include((AppSettings x) => x.DueDateRememberSmsPattern).Include((AppSettings x) => x.OverDueDateSmsPattern)
			.Include((AppSettings x) => x.BeforeDueDateSmsPattern)
			.Include((AppSettings x) => x.LoginSmsPattern)
			.Include((AppSettings x) => x.RegisterSmsPattern)
			.Include((AppSettings x) => x.RegisterOtpSmsPattern)
			.Include((AppSettings x) => x.WalletChargeSmsPattern)
			.Include((AppSettings x) => x.NewOrderSmsPattern)
			.FirstOrDefaultAsync((AppSettings x) => x.CustomerId == customerId);
	}

	public Task Update(AppSettings entity)
	{
		entity.CustomerId = _currentUserService.CustomerId.Value;
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
