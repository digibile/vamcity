using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Services.Enums;
using LoanSystemAvizhe.Services.FileHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;

namespace LoanSystem.Site.Areas.Agent.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Agent,Admin")]
public class SubAgentTransactionController : BaseAgentController
{
	private readonly ICurrentUserService _currentUserService;

	private readonly ISubAgentTransactionService _subAgentTransactionService;

	private readonly IFileHelper _fileHelper;

	public SubAgentTransactionController(ILogger<UserLoanController> logger, ICurrentUserService currentUserService, ISubAgentTransactionService subAgentTransactionService, IFileHelper fileHelper)
		: base(logger)
	{
		_currentUserService = currentUserService;
		_subAgentTransactionService = subAgentTransactionService;
		_fileHelper = fileHelper;
	}

	[HttpGet("[action]/{agentId?}/{userLoanId?}")]
	public Task<ResponseState<List<SubAgentTransactionModel>>> List(long? agentId, Guid? userLoanId, DateTime? start, DateTime? end)
	{
		return TryCatch(async () =>
		{
			if (!agentId.HasValue)
			{
				agentId = _currentUserService.UserId;
			}
			List<SubAgentTransaction> source = await _subAgentTransactionService.GetByCondition((SubAgentTransaction x) => (((DateTime?)start).HasValue ? (x.SaveDate.Date >= ((DateTime?)start).Value.Date) : true) && (((DateTime?)end).HasValue ? (x.SaveDate.Date <= ((DateTime?)end).Value.Date) : true) && x.UserLoan.SubAgentId == agentId && x.IsWithdraw, "UserLoan.Loan", "UserLoan.User");
			if (userLoanId.HasValue)
			{
				source = source.Where((SubAgentTransaction x) =>
				{
					Guid? userLoanId2 = x.UserLoanId;
					Guid? guid = userLoanId;
					if (userLoanId2.HasValue != guid.HasValue)
					{
						return false;
					}
					return !userLoanId2.HasValue || userLoanId2.GetValueOrDefault() == guid.GetValueOrDefault();
				}).ToList();
			}
			List<SubAgentTransactionModel> data = (from x in source
				select new SubAgentTransactionModel(x) into x
				orderby x.SaveDate descending
				select x).ToList();
			return Success(data);
		});
	}

	[HttpGet("[action]/{id}")]
	public Task<ResponseState<SubAgentTransactionModel>> Get(Guid id)
	{
		return TryCatch(async () =>
		{
			if (id == Guid.Empty)
			{
				return Success(new SubAgentTransactionModel
				{
					SaveDate = DateTime.Now
				});
			}
			SubAgentTransaction subAgentTransaction = await _subAgentTransactionService.GetById(id, "UserLoan");
			if (subAgentTransaction == null)
			{
				return Error<SubAgentTransactionModel>(null, "اطلاعات مورد نظر یافت نشد");
			}
			SubAgentTransactionModel subAgentTransactionModel = new SubAgentTransactionModel(subAgentTransaction);
			subAgentTransactionModel.FillImageBase64Prop(_fileHelper, subAgentTransaction.CustomerId);
			return Success(subAgentTransactionModel);
		});
	}

