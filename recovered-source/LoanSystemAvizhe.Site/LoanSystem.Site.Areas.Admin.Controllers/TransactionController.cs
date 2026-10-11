using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LoanSystem.Core.Configuration;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Services.Models.OnlineMoneyTransferPanel;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Data.Enums;
using LoanSystemAvizhe.Services.Enums;
using LoanSystemAvizhe.Services.FileHelper;
using LoanSystemAvizhe.Services.Inquiry.Models;
using LoanSystemAvizhe.Services.Models.Shahin;
using LoanSystemAvizhe.Services.Sms.Models.MelliPayamak;
using LoanSystemAvizhe.Services.ThirdParty;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LoanSystem.Site.Areas.Admin.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Admin")]
public class TransactionController : BaseAdminController
{
	private readonly ITransactionService _transactionService;

	private readonly IOnlineMoneyTransferService _onlineMoneyTransferService;

	private readonly IUserPaymentService _userPaymentService;

	private readonly IUserLoanService _userLoanService;

	private readonly IFileHelper _fileHelper;

	private readonly IUserLoanRequestService _userLoanRequestService;

	private readonly ILoanRequestStatusService _loanRequestStatusService;

	private readonly IAgentFactorService _agentFactorService;

	private readonly IWithdrawAgentFactorService _withdrawAgentFactorService;

	private readonly IAppSmsService _appSmsService;

	private readonly ICurrentUserService _currentUserService;

	private readonly IntegratedSmsSenderService _integratedSmsSenderService;

	private readonly AppConfig _appConfig;

	private readonly OnlineMoneyTransferPanelService _onlineMoneyTransferPanelService;

	private readonly ShahinService _shahinService;

	private readonly IntegratedInquiryService _integratedInquiryService;

	private readonly IWebHostEnvironment _webHostEnvironment;

	public TransactionController(ILogger<TransactionController> logger, ITransactionService transactionService, IUserPaymentService userPaymentService, IUserLoanService userLoanService, IUserLoanRequestService userLoanRequestService, IFileHelper fileHelper, ILoanRequestStatusService loanRequestStatusService, ICurrentUserService currentUserService, IOnlineMoneyTransferService onlineMoneyTransferService, IAgentFactorService agentFactorService, IWithdrawAgentFactorService withdrawAgentFactorService, IAppSmsService appSmsService, IntegratedSmsSenderService integratedSmsSenderService, IOptions<AppConfig> appConfig, ShahinService shahinService, OnlineMoneyTransferPanelService onlineMoneyTransferPanelService, IntegratedInquiryService integratedInquiryService, IWebHostEnvironment webHostEnvironment)
		: base(logger)
	{
		_transactionService = transactionService;
		_userPaymentService = userPaymentService;
		_userLoanRequestService = userLoanRequestService;
		_userLoanService = userLoanService;
		_agentFactorService = agentFactorService;
		_withdrawAgentFactorService = withdrawAgentFactorService;
		_appSmsService = appSmsService;
		_fileHelper = fileHelper;
		_loanRequestStatusService = loanRequestStatusService;
		_currentUserService = currentUserService;
		_onlineMoneyTransferService = onlineMoneyTransferService;
		_integratedSmsSenderService = integratedSmsSenderService;
		_appConfig = appConfig.Value;
		_onlineMoneyTransferPanelService = onlineMoneyTransferPanelService;
		_shahinService = shahinService;
		_integratedInquiryService = integratedInquiryService;
		_webHostEnvironment = webHostEnvironment;
		_shahinService.SetWebHostRoot(_webHostEnvironment.ContentRootPath);
		_integratedInquiryService.SetWebHostRoot(_webHostEnvironment.ContentRootPath);
	}

	[HttpGet("[action]")]
	public Task<ResponseState<List<TransactionModel>>> List()
	{
		return TryCatch(async () =>
		{
			List<TransactionModel> data = (await _transactionService.GetAllWithdraws(null)).Select((Transaction x) => new TransactionModel(x)).ToList();
			return Success(data);
		});
	}

	[HttpGet("[action]/{id}")]
	public Task<ResponseState<TransactionModel>> Get(Guid id)
	{
		return TryCatch(async () =>
		{
			if (id == Guid.Empty)
			{
				return Success(new TransactionModel
				{
					SaveDate = DateTime.Now
				});
			}
			Transaction entity = await _transactionService.GetById(id, "UserLoan", "Agent", "Contact", "UserLoanContact", "OnlineMoneyTransfers", "CustomerMarketerCommission.Marketer");
			if (entity == null)
			{
				return Error<TransactionModel>(null, "اطلاعات مورد نظر یافت نشد");
			}
			TransactionModel transactionModel = new TransactionModel(entity);
			if (entity.SuccessfullOnlineMoneyTransferId.HasValue)
			{
				OnlineMoneyTransfer onlineMoneyTransfer = entity.OnlineMoneyTransfers?.FirstOrDefault((OnlineMoneyTransfer x) =>
				{
					Guid id2 = x.Id;
					Guid? successfullOnlineMoneyTransferId = entity.SuccessfullOnlineMoneyTransferId;
					return id2 == successfullOnlineMoneyTransferId;
				});
				if (onlineMoneyTransfer != null)
				{
					transactionModel.OnlineMoneyTransfer = new OnlineMoneyTransferModel(onlineMoneyTransfer);
				}
			}
			transactionModel.FillImageBase64Prop(_fileHelper, entity.CustomerId);
			return Success(transactionModel);
		});
	}

