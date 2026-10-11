using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using LoanSystem.Core.Exceptions;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Models;
using LoanSystemAvizhe.Core;
using Microsoft.AspNetCore.Http;

namespace LoanSystem.Services.Base;

public class ScheduleJobManager
{
	private readonly IntegratedSmsSenderService _integratedSmsSenderService;

	private readonly IntegratedEmailSenderService _integratedEmailSenderService;

	private readonly IUserPaymentService _userPaymentService;

	private readonly ICurrentUserService _currentUserService;

	private readonly IAppSettingsService _appSettingsService;

	private readonly ICustomerService _customerService;

	private readonly IReportService _reportService;

	private readonly IHttpContextAccessor _httpContextAccessor;

	public ScheduleJobManager(IReportService reportService, IUserPaymentService userPaymentService, IAppSettingsService appSettingsService, IHttpContextAccessor httpContextAccessor, ICurrentUserService currentUserService, ICustomerService customerService, IntegratedSmsSenderService integratedSmsSenderService, IntegratedEmailSenderService integratedEmailSenderService)
	{
		_reportService = reportService;
		_userPaymentService = userPaymentService;
		_appSettingsService = appSettingsService;
		_httpContextAccessor = httpContextAccessor;
		_currentUserService = currentUserService;
		_customerService = customerService;
		_integratedSmsSenderService = integratedSmsSenderService;
		_integratedEmailSenderService = integratedEmailSenderService;
	}

	public async Task SetOrDeleteDueDateSmsJob()
	{
		AppSettings appSettings = await _appSettingsService.GetAppSettings();
		AppSettings appSettings2 = appSettings;
		if (appSettings2 == null || appSettings2.DueDatedRememberSmsTime.HasValue)
		{
			AppSettings appSettings3 = appSettings;
			if (appSettings3 != null && appSettings3.IsRememberSmsActive)
			{
				int value = (appSettings.DueDatedRememberSmsTime?.Hours).Value;
				int value2 = (appSettings.DueDatedRememberSmsTime?.Minutes).Value;
				RecurringJob.AddOrUpdate("duedate-" + _currentUserService.CustomerId.ToString(), () => SendDailySms((Guid)_currentUserService.CustomerId, appSettings), Cron.Daily(value, value2), new RecurringJobOptions
				{
					TimeZone = TimeZoneInfo.Local
				});
				return;
			}
		}
		RecurringJob.RemoveIfExists("duedate-" + _currentUserService.CustomerId.ToString());
	}

	public async Task SetOrDeleteDueDateEmailJob()
	{
		AppSettings appSettings = await _appSettingsService.GetAppSettings();
		AppSettings appSettings2 = appSettings;
		if (appSettings2 == null || appSettings2.DueDatedRememberEmailTime.HasValue)
		{
			AppSettings appSettings3 = appSettings;
			if (appSettings3 != null && appSettings3.IsRememberEmailActive)
			{
				int value = (appSettings.DueDatedRememberEmailTime?.Hours).Value;
				int value2 = (appSettings.DueDatedRememberEmailTime?.Minutes).Value;
				RecurringJob.AddOrUpdate("duedate-email-" + _currentUserService.CustomerId.ToString(), () => SendDailyEmail((Guid)_currentUserService.CustomerId, appSettings), Cron.Daily(value, value2), new RecurringJobOptions
				{
					TimeZone = TimeZoneInfo.Local
				});
				return;
			}
		}
		RecurringJob.RemoveIfExists("duedate-email-" + _currentUserService.CustomerId.ToString());
	}

	public async Task SetOrDeleteDailyReportEmailJob()
	{
		AppSettings appSettings = await _appSettingsService.GetAppSettings();
		AppSettings appSettings2 = appSettings;
		if (appSettings2 == null || appSettings2.DailyReportEmailTime.HasValue)
		{
			AppSettings appSettings3 = appSettings;
			if (appSettings3 != null && appSettings3.IsDailyReportEmailActive)
			{
				int value = (appSettings.DailyReportEmailTime?.Hours).Value;
				int value2 = (appSettings.DailyReportEmailTime?.Minutes).Value;
				RecurringJob.AddOrUpdate("daily-report-email-" + _currentUserService.CustomerId.ToString(), () => SendEmailReport((Guid)_currentUserService.CustomerId, appSettings), Cron.Daily(value, value2), new RecurringJobOptions
				{
					TimeZone = TimeZoneInfo.Local
				});
				return;
			}
		}
		RecurringJob.RemoveIfExists("daily-report-email-" + _currentUserService.CustomerId.ToString());
	}

