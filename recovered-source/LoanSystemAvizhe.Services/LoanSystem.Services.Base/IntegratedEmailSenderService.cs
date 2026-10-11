using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LoanSystem.Core;
using LoanSystem.Core.Configuration;
using LoanSystem.Core.Exceptions;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Models;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Data.Enums;
using LoanSystemAvizhe.Services.Base;
using LoanSystemAvizhe.Services.Email;
using LoanSystemAvizhe.Services.Models.Email;
using LoanSystemAvizhe.Services.Sms.Models.KaveNegar;
using LoanSystemAvizhe.Services.Sms.Models.MelliPayamak;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using NikanBase.Services.Security;
using UAParser;

namespace LoanSystem.Services.Base;

public class IntegratedEmailSenderService
{
	private readonly string EMAIL_LAYOUT = "statics\\emailLayout.html";

	private readonly ICustomerService _customerService;

	private readonly ICurrentUserService _currentUserService;

	private readonly IAppSettingsService _appSettingsService;

	private readonly AppConfig _appConfig;

	private readonly EmailConfig _emailConfig;

	private readonly IHttpContextAccessor _httpContextAccessor;

	private readonly IHostingEnvironment _hostingEnvironment;

	private readonly IEncryptionService _encryptionService;

	private readonly IServiceProvider _serviceProvider;

	private readonly IAppEmailService _appEmailService;

	private readonly ILogger<IntegratedEmailSenderService> _logger;

	private readonly IEmailService _emailService;

	public IntegratedEmailSenderService(ICurrentUserService currentUserService, ILogger<IntegratedEmailSenderService> logger, IOptions<AppConfig> appConfig, IOptions<EmailConfig> emailConfig, IAppSettingsService appSettingsService, ICustomerService customerService, IHttpContextAccessor httpContextAccessor, IHostingEnvironment hostingEnvironment, IEncryptionService encryptionService, IServiceProvider serviceProvider, IEmailService emailService, IAppEmailService appEmailService)
	{
		_currentUserService = currentUserService;
		_appConfig = appConfig.Value;
		_emailConfig = emailConfig.Value;
		_appSettingsService = appSettingsService;
		_customerService = customerService;
		_httpContextAccessor = httpContextAccessor;
		_hostingEnvironment = hostingEnvironment;
		_encryptionService = encryptionService;
		_serviceProvider = serviceProvider;
		_appEmailService = appEmailService;
		_emailService = emailService;
		_logger = logger;
	}

	public async Task<int> GetTodaySuccessEmailCount()
	{
		return await _appEmailService.GetCountByCondition((AppEmail x) => x.Date.Date == DateTime.Now.Date, false);
	}

	public async Task SendOnlineMoneyTransferOTP(string to, string code, decimal amount, Guid customerId)
	{
		if (!(_hostingEnvironment.EnvironmentName == "Development"))
		{
			_currentUserService.CustomerId = customerId;
			await SetEmailService();
			if (_emailService == null)
			{
				throw new CustomException("سرویس ایمیل یافت نشد");
			}
			if (string.IsNullOrEmpty(to))
			{
				throw new CustomException("ایمیل گیرنده ثبت نشده است.");
			}
			string path = Path.Combine(_hostingEnvironment.WebRootPath, EMAIL_LAYOUT);
			string text = "";
			if (File.Exists(path))
			{
				text = File.ReadAllText(path);
			}
			string text2 = "کد تائیدیه انتقال وجه: " + code + "\r\nمبلغ: " + (amount * 10m).ToString("N0") + " ریال\r\n" + _appConfig.FullTitle;
			if (!string.IsNullOrWhiteSpace(text))
			{
				text2 = text.Replace("@CONTENT", text2);
			}
			EmailMessage emailMessage = await CreateEmailMessage(text2, to, "رمز تائیدیه انتقال وجه آنلاین");
			_emailService.Send(emailMessage);
		}
	}

	public async Task SendNewConsultantNotification(string name)
	{
		await SetEmailService(new AppSettings
		{
			DefaultSmsPanel = SmsPanel.JetAqsaat
		});
		if (_emailService != null && !string.IsNullOrEmpty(_appConfig.AdminEmail))
		{
			string path = Path.Combine(_hostingEnvironment.WebRootPath, EMAIL_LAYOUT);
			string text = "";
			if (File.Exists(path))
			{
				text = File.ReadAllText(path);
			}
			string text2 = "درخواست مشاوره جدید توسط " + name + " در سایت جت اقساط ثبت شده است";
			if (!string.IsNullOrWhiteSpace(text))
			{
				text2 = text.Replace("@CONTENT", text2);
			}
			EmailMessage emailMessage = await CreateEmailMessage(text2, _appConfig.AdminEmail, "درخواست مشاوره جدید در جت اقساط");
			_emailService.Send(emailMessage);
		}
	}

	public async Task SendNewCustomerOrderNotification(CustomerOrderModel model, Guid customerId)
	{
		if (!(_hostingEnvironment.EnvironmentName == "Development"))
		{
			_currentUserService.CustomerId = customerId;
			Customer customer = await _customerService.GetById(customerId);
			AppSettings appSettings = await _appSettingsService.GetAppSettings();
			await SetEmailService(appSettings);
			if (_emailService == null)
			{
				throw new CustomException("سرویس ایمیل یافت نشد");
			}
			if (appSettings?.NewOrderEmailPattern == null)
			{
				throw new CustomException("قالب ایمیل اطلاع سفارش جدید به مدیر تعیین نشده است.");
			}
			string to = customer.Email;
			if (string.IsNullOrEmpty(to))
			{
				throw new CustomException("ایمیل دریافت کننده ثبت نشده است.");
			}
			if (appSettings.NewOrderSmsPattern.IsPatternBase)
			{
				_ = !string.IsNullOrEmpty(appSettings.NewOrderSmsPattern.PatternBaseCode);
			}
			string text = await ExtractTextFromPattern(model, appSettings.NewOrderEmailPattern.Pattern, customer);
			new ResponsePayamak();
			EmailMessage emailMessage = await CreateEmailMessage(text, to, GetEmailSubject(AppSmsType.Thanks), appSettings);
			_emailService.Send(emailMessage);
			AppEmail entity = new AppEmail
			{
				Id = emailMessage.AppEmailId,
				AppEmailType = AppSmsType.Thanks,
				Date = DateTime.Now,
				Text = text,
				PatternTitle = appSettings.ThanksPaymentEmailPattern.Title,
				To = to,
				EntityType = "CustomerOrder",
				EntityId = model.Id,
				EmailPanel = appSettings.DefaultEmailPanel
			};
			await _appEmailService.Insert(entity);
		}
	}

	public async Task SendRegisterationCode(string to, string code)
	{
		await SetEmailService(new AppSettings
		{
			DefaultSmsPanel = SmsPanel.JetAqsaat
		});
		if (_emailService != null && !string.IsNullOrEmpty(to))
		{
			string body = "رمز ورود به اپلیکیشن : " + code + "\r\n" + _appConfig.FullTitle;
			EmailMessage emailMessage = await CreateEmailMessage(body, to, GetEmailSubject(AppSmsType.VerifyCode));
			_emailService.Send(emailMessage);
		}
	}

	public async Task SendMemberRegisterationCode(string to, string code, Guid customerId, bool insertAppEmail = false)
	{
		_currentUserService.CustomerId = customerId;
		AppSettings appSettings = await _appSettingsService.GetAppSettings();
		await SetEmailService(appSettings);
		if (_emailService == null)
		{
			throw new CustomException("سرویس ایمیل یافت نشد");
		}
		if (appSettings?.RegisterOtpEmailPattern == null)
		{
			throw new CustomException("قالب ایمیل رمز تاییدیه تعیین نشده است.");
		}
		if (string.IsNullOrEmpty(to))
		{
			throw new CustomException("ایمیل گیرنده ثبت نشده است.");
		}
		RegisterOtpModel registerOtpModel = new RegisterOtpModel
		{
			Code = code,
			Date = DateTime.Now,
			Email = to
		};
		string text = await ExtractTextFromPattern(registerOtpModel, appSettings.RegisterOtpEmailPattern.Pattern);
		EmailMessage emailMessage = await CreateEmailMessage(text, to, GetEmailSubject(AppSmsType.VerifyCode), appSettings);
		_emailService.Send(emailMessage);
		if (insertAppEmail)
		{
			AppEmail entity = new AppEmail
			{
				Id = emailMessage.AppEmailId,
				AppEmailType = AppSmsType.VerifyCode,
				Date = DateTime.Now,
				Text = text,
				PatternTitle = appSettings.RegisterOtpEmailPattern.Title,
				To = to,
				EntityType = "",
				EntityId = Guid.Empty,
				EmailPanel = appSettings.DefaultEmailPanel,
				Tag = code
			};
			await _appEmailService.Insert(entity);
		}
	}

