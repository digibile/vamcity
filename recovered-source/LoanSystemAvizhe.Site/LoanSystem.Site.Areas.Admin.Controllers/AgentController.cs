using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LoanSystem.Core.Configuration;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Services.Users;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Services.Enums;
using LoanSystemAvizhe.Services.FileHelper;
using LoanSystemAvizhe.Site.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LoanSystem.Site.Areas.Admin.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Admin")]
[RequestFormLimits(ValueCountLimit = 8192)]
public class AgentController : BaseAdminController
{
	private readonly IAgentService _agentService;

	private readonly IAgentLoanService _agentLoanService;

	private readonly ITransactionService _transactionService;

	private readonly IFileHelper _fileHelper;

	private readonly ISubAgentTransactionService _subAgentTransactionService;

	private readonly IUserLoanService _userLoanService;

	private readonly IUserLoanRequestService _userLoanRequestService;

	private readonly ICurrentUserService _currentUserService;

	private readonly ICustomerService _customerService;

	private readonly UserManager _userManager;

	private readonly AppConfig _appConfig;

	public AgentController(ILogger<AgentController> logger, IOptions<AppConfig> appConfig, IAgentService agentService, IFileHelper fileHelper, ICurrentUserService currentUserService, IUserLoanService userLoanService, IUserLoanRequestService userLoanRequestService, UserManager userManager, ICustomerService customerService, ITransactionService transactionService, IAgentLoanService agentLoanService, ISubAgentTransactionService subAgentTransactionService)
		: base(logger)
	{
		_agentService = agentService;
		_currentUserService = currentUserService;
		_appConfig = appConfig.Value;
		_userLoanService = userLoanService;
		_userLoanRequestService = userLoanRequestService;
		_userManager = userManager;
		_customerService = customerService;
		_transactionService = transactionService;
		_subAgentTransactionService = subAgentTransactionService;
		_agentLoanService = agentLoanService;
		_fileHelper = fileHelper;
	}

	[HttpGet("[action]")]
	public Task<ResponseState<List<AgentModel>>> List()
	{
		return TryCatch(async () =>
		{
			List<UserLoan> userLoans = await _userLoanService.GetUserLoansHasAgent("Loan", "UserLoanRequest");
			List<Transaction> withdraws = await _transactionService.GetByCondition((Transaction x) => x.IsWithdraw && x.AgentId.HasValue);
			List<UserLoanRequest> userLoanRequests = await _userLoanRequestService.GetAllUserLoanRequests("AgentStatus");
			List<User> agents = await _agentService.GetAllAgents();
			List<AgentModel> data = (from x in agents
				select new AgentModel
				{
					Id = x.Id,
					AgentId = x.AgentId,
					UserName = x.UserName,
					FirstName = x.FirstName,
					LastName = x.LastName,
					Mobile = x.Mobile,
					Phone = x.Phone,
					LastLoginDate = x.LastLoginDate,
					BusinessName = x.BusinessName,
					NationalCode = x.NationalCode,
					IsActive = x.IsActive,
					BankAccountsJson = x.BankAccountsJson,
					City = x.City,
					DynamicLoanRequstCounts = CalcLoanRequstCounts(userLoanRequests, x.Id),
					TotalAgentShare = userLoans.Where((UserLoan ul) => ul.AgentId == x.Id).Sum((UserLoan ul) => ul.AgentPrice),
					LoanCount = userLoans.Count((UserLoan ul) => ul.AgentId == x.Id),
					TotalLoans = userLoans.Where((UserLoan ul) => ul.AgentId == x.Id).Sum((UserLoan ul) => ul.PayableAmount),
					TotalReceives = (withdraws.Any((Transaction w) => w.AgentId == x.Id) ? withdraws.Where((Transaction w) => w.AgentId == x.Id).Sum((Transaction transaction) => transaction.Amount) : 0m),
					AgentLoans = x.AgentLoans?.Select((AgentLoan ag) => new AgentLoanModel(ag)).ToList(),
					SubAgentCount = agents.Count((User ag) => ag.AgentId == x.Id)
				} into x
				orderby x.Id descending
				select x).ToList();
			return Success(data);
		});
	}

