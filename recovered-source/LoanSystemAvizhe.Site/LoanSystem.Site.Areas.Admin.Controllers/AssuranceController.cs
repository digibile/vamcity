using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Areas.Admin.Models;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Data.Enums;
using LoanSystemAvizhe.Services.Enums;
using LoanSystemAvizhe.Services.FileHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Spm.Services.Infrastructure;

namespace LoanSystem.Site.Areas.Admin.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Admin,Agent")]
public class AssuranceController : BaseAdminController
{
	private readonly IAssuranceService _assuranceService;

	private readonly IFileHelper _fileHelper;

	private readonly ICurrentUserService _currentUserService;

	private readonly IAppSettingsService _appSettingsService;

	private readonly IAppSmsService _appSmsService;

	public AssuranceController(ILogger<AssuranceController> logger, IAssuranceService assuranceService, IFileHelper fileHelper, ICurrentUserService currentUserService, IAppSettingsService appSettingsService, IAppSmsService appSmsService)
		: base(logger)
	{
		_assuranceService = assuranceService;
		_fileHelper = fileHelper;
		_currentUserService = currentUserService;
		_appSettingsService = appSettingsService;
		_appSmsService = appSmsService;
	}

	[HttpGet("[action]/{id}/{fillImages?}")]
	public Task<ResponseState<List<AssuranceModel>>> ListByUserLoanId(Guid id, bool? fillImages = true)
	{
		return TryCatch(async () =>
		{
			List<AssuranceModel> data = (await _assuranceService.GetByUserLoanId(id)).Select((Assurance x) =>
			{
				AssuranceModel assuranceModel = new AssuranceModel(x);
				if (fillImages.Value)
				{
					assuranceModel.FillImageBase64Prop(_fileHelper, _currentUserService.CustomerId.Value);
					assuranceModel.AssuranceChequeItems.ForEach((AssuranceChequeItemModel assuranceChequeItemModel) =>
					{
						assuranceChequeItemModel.FillImageBase64Prop(_fileHelper, _currentUserService.CustomerId.Value);
					});
				}
				return assuranceModel;
			}).ToList();
			return Success(data);
		});
	}

	[HttpPost("[action]")]
	public Task<ResponseState<Guid>> Create(AssuranceModel model)
	{
		return TryCatch(async () =>
		{
			string text = ValidateAssurance(model);
			if (text != string.Empty)
			{
				return Error(text);
			}
			List<AssuranceFile> manageFiles = new List<AssuranceFile>();
			long fileSize = 0L;
			string fileType = "";
			string fileName = "";
			if (model.File != null)
			{
				fileSize = model.File.Length;
				fileType = Path.GetExtension(model.File.FileName);
				fileName = Guid.NewGuid().ToString().Replace("-", "") + fileType;
				manageFiles.Add(new AssuranceFile
				{
					File = model.File,
					FileName = fileName,
					ForAdd = true,
					IsAssurance = true
				});
			}
			bool mobileVerified = false;
			AppSettings appSettings = await _appSettingsService.GetAppSettings();
			if (appSettings != null)
			{
				_ = appSettings.DefaultSmsPanel;
				if (appSettings.RegisterOtpSmsPatternId.HasValue && model.AssuranceType == AssuranceType.OtherOrganization)
				{
					if (string.IsNullOrEmpty(model.MobileOtp))
					{
						return Error("کد تاییدیه موبایل وارد نشده است.");
					}
					List<AppSms> list = await _appSmsService.GetByCondition((AppSms x) => x.To == model.PersonMobile && (int)x.AppSmsType == 8 && x.Date >= DateTime.Now.AddMinutes(-2.0) && x.Tag == model.MobileOtp, false);
					if (list == null || !list.Any())
					{
						return Error("کد تاییدیه موبایل معتبر نمی باشد.");
					}
					mobileVerified = true;
				}
			}
			Assurance entity = null;
			await _assuranceService.RunInTransaction(async (DbTransaction trs) =>
			{
				entity = new Assurance
				{
					Description = model.Description,
					FileName = fileName,
					UserLoanId = model.UserLoanModel.Id,
					FileType = fileType,
					FileSize = fileSize,
					AssuranceType = model.AssuranceType,
					GoldWeight = model.GoldWeight,
					CoinType = model.CoinType,
					IsReturned = model.IsReturned,
					OrganizationId = model.Organization?.Id,
					PersonFullName = model.PersonFullName,
					PersonNationalCode = model.PersonNationalCode,
					PersonalCode = model.PersonalCode,
					PersonMobile = model.PersonMobile,
					MobileVerified = mobileVerified,
					ReturnDateTime = model.ReturnDateTime,
					ReturnedToPerson = model.ReturnedToPerson,
					AssuranceChequeItems = ((model.AssuranceChequeItems?.Any() ?? false) ? model.AssuranceChequeItems.Select((AssuranceChequeItemModel x) => new AssuranceChequeItem
					{
						ChequeNumber = x.ChequeNumber,
						DueDate = x.DueDate,
						FileName = x.FileName,
						Price = x.Price,
						SayadiId = x.SayadiId,
						IsCollected = x.IsCollected,
						IsReturned = x.IsReturned
					}).ToList() : new List<AssuranceChequeItem>())
				};
				await _assuranceService.Insert(entity);
				foreach (AssuranceChequeItemModel item in model.AssuranceChequeItems?.Where((AssuranceChequeItemModel x) => x.File != null))
				{
					manageFiles.Add(new AssuranceFile
					{
						File = item.File,
						FileName = item.FileName,
						ForAdd = true,
						IsAssuranceCheque = true
					});
				}
				await ProcessChequeFiles(manageFiles);
			});
			return Success(entity.Id, "عملیات با موفقیت انجام شد.");
		});
	}