	public async Task SendLoginOTPCode(string to, string otpCode, Guid customerId, bool insertAppEmail = false)
	{
		_currentUserService.CustomerId = customerId;
		AppSettings appSettings = await _appSettingsService.GetAppSettings();
		await SetEmailService(appSettings);
		if (_emailService == null)
		{
			throw new CustomException("سرویس ایمیل یافت نشد");
		}
		if (appSettings?.RegisterOtpEmailPattern == null)
		{
			throw new CustomException("قالب ایمیل رمز تاییدیه تعیین نشده است.");
		}
		if (string.IsNullOrEmpty(to))
		{
			throw new CustomException("ایمیل گیرنده ثبت نشده است.");
		}
		RegisterOtpModel registerOtpModel = new RegisterOtpModel
		{
			Code = otpCode,
			Date = DateTime.Now,
			Email = to
		};
		string text = await ExtractTextFromPattern(registerOtpModel, appSettings.RegisterOtpEmailPattern.Pattern);
		EmailMessage emailMessage = await CreateEmailMessage(text, to, GetEmailSubject(AppSmsType.LoginOTP), appSettings);
		_emailService.Send(emailMessage);
		if (insertAppEmail)
		{
			AppEmail entity = new AppEmail
			{
				Id = emailMessage.AppEmailId,
				AppEmailType = AppSmsType.LoginOTP,
				Date = DateTime.Now,
				Text = text,
				PatternTitle = appSettings.RegisterOtpEmailPattern.Title,
				To = to,
				EntityType = "",
				EntityId = Guid.Empty,
				EmailPanel = appSettings.DefaultEmailPanel,
				Tag = otpCode
			};
			await _appEmailService.Insert(entity);
		}
	}

	public async Task<AppEmail> SendWalletCharge(WalletChargeModel model)
	{
		if (_hostingEnvironment.EnvironmentName == "Development")
		{
			return null;
		}
		AppSettings appSettings = await _appSettingsService.GetAppSettings();
		await SetEmailService(appSettings);
		if (_emailService == null)
		{
			throw new CustomException("سرویس ایمیل یافت نشد");
		}
		if (appSettings?.WalletChargeEmailPattern == null)
		{
			throw new CustomException("قالب ایمیل شارژ کیف پول تنظیم نشده است.");
		}
		string to = model.Member.Email ?? "";
		if (string.IsNullOrEmpty(to))
		{
			throw new CustomException("ایمیل گیرنده ثبت نشده است.");
		}
		string text = await ExtractTextFromPattern(model, appSettings.WalletChargeEmailPattern.Pattern);
		EmailMessage emailMessage = await CreateEmailMessage(text, to, GetEmailSubject(AppSmsType.WalletCharge), appSettings);
		_emailService.Send(emailMessage);
		AppEmail appEmail = new AppEmail
		{
			Id = emailMessage.AppEmailId,
			AppEmailType = AppSmsType.WalletCharge,
			Date = DateTime.Now,
			Text = text,
			PatternTitle = appSettings.WalletChargeEmailPattern.Title,
			To = to,
			EntityType = "",
			EntityId = Guid.Empty,
			EmailPanel = appSettings.DefaultEmailPanel
		};
		await _appEmailService.Insert(appEmail);
		return appEmail;
	}

	public async Task<AppEmail> SendThanks(LoanPaymentModel loanPayment)
	{
		if (_hostingEnvironment.EnvironmentName == "Development")
		{
			return null;
		}
		AppSettings appSettings = await _appSettingsService.GetAppSettings();
		await SetEmailService(appSettings);
		if (_emailService == null)
		{
			throw new CustomException("سرویس ایمیل یافت نشد");
		}
		if (appSettings?.ThanksPaymentEmailPattern == null)
		{
			throw new CustomException("قالب ایمیل تشکر تنظیم نشده است.");
		}
		string to = loanPayment?.UserLoanModel?.MemberModel?.Email ?? "";
		if (string.IsNullOrEmpty(to))
		{
			throw new CustomException("ایمیل گیرنده ثبت نشده است.");
		}
		string text = await ExtractTextFromPattern(loanPayment, appSettings.ThanksPaymentEmailPattern.Pattern);
		new ResponsePayamak();
		EmailMessage emailMessage = await CreateEmailMessage(text, to, GetEmailSubject(AppSmsType.Thanks), appSettings);
		_emailService.Send(emailMessage);
		AppEmail appEmail = new AppEmail
		{
			Id = emailMessage.AppEmailId,
			AppEmailType = AppSmsType.Thanks,
			Date = DateTime.Now,
			Text = text,
			PatternTitle = appSettings.ThanksPaymentEmailPattern.Title,
			To = to,
			EntityType = "UserLoanPayment",
			EntityId = loanPayment.Id,
			EmailPanel = appSettings.DefaultEmailPanel
		};
		await _appEmailService.Insert(appEmail);
		return appEmail;
	}

	public async Task<AppEmail> SendDueDate(LoanPaymentModel loanPayment)
	{
		if (_hostingEnvironment.EnvironmentName == "Development")
		{
			return null;
		}
		AppSettings appSettings = await _appSettingsService.GetAppSettings();
		await SetEmailService(appSettings);
		if (_emailService == null)
		{
			throw new CustomException("سرویس ایمیل یافت نشد");
		}
		if (appSettings?.DueDateRememberEmailPattern == null)
		{
			throw new CustomException("قالب ایمیل یادآوری سرسید تنظیم نشده است.");
		}
		string to = loanPayment?.UserLoanModel?.MemberModel?.Email ?? "";
		if (string.IsNullOrEmpty(to))
		{
			throw new CustomException("ایمیل گیرنده ثبت نشده است.");
		}
		if (appSettings.DueDateRememberEmailPattern.IsPatternBase)
		{
			_ = !string.IsNullOrEmpty(appSettings.DueDateRememberEmailPattern.PatternBaseCode);
		}
		string text = await ExtractTextFromPattern(loanPayment, appSettings.DueDateRememberEmailPattern.Pattern);
		EmailMessage emailMessage = await CreateEmailMessage(text, to, GetEmailSubject(AppSmsType.DueDate), appSettings);
		_emailService.Send(emailMessage);
		AppEmail appEmail = new AppEmail
		{
			Id = emailMessage.AppEmailId,
			AppEmailType = AppSmsType.DueDate,
			Date = DateTime.Now,
			Text = text,
			PatternTitle = appSettings.DueDateRememberEmailPattern.Title,
			To = to,
			EntityType = "UserLoanPayment",
			EntityId = loanPayment.Id,
			EmailPanel = appSettings.DefaultEmailPanel
		};
		await _appEmailService.Insert(appEmail);
		return appEmail;
	}

	public async Task<AppEmail> SendOverDueDate(LoanPaymentModel loanPayment)
	{
		if (_hostingEnvironment.EnvironmentName == "Development")
		{
			return null;
		}
		AppSettings appSettings = await _appSettingsService.GetAppSettings();
		await SetEmailService(appSettings);
		if (_emailService == null)
		{
			throw new CustomException("سرویس ایمیل یافت نشد");
		}
		if (appSettings?.OverDueDateEmailPattern == null)
		{
			throw new CustomException("قالب ایمیل قسط معوق تنظیم نشده است.");
		}
		string to = loanPayment?.UserLoanModel?.MemberModel?.Email ?? "";
		if (string.IsNullOrEmpty(to))
		{
			throw new CustomException("ایمیل گیرندده ثبت نشده است.");
		}
		string text = await ExtractTextFromPattern(loanPayment, appSettings.OverDueDateEmailPattern.Pattern);
		EmailMessage emailMessage = await CreateEmailMessage(text, to, GetEmailSubject(AppSmsType.OverDueDate), appSettings);
		_emailService.Send(emailMessage);
		AppEmail appEmail = new AppEmail
		{
			Id = emailMessage.AppEmailId,
			AppEmailType = AppSmsType.OverDueDate,
			Date = DateTime.Now,
			Text = text,
			PatternTitle = appSettings.OverDueDateEmailPattern.Title,
			To = to,
			EntityType = "UserLoanPayment",
			EntityId = loanPayment.Id,
			EmailPanel = appSettings.DefaultEmailPanel
		};
		await _appEmailService.Insert(appEmail);
		return appEmail;
	}