	[HttpGet("[action]")]
	public Task<ResponseState<List<AgentModel>>> ListLight()
	{
		return TryCatch(async () =>
		{
			List<AgentModel> data = (from x in await _agentService.GetAllAgents()
				select new AgentModel
				{
					Id = x.Id,
					FirstName = x.FirstName,
					LastName = x.LastName,
					Mobile = x.Mobile,
					Phone = x.Phone,
					BusinessName = x.BusinessName,
					IsActive = x.IsActive
				} into x
				orderby x.Id descending
				select x).ToList();
			return Success(data);
		});
	}

	[HttpGet("[action]/{parentId?}")]
	public Task<ResponseState<List<AgentModel>>> SubAgentList(long? parentId)
	{
		return TryCatch(async () =>
		{
			List<User> agents = await _agentService.GetSubAgents(parentId, includeParent: true);
			List<UserLoan> userLoans = await _userLoanService.GetByCondition((UserLoan x) => agents.Select((User ag) => ag.Id).ToList().Contains((long)x.SubAgentId), "Loan", "UserLoanRequest");
			List<SubAgentTransaction> withdraws = await _subAgentTransactionService.GetByCondition((SubAgentTransaction x) => x.IsWithdraw && x.UserLoan.SubAgentId.HasValue, "UserLoan");
			List<UserLoanRequest> userLoanRequests = await _userLoanRequestService.GetByCondition((UserLoanRequest x) => x.AgentId.HasValue && agents.Select((User ag) => ag.Id).ToList().Contains((long)x.AgentId), false);
			List<AgentModel> data = (from x in agents
				select new AgentModel
				{
					Id = x.Id,
					AgentId = x.Agent?.Id,
					ParentAgentName = x.Agent?.FirstName,
					ParentAgentBusinessName = x.Agent?.BusinessName,
					UserName = x.UserName,
					FirstName = x.FirstName,
					LastName = x.LastName,
					Mobile = x.Mobile,
					Phone = x.Phone,
					LastLoginDate = x.LastLoginDate,
					BusinessName = x.BusinessName,
					NationalCode = x.NationalCode,
					IsActive = x.IsActive,
					BankAccountsJson = x.BankAccountsJson,
					City = x.City,
					DynamicLoanRequstCounts = CalcLoanRequstCounts(userLoanRequests, x.Id),
					TotalAgentShare = userLoans.Where((UserLoan ul) => ul.SubAgentId == x.Id).Sum((UserLoan ul) => ul.SubAgentPrice),
					LoanCount = userLoans.Count((UserLoan ul) => ul.SubAgentId == x.Id),
					TotalLoans = userLoans.Where((UserLoan ul) => ul.SubAgentId == x.Id).Sum((UserLoan ul) => ul.PayableAmount),
					TotalReceives = (withdraws.Any((SubAgentTransaction w) => w.UserLoan.SubAgentId == x.Id) ? withdraws.Where((SubAgentTransaction w) => w.UserLoan.SubAgentId == x.Id).Sum((SubAgentTransaction subAgentTransaction) => subAgentTransaction.Amount) : 0m),
					AgentLoans = x.AgentLoans?.Select((AgentLoan ag) => new AgentLoanModel(ag)).ToList()
				} into x
				orderby x.Id descending
				select x).ToList();
			return Success(data);
		});
	}