	[HttpPost("[action]")]
	public Task<ResponseState<Guid>> Edit(AssuranceModel model)
	{
		return TryCatch(async () =>
		{
			string text = ValidateAssurance(model);
			if (text != string.Empty)
			{
				return Error(text);
			}
			Assurance entity = await _assuranceService.GetById(model.Id, "AssuranceChequeItems");
			if (entity == null)
			{
				return Error("ضمانت یافت نشد.");
			}
			AppSettings appSettings = await _appSettingsService.GetAppSettings();
			if (!entity.MobileVerified && appSettings != null)
			{
				_ = appSettings.DefaultSmsPanel;
				if (appSettings.RegisterOtpSmsPatternId.HasValue && model.AssuranceType == AssuranceType.OtherOrganization)
				{
					if (string.IsNullOrEmpty(model.MobileOtp))
					{
						return Error("کد تاییدیه موبایل وارد نشده است.");
					}
					List<AppSms> list = await _appSmsService.GetByCondition((AppSms x) => x.To == model.PersonMobile && (int)x.AppSmsType == 8 && x.Date >= DateTime.Now.AddMinutes(-2.0) && x.Tag == model.MobileOtp, false);
					if (list == null || !list.Any())
					{
						return Error("کد تاییدیه موبایل معتبر نمی باشد.");
					}
					entity.MobileVerified = true;
				}
			}
			await _assuranceService.RunInTransaction(async (DbTransaction trs) =>
			{
				entity.Description = model.Description;
				entity.AssuranceType = model.AssuranceType;
				entity.GoldWeight = model.GoldWeight;
				entity.CoinType = model.CoinType;
				entity.IsReturned = model.IsReturned;
				entity.OrganizationId = model.Organization?.Id;
				entity.PersonFullName = model.PersonFullName;
				entity.PersonNationalCode = model.PersonNationalCode;
				entity.PersonalCode = model.PersonalCode;
				entity.PersonMobile = model.PersonMobile;
				entity.ReturnDateTime = model.ReturnDateTime;
				entity.ReturnedToPerson = model.ReturnedToPerson;
				List<AssuranceFile> manageFiles = new List<AssuranceFile>();
				List<AssuranceChequeItemModel> list2 = model.AssuranceChequeItems.Where((AssuranceChequeItemModel x) => !entity.AssuranceChequeItems.Select((AssuranceChequeItem ass) => ass.Id).ToList().Contains(x.Id)).ToList();
				if (list2.Any())
				{
					foreach (AssuranceChequeItemModel item in list2)
					{
						entity.AssuranceChequeItems.Add(new AssuranceChequeItem
						{
							AssuranceId = item.AssuranceId,
							ChequeNumber = item.ChequeNumber,
							DueDate = item.DueDate,
							FileName = item.FileName,
							Price = item.Price,
							SayadiId = item.SayadiId
						});
						if (item.File != null)
						{
							manageFiles.Add(new AssuranceFile
							{
								File = item.File,
								FileName = item.FileName,
								ForAdd = true,
								IsAssuranceCheque = true
							});
						}
					}
				}
				List<AssuranceChequeItem> list3 = entity.AssuranceChequeItems.Where((AssuranceChequeItem x) => !model.AssuranceChequeItems.Select((AssuranceChequeItemModel assuranceChequeItemModel) => assuranceChequeItemModel.Id).ToList().Contains(x.Id)).ToList();
				if (list3.Any())
				{
					foreach (AssuranceChequeItem item2 in list3)
					{
						entity.AssuranceChequeItems.Remove(item2);
						if (!string.IsNullOrWhiteSpace(item2.FileName))
						{
							manageFiles.Add(new AssuranceFile
							{
								File = null,
								FileName = item2.FileName,
								ForDelete = true,
								IsAssuranceCheque = true
							});
						}
					}
				}
				model.AssuranceChequeItems.Where((AssuranceChequeItemModel x) => x.IsChanged).ToList().ForEach((AssuranceChequeItemModel modelItem) =>
				{
					AssuranceChequeItem assuranceChequeItem = entity.AssuranceChequeItems.FirstOrDefault((AssuranceChequeItem x) => x.Id == modelItem.Id);
					if (modelItem.File != null)
					{
						manageFiles.Add(new AssuranceFile
						{
							File = modelItem.File,
							FileName = modelItem.FileName,
							ForAdd = true,
							IsAssuranceCheque = true
						});
					}
					if (assuranceChequeItem != null)
					{
						if (!string.IsNullOrWhiteSpace(assuranceChequeItem.FileName) && assuranceChequeItem.FileName != modelItem.FileName)
						{
							manageFiles.Add(new AssuranceFile
							{
								File = null,
								FileName = assuranceChequeItem.FileName,
								ForDelete = true,
								IsAssuranceCheque = true
							});
						}
						assuranceChequeItem.ChequeNumber = modelItem.ChequeNumber;
						assuranceChequeItem.DueDate = modelItem.DueDate;
						assuranceChequeItem.FileName = modelItem.FileName;
						assuranceChequeItem.SayadiId = modelItem.SayadiId;
						assuranceChequeItem.Price = modelItem.Price;
						assuranceChequeItem.IsReturned = modelItem.IsReturned;
						assuranceChequeItem.IsCollected = modelItem.IsCollected;
					}
				});
				if (model.File != null)
				{
					long length = model.File.Length;
					string extension = Path.GetExtension(model.File.FileName);
					manageFiles.Add(new AssuranceFile
					{
						File = null,
						FileName = entity.FileName,
						ForDelete = true,
						IsAssurance = true
					});
					string fileName = Guid.NewGuid().ToString().Replace("-", "") + extension;
					manageFiles.Add(new AssuranceFile
					{
						File = model.File,
						FileName = fileName,
						ForAdd = true,
						IsAssurance = true
					});
					entity.FileType = extension;
					entity.FileName = fileName;
					entity.FileSize = length;
				}
				else if (!string.IsNullOrWhiteSpace(entity.FileName) && string.IsNullOrWhiteSpace(model.FileName))
				{
					manageFiles.Add(new AssuranceFile
					{
						File = null,
						FileName = entity.FileName,
						ForDelete = true,
						IsAssurance = true
					});
					entity.FileType = string.Empty;
					entity.FileName = string.Empty;
					entity.FileSize = 0L;
				}
				await _assuranceService.Update(entity);
				await ProcessChequeFiles(manageFiles);
			});
			return Success(entity.Id, "عملیات با موفقیت انجام شد.");
		});
	}