	public async Task<AppEmail> SendBeforeDueDate(LoanPaymentModel loanPayment)
	{
		if (_hostingEnvironment.EnvironmentName == "Development")
		{
			return null;
		}
		AppSettings appSettings = await _appSettingsService.GetAppSettings();
		await SetEmailService(appSettings);
		if (_emailService == null)
		{
			throw new CustomException("سرویس ایمیل یافت نشد");
		}
		if (appSettings?.BeforeDueDateEmailPattern == null)
		{
			throw new CustomException("قالب ایمیل سررسید پیش از موعد تنظیم نشده است.");
		}
		string to = loanPayment?.UserLoanModel?.MemberModel?.Email ?? "";
		if (string.IsNullOrEmpty(to))
		{
			throw new CustomException("ایمیل گیرنده ثبت نشده است.");
		}
		if (appSettings.BeforeDueDateEmailPattern.IsPatternBase)
		{
			_ = !string.IsNullOrEmpty(appSettings.BeforeDueDateEmailPattern.PatternBaseCode);
		}
		string text = await ExtractTextFromPattern(loanPayment, appSettings.BeforeDueDateEmailPattern.Pattern);
		EmailMessage emailMessage = await CreateEmailMessage(text, to, GetEmailSubject(AppSmsType.BeforeDueDate), appSettings);
		_emailService.Send(emailMessage);
		AppEmail appEmail = new AppEmail
		{
			Id = emailMessage.AppEmailId,
			AppEmailType = AppSmsType.BeforeDueDate,
			Date = DateTime.Now,
			Text = text,
			PatternTitle = appSettings.BeforeDueDateEmailPattern.Title,
			To = to,
			EntityType = "UserLoanPayment",
			EntityId = loanPayment.Id,
			EmailPanel = appSettings.DefaultEmailPanel
		};
		await _appEmailService.Insert(appEmail);
		return appEmail;
	}

	public async Task<AppEmail> ReSend(AppEmail appEmail)
	{
		if (_hostingEnvironment.EnvironmentName == "Development")
		{
			return null;
		}
		AppSettings appSettings = await _appSettingsService.GetAppSettings();
		await SetEmailService(appSettings);
		if (_emailService == null)
		{
			throw new CustomException("سرویس ایمیل یافت نشد");
		}
		if (string.IsNullOrEmpty(appEmail.To))
		{
			throw new CustomException("ایمیل گیرنده ثبت نشده است.");
		}
		EmailMessage emailMessage = await CreateEmailMessage(appEmail.Text, appEmail.To, GetEmailSubject(appEmail.AppEmailType), appSettings);
		_emailService.Send(emailMessage);
		AppEmail newAppSms = new AppEmail
		{
			Id = emailMessage.AppEmailId,
			AppEmailType = appEmail.AppEmailType,
			Date = DateTime.Now,
			Text = appEmail.Text,
			PatternTitle = appEmail.PatternTitle,
			To = appEmail.To,
			EntityType = appEmail.EntityType,
			EntityId = appEmail.EntityId,
			EmailPanel = appEmail.EmailPanel
		};
		await _appEmailService.Insert(newAppSms);
		return newAppSms;
	}

	public async Task<AppEmail> SendChangeLoanRequestStatus(UserLoanRequestModel userLoanRequestModel)
	{
		if (_hostingEnvironment.EnvironmentName == "Development")
		{
			return null;
		}
		AppSettings appSettings = await _appSettingsService.GetAppSettings();
		await SetEmailService(appSettings);
		if (_emailService == null)
		{
			throw new CustomException("سرویس ایمیل یافت نشد");
		}
		if (userLoanRequestModel?.Status?.SmsPattern == null)
		{
			throw new CustomException("قالب ایمیل تغییر وضعیت یافت نشد.");
		}
		string to = userLoanRequestModel?.MemberModel?.Email ?? "";
		if (string.IsNullOrEmpty(to))
		{
			throw new CustomException("ایمیل گیرنده ثبت نشده است.");
		}
		if (userLoanRequestModel.Status.SmsPattern.IsPatternBase)
		{
			_ = !string.IsNullOrEmpty(userLoanRequestModel.Status.SmsPattern.PatternBaseCode);
		}
		string text = await ExtractTextFromPattern(userLoanRequestModel, userLoanRequestModel.Status.SmsPattern.Pattern);
		EmailMessage emailMessage = await CreateEmailMessage(text, to, GetEmailSubject(AppSmsType.ChangeStatus), appSettings);
		_emailService.Send(emailMessage);
		AppEmail appEmail = new AppEmail
		{
			Id = emailMessage.AppEmailId,
			AppEmailType = AppSmsType.ChangeStatus,
			Date = DateTime.Now,
			Text = text,
			PatternTitle = userLoanRequestModel.Status.SmsPattern.Title,
			To = to,
			EntityType = "UserLoanRequest",
			EntityId = userLoanRequestModel.Id,
			EmailPanel = appSettings.DefaultEmailPanel
		};
		await _appEmailService.Insert(appEmail);
		return appEmail;
	}

	public async Task<AppEmail> SendChangeLoanRequestAgentStatus(UserLoanRequestModel userLoanRequestModel)
	{
		if (_hostingEnvironment.EnvironmentName == "Development")
		{
			return null;
		}
		AppSettings appSettings = await _appSettingsService.GetAppSettings();
		await SetEmailService(appSettings);
		if (_emailService == null)
		{
			throw new CustomException("سرویس ایمیل یافت نشد");
		}
		if (userLoanRequestModel?.AgentStatus?.SmsPattern == null)
		{
			throw new CustomException("قالب ایمیل تغییر وضعیت نماینده یافت نشد.");
		}
		string to = userLoanRequestModel?.AgentModel?.Email ?? "";
		if (string.IsNullOrEmpty(to))
		{
			throw new CustomException("ایمیل گیرنده ثبت نشده است.");
		}
		if (userLoanRequestModel.AgentStatus.SmsPattern.IsPatternBase)
		{
			_ = !string.IsNullOrEmpty(userLoanRequestModel.AgentStatus.SmsPattern.PatternBaseCode);
		}
		string text = await ExtractTextFromPattern(userLoanRequestModel, userLoanRequestModel.AgentStatus.SmsPattern.Pattern);
		EmailMessage emailMessage = await CreateEmailMessage(text, to, GetEmailSubject(AppSmsType.ChangeAgentStatus), appSettings);
		_emailService.Send(emailMessage);
		AppEmail appEmail = new AppEmail
		{
			Id = emailMessage.AppEmailId,
			AppEmailType = AppSmsType.ChangeAgentStatus,
			Date = DateTime.Now,
			Text = text,
			PatternTitle = userLoanRequestModel.AgentStatus.SmsPattern.Title,
			To = to,
			EntityType = "UserLoanRequest",
			EntityId = userLoanRequestModel.Id,
			EmailPanel = appSettings.DefaultEmailPanel
		};
		await _appEmailService.Insert(appEmail);
		return appEmail;
	}