	private List<dynamic> CalcLoanRequstCounts(List<UserLoanRequest> userLoanRequests, long agentId)
	{
		List<object> list = new List<object>();
		foreach (LoanRequestStatus agentStatus in (from x in userLoanRequests
			where x.AgentId == agentId && x.AgentStatus != null
			select x.AgentStatus).Distinct(new LoanRequestStatusComparer()).ToList())
		{
			list.Add(new
			{
				title = agentStatus.Title.ToString().Replace("_", " "),
				count = userLoanRequests.Count((UserLoanRequest x) => x.AgentId == agentId && x.AgentStatus?.Id == agentStatus.Id)
			});
		}
		list.Add(new
		{
			title = "تعداد کل",
			count = userLoanRequests.Count((UserLoanRequest x) => x.AgentId == agentId)
		});
		return list;
	}

	[HttpGet("[action]/{id}")]
	public Task<ResponseState<AgentModel>> Get(long id)
	{
		return TryCatch(async () =>
		{
			AgentModel model = new AgentModel
			{
				AgentLoans = new List<AgentLoanModel>(),
				IsActive = true
			};
			List<User> allAgents;
			if (id != 0L)
			{
				User entity = await _agentService.GetById(id, "AgentLoans.Contact", "AgentMetaData");
				if (entity == null)
				{
					return Error<AgentModel>(null, "اطلاعات مورد نظر یافت نشد");
				}
				model = new AgentModel(entity);
				model.Password = string.Empty;
				model.ConfirmPassword = string.Empty;
				if (entity.AgentId.HasValue)
				{
					model.Parents = new List<ParentAgent>();
					allAgents = await _agentService.GetAllAgents();
					FillParentAgent(entity);
					model.Parents.Reverse();
				}
				model.FillImageBase64Props(_fileHelper, _currentUserService.CustomerId.Value);
			}
			return Success(model);
			void FillParentAgent(User agent)
			{
				User user = allAgents.FirstOrDefault((User x) => x.Id == agent.AgentId.Value);
				if (user != null)
				{
					model.Parents.Add(new ParentAgent
					{
						Id = user.Id,
						BusinessName = user.BusinessName,
						FullName = user.FullName
					});
					if (user.AgentId.HasValue)
					{
						FillParentAgent(user);
					}
				}
			}
		});
	}

