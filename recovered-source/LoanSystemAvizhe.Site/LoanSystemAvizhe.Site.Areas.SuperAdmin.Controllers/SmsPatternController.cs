using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Areas.SuperAdmin.Controllers;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Data.Enums;
using LoanSystemAvizhe.Site.Areas.SuperAdmin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoanSystemAvizhe.Site.Areas.SuperAdmin.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "SuperAdmin")]
public class SmsPatternController : BaseSuperAdminController
{
	private readonly ISmsPatternService _smsPatternService;

	private readonly IAppSettingsService _appSettingsService;

	public SmsPatternController(ILogger<SmsPatternController> logger, ISmsPatternService smsPatternService, IAppSettingsService appSettingsService)
		: base(logger)
	{
		_smsPatternService = smsPatternService;
		_appSettingsService = appSettingsService;
	}

	[HttpGet("[action]/{all}")]
	public Task<ResponseState<SuperAdminSmsPatterns>> List(bool all)
	{
		return TryCatch(async () =>
		{
			List<SmsPattern> entities = await _smsPatternService.GetAll(null, all ? ((SmsPatternStatus?)null) : new SmsPatternStatus?(SmsPatternStatus.Pending), "Customer");
			int countAll = (all ? entities.Count : (await _smsPatternService.GetCount(null, null)));
			List<SmsPatternModel> smsPatterns = entities.Select((SmsPattern x) => new SmsPatternModel(x)).ToList();
			SuperAdminSmsPatterns data = new SuperAdminSmsPatterns
			{
				SmsPatterns = smsPatterns,
				CountAll = countAll
			};
			return Success(data);
		});
	}

	[HttpGet("[action]/{id}")]
	public Task<ResponseState<SmsPatternModel>> Get(Guid id)
	{
		return TryCatch(async () =>
		{
			SmsPatternModel data = new SmsPatternModel();
			if (id != Guid.Empty)
			{
				SmsPattern smsPattern = await _smsPatternService.GetById(id);
				if (smsPattern == null)
				{
					return Error<SmsPatternModel>(null, "اطلاعات مورد نظر یافت نشد");
				}
				data = new SmsPatternModel(smsPattern);
			}
			return Success(data);
		});
	}

	[HttpPost("[action]")]
	public Task<DefaultResponseState> Edit([FromBody] SmsPatternModel model)
	{
		return TryCatch(async () =>
		{
			SmsPattern smsPattern = await _smsPatternService.GetById(model.Id);
			if (smsPattern == null)
			{
				return Error("اطلاعات مورد نظر یافت نشد.");
			}
			smsPattern.Status = model.Status;
			smsPattern.RejectReason = model.RejectReason;
			smsPattern.Pattern = model.Pattern;
			smsPattern.Title = model.Title;
			try
			{
				await _smsPatternService.Update(smsPattern);
			}
			catch (Exception)
			{
				throw;
			}
			return Success("عملیات با موفقیت انجام شد.");
		});
	}

	[HttpPost("[action]/{id}")]
	public Task<DefaultResponseState> Delete(Guid id)
	{
		return TryCatch(async () =>
		{
			AppSettings appSettings = await _appSettingsService.GetAppSettings();
			if (appSettings.ThanksPaymentSmsPatternId == id || appSettings.DueDateRememberSmsPatternId == id)
			{
				return Error("این قالب در تنظیمات اپلیکیشن استفاده شده است، جهت حذف ایتدا باید از تنظیمات خارج شود.");
			}
			SmsPattern smsPattern = await _smsPatternService.GetById(id);
			if (smsPattern == null)
			{
				return Error("امکان حذف وجود ندارد. لطفا بعدا دوباره تلاش نمایید");
			}
			await _smsPatternService.Delete(smsPattern);
			return Success();
		});
	}
}