	public async Task<AppEmail> SendDailyReport(EmailReportModel emailReportModel)
	{
		if (_hostingEnvironment.EnvironmentName == "Development")
		{
			return null;
		}
		AppSettings appSettings = await _appSettingsService.GetAppSettings();
		AppSettingsModel appSettingsModel = new AppSettingsModel(appSettings);
		await SetEmailService(appSettings);
		if (_emailService == null)
		{
			throw new CustomException("سرویس ایمیل یافت نشد");
		}
		string to = appSettingsModel.EmailConfig?.AdminEmail ?? "";
		if (string.IsNullOrEmpty(to))
		{
			throw new CustomException("ایمیل گیرنده ثبت نشده است.");
		}
		StringBuilder html = new StringBuilder();
		html.Append("<div>");
		html.Append("<div style=\"display: flex;width: 100%;border-bottom: 2px solid #ddd;font-weight: bold;\">");
		html.Append("<div style=\"flex-grow: 2;text-align: right;padding: 5px;\">شرح</div>");
		html.Append("<div style=\"text-align: center;flex-basis: 100px;padding: 5px;\">امروز</div>");
		html.Append("<div style=\"text-align: center;flex-basis: 100px;padding: 5px;\">هفته گذشته</div>");
		html.Append("<div style=\"text-align: center;flex-basis: 100px;padding: 5px;\">ماه گذشته</div>");
		html.Append("</div>");
		html.Append("<div style=\"display: flex;width: 100%;border-bottom: 2px dashed #ddd;\">");
		html.Append("<div style=\"flex-grow: 2;text-align: right;padding: 5px;\">تعداد درخواست وام</div>");
		html.Append($"<div style=\"text-align: center;flex-basis: 100px;padding: 5px;\">{emailReportModel.Daily.LoanRequestCount}</div>");
		html.Append($"<div style=\"text-align: center;flex-basis: 100px;padding: 5px;\">{emailReportModel.Weekly.LoanRequestCount}</div>");
		html.Append($"<div style=\"text-align: center;flex-basis: 100px;padding: 5px;\">{emailReportModel.Monthly.LoanRequestCount}</div>");
		html.Append("</div>");
		html.Append("<div style=\"display: flex;width: 100%;border-bottom: 2px dashed #ddd;\">");
		html.Append("<div style=\"flex-grow: 2;text-align: right;padding: 5px;\">تعداد وام اعطا شده</div>");
		html.Append($"<div style=\"text-align: center;flex-basis: 100px;padding: 5px;\">{emailReportModel.Daily.LoanCount}</div>");
		html.Append($"<div style=\"text-align: center;flex-basis: 100px;padding: 5px;\">{emailReportModel.Weekly.LoanCount}</div>");
		html.Append($"<div style=\"text-align: center;flex-basis: 100px;padding: 5px;\">{emailReportModel.Monthly.LoanCount}</div>");
		html.Append("</div>");
		html.Append("<div style=\"display: flex;width: 100%;border-bottom: 2px dashed #ddd;\">");
		html.Append("<div style=\"flex-grow: 2;text-align: right;padding: 5px;\">اقساط سررسید شده</div>");
		html.Append($"<div style=\"text-align: center;flex-basis: 100px;padding: 5px;\">{emailReportModel.Daily.DueDatedLoanPaymentCount}</div>");
		html.Append($"<div style=\"text-align: center;flex-basis: 100px;padding: 5px;\">{emailReportModel.Weekly.DueDatedLoanPaymentCount}</div>");
		html.Append($"<div style=\"text-align: center;flex-basis: 100px;padding: 5px;\">{emailReportModel.Monthly.DueDatedLoanPaymentCount}</div>");
		html.Append("</div>");
		html.Append("<div style=\"display: flex;width: 100%;border-bottom: 2px dashed #ddd;\">");
		html.Append("<div style=\"flex-grow: 2;text-align: right;padding: 5px;\">اقساط معوق شده</div>");
		html.Append($"<div style=\"text-align: center;flex-basis: 100px;padding: 5px;\">{emailReportModel.Daily.OverDueDatedLoanPaymentCount}</div>");
		html.Append($"<div style=\"text-align: center;flex-basis: 100px;padding: 5px;\">{emailReportModel.Weekly.OverDueDatedLoanPaymentCount}</div>");
		html.Append($"<div style=\"text-align: center;flex-basis: 100px;padding: 5px;\">{emailReportModel.Monthly.OverDueDatedLoanPaymentCount}</div>");
		html.Append("</div>");
		html.Append("<div style=\"display: flex;width: 100%;border-bottom: 2px dashed #ddd;\">");
		html.Append("<div style=\"flex-grow: 2;text-align: right;padding: 5px;\">تعداد اعضا جدید</div>");
		html.Append($"<div style=\"text-align: center;flex-basis: 100px;padding: 5px;\">{emailReportModel.Daily.MemberCount}</div>");
		html.Append($"<div style=\"text-align: center;flex-basis: 100px;padding: 5px;\">{emailReportModel.Weekly.MemberCount}</div>");
		html.Append($"<div style=\"text-align: center;flex-basis: 100px;padding: 5px;\">{emailReportModel.Monthly.MemberCount}</div>");
		html.Append("</div>");
		html.Append("<div style=\"display: flex;width: 100%;border-bottom: 2px dashed #ddd;\">");
		html.Append("<div style=\"flex-grow: 2;text-align: right;padding: 5px;\">تعداد تیکت</div>");
		html.Append($"<div style=\"text-align: center;flex-basis: 100px;padding: 5px;\">{emailReportModel.Daily.Ticket}</div>");
		html.Append($"<div style=\"text-align: center;flex-basis: 100px;padding: 5px;\">{emailReportModel.Weekly.Ticket}</div>");
		html.Append($"<div style=\"text-align: center;flex-basis: 100px;padding: 5px;\">{emailReportModel.Monthly.Ticket}</div>");
		html.Append("</div>");
		html.Append("<div style=\"display: flex;width: 100%;border-bottom: 2px dashed #ddd;\">");
		html.Append("<div style=\"flex-grow: 2;text-align: right;padding: 5px;\">تعداد تیکت بسته شده</div>");
		html.Append($"<div style=\"text-align: center;flex-basis: 100px;padding: 5px;\">{emailReportModel.Daily.ClosedTicket}</div>");
		html.Append($"<div style=\"text-align: center;flex-basis: 100px;padding: 5px;\">{emailReportModel.Weekly.ClosedTicket}</div>");
		html.Append($"<div style=\"text-align: center;flex-basis: 100px;padding: 5px;\">{emailReportModel.Monthly.ClosedTicket}</div>");
		html.Append("</div>");
		html.Append("<div style=\"display: flex;width: 100%;border-bottom: 2px dashed #ddd;\">");
		html.Append("<div style=\"flex-grow: 2;text-align: right;padding: 5px;\">تعداد تیکت پاسخ داده شده</div>");
		html.Append($"<div style=\"text-align: center;flex-basis: 100px;padding: 5px;\">{emailReportModel.Daily.RepliedTicket}</div>");
		html.Append($"<div style=\"text-align: center;flex-basis: 100px;padding: 5px;\">{emailReportModel.Weekly.RepliedTicket}</div>");
		html.Append($"<div style=\"text-align: center;flex-basis: 100px;padding: 5px;\">{emailReportModel.Monthly.RepliedTicket}</div>");
		html.Append("</div>");
		html.Append("<div style=\"display: flex;width: 100%;border-bottom: 2px dashed #ddd;\">");
		html.Append("<div style=\"flex-grow: 2;text-align: right;padding: 5px;\">تعداد واریزی های درگاه</div>");
		html.Append($"<div style=\"text-align: center;flex-basis: 100px;padding: 5px;\">{emailReportModel.Daily.OnlinePayment}</div>");
		html.Append($"<div style=\"text-align: center;flex-basis: 100px;padding: 5px;\">{emailReportModel.Weekly.OnlinePayment}</div>");
		html.Append($"<div style=\"text-align: center;flex-basis: 100px;padding: 5px;\">{emailReportModel.Monthly.OnlinePayment}</div>");
		html.Append("</div>");
		html.Append("<div style=\"display: flex;width: 100%;border-bottom: 2px dashed #ddd;\">");
		html.Append("<div style=\"flex-grow: 2;text-align: right;padding: 5px;\">مبلغ واریزی های درگاه</div>");
		html.Append($"<div style=\"text-align: center;flex-basis: 100px;padding: 5px;\">{emailReportModel.Daily.OnlinePaymentPrice}</div>");
		html.Append($"<div style=\"text-align: center;flex-basis: 100px;padding: 5px;\">{emailReportModel.Weekly.OnlinePaymentPrice}</div>");
		html.Append($"<div style=\"text-align: center;flex-basis: 100px;padding: 5px;\">{emailReportModel.Monthly.OnlinePaymentPrice}</div>");
		html.Append("</div>");
		html.Append("<div style=\"display: flex;width: 100%;border-bottom: 2px dashed #ddd;\">");
		html.Append("<div style=\"flex-grow: 2;text-align: right;padding: 5px;\">تعداد واریزی های بانک</div>");
		html.Append($"<div style=\"text-align: center;flex-basis: 100px;padding: 5px;\">{emailReportModel.Daily.BankPayment}</div>");
		html.Append($"<div style=\"text-align: center;flex-basis: 100px;padding: 5px;\">{emailReportModel.Weekly.BankPayment}</div>");
		html.Append($"<div style=\"text-align: center;flex-basis: 100px;padding: 5px;\">{emailReportModel.Monthly.BankPayment}</div>");
		html.Append("</div>");
		html.Append("<div style=\"display: flex;width: 100%;border-bottom: 2px dashed #ddd;\">");
		html.Append("<div style=\"flex-grow: 2;text-align: right;padding: 5px;\">مبلغ واریزی های بانک</div>");
		html.Append($"<div style=\"text-align: center;flex-basis: 100px;padding: 5px;\">{emailReportModel.Daily.BankPaymentPrice}</div>");
		html.Append($"<div style=\"text-align: center;flex-basis: 100px;padding: 5px;\">{emailReportModel.Weekly.BankPaymentPrice}</div>");
		html.Append($"<div style=\"text-align: center;flex-basis: 100px;padding: 5px;\">{emailReportModel.Monthly.BankPaymentPrice}</div>");
		html.Append("</div>");
		html.Append("<div style=\"display: flex;width: 100%;border-bottom: 2px dashed #ddd;\">");
		html.Append("<div style=\"flex-grow: 2;text-align: right;padding: 5px;\">تعداد واریزی های صندوق</div>");
		html.Append($"<div style=\"text-align: center;flex-basis: 100px;padding: 5px;\">{emailReportModel.Daily.CashPayment}</div>");
		html.Append($"<div style=\"text-align: center;flex-basis: 100px;padding: 5px;\">{emailReportModel.Weekly.CashPayment}</div>");
		html.Append($"<div style=\"text-align: center;flex-basis: 100px;padding: 5px;\">{emailReportModel.Monthly.CashPayment}</div>");
		html.Append("</div>");
		html.Append("<div style=\"display: flex;width: 100%;border-bottom: 2px dashed #ddd;\">");
		html.Append("<div style=\"flex-grow: 2;text-align: right;padding: 5px;\">مبلغ واریزی های صندوق</div>");
		html.Append($"<div style=\"text-align: center;flex-basis: 100px;padding: 5px;\">{emailReportModel.Daily.CashPaymentPrice}</div>");
		html.Append($"<div style=\"text-align: center;flex-basis: 100px;padding: 5px;\">{emailReportModel.Weekly.CashPaymentPrice}</div>");
		html.Append($"<div style=\"text-align: center;flex-basis: 100px;padding: 5px;\">{emailReportModel.Monthly.CashPaymentPrice}</div>");
		html.Append("</div>");
		html.Append("<div style=\"display: flex;width: 100%;border-bottom: 2px dashed #ddd;\">");
		html.Append("<div style=\"flex-grow: 2;text-align: right;padding: 5px;\">تعداد شارژ کیف پول</div>");
		html.Append($"<div style=\"text-align: center;flex-basis: 100px;padding: 5px;\">{emailReportModel.Daily.WalletCharge}</div>");
		html.Append($"<div style=\"text-align: center;flex-basis: 100px;padding: 5px;\">{emailReportModel.Weekly.WalletCharge}</div>");
		html.Append($"<div style=\"text-align: center;flex-basis: 100px;padding: 5px;\">{emailReportModel.Monthly.WalletCharge}</div>");
		html.Append("</div>");
		html.Append("<div style=\"display: flex;width: 100%;border-bottom: 2px dashed #ddd;\">");
		html.Append("<div style=\"flex-grow: 2;text-align: right;padding: 5px;\">مبلغ شارژ کیف پول</div>");
		html.Append($"<div style=\"text-align: center;flex-basis: 100px;padding: 5px;\">{emailReportModel.Daily.WalletChargePrice}</div>");
		html.Append($"<div style=\"text-align: center;flex-basis: 100px;padding: 5px;\">{emailReportModel.Weekly.WalletChargePrice}</div>");
		html.Append($"<div style=\"text-align: center;flex-basis: 100px;padding: 5px;\">{emailReportModel.Monthly.WalletChargePrice}</div>");
		html.Append("</div>");
		html.Append("<div style=\"display: flex;width: 100%;border-bottom: 2px dashed #ddd;\">");
		html.Append("<div style=\"flex-grow: 2;text-align: right;padding: 5px;\">تعداد واریز به نماینده</div>");
		html.Append($"<div style=\"text-align: center;flex-basis: 100px;padding: 5px;\">{emailReportModel.Daily.AgentTransaction}</div>");
		html.Append($"<div style=\"text-align: center;flex-basis: 100px;padding: 5px;\">{emailReportModel.Weekly.AgentTransaction}</div>");
		html.Append($"<div style=\"text-align: center;flex-basis: 100px;padding: 5px;\">{emailReportModel.Monthly.AgentTransaction}</div>");
		html.Append("</div>");
		html.Append("<div style=\"display: flex;width: 100%;border-bottom: 2px dashed #ddd;\">");
		html.Append("<div style=\"flex-grow: 2;text-align: right;padding: 5px;\">مبلغ واریز به نماینده</div>");
		html.Append($"<div style=\"text-align: center;flex-basis: 100px;padding: 5px;\">{emailReportModel.Daily.AgentTransactionPrice}</div>");
		html.Append($"<div style=\"text-align: center;flex-basis: 100px;padding: 5px;\">{emailReportModel.Weekly.AgentTransactionPrice}</div>");
		html.Append($"<div style=\"text-align: center;flex-basis: 100px;padding: 5px;\">{emailReportModel.Monthly.AgentTransactionPrice}</div>");
		html.Append("</div>");
		html.Append("<div style=\"display: flex;width: 100%;border-bottom: 2px dashed #ddd;\">");
		html.Append("<div style=\"flex-grow: 2;text-align: right;padding: 5px;\">تعداد سررسید شده های اسپانسر</div>");
		html.Append($"<div style=\"text-align: center;flex-basis: 100px;padding: 5px;\">{emailReportModel.Daily.SponsorDueDate}</div>");
		html.Append($"<div style=\"text-align: center;flex-basis: 100px;padding: 5px;\">{emailReportModel.Weekly.SponsorDueDate}</div>");
		html.Append($"<div style=\"text-align: center;flex-basis: 100px;padding: 5px;\">{emailReportModel.Monthly.SponsorDueDate}</div>");
		html.Append("</div>");
		html.Append("</div>");
		EmailMessage emailMessage = await CreateEmailMessage(html.ToString(), to, GetEmailSubject(AppSmsType.DailyReport), appSettings);
		_emailService.Send(emailMessage);
		AppEmail appEmail = new AppEmail
		{
			Id = emailMessage.AppEmailId,
			AppEmailType = AppSmsType.DailyReport,
			Date = DateTime.Now,
			Text = html.ToString(),
			PatternTitle = "بدون پترن",
			To = to,
			EmailPanel = appSettings.DefaultEmailPanel
		};
		await _appEmailService.Insert(appEmail);
		return appEmail;
	}

