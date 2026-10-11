using System.Collections.Generic;
using System.Threading.Tasks;
using LoanSystem.Core.Configuration;
using LoanSystem.Core.Exceptions;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Helpers.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LoanSystem.Site.Areas.Admin.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Admin")]
public class BackupController : BaseAdminController
{
	private readonly AppConfig _appConfig;

	private readonly IBackupService _backupService;

	public BackupController(ILogger<BackupController> logger, IBackupService backupService, IOptions<AppConfig> appConfig)
		: base(logger)
	{
		_backupService = backupService;
		_appConfig = appConfig.Value;
	}

	[HttpGet("[action]")]
	public Task<ResponseState<List<dynamic>>> List()
	{
		return TryCatch(async () => Success(await _backupService.GetBackupList()));
	}

	[HttpPost("[action]")]
	public Task<ResponseState<string>> Create()
	{
		return TryCatch(async () =>
		{
			try
			{
				if (_appConfig.IsCloud)
				{
					return Error("", "در حال حاضر در نسخه ابری امکان بکاپ گیری وجود ندارد.");
				}
				return Success(await _backupService.Create(), "");
			}
			catch (CustomException ex)
			{
				return Error("", "خطا در ایجاد فایل بکاپ." + ex.Message);
			}
		});
	}

	[HttpPost("[action]")]
	public Task<ResponseState<string>> Restore(RestoreBackupModel model)
	{
		return TryCatch(async () =>
		{
			if (_appConfig.IsCloud)
			{
				return Error("", "در حال حاضر در نسخه ابری امکان بازیابی بکاپ وجود ندارد.");
			}
			if ((model.File == null || model.File.Length == 0L) && string.IsNullOrEmpty(model.FileName))
			{
				return Error("", "فایل خالی است.");
			}
			await _backupService.Restore(model);
			return Success("", "");
		});
	}
}
