using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models.IranValidity;
using LoanSystemAvizhe.Data.Enums;
using LoanSystemAvizhe.Services.FileHelper;
using LoanSystemAvizhe.Services.Models.IranValidity;
using LoanSystemAvizhe.Services.ThirdParty;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LoanSystem.Site.Controllers;

[ApiController]
[Route("[controller]")]
public class IranValidityController : BaseDefaultController
{
	private readonly ICustomerService _customerService;

	private readonly IIranValidityReportService _iranValidityReportService;

	private readonly IAppSettingsService _appSettingsService;

	private readonly IHostingEnvironment _hostingEnvironment;

	public IranValidityController(ILogger<SiteController> logger, IAppSettingsService appSettingsService, ICustomerService customerService, IIranValidityReportService iranValidityReportService, IHostingEnvironment hostingEnvironment)
		: base(logger)
	{
		_customerService = customerService;
		_iranValidityReportService = iranValidityReportService;
		_hostingEnvironment = hostingEnvironment;
		_appSettingsService = appSettingsService;
	}

	[HttpGet("[action]/{customerCode}")]
	public async Task<IActionResult> ReportReady(int customerCode)
	{
		Customer customer = await _customerService.GetByCode(customerCode);
		if (customer == null)
		{
			return Ok();
		}
		if (!Request.Query.TryGetValue("message", out var value))
		{
			return Ok();
		}
		if (string.IsNullOrWhiteSpace(value) || !value.ToString().Contains('-'))
		{
			return BadRequest();
		}
		string[] array = value.ToString().Split('-');
		string msg = array[0];
		string hashCode = array[1].Split('@')[0];
		string reportLink = (array[1].Contains('@') ? array[1].Split('@')[1] : "");
		IranValidityReport report = (await _iranValidityReportService.GetByCondition((IranValidityReport x) => x.CustomerId == customer.Id && x.ReportID == hashCode))?.FirstOrDefault();
		if (report == null || report.Status == IranValidityReportStatus.گزارش_آماده_شده)
		{
			return BadRequest();
		}
		SetReportStatus(report, msg);
		if (report.Status == IranValidityReportStatus.گزارش_آماده_شده)
		{
			try
			{
				string text = Directories.IranValidityReportPdf(customer.Id.ToString());
				if (text != null)
				{
					string contentRootPath = _hostingEnvironment.ContentRootPath;
					string text2 = Path.Combine(contentRootPath, text);
					if (!Directory.Exists(text2))
					{
						Directory.CreateDirectory(text2);
					}
					string fileName = Guid.NewGuid().ToString().Replace("-", "") + ".pdf";
					string path = Path.Combine(text2, fileName);
					IranValidityConfig iranValidityConfig = JsonSerializer.Deserialize<IranValidityConfig>((await _appSettingsService.GetAppSettings(customer.Id)).IranValidityConfigJson);
					IranValidityService iranValidityService = new IranValidityService(iranValidityConfig, !(_hostingEnvironment.EnvironmentName == "Development") && false);
					string hashCode2 = reportLink.Substring(reportLink.LastIndexOf('/'));
					IranValidityPdfReportData iranValidityPdfReportData = await iranValidityService.GetReportPDF(new IranValidityStatusRequest
					{
						HashCode = hashCode2
					});
					if (iranValidityPdfReportData.Data.Length != 0)
					{
						System.IO.File.WriteAllBytes(path, iranValidityPdfReportData.Data);
					}
					report.ReportPdfFileName = fileName;
				}
				report.Url = reportLink.ToString();
				report.Messages = "";
			}
			catch
			{
			}
		}
		await _iranValidityReportService.Update(report);
		return Ok("");
	}

	private void SetReportStatus(IranValidityReport report, string msg)
	{
		if (msg == null)
		{
			return;
		}
		switch (msg.Length)
		{
		case 11:
			if (msg == "OTPAccepted")
			{
				report.Messages = "کد مجوز پیامکی تایید شد.";
			}
			break;
		case 9:
			if (msg == "OTPLocked")
			{
				report.Messages = "OTP قفل شد، کاربر نمی تواند کد جدید دریافت کند.";
			}
			break;
		case 13:
			if (msg == "ShahkarFailed")
			{
				report.Messages = "شماره ملی و شماره موبایل با هم تطابق ندارند.";
				report.Status = IranValidityReportStatus.خطا_در_ایجاد_گزارش_توسط_شرکت_رتبه_بندی;
			}
			break;
		case 12:
			if (msg == "ShahkarError")
			{
				report.Messages = "ارتباط با سامانه شاهکار، برقرار نشد یا درخواست با خطا مواجه شد.";
				report.Status = IranValidityReportStatus.خطا_در_ایجاد_گزارش_توسط_شرکت_رتبه_بندی;
			}
			break;
		case 21:
			if (msg == "IdentityCheckAccepted")
			{
				report.Messages = "احراز هویت کاربر با موفقیت انجام شد.";
			}
			break;
		case 15:
			if (msg == "ReportGenerated")
			{
				report.Messages = "گزارش ایجاد شد.";
				report.Status = IranValidityReportStatus.گزارش_آماده_شده;
			}
			break;
		case 20:
			if (msg == "GenerateReportFailed")
			{
				report.Messages = "دریافت گزارش با مشکل مواجه شد.";
				report.Status = IranValidityReportStatus.خطا_در_ایجاد_گزارش_توسط_شرکت_رتبه_بندی;
			}
			break;
		case 14:
			if (msg == "ReportDataLost")
			{
				report.Messages = "گزارش ایجاد شد ولی در ذخیره سازی با خطا مواجه شد.";
				report.Status = IranValidityReportStatus.خطا_در_ایجاد_گزارش_توسط_شرکت_رتبه_بندی;
			}
			break;
		case 10:
		case 16:
		case 17:
		case 18:
		case 19:
			break;
		}
	}

	[HttpGet("[action]/{reportId}")]
	public async Task<IActionResult> DownloadPdf(Guid reportId)
	{
		IranValidityReport iranValidityReport = await _iranValidityReportService.AsQueryable().Include((IranValidityReport x) => x.Member).IgnoreQueryFilters()
			.FirstOrDefaultAsync((IranValidityReport x) => x.Id == reportId);
		if (iranValidityReport == null || string.IsNullOrEmpty(iranValidityReport.ReportPdfFileName))
		{
			return NotFound();
		}
		string path = Directories.IranValidityReportPdf(iranValidityReport.CustomerId.ToString());
		string contentRootPath = _hostingEnvironment.ContentRootPath;
		string path2 = Path.Combine(contentRootPath, path, iranValidityReport.ReportPdfFileName);
		if (!System.IO.File.Exists(path2))
		{
			return NotFound();
		}
		byte[] fileContents = System.IO.File.ReadAllBytes(path2);
		return File(fileContents, "application/pdf", "گزارش اعتبارسنجی " + iranValidityReport.Member?.FullName + ".pdf");
	}
}