	public async Task<AppEmail> SendWalletWithdrawOtp(AgentFactorModel agentFactorModel, string code)
	{
		if (_hostingEnvironment.EnvironmentName == "Development")
		{
			return null;
		}
		AppSettings appSettings = await _appSettingsService.GetAppSettings();
		await SetEmailService(appSettings);
		if (_emailService == null)
		{
			throw new CustomException("سرویس ایمیل یافت نشد");
		}
		if (appSettings.WalletWithdrawOtpEmailPattern == null)
		{
			throw new CustomException("قالب ایمیل کد تائید برداشت از کیف پول یافت نشد.");
		}
		string to = agentFactorModel?.Member?.Email ?? "";
		if (string.IsNullOrEmpty(to))
		{
			throw new CustomException("ایمیل گیرنده ثبت نشده است.");
		}
		bool isPatternBase = appSettings.WalletWithdrawOtpEmailPattern.IsPatternBase && !string.IsNullOrEmpty(appSettings.WalletWithdrawOtpEmailPattern.PatternBaseCode);
		string text = await ExtractTextFromPattern(agentFactorModel, appSettings.WalletWithdrawOtpEmailPattern.Pattern, isPatternBase, code);
		EmailMessage emailMessage = await CreateEmailMessage(text, to, GetEmailSubject(AppSmsType.WalletWithdrawOtp), appSettings);
		_emailService.Send(emailMessage);
		AppEmail appEmail = new AppEmail
		{
			Id = emailMessage.AppEmailId,
			AppEmailType = AppSmsType.WalletWithdrawOtp,
			Date = DateTime.Now,
			Text = text,
			PatternTitle = appSettings.WalletWithdrawOtpEmailPattern.Title,
			To = to,
			EntityType = "AgentFactor",
			EntityId = agentFactorModel.Id,
			EmailPanel = appSettings.DefaultEmailPanel
		};
		await _appEmailService.Insert(appEmail);
		return appEmail;
	}