	[HttpPost("[action]")]
	public Task<ResponseState<AssuranceModel>> SubmitReturn(SubmitAssuranceReturn model)
	{
		return TryCatch(async () =>
		{
			Assurance entity = await _assuranceService.GetById(model.AssuranceId);
			if (entity == null)
			{
				return Error<AssuranceModel>(null, "ضمانت یافت نشد.");
			}
			if (model.ReturnFile != null)
			{
				if (model.ReturnFile.Length == 0L)
				{
					return Error<AssuranceModel>(null, "فایل خالی است.");
				}
				entity.ReturnFileSize = model.ReturnFile.Length;
				if (!_fileHelper.CheckFileIsImageOrPdfOrRarOrZipOrXls(model.ReturnFile))
				{
					return Error<AssuranceModel>(null, "فقط فایل تصویر و pdf و zip مجاز می باشد.");
				}
				entity.ReturnFileType = Path.GetExtension(model.ReturnFile.FileName);
				Assurance assurance = entity;
				assurance.ReturnFileName = await _fileHelper.UploadFile(model.ReturnFile, FileUploadDirectory.Assurance, _currentUserService.CustomerId.ToString());
			}
			else if (!string.IsNullOrWhiteSpace(entity.ReturnFileName) && string.IsNullOrWhiteSpace(model.ReturnFileName))
			{
				_fileHelper.DeleteFile(entity.ReturnFileName, Directories.Assurance(_currentUserService.CustomerId.ToString()));
				entity.ReturnFileType = string.Empty;
				entity.ReturnFileName = string.Empty;
				entity.ReturnFileSize = 0L;
			}
			entity.IsReturned = model.IsReturned;
			entity.ReturnDateTime = model.ReturnDateTime;
			entity.ReturnedToPerson = model.ReturnedToPerson;
			await _assuranceService.Update(entity);
			return Success(new AssuranceModel(entity), "عملیات با موفقیت انجام شد.");
		});
	}