	[HttpPost("[action]")]
	public Task<ResponseState<long>> Create([FromForm] AgentModel model)
	{
		return TryCatch(async () =>
		{
			Customer customer = await _customerService.GetById(_currentUserService.CustomerId.Value);
			model.UserName = GenerateUserName(customer.Code, model);
			if (await _userManager.FindByUserName(model.UserName) != null)
			{
				return Error(0L, "نام کاربری وارد شده تکراری است.");
			}
			User user = await _userManager.GetById(_currentUserService.UserId.Value);
			User entity = new User
			{
				Id = model.Id,
				UserName = model.UserName,
				FirstName = model.FirstName,
				Address = model.Address,
				Email = model.Email,
				PostalCode = model.PostalCode,
				LastLoginDate = model.LastLoginDate,
				Mobile = model.Mobile,
				Shenasname = model.Shenasname,
				FatherName = model.FatherName,
				Phone = model.Phone,
				BusinessName = model.BusinessName,
				NationalCode = model.NationalCode,
				RegDate = DateTime.Now,
				IsActive = model.IsActive,
				BankAccountsJson = model.BankAccountsJson,
				City = model.City,
				SmsActiveForMembers = model.SmsActiveForMembers,
				AgentMetaData = ((model.MetaData != null) ? new AgentMetaData
				{
					AgentCategoryActivities = model.MetaData.AgentCategoryActivities,
					CustomerId = customer.Id,
					SiteUrl = model.MetaData.SiteUrl,
					IsOnline = model.MetaData.IsOnline,
					IsPhysical = model.MetaData.IsPhysical,
					ShenaseMelli = model.MetaData.ShenaseMelli,
					ShomareSabt = model.MetaData.ShomareSabt,
					SignaturesOwnerNames = model.MetaData.SignaturesOwnerNames,
					Status = model.MetaData.Status,
					RejectReason = model.MetaData.RejectReason,
					LastAdminChangeDateTime = DateTime.Now,
					CheckedBy = user?.FullName,
					CompanyName = model.MetaData.CompanyName,
					AgentType = model.MetaData.AgentType,
					UseDefaultCommission = model.MetaData.UseDefaultCommission,
					MaxAgentFactorCommission = model.MetaData.MaxAgentFactorCommission,
					AgentFactorCommission = model.MetaData.AgentFactorCommission
				} : null)
			};
			entity.CustomerId = _currentUserService.CustomerId.Value;
			entity.UserRoleNames = new List<string> { "Agent" };
			PasswordHasher<User> passwordHasher = new PasswordHasher<User>();
			string passwordHash = passwordHasher.HashPassword(entity, model.Password);
			entity.PasswordHash = passwordHash;
			if (model.MetaData?.ThumbnailFile != null)
			{
				if (model.MetaData.ThumbnailFile.Length == 0L)
				{
					return Error(0L, "فایل کارت ملی خالی است.");
				}
				if (!_fileHelper.CheckFileIsImage(model.MetaData.ThumbnailFile))
				{
					return Error(0L, "فقط فایل با فرمت تصویر و pdf و rar مجاز می باشد.");
				}
				AgentMetaData agentMetaData = entity.AgentMetaData;
				agentMetaData.ThumbnailFileName = await _fileHelper.UploadFile(model.MetaData.ThumbnailFile, FileUploadDirectory.AgentAttachment, customer.Id.ToString());
			}
			if (model.MetaData?.KartMelliFile != null)
			{
				if (model.MetaData.KartMelliFile.Length == 0L)
				{
					return Error(0L, "فایل کارت ملی خالی است.");
				}
				if (!_fileHelper.CheckFileIsImageOrPdfOrRarOrZipOrXls(model.MetaData.KartMelliFile))
				{
					return Error(0L, "فقط فایل با فرمت تصویر و pdf و rar مجاز می باشد.");
				}
				AgentMetaData agentMetaData = entity.AgentMetaData;
				agentMetaData.KartMelliFileName = await _fileHelper.UploadFile(model.MetaData.KartMelliFile, FileUploadDirectory.AgentAttachment, customer.Id.ToString());
			}
			if (model.MetaData?.JavazKasbFile != null)
			{
				if (model.MetaData.JavazKasbFile.Length == 0L)
				{
					return Error(0L, "فایل کارت ملی خالی است.");
				}
				if (!_fileHelper.CheckFileIsImageOrPdfOrRarOrZipOrXls(model.MetaData.JavazKasbFile))
				{
					return Error(0L, "فقط فایل با فرمت تصویر و pdf و rar مجاز می باشد.");
				}
				AgentMetaData agentMetaData = entity.AgentMetaData;
				agentMetaData.JavazKasbFileName = await _fileHelper.UploadFile(model.MetaData.JavazKasbFile, FileUploadDirectory.AgentAttachment, customer.Id.ToString());
			}
			if (model.MetaData?.KartMelliFile != null)
			{
				if (model.MetaData.KartMelliFile.Length == 0L)
				{
					return Error(0L, "فایل کارت ملی خالی است.");
				}
				if (!_fileHelper.CheckFileIsImageOrPdfOrRarOrZipOrXls(model.MetaData.KartMelliFile))
				{
					return Error(0L, "فقط فایل با فرمت تصویر و pdf و rar مجاز می باشد.");
				}
				AgentMetaData agentMetaData = entity.AgentMetaData;
				agentMetaData.KartMelliFileName = await _fileHelper.UploadFile(model.MetaData.KartMelliFile, FileUploadDirectory.AgentAttachment, customer.Id.ToString());
			}
			if (model.MetaData?.EjarenameFile != null)
			{
				if (model.MetaData.EjarenameFile.Length == 0L)
				{
					return Error(0L, "فایل اجاره نامه خالی است.");
				}
				if (!_fileHelper.CheckFileIsImageOrPdfOrRarOrZipOrXls(model.MetaData.EjarenameFile))
				{
					return Error(0L, "فقط فایل با فرمت تصویر و pdf و rar مجاز می باشد.");
				}
				AgentMetaData agentMetaData = entity.AgentMetaData;
				agentMetaData.EjarenameFileName = await _fileHelper.UploadFile(model.MetaData.EjarenameFile, FileUploadDirectory.AgentAttachment, customer.Id.ToString());
			}
			if (model.MetaData?.AsasnameFile != null)
			{
				if (model.MetaData.AsasnameFile.Length == 0L)
				{
					return Error(0L, "فایل اساسنامه خالی است.");
				}
				if (!_fileHelper.CheckFileIsImageOrPdfOrRarOrZipOrXls(model.MetaData.AsasnameFile))
				{
					return Error(0L, "فقط فایل با فرمت تصویر و pdf و rar مجاز می باشد.");
				}
				AgentMetaData agentMetaData = entity.AgentMetaData;
				agentMetaData.AsasnameFileName = await _fileHelper.UploadFile(model.MetaData.AsasnameFile, FileUploadDirectory.AgentAttachment, customer.Id.ToString());
			}
			if (model.MetaData?.RoznameRasmiFile != null)
			{
				if (model.MetaData.RoznameRasmiFile.Length == 0L)
				{
					return Error(0L, "فایل روزنامه رسمی خالی است.");
				}
				if (!_fileHelper.CheckFileIsImageOrPdfOrRarOrZipOrXls(model.MetaData.RoznameRasmiFile))
				{
					return Error(0L, "فقط فایل با فرمت تصویر و pdf و rar مجاز می باشد.");
				}
				AgentMetaData agentMetaData = entity.AgentMetaData;
				agentMetaData.RoznameRasmiFileName = await _fileHelper.UploadFile(model.MetaData.RoznameRasmiFile, FileUploadDirectory.AgentAttachment, customer.Id.ToString());
			}
			if (model.MetaData?.PledgeChequeFile != null)
			{
				if (model.MetaData.PledgeChequeFile.Length == 0L)
				{
					return Error(0L, "فایل روزنامه رسمی خالی است.");
				}
				if (!_fileHelper.CheckFileIsImageOrPdfOrRarOrZipOrXls(model.MetaData.PledgeChequeFile))
				{
					return Error(0L, "فقط فایل با فرمت تصویر و pdf و rar مجاز می باشد.");
				}
				AgentMetaData agentMetaData = entity.AgentMetaData;
				agentMetaData.PledgeChequeFileName = await _fileHelper.UploadFile(model.MetaData.PledgeChequeFile, FileUploadDirectory.AgentAttachment, customer.Id.ToString());
			}
			IdentityResult identityResult = await _userManager.Insert(entity, model.Password, entity.UserRoleNames);
			if (identityResult.Succeeded)
			{
				List<AgentLoan> agentLoans = ((model.AgentLoans?.Any() ?? false) ? model.AgentLoans.Select((AgentLoanModel x) =>
				{
					AgentLoan agentLoan = new AgentLoan
					{
						Id = x.Id,
						AgentId = entity.Id,
						CustomerId = _currentUserService.CustomerId.Value,
						LoanId = x.LoanId,
						Percent = x.Percent,
						DefaultCheckoutDays = x.DefaultCheckoutDays,
						ContactId = x.Contact?.Id,
						ContactPercent = x.ContactPercent,
						ProductPercent = x.ProductPercent,
						ProductCheckoutDays = x.ProductCheckoutDays
					};
					List<OtherPercent> otherPercents = x.OtherPercents;
					agentLoan.OtherPercentsJson = ((otherPercents != null && otherPercents.Any()) ? JsonSerializer.Serialize(x.OtherPercents) : null);
					return agentLoan;
				}).ToList() : new List<AgentLoan>());
				await _agentLoanService.Modify(agentLoans, entity.Id);
				return Success(entity.Id, "عملیات با موفقیت انجام شد.");
			}
			return Error(500L, string.Join(".", identityResult.Errors.Select((IdentityError x) => x.Description)));
		});
	}