	public async Task<AppEmail> SendWalletWithdrawNotification(AgentFactorModel agentFactorModel)
	{
		if (_hostingEnvironment.EnvironmentName == "Development")
		{
			return null;
		}
		AppSettings appSettings = await _appSettingsService.GetAppSettings();
		await SetEmailService(appSettings);
		if (_emailService == null)
		{
			throw new CustomException("سرویس ایمیل یافت نشد");
		}
		if (appSettings.WalletWithdrawOtpEmailPattern == null)
		{
			throw new CustomException("قالب ایمیل کد تائید برداشت از کیف پول یافت نشد.");
		}
		string to = agentFactorModel?.Member?.Email ?? "";
		if (string.IsNullOrEmpty(to))
		{
			throw new CustomException("ایمیل گیرنده ثبت نشده است.");
		}
		bool isPatternBase = appSettings.WalletWithdrawOtpEmailPattern.IsPatternBase && !string.IsNullOrEmpty(appSettings.WalletWithdrawOtpEmailPattern.PatternBaseCode);
		string text = await ExtractTextFromPattern(agentFactorModel, appSettings.WalletWithdrawOtpEmailPattern.Pattern, isPatternBase, null);
		EmailMessage emailMessage = await CreateEmailMessage(text, to, GetEmailSubject(AppSmsType.WalletWithdrawOtp), appSettings);
		_emailService.Send(emailMessage);
		AppEmail appEmail = new AppEmail
		{
			Id = emailMessage.AppEmailId,
			AppEmailType = AppSmsType.WalletWithdrawNotification,
			Date = DateTime.Now,
			Text = text,
			PatternTitle = appSettings.WalletWithdrawOtpEmailPattern.Title,
			To = to,
			EntityType = "AgentFactor",
			EntityId = agentFactorModel.Id,
			EmailPanel = appSettings.DefaultEmailPanel
		};
		await _appEmailService.Insert(appEmail);
		return appEmail;
	}