	public async Task SendDailySms(Guid customerId, AppSettings appSettings)
	{
		if (appSettings.OverDueDaysCount > 0)
		{
			await SendOverDueSms(customerId, appSettings.OverDueDaysCount, appSettings.OverDueDayRemeberOnce, appSettings.OverDueDayGapPeriod);
		}
		if (appSettings.IsDueDateSmsActive)
		{
			await SendDueDatedSms(customerId);
		}
		if (appSettings.BeforeDueDaysCount > 0)
		{
			await SendBeforeDueDatedSms(customerId, appSettings.BeforeDueDaysCount);
		}
	}

	public async Task SendOverDueSms(Guid customerId, int overDueDaysCount, bool onlyOnce, int gapPeriod)
	{
		_httpContextAccessor.HttpContext = new DefaultHttpContext();
		_httpContextAccessor.HttpContext.Request.Headers.Add(CurrentUserService.Customer_header_key, customerId.ToString());
		List<LoanPaymentModel> overdueDatedItems = await _userPaymentService.GetOverDueDatedItemsForSms(overDueDaysCount, onlyOnce, gapPeriod);
		Customer customer = await _customerService.GetById(customerId);
		if (customer == null || !(customer.ExpireDate >= DateTime.Now))
		{
			return;
		}
		foreach (IGrouping<Guid, LoanPaymentModel> item in from x in overdueDatedItems
			group x by x.UserLoanModel.Id)
		{
			try
			{
				LoanPaymentModel loanPayment = item.OrderByDescending((LoanPaymentModel x) => x.TargetMonth).FirstOrDefault();
				if ((loanPayment.UserLoanModel?.AgentModel == null || loanPayment.UserLoanModel.AgentModel.SmsActiveForMembers) && await _integratedSmsSenderService.SendOverDueDate(loanPayment, item.Where((LoanPaymentModel x) => x.Id != loanPayment.Id).ToList()) != null)
				{
					await _userPaymentService.MarkAsSentOverDueDateSms(loanPayment.Id);
				}
			}
			catch (CustomException)
			{
			}
		}
	}

	public async Task SendDueDatedSms(Guid customerId)
	{
		_httpContextAccessor.HttpContext = new DefaultHttpContext();
		_httpContextAccessor.HttpContext.Request.Headers.Add(CurrentUserService.Customer_header_key, customerId.ToString());
		List<LoanPaymentModel> dueDatedItems = await _userPaymentService.GetDueDatedItemsForSms();
		Customer customer = await _customerService.GetById(customerId);
		if (customer == null || !(customer.ExpireDate >= DateTime.Now))
		{
			return;
		}
		foreach (LoanPaymentModel loanPayment in dueDatedItems)
		{
			try
			{
				if ((loanPayment.UserLoanModel?.AgentModel == null || loanPayment.UserLoanModel.AgentModel.SmsActiveForMembers) && await _integratedSmsSenderService.SendDueDate(loanPayment) != null)
				{
					await _userPaymentService.MarkAsSentDueDateSms(loanPayment.Id);
				}
			}
			catch (CustomException)
			{
			}
		}
	}

	public async Task SendBeforeDueDatedSms(Guid customerId, int days)
	{
		_httpContextAccessor.HttpContext = new DefaultHttpContext();
		_httpContextAccessor.HttpContext.Request.Headers.Add(CurrentUserService.Customer_header_key, customerId.ToString());
		List<LoanPaymentModel> dueDatedItems = await _userPaymentService.GetBeforeDueDatedItemsForSms(days);
		Customer customer = await _customerService.GetById(customerId);
		if (customer == null || !(customer.ExpireDate >= DateTime.Now))
		{
			return;
		}
		foreach (LoanPaymentModel loanPayment in dueDatedItems)
		{
			try
			{
				if ((loanPayment.UserLoanModel?.AgentModel == null || loanPayment.UserLoanModel.AgentModel.SmsActiveForMembers) && await _integratedSmsSenderService.SendBeforeDueDate(loanPayment) != null)
				{
					await _userPaymentService.MarkAsSentBeforeDueDateSms(loanPayment.Id);
				}
			}
			catch (CustomException)
			{
			}
		}
	}