	[HttpPost("[action]")]
	public Task<ResponseState<long>> Edit([FromForm] AgentModel model)
	{
		return TryCatch(async () =>
		{
			User currentUser = await _userManager.GetById(_currentUserService.UserId.Value);
			Customer customer = await _customerService.GetById(_currentUserService.CustomerId.Value);
			model.UserName = GenerateUserName(customer.Code, model);
			User user = await _userManager.FindByUserName(model.UserName);
			if (user != null && user.Id != model.Id)
			{
				return Error(0L, "نام کاربری وارد شده تکراری است.");
			}
			User entity = await _agentService.GetById(model.Id, "AgentMetaData");
			if (model.MetaData != null)
			{
				if (entity.AgentMetaData == null)
				{
					entity.AgentMetaData = new AgentMetaData();
				}
				entity.AgentMetaData.AgentCategoryActivities = model.MetaData.AgentCategoryActivities;
				entity.AgentMetaData.CustomerId = customer.Id;
				entity.AgentMetaData.SiteUrl = model.MetaData.SiteUrl;
				entity.AgentMetaData.IsOnline = model.MetaData.IsOnline;
				entity.AgentMetaData.IsPhysical = model.MetaData.IsPhysical;
				entity.AgentMetaData.ShenaseMelli = model.MetaData.ShenaseMelli;
				entity.AgentMetaData.ShomareSabt = model.MetaData.ShomareSabt;
				entity.AgentMetaData.SignaturesOwnerNames = model.MetaData.SignaturesOwnerNames;
				entity.AgentMetaData.LastAdminChangeDateTime = ((entity.AgentMetaData.Status != model.MetaData.Status || model.MetaData.Id == Guid.Empty) ? DateTime.Now : entity.AgentMetaData.LastAdminChangeDateTime);
				entity.AgentMetaData.Status = model.MetaData.Status;
				entity.AgentMetaData.RejectReason = model.MetaData.RejectReason;
				entity.AgentMetaData.CheckedBy = currentUser?.FullName;
				entity.AgentMetaData.CompanyName = model.MetaData.CompanyName;
				entity.AgentMetaData.AgentType = model.MetaData.AgentType;
				entity.AgentMetaData.UseDefaultCommission = model.MetaData.UseDefaultCommission;
				entity.AgentMetaData.MaxAgentFactorCommission = model.MetaData.MaxAgentFactorCommission;
				entity.AgentMetaData.AgentFactorCommission = model.MetaData.AgentFactorCommission;
			}
			entity.Id = model.Id;
			entity.UserName = model.UserName;
			entity.FirstName = model.FirstName;
			entity.LastName = model.LastName;
			entity.LastLoginDate = model.LastLoginDate;
			entity.BusinessName = model.BusinessName;
			entity.Mobile = model.Mobile;
			entity.Phone = model.Phone;
			entity.FatherName = model.FatherName;
			entity.Address = model.Address;
			entity.Email = model.Email;
			entity.PostalCode = model.PostalCode;
			entity.Shenasname = model.Shenasname;
			entity.NationalCode = model.NationalCode;
			entity.IsActive = model.IsActive;
			entity.CustomerId = _currentUserService.CustomerId.Value;
			entity.BankAccountsJson = model.BankAccountsJson;
			entity.City = model.City;
			entity.SmsActiveForMembers = model.SmsActiveForMembers;
			if (!string.IsNullOrEmpty(model.Password))
			{
				PasswordHasher<User> passwordHasher = new PasswordHasher<User>();
				string passwordHash = passwordHasher.HashPassword(entity, model.Password);
				entity.PasswordHash = passwordHash;
			}
			if (model.MetaData?.ThumbnailFile != null)
			{
				if (model.MetaData.ThumbnailFile.Length == 0L)
				{
					return Error(0L, "فایل کارت ملی خالی است.");
				}
				if (!_fileHelper.CheckFileIsImage(model.MetaData.ThumbnailFile))
				{
					return Error(0L, "فقط فایل با فرمت تصویر و pdf و rar مجاز می باشد.");
				}
				AgentMetaData agentMetaData = entity.AgentMetaData;
				agentMetaData.ThumbnailFileName = await _fileHelper.UploadFile(model.MetaData.ThumbnailFile, FileUploadDirectory.AgentAttachment, customer.Id.ToString());
			}
			if (model.MetaData?.KartMelliFile != null)
			{
				if (model.MetaData.KartMelliFile.Length == 0L)
				{
					return Error(0L, "فایل کارت ملی خالی است.");
				}
				if (!_fileHelper.CheckFileIsImageOrPdfOrRarOrZipOrXls(model.MetaData.KartMelliFile))
				{
					return Error(0L, "فقط فایل با فرمت تصویر و pdf و rar مجاز می باشد.");
				}
				AgentMetaData agentMetaData = entity.AgentMetaData;
				agentMetaData.KartMelliFileName = await _fileHelper.UploadFile(model.MetaData.KartMelliFile, FileUploadDirectory.AgentAttachment, customer.Id.ToString());
			}
			if (model.MetaData?.JavazKasbFile != null)
			{
				if (model.MetaData.JavazKasbFile.Length == 0L)
				{
					return Error(0L, "فایل کارت ملی خالی است.");
				}
				if (!_fileHelper.CheckFileIsImageOrPdfOrRarOrZipOrXls(model.MetaData.JavazKasbFile))
				{
					return Error(0L, "فقط فایل با فرمت تصویر و pdf و rar مجاز می باشد.");
				}
				AgentMetaData agentMetaData = entity.AgentMetaData;
				agentMetaData.JavazKasbFileName = await _fileHelper.UploadFile(model.MetaData.JavazKasbFile, FileUploadDirectory.AgentAttachment, customer.Id.ToString());
			}
			if (model.MetaData?.KartMelliFile != null)
			{
				if (model.MetaData.KartMelliFile.Length == 0L)
				{
					return Error(0L, "فایل کارت ملی خالی است.");
				}
				if (!_fileHelper.CheckFileIsImageOrPdfOrRarOrZipOrXls(model.MetaData.KartMelliFile))
				{
					return Error(0L, "فقط فایل با فرمت تصویر و pdf و rar مجاز می باشد.");
				}
				AgentMetaData agentMetaData = entity.AgentMetaData;
				agentMetaData.KartMelliFileName = await _fileHelper.UploadFile(model.MetaData.KartMelliFile, FileUploadDirectory.AgentAttachment, customer.Id.ToString());
			}
			if (model.MetaData?.EjarenameFile != null)
			{
				if (model.MetaData.EjarenameFile.Length == 0L)
				{
					return Error(0L, "فایل اجاره نامه خالی است.");
				}
				if (!_fileHelper.CheckFileIsImageOrPdfOrRarOrZipOrXls(model.MetaData.EjarenameFile))
				{
					return Error(0L, "فقط فایل با فرمت تصویر و pdf و rar مجاز می باشد.");
				}
				AgentMetaData agentMetaData = entity.AgentMetaData;
				agentMetaData.EjarenameFileName = await _fileHelper.UploadFile(model.MetaData.EjarenameFile, FileUploadDirectory.AgentAttachment, customer.Id.ToString());
			}
			if (model.MetaData?.AsasnameFile != null)
			{
				if (model.MetaData.AsasnameFile.Length == 0L)
				{
					return Error(0L, "فایل اساسنامه خالی است.");
				}
				if (!_fileHelper.CheckFileIsImageOrPdfOrRarOrZipOrXls(model.MetaData.AsasnameFile))
				{
					return Error(0L, "فقط فایل با فرمت تصویر و pdf و rar مجاز می باشد.");
				}
				AgentMetaData agentMetaData = entity.AgentMetaData;
				agentMetaData.AsasnameFileName = await _fileHelper.UploadFile(model.MetaData.AsasnameFile, FileUploadDirectory.AgentAttachment, customer.Id.ToString());
			}
			if (model.MetaData?.RoznameRasmiFile != null)
			{
				if (model.MetaData.RoznameRasmiFile.Length == 0L)
				{
					return Error(0L, "فایل روزنامه رسمی خالی است.");
				}
				if (!_fileHelper.CheckFileIsImageOrPdfOrRarOrZipOrXls(model.MetaData.RoznameRasmiFile))
				{
					return Error(0L, "فقط فایل با فرمت تصویر و pdf و rar مجاز می باشد.");
				}
				AgentMetaData agentMetaData = entity.AgentMetaData;
				agentMetaData.RoznameRasmiFileName = await _fileHelper.UploadFile(model.MetaData.RoznameRasmiFile, FileUploadDirectory.AgentAttachment, customer.Id.ToString());
			}
			if (model.MetaData?.PledgeChequeFile != null)
			{
				if (model.MetaData.PledgeChequeFile.Length == 0L)
				{
					return Error(0L, "فایل روزنامه رسمی خالی است.");
				}
				if (!_fileHelper.CheckFileIsImageOrPdfOrRarOrZipOrXls(model.MetaData.PledgeChequeFile))
				{
					return Error(0L, "فقط فایل با فرمت تصویر و pdf و rar مجاز می باشد.");
				}
				AgentMetaData agentMetaData = entity.AgentMetaData;
				agentMetaData.PledgeChequeFileName = await _fileHelper.UploadFile(model.MetaData.PledgeChequeFile, FileUploadDirectory.AgentAttachment, customer.Id.ToString());
			}
			try
			{
				await _userManager.UpdateAsync(entity);
				List<AgentLoan> agentLoans = ((model.AgentLoans?.Any() ?? false) ? model.AgentLoans.Select((AgentLoanModel x) =>
				{
					AgentLoan agentLoan = new AgentLoan
					{
						Id = x.Id,
						AgentId = x.AgentId,
						CustomerId = _currentUserService.CustomerId.Value,
						LoanId = x.LoanId,
						Percent = x.Percent,
						DefaultCheckoutDays = x.DefaultCheckoutDays,
						ContactId = x.Contact?.Id,
						ContactPercent = x.ContactPercent,
						ProductPercent = x.ProductPercent,
						ProductCheckoutDays = x.ProductCheckoutDays
					};
					List<OtherPercent> otherPercents = x.OtherPercents;
					agentLoan.OtherPercentsJson = ((otherPercents != null && otherPercents.Any()) ? JsonSerializer.Serialize(x.OtherPercents) : null);
					return agentLoan;
				}).ToList() : new List<AgentLoan>());
				await _agentLoanService.Modify(agentLoans, entity.Id);
			}
			catch (Exception)
			{
				throw;
			}
			return Success(entity.Id, "عملیات با موفقیت انجام شد.");
		});
	}

	[HttpPost("[action]/{id}")]
	public Task<DefaultResponseState> Delete(long id)
	{
		return TryCatch(async () =>
		{
			if ((await _userLoanService.GetByAgentId(id)).Any())
			{
				return Error("این نماینده دارای وام می باشد و قابل حذف  نمی باشد. برای حذف نماینده ابتدا وام های ثبت شده را حذف نمایید.");
			}
			User user = await _agentService.GetById(id);
			if (user == null)
			{
				return Error("امکان حذف وجود ندارد. لطفا بعدا دوباره تلاش نمایید");
			}
			await _agentService.Delete(user);
			return Success();
		});
	}

	private string GenerateUserName(int customerCode, AgentModel model)
	{
		if (_appConfig.IsCloud)
		{
			return $"{customerCode}-n-{model.Mobile}";
		}
		return "n" + model.Mobile;
	}
}