	[HttpPost("[action]/{id}")]
	public Task<DefaultResponseState> Delete(Guid id)
	{
		return TryCatch(async () =>
		{
			Assurance entity = await _assuranceService.GetById(id, "AssuranceChequeItems");
			if (entity == null)
			{
				return Error("امکان حذف وجود ندارد. لطفا بعدا دوباره تلاش نمایید");
			}
			await _assuranceService.RunInTransaction(async (DbTransaction trs) =>
			{
				await _assuranceService.Delete(entity);
				List<AssuranceFile> list = new List<AssuranceFile>();
				if (!string.IsNullOrWhiteSpace(entity.FileName))
				{
					list.Add(new AssuranceFile
					{
						File = null,
						FileName = entity.FileName,
						ForDelete = true,
						IsAssurance = true
					});
				}
				foreach (AssuranceChequeItem item in entity.AssuranceChequeItems.Where((AssuranceChequeItem x) => !string.IsNullOrWhiteSpace(x.FileName)))
				{
					list.Add(new AssuranceFile
					{
						File = null,
						FileName = item.FileName,
						ForDelete = true,
						IsAssurance = false
					});
				}
				await ProcessChequeFiles(list);
			});
			return Success();
		});
	}

	private string ValidateAssurance(AssuranceModel model)
	{
		if (model.File != null)
		{
			if (model.File.Length == 0L)
			{
				return "فایل خالی است.";
			}
			if (!_fileHelper.CheckFileIsImage(model.File))
			{
				return "فقط فایل تصویر مجاز می باشد.";
			}
		}
		if (model.AssuranceType == AssuranceType.Cheque)
		{
			List<AssuranceChequeItemModel> assuranceChequeItems = model.AssuranceChequeItems;
			if (assuranceChequeItems == null || !assuranceChequeItems.Any())
			{
				return "هیچ چکی ثبت نشده است";
			}
		}
		if (model.AssuranceType == AssuranceType.Cheque && model.AssuranceChequeItems.Any((AssuranceChequeItemModel x) => x.Price <= 0m))
		{
			return "مبلغ چک(ها) صحیح نمی باشد";
		}
		if (model.AssuranceType == AssuranceType.Gold && model.GoldWeight < 0m)
		{
			return "وزن طلا صحیح نمی باشد";
		}
		if (model.AssuranceType == AssuranceType.Gold && model.GoldWeight == 0m && (model.CoinType == AssuranceCoinType.نامشخص || model.CoinType == AssuranceCoinType.پارسیان))
		{
			return "وزن طلا صحیح نمی باشد";
		}
		if (model.AssuranceType == AssuranceType.OtherOrganization && model.Organization == null)
		{
			return "سازمان تعیین نشده است";
		}
		if (model.AssuranceType == AssuranceType.SelfOrganization && model.Organization == null)
		{
			return "سازمان تعیین نشده است";
		}
		if ((model.AssuranceType == AssuranceType.OtherOrganization || model.AssuranceType == AssuranceType.Cheque) && string.IsNullOrEmpty(model.PersonFullName))
		{
			return "نام شخص طرف حساب وارد نشده است";
		}
		List<AssuranceChequeItemModel> assuranceChequeItems2 = model.AssuranceChequeItems;
		if (assuranceChequeItems2 != null && assuranceChequeItems2.Any())
		{
			foreach (AssuranceChequeItemModel assuranceChequeItem in model.AssuranceChequeItems)
			{
				if (assuranceChequeItem.File != null)
				{
					if (assuranceChequeItem.File.Length == 0L)
					{
						return "فایل خالی است.";
					}
					if (!_fileHelper.CheckFileIsImage(assuranceChequeItem.File))
					{
						return "فقط فایل تصویر مجاز می باشد.";
					}
					assuranceChequeItem.FileName = Guid.NewGuid().ToString().Replace("-", "") + Path.GetExtension(assuranceChequeItem.File.FileName);
				}
			}
		}
		return string.Empty;
	}

	private async Task ProcessChequeFiles(List<AssuranceFile> manageFiles)
	{
		foreach (AssuranceFile item in manageFiles.Where((AssuranceFile x) => x.File != null && x.ForAdd))
		{
			await _fileHelper.UploadFile(item.File, item.IsAssurance ? FileUploadDirectory.Assurance : FileUploadDirectory.AssuranceCheque, _currentUserService.CustomerId.ToString(), item.FileName);
		}
		foreach (AssuranceFile item2 in manageFiles.Where((AssuranceFile x) => !string.IsNullOrWhiteSpace(x.FileName) && x.ForDelete))
		{
			_fileHelper.DeleteFile(item2.FileName, item2.IsAssurance ? Directories.Assurance(_currentUserService.CustomerId.ToString()) : Directories.AssuranceCheque(_currentUserService.CustomerId.ToString()));
		}
	}
}