	public async Task SendDailyEmail(Guid customerId, AppSettings appSettings)
	{
		if (appSettings.OverDueDaysCount_Email > 0)
		{
			await SendOverDueEmail(customerId, appSettings.OverDueDaysCount_Email, appSettings.OverDueDayRemeberOnce_Email);
		}
		if (appSettings.IsDueDateEmailActive)
		{
			await SendDueDatedEmail(customerId);
		}
		if (appSettings.BeforeDueDaysCount_Email > 0)
		{
			await SendBeforeDueDatedEmail(customerId, appSettings.BeforeDueDaysCount_Email);
		}
	}

	public async Task SendOverDueEmail(Guid customerId, int overDueDaysCount, bool onlyOnce)
	{
		_httpContextAccessor.HttpContext = new DefaultHttpContext();
		_httpContextAccessor.HttpContext.Request.Headers.Add(CurrentUserService.Customer_header_key, customerId.ToString());
		List<LoanPaymentModel> overdueDatedItems = await _userPaymentService.GetOverDueDatedItemsForEmail(overDueDaysCount, onlyOnce);
		Customer customer = await _customerService.GetById(customerId);
		if (customer == null || !(customer.ExpireDate >= DateTime.Now))
		{
			return;
		}
		foreach (LoanPaymentModel loanPayment in overdueDatedItems)
		{
			try
			{
				if ((loanPayment.UserLoanModel?.AgentModel == null || loanPayment.UserLoanModel.AgentModel.SmsActiveForMembers) && await _integratedEmailSenderService.SendOverDueDate(loanPayment) != null)
				{
					await _userPaymentService.MarkAsSentOverDueDateEmail(loanPayment.Id);
				}
			}
			catch (CustomException)
			{
			}
		}
	}

	public async Task SendDueDatedEmail(Guid customerId)
	{
		_httpContextAccessor.HttpContext = new DefaultHttpContext();
		_httpContextAccessor.HttpContext.Request.Headers.Add(CurrentUserService.Customer_header_key, customerId.ToString());
		List<LoanPaymentModel> dueDatedItems = await _userPaymentService.GetDueDatedItemsForEmail();
		Customer customer = await _customerService.GetById(customerId);
		if (customer == null || !(customer.ExpireDate >= DateTime.Now))
		{
			return;
		}
		foreach (LoanPaymentModel loanPayment in dueDatedItems)
		{
			try
			{
				if ((loanPayment.UserLoanModel?.AgentModel == null || loanPayment.UserLoanModel.AgentModel.SmsActiveForMembers) && await _integratedEmailSenderService.SendDueDate(loanPayment) != null)
				{
					await _userPaymentService.MarkAsSentDueDateEmail(loanPayment.Id);
				}
			}
			catch (CustomException)
			{
			}
		}
	}

	public async Task SendBeforeDueDatedEmail(Guid customerId, int days)
	{
		_httpContextAccessor.HttpContext = new DefaultHttpContext();
		_httpContextAccessor.HttpContext.Request.Headers.Add(CurrentUserService.Customer_header_key, customerId.ToString());
		List<LoanPaymentModel> dueDatedItems = await _userPaymentService.GetBeforeDueDatedItemsForEmail(days);
		Customer customer = await _customerService.GetById(customerId);
		if (customer == null || !(customer.ExpireDate >= DateTime.Now))
		{
			return;
		}
		foreach (LoanPaymentModel loanPayment in dueDatedItems)
		{
			try
			{
				if ((loanPayment.UserLoanModel?.AgentModel == null || loanPayment.UserLoanModel.AgentModel.SmsActiveForMembers) && await _integratedEmailSenderService.SendBeforeDueDate(loanPayment) != null)
				{
					await _userPaymentService.MarkAsSentBeforeDueDateEmail(loanPayment.Id);
				}
			}
			catch (CustomException)
			{
			}
		}
	}

	public async Task SendEmailReport(Guid customerId, AppSettings appSettings)
	{
		_httpContextAccessor.HttpContext = new DefaultHttpContext();
		_httpContextAccessor.HttpContext.Request.Headers.Add(CurrentUserService.Customer_header_key, customerId.ToString());
		Customer customer = await _customerService.GetById(customerId);
		if (customer != null && customer.ExpireDate >= DateTime.Now)
		{
			EmailReportModel emailReportModel = await _reportService.GetDailyReport(new AppSettingsModel(appSettings));
			await _integratedEmailSenderService.SendDailyReport(emailReportModel);
		}
	}
}