	public async Task<bool> TrySendLogin(LoginRegisterModel loginRegisterModel, Guid customerId)
	{
		_ = 4;
		try
		{
			if (_hostingEnvironment.EnvironmentName == "Development")
			{
				return false;
			}
			_currentUserService.CustomerId = customerId;
			AppSettings appSettings = await _appSettingsService.GetAppSettings();
			await SetEmailService(appSettings);
			if (_emailService == null)
			{
				return false;
			}
			if (appSettings?.LoginEmailPattern == null)
			{
				return false;
			}
			string to = loginRegisterModel.Email;
			if (string.IsNullOrEmpty(to))
			{
				return false;
			}
			if (appSettings.LoginEmailPattern.IsPatternBase)
			{
				_ = !string.IsNullOrEmpty(appSettings.LoginEmailPattern.PatternBaseCode);
			}
			string text = await ExtractTextFromPattern(loginRegisterModel, appSettings.LoginEmailPattern.Pattern);
			EmailMessage emailMessage = await CreateEmailMessage(text, to, GetEmailSubject(AppSmsType.Login), appSettings);
			_emailService.Send(emailMessage);
			AppEmail entity = new AppEmail
			{
				Id = emailMessage.AppEmailId,
				AppEmailType = AppSmsType.Login,
				Date = DateTime.Now,
				Text = text,
				PatternTitle = appSettings.LoginEmailPattern.Title,
				To = to,
				EmailPanel = appSettings.DefaultEmailPanel
			};
			await _appEmailService.Insert(entity);
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	public async Task<bool> TrySendRegister(LoginRegisterModel loginRegisterModel, Guid customerId)
	{
		_ = 4;
		try
		{
			if (_hostingEnvironment.EnvironmentName == "Development")
			{
				return false;
			}
			_currentUserService.CustomerId = customerId;
			AppSettings appSettings = await _appSettingsService.GetAppSettings();
			await SetEmailService(appSettings);
			if (_emailService == null)
			{
				return false;
			}
			if (appSettings?.RegisterEmailPattern == null)
			{
				return false;
			}
			string to = loginRegisterModel.Email;
			if (string.IsNullOrEmpty(to))
			{
				return false;
			}
			string text = await ExtractTextFromPattern(loginRegisterModel, appSettings.RegisterEmailPattern.Pattern);
			EmailMessage emailMessage = await CreateEmailMessage(text, to, GetEmailSubject(AppSmsType.Register), appSettings);
			_emailService.Send(emailMessage);
			AppEmail entity = new AppEmail
			{
				Id = emailMessage.AppEmailId,
				AppEmailType = AppSmsType.Register,
				Date = DateTime.Now,
				Text = text,
				PatternTitle = appSettings.RegisterEmailPattern.Title,
				To = to,
				EmailPanel = appSettings.DefaultEmailPanel
			};
			await _appEmailService.Insert(entity);
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	private async Task<string> ExtractTextFromPattern(CustomerOrderModel model, string pattern, Customer customer)
	{
		foreach (string item in new List<string> { "{نام مشتری}", "{نام فروشگاه}", "{کد ملی}", "{تاریخ}", "{کالای مورد درخواست}" })
		{
			switch (item)
			{
			case "{نام مشتری}":
				pattern = pattern.Replace(item, model.Name);
				break;
			case "{نام فروشگاه}":
				pattern = pattern.Replace(item, customer.Name);
				break;
			case "{تاریخ}":
				pattern = pattern.Replace(item, model.SaveDate_Persian.Date);
				break;
			case "{کد ملی}":
				pattern = pattern.Replace(item, model.NationalCode);
				break;
			case "{کالای مورد درخواست}":
				pattern = pattern.Replace(item, model.ProductName);
				break;
			}
		}
		return pattern;
	}

	private async Task<string> ExtractTextFromPattern(AgentFactorModel agentFactor, string pattern, bool isPatternBase, string code)
	{
		foreach (string item in new List<string> { "{نام تجاری نماینده}", "{نام مشتری}", "{تاریخ}", "{مبلغ}", "{مانده اعتبار کیف پول}", "{کد تائیدیه}", "{کد رهگیری}" })
		{
			switch (item)
			{
			case "{کد رهگیری}":
			{
				string text = agentFactor.Code.ToString();
				pattern = pattern.Replace(item, isPatternBase ? ("{" + text + "}") : text);
				break;
			}
			case "{کد تائیدیه}":
			{
				string text = code.ToString();
				pattern = pattern.Replace(item, isPatternBase ? ("{" + text + "}") : text);
				break;
			}
			case "{نام مشتری}":
				pattern = pattern.Replace(item, isPatternBase ? ("{" + agentFactor?.Member.FullName + "}") : agentFactor?.Member.FullName);
				break;
			case "{نام تجاری نماینده}":
				pattern = pattern.Replace(item, isPatternBase ? ("{" + agentFactor?.Agent.FullName + "}") : agentFactor?.Agent.FullName);
				break;
			case "{تاریخ}":
			{
				string text = agentFactor?.CreateAt_Persian.Date ?? "";
				pattern = pattern.Replace(item, isPatternBase ? ("{" + text + "}") : text);
				break;
			}
			case "{مبلغ}":
			{
				string text = ((double)(agentFactor?.Amount).Value).ToString("N0");
				pattern = pattern.Replace(item, isPatternBase ? ("{" + text + "}") : text);
				break;
			}
			case "{مانده اعتبار کیف پول}":
			{
				string text = agentFactor.Member.Credit.ToString("N0");
				pattern = pattern.Replace(item, isPatternBase ? ("{" + text + "}") : text);
				break;
			}
			}
		}
		return pattern;
	}

	private async Task<string> ExtractTextFromPattern(LoanPaymentModel loanPayment, string pattern)
	{
		List<string> list = new List<string> { "{نام مشتری}", "{نام فروشگاه}", "{مبلغ قسط}", "{شماره قسط}", "{مبلغ واریز}", "{تاریخ واریز}", "{مانده وام}", "{لینک پرداخت}", "{بخش متغیر لینک پرداخت}", "{شماره پیگیری تراکنش}" };
		foreach (string tag in list)
		{
			switch (tag)
			{
			case "{نام مشتری}":
				pattern = pattern.Replace(tag, loanPayment?.UserLoanModel?.MemberModel.FullName);
				break;
			case "{نام فروشگاه}":
				pattern = pattern.Replace(tag, (await _customerService.GetById(_currentUserService.CustomerId.Value)).Name);
				break;
			case "{مبلغ قسط}":
			{
				string val = ((double)(loanPayment?.UserLoanModel?.Installment).Value).ToString("N0");
				pattern = pattern.Replace(tag, val);
				break;
			}
			case "{شماره قسط}":
			{
				string val = (loanPayment?.InstallmentNumber).ToString() ?? "";
				pattern = pattern.Replace(tag, val);
				break;
			}
			case "{مبلغ واریز}":
			{
				string val = ((double)(loanPayment?.SumAmount).Value).ToString("N0");
				pattern = pattern.Replace(tag, val);
				break;
			}
			case "{تاریخ واریز}":
			{
				string val = loanPayment?.SaveDate_Persian.Date ?? "";
				pattern = pattern.Replace(tag, val);
				break;
			}
			case "{مانده وام}":
			{
				string val = ((double)(loanPayment?.UserLoanModel.Remain).Value).ToString("N0");
				pattern = pattern.Replace(tag, val);
				break;
			}
			case "{لینک پرداخت}":
			{
				Customer customer = await _customerService.GetById(_currentUserService.CustomerId.Value);
				string newValue = _appConfig.BaseUrl.TrimEnd('/') + "/pop/" + customer.Code + "/" + loanPayment.Code;
				pattern = pattern.Replace(tag, newValue);
				break;
			}
			case "{بخش متغیر لینک پرداخت}":
			{
				string newValue = (await _customerService.GetById(_currentUserService.CustomerId.Value)).Code + "/" + loanPayment.Code;
				pattern = pattern.Replace(tag, newValue);
				break;
			}
			case "شماره پیگیری تراکنش":
			{
				string val = "";
				if (loanPayment.PaymentMethod == PaymentMethods.Online)
				{
					using IServiceScope scope = _serviceProvider.CreateScope();
					MemberPurchase memberPurchase = await scope.ServiceProvider.GetRequiredService<MemberPurchaseService>().GetByUserLoanPaymentId(loanPayment.Id);
					if (memberPurchase != null)
					{
						MemberPurchaseModel memberPurchaseModel = new MemberPurchaseModel(memberPurchase);
						val = memberPurchaseModel.TransactionCode;
					}
				}
				else
				{
					val = loanPayment.Code.ToString();
				}
				pattern = pattern.Replace(tag, val);
				break;
			}
			}
		}
		return pattern;
	}

	private async Task<string> ExtractTextFromPattern(UserLoanRequestModel loanRequestModel, string pattern)
	{
		List<string> list = new List<string>
		{
			"{نام مشتری}", "{نام فروشگاه}", "{مبلغ هر قسط}", "{مبلغ درخواستی}", "{تعداد قسط}", "{تاریخ درخواست}", "{نوع وام}", "{شماره درخواست}", "{تاریخ تغییر وضعیت}", "{نام نماینده}",
			"{وضعیت جدید}", "{وضعیت جدید نماینده}", "{سررسید واریز نماینده}", "{توسط نماینده با لیبل}"
		};
		foreach (string tag in list)
		{
			switch (tag)
			{
			case "{نام مشتری}":
				pattern = pattern.Replace(tag, loanRequestModel?.MemberModel.FullName);
				break;
			case "{نام فروشگاه}":
				pattern = pattern.Replace(tag, (await _customerService.GetById(_currentUserService.CustomerId.Value)).Name);
				break;
			case "{مبلغ هر قسط}":
			{
				string newValue = ((double)(loanRequestModel?.Installment).Value).ToString("N0");
				pattern = pattern.Replace(tag, newValue);
				break;
			}
			case "{مبلغ درخواستی}":
			{
				string newValue = (loanRequestModel?.Amount).ToString() ?? "";
				pattern = pattern.Replace(tag, newValue);
				break;
			}
			case "{تعداد قسط}":
			{
				string newValue = ((double)(loanRequestModel?.InstallmentCount).Value).ToString("N0");
				pattern = pattern.Replace(tag, newValue);
				break;
			}
			case "{تاریخ درخواست}":
			{
				string newValue = loanRequestModel?.RequestDate_Persian.Date ?? "";
				pattern = pattern.Replace(tag, newValue);
				break;
			}
			case "{نوع وام}":
			{
				string newValue = loanRequestModel?.LoanModel?.Title;
				pattern = pattern.Replace(tag, newValue);
				break;
			}
			case "{شماره درخواست}":
			{
				string newValue = loanRequestModel?.Code.ToString();
				pattern = pattern.Replace(tag, newValue);
				break;
			}
			case "{تاریخ تغییر وضعیت}":
			{
				string newValue = DateTime.Now.ToPersianDateObject().Date;
				pattern = pattern.Replace(tag, newValue);
				break;
			}
			case "{نام نماینده}":
			{
				string newValue = loanRequestModel?.AgentModel?.BusinessName ?? "";
				pattern = pattern.Replace(tag, newValue);
				break;
			}
			case "{توسط نماینده با لیبل}":
			{
				string newValue = loanRequestModel?.AgentModel?.BusinessName ?? "";
				if (!string.IsNullOrEmpty(newValue))
				{
					newValue = "توسط نماینده " + newValue;
				}
				pattern = pattern.Replace(tag, newValue);
				break;
			}
			case "{وضعیت جدید}":
			{
				string newValue = loanRequestModel?.Str_Status;
				pattern = pattern.Replace(tag, newValue);
				break;
			}
			case "{وضعیت جدید نماینده}":
			{
				string newValue = loanRequestModel?.Str_AgentStatus;
				pattern = pattern.Replace(tag, newValue);
				break;
			}
			case "{سررسید واریز نماینده}":
			{
				string newValue = ((loanRequestModel == null || loanRequestModel.UserLoan?.SubAgentDueDate.HasValue != true || !(loanRequestModel?.UserLoan?.SubAgentDueDate != DateTime.MinValue)) ? ((loanRequestModel?.UserLoan?.AgentDueDate_Persian.Date ?? "") ?? "") : ((loanRequestModel?.UserLoan?.SubAgentDueDate_Persian.Date ?? "") ?? ""));
				pattern = pattern.Replace(tag, newValue);
				break;
			}
			}
		}
		return pattern;
	}

	private async Task<string> ExtractTextFromPattern(LoginRegisterModel loginRegisterModel, string pattern)
	{
		foreach (string item in new List<string> { "{نام و نام خانوادگی}", "{نام کاربری}", "{تاریخ}", "{تاریخ و ساعت}", "{آی پی}", "{مرورگر}", "{نام تجاری نماینده}" })
		{
			switch (item)
			{
			case "{نام و نام خانوادگی}":
				pattern = pattern.Replace(item, loginRegisterModel.User_FullName);
				break;
			case "{نام کاربری}":
				pattern = pattern.Replace(item, loginRegisterModel.User_UserName);
				break;
			case "{تاریخ و ساعت}":
				pattern = pattern.Replace(item, loginRegisterModel.PersianDate.Date + " " + loginRegisterModel.PersianDate.Time);
				break;
			case "{تاریخ}":
				pattern = pattern.Replace(item, loginRegisterModel.PersianDate.Date);
				break;
			case "{آی پی}":
			{
				string newValue = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
				pattern = pattern.Replace(item, newValue);
				break;
			}
			case "{مرورگر}":
			{
				StringValues stringValues = _httpContextAccessor.HttpContext.Request.Headers["User-Agent"];
				ClientInfo clientInfo = Parser.GetDefault().Parse(stringValues);
				string newValue = clientInfo.UA.Family + " " + clientInfo.OS.Family;
				pattern = pattern.Replace(item, newValue);
				break;
			}
			case "{نام تجاری نماینده}":
				pattern = pattern.Replace(item, loginRegisterModel.Agent_BusinessName ?? "");
				break;
			}
		}
		return pattern;
	}

	private async Task<string> ExtractTextFromPattern(RegisterOtpModel registerOtpModel, string pattern)
	{
		foreach (string item in new List<string> { "{رمز تاییدیه}", "{تاریخ}", "{تاریخ و ساعت}", "{آی پی}", "{مرورگر}" })
		{
			switch (item)
			{
			case "{رمز تاییدیه}":
				pattern = pattern.Replace(item, registerOtpModel.Code ?? "");
				break;
			case "{تاریخ و ساعت}":
				pattern = pattern.Replace(item, registerOtpModel.PersianDate.Date + " " + registerOtpModel.PersianDate.Time);
				break;
			case "{تاریخ}":
				pattern = pattern.Replace(item, registerOtpModel.PersianDate.Date);
				break;
			case "{آی پی}":
			{
				string newValue = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
				pattern = pattern.Replace(item, newValue);
				break;
			}
			case "{مرورگر}":
			{
				StringValues stringValues = _httpContextAccessor.HttpContext.Request.Headers["User-Agent"];
				ClientInfo clientInfo = Parser.GetDefault().Parse(stringValues);
				string newValue = clientInfo.UA.Family + " " + clientInfo.OS.Family;
				pattern = pattern.Replace(item, newValue);
				break;
			}
			}
		}
		return pattern;
	}

	private async Task<string> ExtractTextFromPattern(WalletChargeModel walletModel, string pattern)
	{
		List<string> list = new List<string> { "{نام مشتری}", "{نام فروشگاه}", "{لینک پرداخت}", "{تاریخ}" };
		foreach (string tag in list)
		{
			switch (tag)
			{
			case "{نام مشتری}":
				pattern = pattern.Replace(tag, walletModel.Member.FullName);
				break;
			case "{نام فروشگاه}":
				pattern = pattern.Replace(tag, (await _customerService.GetById(_currentUserService.CustomerId.Value)).Name);
				break;
			case "{تاریخ}":
				pattern = pattern.Replace(tag, walletModel.PersianDate.Date);
				break;
			case "{لینک پرداخت}":
			{
				Customer customer = await _customerService.GetById(_currentUserService.CustomerId.Value);
				string newValue = _appConfig.BaseUrl.TrimEnd('/') + $"walletcharge/?ccode={customer.Code}&mid={walletModel.Member.Id}";
				pattern = pattern.Replace(tag, newValue);
				break;
			}
			}
		}
		return pattern;
	}

	private async Task SetEmailService(AppSettings appSettings = null)
	{
		if (appSettings == null)
		{
			appSettings = await _appSettingsService.GetAppSettings();
		}
		if (appSettings == null)
		{
			return;
		}
		AppSettingsModel appSettingsModel = new AppSettingsModel(appSettings);
		switch (appSettingsModel.DefaultEmailPanel)
		{
		case EmailPanel.JetAqsaat:
			if (_emailConfig != null)
			{
				_emailService.SetEmailConfig(new EmailConfig
				{
					Name = _emailConfig.Name,
					SmtpPassword = _emailConfig.SmtpPassword,
					SmtpUsername = _emailConfig.SmtpUsername,
					SmtpHost = _emailConfig.SmtpHost,
					SmtpPort = _emailConfig.SmtpPort
				});
			}
			break;
		case EmailPanel.Custom:
			if (appSettingsModel.EmailConfig != null)
			{
				_emailService.SetEmailConfig(new EmailConfig
				{
					Name = appSettingsModel.EmailConfig.Name,
					SmtpUsername = appSettingsModel.EmailConfig.SmtpUsername,
					SmtpHost = appSettingsModel.EmailConfig.SmtpHost,
					SmtpPort = appSettingsModel.EmailConfig.SmtpPort,
					SmtpPassword = appSettingsModel.EmailConfig.SmtpPassword
				});
			}
			break;
		case EmailPanel.Unknown:
			break;
		}
	}

	private void PrepareKavehTokens(SendKaveNegar sendPayamak, string input)
	{
		MatchCollection matchCollection = new Regex("{(.*?)}").Matches(input);
		int num = 0;
		foreach (Match item in matchCollection)
		{
			switch (num++)
			{
			case 0:
				sendPayamak.Token1 = item.Groups[1].Value.Replace(" ", "_");
				break;
			case 1:
				sendPayamak.Token2 = item.Groups[1].Value.Replace(" ", "_");
				break;
			case 2:
				sendPayamak.Token3 = item.Groups[1].Value.Replace(" ", "_");
				break;
			case 3:
				sendPayamak.Token10 = item.Groups[1].Value.Replace(" ", "_");
				break;
			case 4:
				sendPayamak.Token20 = item.Groups[1].Value.Replace(" ", "_");
				break;
			}
		}
	}

	private void PrepareMelliPayamakTextPattern(SendMelliPayamak sendMelliPayamak, string input)
	{
		MatchCollection source = new Regex("{(.*?)}").Matches(input);
		sendMelliPayamak.Text = string.Join(';', source.Select((Match x) => x.Groups[1].Value));
	}

	private async Task<EmailMessage> CreateEmailMessage(string body, string to, string subject, AppSettings appSettings = null)
	{
		if (appSettings == null)
		{
			appSettings = await _appSettingsService.GetAppSettings();
		}
		string path = Path.Combine(_hostingEnvironment.WebRootPath, EMAIL_LAYOUT);
		string text = "";
		if (File.Exists(path))
		{
			text = File.ReadAllText(path);
		}
		if (!string.IsNullOrWhiteSpace(text))
		{
			body = text.Replace("@CONTENT", body).Replace("@TITLE", appSettings.BusinessTitle).Replace("@SOCIALS", CreateSocialLinks())
				.Replace("@URLNOHTTP", _appConfig.BaseUrl.Replace("https://", "").Replace("http://", ""))
				.Replace("@URL", _appConfig.BaseUrl);
		}
		return new EmailMessage
		{
			AppEmailId = Guid.NewGuid(),
			Content = body,
			ToAddress = new EmailAddress
			{
				Address = to
			},
			Subject = subject
		};
		string CreateSocialLinks()
		{
			string text2 = "";
			if (!string.IsNullOrWhiteSpace(appSettings.TelegramLink))
			{
				text2 = text2 + "<a href=\"" + appSettings.TelegramLink + "\" target=\"_blank\"><img src=\"" + _appConfig.BaseUrl.TrimEnd('/') + "/icons-telegram.png\" /></a>";
			}
			if (!string.IsNullOrWhiteSpace(appSettings.WhatsappLink))
			{
				text2 = text2 + "<a href=\"" + appSettings.WhatsappLink + "\" target=\"_blank\"><img src=\"" + _appConfig.BaseUrl.TrimEnd('/') + "/icons-whatsapp.png\" /></a>";
			}
			if (!string.IsNullOrWhiteSpace(appSettings.InstagramLink))
			{
				text2 = text2 + "<a href=\"" + appSettings.InstagramLink + "\" target=\"_blank\"><img src=\"" + _appConfig.BaseUrl.TrimEnd('/') + "/icons-instagram.png\" /></a>";
			}
			if (!string.IsNullOrWhiteSpace(appSettings.RubikaLink))
			{
				text2 = text2 + "<a href=\"" + appSettings.RubikaLink + "\" target=\"_blank\"><img src=\"" + _appConfig.BaseUrl.TrimEnd('/') + "/icons-rubika.png\" /></a>";
			}
			if (!string.IsNullOrWhiteSpace(appSettings.AparatLink))
			{
				text2 = text2 + "<a href=\"" + appSettings.AparatLink + "\" target=\"_blank\"><img src=\"" + _appConfig.BaseUrl.TrimEnd('/') + "/icons-aparat.png\" /></a>";
			}
			return text2;
		}
	}

	private string GetEmailSubject(AppSmsType type)
	{
		return type switch
		{
			AppSmsType.DueDate => "یادآوری سررسید قسط", 
			AppSmsType.Thanks => "تشکر از پرداخت قسط", 
			AppSmsType.OverDueDate => "قسط معوق", 
			AppSmsType.BeforeDueDate => "یادآوری پیش از موعد", 
			AppSmsType.Other => "سایر", 
			AppSmsType.ChangeStatus => "تغییر وضعیت درخواست", 
			AppSmsType.ChangeAgentStatus => "تغییر وضعیت درخواست", 
			AppSmsType.VerifyCode => "کد تائیدیه", 
			AppSmsType.Login => "شناسایی ورود به نرم افزار", 
			AppSmsType.Register => "خوش آمد ثبت نام", 
			AppSmsType.WalletCharge => "شارژ کیف پول", 
			AppSmsType.LoginOTP => "کد تائیدیه ورود به سیستم", 
			AppSmsType.DailyReport => "گزارش روزانه", 
			_ => "", 
		};
	}
}