	[HttpPost("[action]")]
	public Task<ResponseState<TransactionModel>> Create(TransactionModel model)
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
			if (model.Agent != null && model.UserLoan == null)
			{
				return Error<TransactionModel>(null, "در حالت ثبت واریزی برای نماینده باید وام مرتبط نیز انتخاب شده باشد.");
			}
			if (model.Agent == null && model.UserLoan != null)
			{
				return Error<TransactionModel>(null, "نماینده انتخاب نشده است.");
			}
			if (!model.IsWithdraw && model.SponsoredLoan != null)
			{
				return Error<TransactionModel>(null, "ثبت واریز برای وام های اسپانسری قابل انجام نمی باشد. فقط برداشت مجاز می باشد.");
			}
			if (model.OnlineMoneyTransfer != null)
			{
				if (!model.IsWithdraw || model.PaymentMethod != PaymentMethods.Bank)
				{
					return Error<TransactionModel>(null, "انتقال وجه آنلاین فقط برای برداشت های بانکی امکان پذیر می باشد.");
				}
				if (model.OnlineMoneyTransfer.Provider == OnlineMoneyTransferProviders.EFarda && !model.OnlineMoneyTransfer.OnlineMoneyTransferPanelId.HasValue)
				{
					return Error<TransactionModel>(null, "جهت ثبت انتقال وجه آنلاین ابتدا رمز پیامکی دریافت کنید.");
				}
				if (model.OnlineMoneyTransfer.Provider == OnlineMoneyTransferProviders.Shahin)
				{
					if (string.IsNullOrWhiteSpace(model.OnlineMoneyTransfer.OTP))
					{
						return Error<TransactionModel>(null, "رمز تاییدیه انتقال وجه وارد نشده است.");
					}
					List<AppSms> list = await _appSmsService.GetByCondition((AppSms x) => (int)x.AppSmsType == 21 && x.Tag == model.OnlineMoneyTransfer.OTP && x.To == _appConfig.AdminPhone && x.Date >= DateTime.Now.AddMinutes(-2.0), false);
					if (list == null || !list.Any())
					{
						return Error<TransactionModel>(null, "رمز تاییدیه انتقال وجه صحیح نمی باشد.");
					}
				}
			}
			string fileName = "";
			if (model.File != null)
			{
				if (model.File.Length == 0L)
				{
					return Error<TransactionModel>(null, "فایل اول خالی است.");
				}
				_ = model.File.Length;
				if (!_fileHelper.CheckFileIsImage(model.File))
				{
					return Error<TransactionModel>(null, "فقط فایل تصویر مجاز می باشد. (فایل اول)");
				}
				fileName = await _fileHelper.UploadFile(model.File, FileUploadDirectory.Transaction, _currentUserService.CustomerId.ToString());
			}
			string fileName2 = "";
			if (model.File2 != null)
			{
				if (model.File2.Length == 0L)
				{
					return Error<TransactionModel>(null, "فایل دوم خالی است.");
				}
				_ = model.File2.Length;
				if (!_fileHelper.CheckFileIsImage(model.File2))
				{
					return Error<TransactionModel>(null, "فقط فایل تصویر مجاز می باشد. (فایل دوم)");
				}
				fileName2 = await _fileHelper.UploadFile(model.File2, FileUploadDirectory.Transaction, _currentUserService.CustomerId.ToString());
			}
			string fileName3 = "";
			if (model.File3 != null)
			{
				if (model.File3.Length == 0L)
				{
					return Error<TransactionModel>(null, "فایل سوم خالی است.");
				}
				_ = model.File3.Length;
				if (!_fileHelper.CheckFileIsImage(model.File3))
				{
					return Error<TransactionModel>(null, "فقط فایل تصویر مجاز می باشد. (فایل سوم)");
				}
				fileName3 = await _fileHelper.UploadFile(model.File3, FileUploadDirectory.Transaction, _currentUserService.CustomerId.ToString());
			}
			Transaction entity = new Transaction
			{
				Id = Guid.Empty,
				Amount = model.Amount,
				SaveDate = new DateTime(model.SaveDate.Year, model.SaveDate.Month, model.SaveDate.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second),
				SavedBy = HttpContext.User.Identity.Name,
				Description = ((!string.IsNullOrEmpty(model.Description)) ? (model.Description + "</br>") : ""),
				GoldRate = model.GoldRate,
				GoldWeight = model.GoldWeight,
				IsUnknown = model.IsUnknown,
				IsWithdraw = model.IsWithdraw,
				PaymentMethod = model.PaymentMethod,
				SponsoredLoanId = model.SponsoredLoan?.Id,
				AgentId = model.Agent?.Id,
				UserLoanId = model.UserLoan?.Id,
				ContactId = model.Contact?.Id,
				UserLoanContactId = model.UserLoanContact?.Id,
				CustomerMarketerCommissionId = model.CustomerMarketerCommission?.Id,
				FileName = fileName,
				FileName2 = fileName2,
				FileName3 = fileName3
			};
			if (model.Agent != null && model.UserLoan != null)
			{
				entity.Description += $"واریز به حساب نماینده [{model.Agent.BusinessName}] بابت وام شماره [{model.UserLoan.Code}]";
			}
			if (model.Contact != null && model.UserLoanContact != null)
			{
				entity.Description += $"واریز به حساب [{model.Contact.BusinessName}] بابت وام شماره [{model.UserLoanContact.UserLoanCode}]";
			}
			if (model.CustomerMarketer != null && model.CustomerMarketerCommission != null)
			{
				entity.Description += $"واریز به حساب بازاریاب [{model.CustomerMarketer.FirstName} {model.CustomerMarketer.BusinessName}] بابت وام شماره [{model.CustomerMarketerCommission.UserLoan.Code}]";
			}
			await _transactionService.Insert(entity);
			model.Id = entity.Id;
			if (model.OnlineMoneyTransfer != null)
			{
				if (model.OnlineMoneyTransfer?.Shahin != null && !model.OnlineMoneyTransfer.Shahin.destinationAccountNumber.ToLower().StartsWith("ir") && model.OnlineMoneyTransfer.Shahin.transferType != "LOCAL")
				{
					model.OnlineMoneyTransfer.Shahin.destinationAccountNumber = "IR" + model.OnlineMoneyTransfer.Shahin.destinationAccountNumber;
				}
				OnlineMoneyTransfer onlineMoneyTransfer = new OnlineMoneyTransfer
				{
					Provider = model.OnlineMoneyTransfer.Provider,
					OTP = model.OnlineMoneyTransfer.OTP,
					SaveDate = DateTime.Now,
					Amount = model.Amount,
					OnlineMoneyTransferStep = OnlineMoneyTransferSteps.RequestToBank,
					From = ((model.OnlineMoneyTransfer.Provider == OnlineMoneyTransferProviders.EFarda) ? model.OnlineMoneyTransfer.From : model.OnlineMoneyTransfer.Shahin.sourceAccount),
					To = ((model.OnlineMoneyTransfer.Provider == OnlineMoneyTransferProviders.EFarda) ? ((model.OnlineMoneyTransfer.To.ToLower().StartsWith("ir") ? "" : "IR") + model.OnlineMoneyTransfer.To) : model.OnlineMoneyTransfer.Shahin.destinationAccountNumber),
					ToName = ((model.OnlineMoneyTransfer.Provider == OnlineMoneyTransferProviders.EFarda) ? model.OnlineMoneyTransfer.ToName : model.OnlineMoneyTransfer.Shahin.destinationAccountName),
					OurTraceNumber = Guid.NewGuid().ToString(),
					TransactionId = entity.Id,
					OnlineMoneyTransferPanelId = model.OnlineMoneyTransfer.OnlineMoneyTransferPanelId,
					ShahinJson = model.OnlineMoneyTransfer.Shahin?.ToString()
				};
				await _onlineMoneyTransferService.Insert(onlineMoneyTransfer);
				if (model.OnlineMoneyTransfer.Provider == OnlineMoneyTransferProviders.Shahin)
				{
					model.OnlineMoneyTransfer.Shahin.amount = onlineMoneyTransfer.Amount * 10m;
					model.OnlineMoneyTransfer.Shahin.documentID = "";
					model.OnlineMoneyTransfer.Shahin.paymentID = "";
					model.OnlineMoneyTransfer.Shahin.transferID = "";
					model.OnlineMoneyTransfer.Shahin.depositDescription = "";
					model.OnlineMoneyTransfer.Shahin.withdrawDescription = "";
					ShahinTransferValidationResult validationResult = await _shahinService.ShahinTransferValidattion(model.OnlineMoneyTransfer.Shahin);
					if (validationResult?.respObject?.transferState != "ALLOWED")
					{
						await _transactionService.Delete(entity);
						return Error(model, validationResult.respObject?.reasonText);
					}
					ShahinTransferResult result = await _shahinService.Transfer(model.OnlineMoneyTransfer.Shahin);
					onlineMoneyTransfer.ShahinResultJson = result.ToString();
					if (!result.success)
					{
						onlineMoneyTransfer.IsSuccess = false;
						model.OnlineMoneyTransfer = new OnlineMoneyTransferModel(onlineMoneyTransfer);
						await _transactionService.Delete(entity);
						await _onlineMoneyTransferService.Update(onlineMoneyTransfer);
						return Error(model, (!string.IsNullOrEmpty(result.message)) ? ("تراکنش ناموفق: " + result.message) : "تراکنش انتقال وجه ناموفق");
					}
					onlineMoneyTransfer.IsSuccess = true;
					model.OnlineMoneyTransfer = new OnlineMoneyTransferModel(onlineMoneyTransfer);
					entity.SuccessfullOnlineMoneyTransferId = onlineMoneyTransfer.Id;
					await _transactionService.Update(entity);
					await _onlineMoneyTransferService.Update(onlineMoneyTransfer);
				}
				else if (model.OnlineMoneyTransfer.Provider == OnlineMoneyTransferProviders.EFarda)
				{
					OnlineMoneyTransferPanelCompleteTransferResult result2 = await _onlineMoneyTransferPanelService.CompleteTransfer(new OnlineMoneyTransferPanelCompleteTransfer
					{
						Id = model.OnlineMoneyTransfer.OnlineMoneyTransferPanelId.Value,
						Amount = model.Amount,
						ClientTraceNumber = onlineMoneyTransfer.OurTraceNumber,
						From = model.OnlineMoneyTransfer.From,
						OTP = model.OnlineMoneyTransfer.OTP,
						To = onlineMoneyTransfer.To
					});
					onlineMoneyTransfer.EFardaJson = result2.ToEFardaJson();
					if (!result2.success)
					{
						onlineMoneyTransfer.IsSuccess = false;
						model.OnlineMoneyTransfer = new OnlineMoneyTransferModel(onlineMoneyTransfer);
						await _transactionService.Delete(entity);
						await _onlineMoneyTransferService.Update(onlineMoneyTransfer);
						return Error(model, (!string.IsNullOrEmpty(result2.message)) ? ("تراکنش ناموفق: " + result2.message) : "تراکنش انتقال وجه ناموفق");
					}
					onlineMoneyTransfer.IsSuccess = true;
					model.OnlineMoneyTransfer = new OnlineMoneyTransferModel(onlineMoneyTransfer);
					entity.SuccessfullOnlineMoneyTransferId = onlineMoneyTransfer.Id;
					await _transactionService.Update(entity);
					await _onlineMoneyTransferService.Update(onlineMoneyTransfer);
				}
			}
			UserLoanModel userLoan = model.UserLoan;
			int num;
			if (userLoan == null)
			{
				num = 0;
			}
			else
			{
				_ = userLoan.Id;
				num = 1;
			}
			if (num != 0 && model.UserLoanRequest == null)
			{
				UserLoan userLoan2 = await _userLoanService.GetById(model.UserLoan.Id);
				if (userLoan2?.UserLoanRequestId.HasValue ?? false)
				{
					decimal num2 = (await _transactionService.GetByCondition((Transaction x) => x.UserLoanId == model.UserLoan.Id)).Sum((Transaction x) => x.Amount);
					if (userLoan2.AgentPrice <= num2)
					{
						UserLoanRequest request = await _userLoanRequestService.GetById(userLoan2.UserLoanRequestId.Value);
						request.AgentStatusId = (await _loanRequestStatusService.GetAllLoanRequestStatuses()).FirstOrDefault((LoanRequestStatus x) => x.SystemAgentRequestStatus == SystemAgentRequestStatus.تسویه_شده)?.Id;
						await _userLoanRequestService.Update(request);
					}
				}
			}
			else
			{
				LoanSystem.Services.Models.BaseModel userLoanRequest = model.UserLoanRequest;
				int num3;
				if (userLoanRequest == null)
				{
					num3 = 0;
				}
				else
				{
					_ = userLoanRequest.Id;
					num3 = 1;
				}
				if (num3 != 0)
				{
					bool update = false;
					UserLoanRequest userLoanRequest2 = await _userLoanRequestService.GetById(model.UserLoanRequest.Id);
					if (userLoanRequest2 != null && model.CustomerStatus != null && userLoanRequest2.StatusId != model.CustomerStatus.Id)
					{
						userLoanRequest2.StatusId = model.CustomerStatus.Id;
						update = true;
					}
					if (userLoanRequest2 != null && model.AgentStatus != null && userLoanRequest2.AgentStatusId != model.AgentStatus.Id)
					{
						userLoanRequest2.AgentStatusId = model.AgentStatus.Id;
						update = true;
					}
					if (update)
					{
						await _userLoanRequestService.Update(userLoanRequest2);
					}
				}
			}
			return Success(model, "عملیات با موفقیت انجام شد.");
		});
	}

	[HttpPost("[action]")]
	public Task<ResponseState<TransactionModel>> Edit(TransactionModel model)
	{
		return TryCatch(async () =>
		{
			Transaction entity = await _transactionService.GetById(model.Id);
			if (model.Agent != null && model.UserLoan == null)
			{
				return Error<TransactionModel>(null, "در حالت ثبت واریزی برای نماینده باید وام مرتبط نیز انتخاب شده باشد.");
			}
			if (model.Agent == null && model.UserLoan != null)
			{
				return Error<TransactionModel>(null, "نماینده انتخاب نشده است.");
			}
			if (!model.IsWithdraw && model.SponsoredLoan != null)
			{
				return Error<TransactionModel>(null, "ثبت واریز برای وام های اسپانسری قابل انجام نمی باشد. فقط برداشت مجاز می باشد.");
			}
			if (entity.SuccessfullOnlineMoneyTransferId.HasValue && entity.Amount != model.Amount)
			{
				return Error<TransactionModel>(null, "به دلیل وجود انتقال وجه آنلاین برای این تراکنش، امکان ویرایش مبلغ وجود ندارد.");
			}
			if (model.OnlineMoneyTransfer != null && !model.OnlineMoneyTransfer.IsSuccess && model.OnlineMoneyTransfer.EFardaResult == null)
			{
				if (!model.IsWithdraw || model.PaymentMethod != PaymentMethods.Bank)
				{
					return Error<TransactionModel>(null, "انتقال وجه آنلاین فقط برای برداشت های بانکی امکان پذیر می باشد.");
				}
				if (!model.OnlineMoneyTransfer.OnlineMoneyTransferPanelId.HasValue)
				{
					return Error<TransactionModel>(null, "جهت ثبت انتقال وجه آنلاین ابتدا رمز پیامکی دریافت کنید.");
				}
				OnlineMoneyTransfer onlineMoneyTransfer = new OnlineMoneyTransfer
				{
					OTP = model.OnlineMoneyTransfer.OTP,
					SaveDate = DateTime.Now,
					Amount = model.Amount,
					OnlineMoneyTransferStep = OnlineMoneyTransferSteps.RequestToBank,
					From = model.OnlineMoneyTransfer.From,
					To = (model.OnlineMoneyTransfer.To.ToLower().StartsWith("ir") ? "" : "IR") + model.OnlineMoneyTransfer.To,
					ToName = model.OnlineMoneyTransfer.ToName,
					OurTraceNumber = Guid.NewGuid().ToString(),
					TransactionId = entity.Id,
					OnlineMoneyTransferPanelId = model.OnlineMoneyTransfer.OnlineMoneyTransferPanelId.Value
				};
				await _onlineMoneyTransferService.Insert(onlineMoneyTransfer);
				OnlineMoneyTransferPanelCompleteTransferResult result = await _onlineMoneyTransferPanelService.CompleteTransfer(new OnlineMoneyTransferPanelCompleteTransfer
				{
					Id = model.OnlineMoneyTransfer.OnlineMoneyTransferPanelId.Value,
					Amount = model.Amount,
					ClientTraceNumber = onlineMoneyTransfer.OurTraceNumber,
					From = model.OnlineMoneyTransfer.From,
					OTP = model.OnlineMoneyTransfer.OTP,
					To = onlineMoneyTransfer.To
				});
				onlineMoneyTransfer.EFardaJson = result.ToEFardaJson();
				if (!result.success)
				{
					onlineMoneyTransfer.IsSuccess = false;
					model.OnlineMoneyTransfer = new OnlineMoneyTransferModel(onlineMoneyTransfer);
					await _onlineMoneyTransferService.Update(onlineMoneyTransfer);
					return Error(model, (!string.IsNullOrEmpty(result.message)) ? ("تراکنش ناموفق: " + result.message) : "تراکنش انتقال وجه ناموفق");
				}
				onlineMoneyTransfer.IsSuccess = true;
				model.OnlineMoneyTransfer = new OnlineMoneyTransferModel(onlineMoneyTransfer);
				entity.SuccessfullOnlineMoneyTransferId = onlineMoneyTransfer.Id;
				await _transactionService.Update(entity);
				await _onlineMoneyTransferService.Update(onlineMoneyTransfer);
			}
			if (model.File != null)
			{
				if (model.File.Length == 0L)
				{
					return Error<TransactionModel>(null, "فایل خالی است.");
				}
				_ = model.File.Length;
				if (!_fileHelper.CheckFileIsImage(model.File))
				{
					return Error<TransactionModel>(null, "فقط فایل تصویر مجاز می باشد.");
				}
				_fileHelper.DeleteFile(entity.FileName, Directories.Transaction(_currentUserService.CustomerId.ToString()));
				entity.FileName = await _fileHelper.UploadFile(model.File, FileUploadDirectory.Transaction, _currentUserService.CustomerId.ToString());
			}
			else if (!string.IsNullOrEmpty(entity.FileName) && string.IsNullOrEmpty(model.FileName))
			{
				_fileHelper.DeleteFile(entity.FileName, Directories.Transaction(_currentUserService.CustomerId.ToString()));
			}
			if (model.File2 != null)
			{
				if (model.File2.Length == 0L)
				{
					return Error<TransactionModel>(null, "فایل خالی است.");
				}
				_ = model.File2.Length;
				if (!_fileHelper.CheckFileIsImage(model.File2))
				{
					return Error<TransactionModel>(null, "فقط فایل تصویر مجاز می باشد.");
				}
				_fileHelper.DeleteFile(entity.FileName2, Directories.Transaction(_currentUserService.CustomerId.ToString()));
				entity.FileName2 = await _fileHelper.UploadFile(model.File2, FileUploadDirectory.Transaction, _currentUserService.CustomerId.ToString());
			}
			else if (!string.IsNullOrEmpty(entity.FileName2) && string.IsNullOrEmpty(model.FileName2))
			{
				_fileHelper.DeleteFile(entity.FileName2, Directories.Transaction(_currentUserService.CustomerId.ToString()));
			}
			if (model.File3 != null)
			{
				if (model.File3.Length == 0L)
				{
					return Error<TransactionModel>(null, "فایل خالی است.");
				}
				_ = model.File3.Length;
				if (!_fileHelper.CheckFileIsImage(model.File3))
				{
					return Error<TransactionModel>(null, "فقط فایل تصویر مجاز می باشد.");
				}
				_fileHelper.DeleteFile(entity.FileName3, Directories.Transaction(_currentUserService.CustomerId.ToString()));
				entity.FileName3 = await _fileHelper.UploadFile(model.File3, FileUploadDirectory.Transaction, _currentUserService.CustomerId.ToString());
			}
			else if (!string.IsNullOrEmpty(entity.FileName3) && string.IsNullOrEmpty(model.FileName3))
			{
				_fileHelper.DeleteFile(entity.FileName3, Directories.Transaction(_currentUserService.CustomerId.ToString()));
			}
			entity.Amount = model.Amount;
			entity.Description = ((!string.IsNullOrEmpty(model.Description)) ? (model.Description + "</br>") : "");
			entity.SaveDate = new DateTime(model.SaveDate.Year, model.SaveDate.Month, model.SaveDate.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
			entity.GoldRate = model.GoldRate;
			entity.GoldWeight = model.GoldWeight;
			entity.IsWithdraw = model.IsWithdraw;
			entity.IsUnknown = model.IsUnknown;
			entity.PaymentMethod = model.PaymentMethod;
			entity.SponsoredLoanId = model.SponsoredLoan?.Id;
			entity.AgentId = model.Agent?.Id;
			entity.UserLoanId = model.UserLoan?.Id;
			entity.UserLoanContactId = model.UserLoanContact?.Id;
			entity.CustomerMarketerCommissionId = model.CustomerMarketerCommission?.Id;
			if (model.Agent != null && model.UserLoan != null)
			{
				entity.Description = entity.Description.Replace($"واریز به حساب نماینده [{model.Agent.BusinessName}] بابت وام شماره [{model.UserLoan.Code}]</br>", "");
				entity.Description += $"واریز به حساب نماینده [{model.Agent.BusinessName}] بابت وام شماره [{model.UserLoan.Code}]";
			}
			if (model.Contact != null && model.UserLoanContact != null)
			{
				entity.Description = entity.Description.Replace($"واریز به حساب [{model.Contact.BusinessName}] بابت وام شماره [{model.UserLoanContact.UserLoanCode}]</br>", "");
				entity.Description += $"واریز به حساب [{model.Contact.BusinessName}] بابت وام شماره [{model.UserLoanContact.UserLoanCode}]";
			}
			if (model.CustomerMarketer != null && model.CustomerMarketerCommission != null)
			{
				entity.Description = entity.Description.Replace($"واریز به حساب [{model.CustomerMarketer.FirstName} {model.CustomerMarketer.BusinessName}] بابت وام شماره [{model.CustomerMarketerCommission.UserLoan.Code}]</br>", "");
				entity.Description += $"واریز به حساب بازاریاب [{model.CustomerMarketer.FirstName} {model.CustomerMarketer.BusinessName}] بابت وام شماره [{model.CustomerMarketerCommission.UserLoan.Code}]";
			}
			await _transactionService.Update(entity);
			UserLoanModel userLoan = model.UserLoan;
			int num;
			if (userLoan == null)
			{
				num = 0;
			}
			else
			{
				_ = userLoan.Id;
				num = 1;
			}
			if (num != 0 && model.UserLoanRequest == null)
			{
				UserLoan userLoan2 = await _userLoanService.GetById(model.UserLoan.Id);
				if (userLoan2?.UserLoanRequestId.HasValue ?? false)
				{
					decimal num2 = (await _transactionService.GetByCondition((Transaction x) => x.UserLoanId == model.UserLoan.Id)).Sum((Transaction x) => x.Amount);
					if (userLoan2.AgentPrice <= num2)
					{
						UserLoanRequest request = await _userLoanRequestService.GetById(userLoan2.UserLoanRequestId.Value);
						request.AgentStatusId = (await _loanRequestStatusService.GetAllLoanRequestStatuses()).FirstOrDefault((LoanRequestStatus x) => x.SystemAgentRequestStatus == SystemAgentRequestStatus.تسویه_شده)?.Id;
						await _userLoanRequestService.Update(request);
					}
				}
			}
			else
			{
				LoanSystem.Services.Models.BaseModel userLoanRequest = model.UserLoanRequest;
				int num3;
				if (userLoanRequest == null)
				{
					num3 = 0;
				}
				else
				{
					_ = userLoanRequest.Id;
					num3 = 1;
				}
				if (num3 != 0)
				{
					bool update = false;
					UserLoanRequest userLoanRequest2 = await _userLoanRequestService.GetById(model.UserLoanRequest.Id);
					if (userLoanRequest2 != null && model.CustomerStatus != null && userLoanRequest2.StatusId != model.CustomerStatus.Id)
					{
						userLoanRequest2.StatusId = model.CustomerStatus.Id;
						update = true;
					}
					if (userLoanRequest2 != null && model.AgentStatus != null && userLoanRequest2.AgentStatusId != model.AgentStatus.Id)
					{
						userLoanRequest2.AgentStatusId = model.AgentStatus.Id;
						update = true;
					}
					if (update)
					{
						await _userLoanRequestService.Update(userLoanRequest2);
					}
				}
			}
			return Success(model, "عملیات با موفقیت انجام شد.");
		});
	}

	[HttpPost("[action]/{id}")]
	public Task<DefaultResponseState> Delete(Guid id)
	{
		return TryCatch(async () =>
		{
			Transaction transaction = await _transactionService.GetById(id);
			if (transaction == null)
			{
				return Error("امکان حذف وجود ندارد. لطفا بعدا دوباره تلاش نمایید");
			}
			_fileHelper.DeleteFile(transaction.FileName, Directories.Transaction(_currentUserService.CustomerId.ToString()));
			await _transactionService.Delete(transaction);
			return Success();
		});
	}

	[HttpGet("[action]/{agentId}/{userLoanId?}")]
	public Task<ResponseState<List<TransactionModel>>> ListByAgentId(long agentId, Guid? userLoanId)
	{
		return TryCatch(async () =>
		{
			List<Transaction> source = await _transactionService.GetByCondition((Transaction x) => x.AgentId == (long?)agentId && x.IsWithdraw, "Agent", "UserLoan");
			if (userLoanId.HasValue)
			{
				source = source.Where((Transaction x) =>
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
			List<TransactionModel> data = (from x in source
				select new TransactionModel(x) into x
				orderby x.SaveDate descending
				select x).ToList();
			return Success(data);
		});
	}

	[HttpGet("[action]/{marketerId}/{customerMarketerCommissionId?}")]
	public Task<ResponseState<List<TransactionModel>>> ListByMarketerId(long marketerId, Guid? customerMarketerCommissionId)
	{
		return TryCatch(async () =>
		{
			List<Transaction> source = await _transactionService.GetByCondition((Transaction x) => x.CustomerMarketerCommission.MarketerId == marketerId && x.IsWithdraw, "CustomerMarketerCommission.UserLoan");
			if (customerMarketerCommissionId.HasValue)
			{
				source = source.Where((Transaction x) =>
				{
					Guid? customerMarketerCommissionId2 = x.CustomerMarketerCommissionId;
					Guid? guid = customerMarketerCommissionId;
					if (customerMarketerCommissionId2.HasValue != guid.HasValue)
					{
						return false;
					}
					return !customerMarketerCommissionId2.HasValue || customerMarketerCommissionId2.GetValueOrDefault() == guid.GetValueOrDefault();
				}).ToList();
			}
			List<TransactionModel> data = (from x in source
				select new TransactionModel(x) into x
				orderby x.SaveDate descending
				select x).ToList();
			return Success(data);
		});
	}

	[HttpGet("[action]/{contactId}/{userLoanContactId?}")]
	public Task<ResponseState<List<TransactionModel>>> ListByContactId(Guid contactId, Guid? userLoanContactId)
	{
		return TryCatch(async () =>
		{
			List<Transaction> source = await _transactionService.GetByCondition((Transaction x) => x.ContactId == contactId && x.IsWithdraw, "UserLoanContact.UserLoan");
			if (userLoanContactId.HasValue)
			{
				source = source.Where((Transaction x) =>
				{
					Guid? userLoanContactId2 = x.UserLoanContactId;
					Guid? guid = userLoanContactId;
					if (userLoanContactId2.HasValue != guid.HasValue)
					{
						return false;
					}
					return !userLoanContactId2.HasValue || userLoanContactId2.GetValueOrDefault() == guid.GetValueOrDefault();
				}).ToList();
			}
			List<TransactionModel> data = (from x in source
				select new TransactionModel(x) into x
				orderby x.SaveDate descending
				select x).ToList();
			return Success(data);
		});
	}

	[HttpPost("[action]")]
	public Task<ResponseState<string>> SendOTP([FromBody] OnlineMoneyTransferModel model)
	{
		return TryCatch(async () =>
		{
			if (model.Provider == OnlineMoneyTransferProviders.EFarda)
			{
				OnlineMoneyTransferPanelInitAndSendOTPResult onlineMoneyTransferPanelInitAndSendOTPResult = await _onlineMoneyTransferPanelService.InitAndSendOTP(new OnlineMoneyTransferPanelInitAndSendOTP
				{
					Amount = model.Amount,
					ClientTraceNumber = Guid.NewGuid().ToString(),
					From = model.From,
					To = model.To,
					ToName = model.ToName
				});
				if (onlineMoneyTransferPanelInitAndSendOTPResult.success)
				{
					return Success(onlineMoneyTransferPanelInitAndSendOTPResult.result, "عملیات با موفقیت انجام شد.");
				}
			}
			else if (model.Provider == OnlineMoneyTransferProviders.Shahin)
			{
				try
				{
					string to = _appConfig.AdminPhone;
					if (string.IsNullOrWhiteSpace(to))
					{
						return Error<string>(null, "شماره تماس مدیر یافت نشد.");
					}
					if ((await _appSmsService.GetByCondition((AppSms x) => x.To == to && (int)x.AppSmsType == 21 && x.Date >= DateTime.Now.AddMinutes(-2.0), false)).Any())
					{
						return Error<string>(null, "از آخرین ارسال باید حداقل 2 دقیقه گذشته باشد.");
					}
					int num = new Random().Next(10000, 99999);
					await _integratedSmsSenderService.SendOnlineMoneyTransferOTP(to, num.ToString(), model.Amount, _currentUserService.CustomerId.Value);
					return Success(Guid.Empty.ToString(), "عملیات با موفقیت انجام شد.");
				}
				catch (Exception ex)
				{
					return Error<string>(null, ex.Message);
				}
			}
			return Error<string>(null, "کد تاییدیه ارسال نشد.");
		});
	}

	[HttpGet("[action]/{iban}")]
	public Task<ResponseState<string>> IbanInquiry(string iban)
	{
		return TryCatch(async () =>
		{
			if (string.IsNullOrEmpty(iban))
			{
				return Error<string>(null, "شماره شبا پر نشده است.");
			}
			AppInquiry appInquiry = await _integratedInquiryService.IbanInfo(new IbanInfoModel
			{
				MemberId = null,
				Iban = iban
			}, AppInquiryType.مشخصات_شبا);
			if (appInquiry.IsSuccess)
			{
				IbanInfoResult ibanInfoResult = JsonSerializer.Deserialize<IbanInfoResult>(appInquiry.PanelResultJson, new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				});
				return Success(ibanInfoResult.Name, "عملیات با موفقیت انجام شد.");
			}
			return Error<string>(null, "خطا در دریافت اطلاعات شبا");
		});
	}

	[HttpPost("[action]")]
	public Task<ResponseState<AgentFactorTransactionModel>> CreateAgentFactorTransaction(AgentFactorTransactionModel model)
	{
		return TryCatch(async () =>
		{
			if (model.Agent == null)
			{
				return Error<AgentFactorTransactionModel>(null, "نماینده انتخاب نشده است.");
			}
			List<IdCode> agentFactors = model.AgentFactors;
			if (agentFactors == null || !agentFactors.Any())
			{
				return Error<AgentFactorTransactionModel>(null, "صورتحساب انتخاب نشده است.");
			}
			List<AgentFactorModel> source = (await _agentFactorService.GetByCondition((AgentFactor x) => model.AgentFactors.Select((IdCode s) => s.Id).ToList().Contains(x.Id), false, "WithdrawAgentFactors.Withdraw", "MemberPurchase")).Select((AgentFactor x) => new AgentFactorModel(x)).ToList();
			if (model.Amount != source.Sum((AgentFactorModel x) => PayedAmountOfFactorByCustomer(x) - x.SumWithdraws - x.Commission))
			{
				return Error<AgentFactorTransactionModel>(null, "مبلغ وارد شده با مجموع تسویه نشده صورتحساب (ها) برابر نمی باشد.");
			}
			string fileName = "";
			if (model.File != null)
			{
				if (model.File.Length == 0L)
				{
					return Error<AgentFactorTransactionModel>(null, "فایل اول خالی است.");
				}
				_ = model.File.Length;
				if (!_fileHelper.CheckFileIsImage(model.File))
				{
					return Error<AgentFactorTransactionModel>(null, "فقط فایل تصویر مجاز می باشد. (فایل اول)");
				}
				fileName = await _fileHelper.UploadFile(model.File, FileUploadDirectory.Transaction, _currentUserService.CustomerId.ToString());
			}
			string fileName2 = "";
			if (model.File2 != null)
			{
				if (model.File2.Length == 0L)
				{
					return Error<AgentFactorTransactionModel>(null, "فایل دوم خالی است.");
				}
				_ = model.File2.Length;
				if (!_fileHelper.CheckFileIsImage(model.File2))
				{
					return Error<AgentFactorTransactionModel>(null, "فقط فایل تصویر مجاز می باشد. (فایل دوم)");
				}
				fileName2 = await _fileHelper.UploadFile(model.File2, FileUploadDirectory.Transaction, _currentUserService.CustomerId.ToString());
			}
			string fileName3 = "";
			if (model.File3 != null)
			{
				if (model.File3.Length == 0L)
				{
					return Error<AgentFactorTransactionModel>(null, "فایل سوم خالی است.");
				}
				_ = model.File3.Length;
				if (!_fileHelper.CheckFileIsImage(model.File3))
				{
					return Error<AgentFactorTransactionModel>(null, "فقط فایل تصویر مجاز می باشد. (فایل سوم)");
				}
				fileName3 = await _fileHelper.UploadFile(model.File3, FileUploadDirectory.Transaction, _currentUserService.CustomerId.ToString());
			}
			Transaction entity = new Transaction
			{
				Id = Guid.Empty,
				Amount = model.Amount,
				SaveDate = new DateTime(model.SaveDate.Year, model.SaveDate.Month, model.SaveDate.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second),
				SavedBy = HttpContext.User.Identity.Name,
				Description = model.Description,
				IsUnknown = model.IsUnknown,
				IsWithdraw = true,
				PaymentMethod = PaymentMethods.Bank,
				AgentId = model.Agent?.Id,
				FileName = fileName,
				FileName2 = fileName2,
				FileName3 = fileName3
			};
			await _transactionService.Insert(entity);
			model.Id = entity.Id;
			await _withdrawAgentFactorService.InsertBulk(model.AgentFactors.Select((IdCode x) => new WithdrawAgentFactor
			{
				AgentFactorId = x.Id,
				WithdrawId = entity.Id
			}).ToList());
			return Success(model, "عملیات با موفقیت انجام شد.");
		});
	}

	private decimal PayedAmountOfFactorByCustomer(AgentFactorModel agentFactor)
	{
		if (agentFactor.CheckoutType == 0)
		{
			return 0m;
		}
		if (agentFactor.CheckoutType != AgentFactorCheckoutTypes.آنلاین)
		{
			return agentFactor.Amount;
		}
		return (agentFactor?.MemberPurchase?.Amount).GetValueOrDefault();
	}

	[HttpPost("[action]")]
	public Task<ResponseState<AgentFactorTransactionModel>> EditAgentFactorTransaction(AgentFactorTransactionModel model)
	{
		return TryCatch(async () =>
		{
			Transaction entity = await _transactionService.GetById(model.Id);
			if (entity == null)
			{
				return Error<AgentFactorTransactionModel>(null, "تراکنش یافت نشد.");
			}
			if (model.Agent == null)
			{
				return Error<AgentFactorTransactionModel>(null, "نماینده انتخاب نشده است.");
			}
			List<IdCode> agentFactors = model.AgentFactors;
			if (agentFactors == null || !agentFactors.Any())
			{
				return Error<AgentFactorTransactionModel>(null, "صورتحساب انتخاب نشده است.");
			}
			List<AgentFactorModel> source = (await _agentFactorService.GetByCondition((AgentFactor x) => model.AgentFactors.Select((IdCode s) => s.Id).ToList().Contains(x.Id), false, "WithdrawAgentFactors", "MemberPurchase")).Select((AgentFactor x) => new AgentFactorModel(x)).ToList();
			if (model.Amount != source.Sum((AgentFactorModel x) => PayedAmountOfFactorByCustomer(x) - x.SumWithdraws - x.Commission))
			{
				return Error<AgentFactorTransactionModel>(null, "مبلغ وارد شده با مجموع تسویه نشده صورتحساب (ها) برابر نمی باشد.");
			}
			if (model.File != null)
			{
				if (model.File.Length == 0L)
				{
					return Error<AgentFactorTransactionModel>(null, "فایل خالی است.");
				}
				_ = model.File.Length;
				if (!_fileHelper.CheckFileIsImage(model.File))
				{
					return Error<AgentFactorTransactionModel>(null, "فقط فایل تصویر مجاز می باشد.");
				}
				_fileHelper.DeleteFile(entity.FileName, Directories.Transaction(_currentUserService.CustomerId.ToString()));
				string fileName = await _fileHelper.UploadFile(model.File, FileUploadDirectory.Transaction, _currentUserService.CustomerId.ToString());
				entity.FileName = fileName;
			}
			else if (!string.IsNullOrEmpty(entity.FileName) && string.IsNullOrEmpty(model.FileName))
			{
				_fileHelper.DeleteFile(entity.FileName, Directories.Transaction(_currentUserService.CustomerId.ToString()));
			}
			if (model.File2 != null)
			{
				if (model.File2.Length == 0L)
				{
					return Error<AgentFactorTransactionModel>(null, "فایل خالی است.");
				}
				_ = model.File2.Length;
				if (!_fileHelper.CheckFileIsImage(model.File2))
				{
					return Error<AgentFactorTransactionModel>(null, "فقط فایل تصویر مجاز می باشد.");
				}
				_fileHelper.DeleteFile(entity.FileName2, Directories.Transaction(_currentUserService.CustomerId.ToString()));
				string fileName2 = await _fileHelper.UploadFile(model.File2, FileUploadDirectory.Transaction, _currentUserService.CustomerId.ToString());
				entity.FileName2 = fileName2;
			}
			else if (!string.IsNullOrEmpty(entity.FileName2) && string.IsNullOrEmpty(model.FileName2))
			{
				_fileHelper.DeleteFile(entity.FileName2, Directories.Transaction(_currentUserService.CustomerId.ToString()));
			}
			if (model.File3 != null)
			{
				if (model.File3.Length == 0L)
				{
					return Error<AgentFactorTransactionModel>(null, "فایل خالی است.");
				}
				_ = model.File3.Length;
				if (!_fileHelper.CheckFileIsImage(model.File3))
				{
					return Error<AgentFactorTransactionModel>(null, "فقط فایل تصویر مجاز می باشد.");
				}
				_fileHelper.DeleteFile(entity.FileName3, Directories.Transaction(_currentUserService.CustomerId.ToString()));
				string fileName3 = await _fileHelper.UploadFile(model.File3, FileUploadDirectory.Transaction, _currentUserService.CustomerId.ToString());
				entity.FileName3 = fileName3;
			}
			else if (!string.IsNullOrEmpty(entity.FileName3) && string.IsNullOrEmpty(model.FileName3))
			{
				_fileHelper.DeleteFile(entity.FileName3, Directories.Transaction(_currentUserService.CustomerId.ToString()));
			}
			entity.Amount = model.Amount;
			entity.Description = model.Description;
			entity.SaveDate = new DateTime(model.SaveDate.Year, model.SaveDate.Month, model.SaveDate.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
			entity.IsUnknown = false;
			entity.PaymentMethod = model.PaymentMethod;
			entity.AgentId = model.Agent?.Id;
			await _transactionService.Update(entity);
			List<WithdrawAgentFactor> currentDbWithdraws = await _withdrawAgentFactorService.GetByCondition((WithdrawAgentFactor x) => x.WithdrawId == entity.Id, false);
			List<WithdrawAgentFactor> mustDelete = currentDbWithdraws.Where((WithdrawAgentFactor x) => !model.AgentFactors.Select((IdCode af) => af.Id).ToList().Contains(x.AgentFactorId)).ToList();
			List<WithdrawAgentFactor> list = (from x in model.AgentFactors.Where((IdCode x) => !currentDbWithdraws.Select((WithdrawAgentFactor c) => c.Id).ToList().Contains(x.Id)).ToList()
				select new WithdrawAgentFactor
				{
					AgentFactorId = x.Id,
					WithdrawId = entity.Id
				}).ToList();
			if (list.Any())
			{
				await _withdrawAgentFactorService.InsertBulk(list);
			}
			if (mustDelete.Any())
			{
				await _withdrawAgentFactorService.Delete(mustDelete);
			}
			return Success(model, "عملیات با موفقیت انجام شد.");
		});
	}

	[HttpGet("[action]")]
	public Task<ResponseState<List<AgentFactorTransactionModel>>> GetAgentFactorTransactionsList(Guid? agentFactorId, long? agentId, DateTime? start, DateTime? end)
	{
		return TryCatch(async () =>
		{
			List<AgentFactorTransactionModel> data = await _transactionService.GetAgentFactorTransactionsListAsync(agentFactorId, agentId, start, end);
			return Success(data);
		});
	}
}