	[HttpPost("[action]")]
	public Task<ResponseState<SubAgentTransactionModel>> Create(SubAgentTransactionModel model)
	{
		foreach (KeyValuePair<string, ModelStateEntry> item in ModelState)
		{
			string key = item.Key;
			if (key.Contains("UserLoanContact.Contact"))
			{
				ModelState[key].Errors.Clear();
				ModelState[key].ValidationState = ModelValidationState.Valid;
			}
		}
		return TryCatch(async () =>
		{
			if (model.UserLoan == null)
			{
				return Error<SubAgentTransactionModel>(null, "در حالت ثبت واریزی برای نماینده باید وام مرتبط نیز انتخاب شده باشد.");
			}
			string fileName = "";
			if (model.File != null)
			{
				if (model.File.Length == 0L)
				{
					return Error<SubAgentTransactionModel>(null, "فایل خالی است.");
				}
				_ = model.File.Length;
				if (!_fileHelper.CheckFileIsImage(model.File))
				{
					return Error<SubAgentTransactionModel>(null, "فقط فایل تصویر مجاز می باشد.");
				}
				Path.GetExtension(model.File.FileName);
				fileName = await _fileHelper.UploadFile(model.File, FileUploadDirectory.SubAgentTransaction, _currentUserService.CustomerId.ToString());
			}
			SubAgentTransaction entity = new SubAgentTransaction
			{
				Id = Guid.Empty,
				Amount = model.Amount,
				SaveDate = new DateTime(model.SaveDate.Year, model.SaveDate.Month, model.SaveDate.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second),
				SavedBy = HttpContext.User.Identity.Name,
				Description = ((!string.IsNullOrEmpty(model.Description)) ? (model.Description + "</br>") : ""),
				IsWithdraw = model.IsWithdraw,
				PaymentMethod = model.PaymentMethod,
				UserLoanId = model.UserLoan.Id,
				FileName = fileName
			};
			if (model.UserLoan?.SubAgentModel != null)
			{
				entity.Description += $"واریز به حساب نماینده [{model.UserLoan.SubAgentModel.BusinessName}] بابت وام شماره [{model.UserLoan.Code}]";
			}
			await _subAgentTransactionService.Insert(entity);
			model.Id = entity.Id;
			return Success(model, "عملیات با موفقیت انجام شد.");
		});
	}

	[HttpPost("[action]")]
	public Task<ResponseState<SubAgentTransactionModel>> Edit(SubAgentTransactionModel model)
	{
		return TryCatch(async () =>
		{
			SubAgentTransaction entity = await _subAgentTransactionService.GetById(model.Id);
			if (model.UserLoan == null)
			{
				return Error<SubAgentTransactionModel>(null, "در حالت ثبت واریزی برای نماینده باید وام مرتبط نیز انتخاب شده باشد.");
			}
			if (model.File != null)
			{
				if (model.File.Length == 0L)
				{
					return Error<SubAgentTransactionModel>(null, "فایل خالی است.");
				}
				_ = model.File.Length;
				if (!_fileHelper.CheckFileIsImage(model.File))
				{
					return Error<SubAgentTransactionModel>(null, "فقط فایل تصویر مجاز می باشد.");
				}
				Path.GetExtension(model.File.FileName);
				_fileHelper.DeleteFile(entity.FileName, Directories.Transaction(_currentUserService.CustomerId.ToString()));
				entity.FileName = await _fileHelper.UploadFile(model.File, FileUploadDirectory.Transaction, _currentUserService.CustomerId.ToString());
			}
			else if (!string.IsNullOrEmpty(entity.FileName) && string.IsNullOrEmpty(model.FileName))
			{
				_fileHelper.DeleteFile(entity.FileName, Directories.Transaction(_currentUserService.CustomerId.ToString()));
			}
			entity.Amount = model.Amount;
			entity.Description = ((!string.IsNullOrEmpty(model.Description)) ? (model.Description + "</br>") : "");
			entity.SaveDate = new DateTime(model.SaveDate.Year, model.SaveDate.Month, model.SaveDate.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
			entity.IsWithdraw = model.IsWithdraw;
			entity.PaymentMethod = model.PaymentMethod;
			entity.UserLoanId = model.UserLoan?.Id;
			if (model.UserLoan?.SubAgentModel != null)
			{
				entity.Description = entity.Description.Replace($"واریز به حساب نماینده [{model.UserLoan.SubAgentModel.BusinessName}] بابت وام شماره [{model.UserLoan.Code}]</br>", "");
				entity.Description += $"واریز به حساب نماینده [{model.UserLoan.SubAgentModel.BusinessName}] بابت وام شماره [{model.UserLoan.Code}]";
			}
			await _subAgentTransactionService.Update(entity);
			return Success(model, "عملیات با موفقیت انجام شد.");
		});
	}

	[HttpPost("[action]/{id}")]
	public Task<DefaultResponseState> Delete(Guid id)
	{
		return TryCatch(async () =>
		{
			SubAgentTransaction subAgentTransaction = await _subAgentTransactionService.GetById(id);
			if (subAgentTransaction == null)
			{
				return Error("امکان حذف وجود ندارد. لطفا بعدا دوباره تلاش نمایید");
			}
			_fileHelper.DeleteFile(subAgentTransaction.FileName, Directories.SubAgentTransaction(_currentUserService.CustomerId.ToString()));
			await _subAgentTransactionService.Delete(subAgentTransaction);
			return Success();
		});
	}
}
