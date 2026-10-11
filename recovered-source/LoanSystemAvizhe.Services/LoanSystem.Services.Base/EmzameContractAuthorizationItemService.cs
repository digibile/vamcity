using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading.Tasks;
using LoanSystem.Core;
using LoanSystem.Core.Exceptions;
using LoanSystem.Data;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Models;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Services.FileHelper;
using LoanSystemAvizhe.Services.Models.Emzame;
using LoanSystemAvizhe.Services.ThirdParty;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Spm.Services.Infrastructure;

namespace LoanSystem.Services.Base;

public class EmzameContractAuthorizationItemService : BaseService, IEmzameContractAuthorizationItemService
{
	private record BankAccount(string AccountNumber, string Bank, string Branch);

	private readonly ICustomerService _customerService;

	private readonly IAppSettingsService _appSettingsService;

	private readonly IPdfHelper _pdfHelper;

	private readonly IAppSmsService _appSmsService;

	private readonly IHostingEnvironment _hostingEnvironment;

	public EmzameContractAuthorizationItemService(AppDbContext context, ICurrentUserService currentUserService, ICustomerService customerService, IAppSettingsService appSettingsService, IPdfHelper pdfHelper, IAppSmsService appSmsService, IHostingEnvironment hostingEnvironment)
		: base(context, currentUserService)
	{
		_customerService = customerService;
		_appSettingsService = appSettingsService;
		_pdfHelper = pdfHelper;
		_appSmsService = appSmsService;
		_hostingEnvironment = hostingEnvironment;
	}

	public Task<EmzameContractAuthorizationItem> GetById(Guid entityId, params string[] includs)
	{
		return GetById<EmzameContractAuthorizationItem>(entityId, includs);
	}

	public async Task<EmzameContractAuthorizationItem> GetByIdIncludeRelations(Guid entityId, params string[] includs)
	{
		return await (from x in Table<EmzameContractAuthorizationItem>().Include((EmzameContractAuthorizationItem x) => x.User).Include((EmzameContractAuthorizationItem x) => x.AuthorizationItem).Include((EmzameContractAuthorizationItem x) => x.UserLoanRequestAuthorizationItem)
				.ThenInclude((UserLoanRequestAuthorizationItem x) => x.UserLoanRequest)
				.ThenInclude((UserLoanRequest x) => x.Agent)
				.Include((EmzameContractAuthorizationItem x) => x.UserLoanRequestAuthorizationItem)
				.ThenInclude((UserLoanRequestAuthorizationItem x) => x.UserLoanRequest)
				.ThenInclude((UserLoanRequest x) => x.Loan)
				.Include((EmzameContractAuthorizationItem x) => x.UserLoanRequestAuthorizationItem)
				.ThenInclude((UserLoanRequestAuthorizationItem x) => x.UserLoanRequest)
				.ThenInclude((UserLoanRequest x) => x.User)
				.Include((EmzameContractAuthorizationItem x) => x.UserLoan)
				.ThenInclude((UserLoan x) => x.User)
				.Include((EmzameContractAuthorizationItem x) => x.UserLoan)
				.ThenInclude((UserLoan x) => x.Loan)
				.Include((EmzameContractAuthorizationItem x) => x.UserLoan)
				.ThenInclude((UserLoan x) => x.Agent)
			where x.Id == entityId
			select x).AsSplitQuery().FirstOrDefaultAsync();
	}

	public async Task<string> ReplaceDataByTags(EmzameContractAuthorizationItemModel model, Dictionary<string, string> manualTags, string pattern)
	{
		List<string> list = new List<string>
		{
			"{نام مشتری}", "{نام پدر مشتری}", "{کد ملی مشتری}", "{آدرس مشتری}", "{کدپستی}", "{شماره موبایل مشتری}", "{شماره حساب مشتری}", "{نام بانک مشتری}", "{نام فروشگاه}", "{نام تجاری نماینده}",
			"{کد نماینده}", "{آدرس نماینده}", "{شماره درخواست وام}", "{نام طرح وام}", "{مبلغ درخواست وام}", "{بازپرداخت با بهره}", "{تعداد اقساط}", "{مبلغ هر قسط}", "{مبلغ وام}", "{تاریخ درخواست}",
			"{تاریخ امروز}", "{نام و نام خانوادگی ضامن}", "{کد ملی ضامن}", "{شماره موبایل ضامن}"
		};
		foreach (string tag in list)
		{
			switch (tag)
			{
			case "{نام مشتری}":
				if (model.UserLoanRequestAuthorizationItem != null)
				{
					pattern = pattern.Replace(tag, model.UserLoanRequestAuthorizationItem?.UserLoanRequest?.MemberModel?.FullName);
				}
				else if (model.UserLoan != null)
				{
					pattern = pattern.Replace(tag, model.UserLoan?.MemberModel?.FullName);
				}
				break;
			case "{نام پدر مشتری}":
				if (model.UserLoanRequestAuthorizationItem != null)
				{
					pattern = pattern.Replace(tag, model.UserLoanRequestAuthorizationItem.UserLoanRequest?.MemberModel?.FatherName);
				}
				else if (model.UserLoan != null)
				{
					pattern = pattern.Replace(tag, model.UserLoan.MemberModel?.FatherName);
				}
				break;
			case "{کد ملی مشتری}":
				if (model.UserLoanRequestAuthorizationItem != null)
				{
					pattern = pattern.Replace(tag, model.UserLoanRequestAuthorizationItem?.UserLoanRequest?.MemberModel?.NationalCode);
				}
				else if (model.UserLoan != null)
				{
					pattern = pattern.Replace(tag, model.UserLoan.MemberModel?.NationalCode);
				}
				break;
			case "{آدرس مشتری}":
				if (model.UserLoanRequestAuthorizationItem != null)
				{
					pattern = pattern.Replace(tag, model.UserLoanRequestAuthorizationItem?.UserLoanRequest?.MemberModel?.Address);
				}
				else if (model.UserLoan != null)
				{
					pattern = pattern.Replace(tag, model.UserLoan.MemberModel?.Address);
				}
				break;
			case "{کدپستی}":
				if (model.UserLoanRequestAuthorizationItem != null)
				{
					pattern = pattern.Replace(tag, model.UserLoanRequestAuthorizationItem?.UserLoanRequest?.MemberModel?.PostalCode);
				}
				else if (model.UserLoan != null)
				{
					pattern = pattern.Replace(tag, model.UserLoan.MemberModel?.PostalCode);
				}
				break;
			case "{شماره موبایل مشتری}":
				if (model.UserLoanRequestAuthorizationItem != null)
				{
					pattern = pattern.Replace(tag, model.UserLoanRequestAuthorizationItem?.UserLoanRequest?.MemberModel?.Mobile);
				}
				else if (model.UserLoan != null)
				{
					pattern = pattern.Replace(tag, model.UserLoan.MemberModel?.Mobile);
				}
				break;
			case "{شماره حساب مشتری}":
				if (model.UserLoanRequestAuthorizationItem != null)
				{
					if (!string.IsNullOrWhiteSpace(model.UserLoanRequestAuthorizationItem?.UserLoanRequest?.MemberModel?.BankAccounts))
					{
						pattern = pattern.Replace(tag, JsonSerializer.Deserialize<List<BankAccount>>(model.UserLoanRequestAuthorizationItem?.UserLoanRequest?.MemberModel?.BankAccounts, new JsonSerializerOptions
						{
							PropertyNameCaseInsensitive = true
						})?.FirstOrDefault()?.AccountNumber);
					}
					else if (model.UserLoan != null && !string.IsNullOrWhiteSpace(model.UserLoan.MemberModel?.BankAccounts))
					{
						pattern = pattern.Replace(tag, JsonSerializer.Deserialize<List<BankAccount>>(model.UserLoan.MemberModel?.BankAccounts, new JsonSerializerOptions
						{
							PropertyNameCaseInsensitive = true
						})?.FirstOrDefault()?.AccountNumber);
					}
				}
				break;
			case "{نام بانک مشتری}":
				if (model.UserLoanRequestAuthorizationItem != null)
				{
					if (!string.IsNullOrWhiteSpace(model.UserLoanRequestAuthorizationItem?.UserLoanRequest?.MemberModel?.BankAccounts))
					{
						pattern = pattern.Replace(tag, JsonSerializer.Deserialize<List<BankAccount>>(model.UserLoanRequestAuthorizationItem?.UserLoanRequest?.MemberModel?.BankAccounts, new JsonSerializerOptions
						{
							PropertyNameCaseInsensitive = true
						})?.FirstOrDefault()?.Bank);
					}
					else if (model.UserLoan != null && !string.IsNullOrWhiteSpace(model.UserLoan.MemberModel?.BankAccounts))
					{
						pattern = pattern.Replace(tag, JsonSerializer.Deserialize<List<BankAccount>>(model.UserLoan.MemberModel?.BankAccounts, new JsonSerializerOptions
						{
							PropertyNameCaseInsensitive = true
						})?.FirstOrDefault()?.Bank);
					}
				}
				break;
			case "{نام فروشگاه}":
				pattern = pattern.Replace(tag, (await _customerService.GetById(_currentUserService.CustomerId.Value))?.Name);
				break;
			case "{نام تجاری نماینده}":
				if (model.UserLoanRequestAuthorizationItem != null)
				{
					pattern = pattern.Replace(tag, model.UserLoanRequestAuthorizationItem?.UserLoanRequest?.AgentModel?.BusinessName);
				}
				else if (model.UserLoan != null)
				{
					pattern = pattern.Replace(tag, model.UserLoan.AgentModel?.BusinessName);
				}
				break;
			case "{کد نماینده}":
				if (model.UserLoanRequestAuthorizationItem != null)
				{
					pattern = pattern.Replace(tag, model.UserLoanRequestAuthorizationItem?.UserLoanRequest?.AgentModel?.Mobile);
				}
				else if (model.UserLoan != null)
				{
					pattern = pattern.Replace(tag, model.UserLoan.AgentModel?.Mobile);
				}
				break;
			case "{آدرس نماینده}":
				if (model.UserLoanRequestAuthorizationItem != null)
				{
					pattern = pattern.Replace(tag, model.UserLoanRequestAuthorizationItem?.UserLoanRequest?.AgentModel?.Address);
				}
				else if (model.UserLoan != null)
				{
					pattern = pattern.Replace(tag, model.UserLoan.AgentModel?.Address);
				}
				break;
			case "{شماره درخواست وام}":
				if (model.UserLoanRequestAuthorizationItem != null)
				{
					pattern = pattern.Replace(tag, model.UserLoanRequestAuthorizationItem?.UserLoanRequest?.Code.ToString());
				}
				else if (model.UserLoan != null)
				{
					pattern = pattern.Replace(tag, model.UserLoan.Code.ToString());
				}
				break;
			case "{نام طرح وام}":
				if (model.UserLoanRequestAuthorizationItem != null)
				{
					pattern = pattern.Replace(tag, model.UserLoanRequestAuthorizationItem?.UserLoanRequest?.LoanModel?.Title);
				}
				else if (model.UserLoan != null)
				{
					pattern = pattern.Replace(tag, model.UserLoan.LoanModel?.Title);
				}
				break;
			case "{مبلغ وام}":
				if (model.UserLoanRequestAuthorizationItem != null)
				{
					pattern = pattern.Replace(tag, model.UserLoanRequestAuthorizationItem?.UserLoanRequest?.Amount.ToString("N0"));
				}
				else if (model.UserLoan != null)
				{
					pattern = pattern.Replace(tag, model.UserLoan.Amount.ToString("N0"));
				}
				break;
			case "{تعداد اقساط}":
				if (model.UserLoanRequestAuthorizationItem != null)
				{
					pattern = pattern.Replace(tag, model.UserLoanRequestAuthorizationItem?.UserLoanRequest?.InstallmentCount.ToString());
				}
				else if (model.UserLoan != null)
				{
					pattern = pattern.Replace(tag, model.UserLoan.Month.ToString());
				}
				break;
			case "{مبلغ هر قسط}":
				if (model.UserLoanRequestAuthorizationItem != null)
				{
					pattern = pattern.Replace(tag, model.UserLoanRequestAuthorizationItem?.UserLoanRequest?.Installment.ToString("N0"));
				}
				else if (model.UserLoan != null)
				{
					pattern = pattern.Replace(tag, model.UserLoan.Installment.ToString("N0"));
				}
				break;
			case "{تاریخ درخواست}":
				if (model.UserLoanRequestAuthorizationItem != null)
				{
					pattern = pattern.Replace(tag, model.UserLoanRequestAuthorizationItem?.UserLoanRequest?.RequestDate_Persian.Date);
				}
				else if (model.UserLoan != null)
				{
					pattern = pattern.Replace(tag, model.UserLoan.StartDate.ToPersianDateObject().Date);
				}
				break;
			case "{تاریخ امروز}":
				pattern = pattern.Replace(tag, DateTime.Now.ToPersianDateObject().Date);
				break;
			case "{نام و نام خانوادگی ضامن}":
				pattern = pattern.Replace(tag, model.User?.FullName);
				break;
			case "{کد ملی ضامن}":
				pattern = pattern.Replace(tag, model.User?.NationalCode);
				break;
			case "{شماره موبایل ضامن}":
				pattern = pattern.Replace(tag, model.User?.Mobile);
				break;
			}
		}
		if (manualTags == null || !manualTags.Any())
		{
			return pattern;
		}
		string text = "{manual:\"";
		string text2 = "\"}";
		int num;
		while ((num = pattern.IndexOf(text)) != -1)
		{
			int num2 = num + text.Length;
			int num3 = pattern.IndexOf(text2, num2);
			if (num3 == -1)
			{
				break;
			}
			string key = pattern.Substring(num2, num3 - num2);
			string text3 = (manualTags.ContainsKey(key) ? manualTags[key] : "");
			pattern = pattern.Substring(0, num) + text3 + pattern.Substring(num3 + text2.Length);
		}
		return pattern;
	}

	public Task<List<EmzameContractAuthorizationItem>> GetByCondition(Expression<Func<EmzameContractAuthorizationItem, bool>> condition, params string[] includs)
	{
		IQueryable<EmzameContractAuthorizationItem> source = Table<EmzameContractAuthorizationItem>();
		foreach (string navigationPropertyPath in includs)
		{
			source = source.Include(navigationPropertyPath);
		}
		source = source.Where(condition);
		return source.AsSplitQuery().ToListAsync();
	}

	public Task<int> GetCountByCondition(Expression<Func<EmzameContractAuthorizationItem, bool>> condition)
	{
		return Table<EmzameContractAuthorizationItem>().Where(condition).CountAsync();
	}

	public async Task Insert(EmzameContractAuthorizationItem entity)
	{
		entity.CustomerId = _currentUserService.CustomerId.Value;
		Insert<EmzameContractAuthorizationItem>(entity);
		await Save();
	}

	public async Task InsertBatch(List<EmzameContractAuthorizationItem> entities)
	{
		entities.ForEach((EmzameContractAuthorizationItem x) =>
		{
			x.CustomerId = _currentUserService.CustomerId.Value;
		});
		_context.EmzameContractAuthorizationItems.AddRange(entities);
		try
		{
			await Save();
		}
		catch (Exception)
		{
			throw;
		}
	}

	public Task Update(EmzameContractAuthorizationItem entity)
	{
		entity.CustomerId = _currentUserService.CustomerId.Value;
		Update(entity, true);
		return Save();
	}

	public Task Delete(EmzameContractAuthorizationItem entity)
	{
		Delete<EmzameContractAuthorizationItem>(entity);
		return Save();
	}

	public async Task SendContractToEmzame(EmzameContractAuthorizationItemModel mdl, string contractTitle, string contractDescription)
	{
		if (mdl.Contract != null)
		{
			throw new CustomException("این قرارداد قبلا با شناسه " + mdl.Contract.id + " در سامانه امضامی ثبت شده است.");
		}
		if (string.IsNullOrWhiteSpace(mdl.Comment))
		{
			throw new CustomException("متن قرارداد خالی است.");
		}
		AppSettings appSettings = await _appSettingsService.GetAppSettings();
		if (string.IsNullOrWhiteSpace(appSettings?.EmzameConfigJson))
		{
			throw new CustomException("تنظیمات امضامی ثبت نشده است.");
		}
		AppSettingsModel appSettingsModel = new AppSettingsModel(appSettings);
		if (string.IsNullOrWhiteSpace(appSettingsModel?.EmzameConfig?.Password) || string.IsNullOrWhiteSpace(appSettingsModel?.EmzameConfig?.NationalCode))
		{
			throw new CustomException("تنظیمات امضامی ثبت نشده است.");
		}
		EmzameService emzameService = new EmzameService(appSettingsModel.EmzameConfig);
		string path = Directories.EmzameContract(_currentUserService.CustomerId.ToString());
		string contentRootPath = _hostingEnvironment.ContentRootPath;
		string text = contractTitle + ".pdf";
		if (!Directory.Exists(Path.Combine(contentRootPath, path)))
		{
			Directory.CreateDirectory(Path.Combine(contentRootPath, path));
		}
		string text2 = Path.Combine(contentRootPath, path, text);
		_ = "<html><head><meta charset='utf-8' /><style> body { font-family: tahoma;direction:rtl; } </style></head><body><div style=\"direction: rtl;font-family: tahoma\">" + mdl.Comment + "</div></body></html>";
		_pdfHelper.ConvertHtmlToPdfAsync(mdl.Comment, text2);
		if (!File.Exists(text2))
		{
			throw new CustomException("فایل pdf قرارداد یافت نشد.");
		}
		using FileStream fileStream = File.OpenRead(text2);
		SendEmzameContractResult sendContractRes = await emzameService.SendContractAsync(new SendEmzameContractRequest
		{
			CollaboratorList = new List<SendEmzameContractCollaborator>
			{
				new SendEmzameContractCollaborator
				{
					NationalCode = mdl.User.NationalCode,
					Order = 1
				}
			},
			Title = contractTitle,
			Description = contractDescription,
			FileDescription = "",
			FileName = text,
			IsBlankPageRequire = false,
			IsElectronicSignatureRequired = false,
			IsSignatureDescriptionRequired = true,
			ShouldEveryPageHaveSignatureImage = false,
			ExpireTime = DateTime.Now.AddDays(30.0).Ticks / 10000
		}, fileStream, _hostingEnvironment.EnvironmentName == "Development");
		if (sendContractRes.errorCode == 0)
		{
			EmzameContractAuthorizationItem emzameContractAuthorizationItem = await GetById(mdl.Id);
			emzameContractAuthorizationItem.ContractJson = JsonSerializer.Serialize(sendContractRes);
			await Update(emzameContractAuthorizationItem);
		}
	}

	public async Task<SendEmzameContractResult> GetContractFromEmzame(Guid id)
	{
		EmzameContractAuthorizationItem entity = await GetById(id, "User");
		if (entity == null)
		{
			throw new CustomException("آیتم قرارداد یافت نشد.");
		}
		EmzameContractAuthorizationItemModel mdl = new EmzameContractAuthorizationItemModel(entity);
		if (mdl.Contract == null)
		{
			throw new CustomException("اطلاعات قرارداد یافت نشد.");
		}
		if (string.IsNullOrWhiteSpace(mdl.Contract._id))
		{
			throw new CustomException("شناسه قرارداد خالی است.");
		}
		AppSettings appSettings = await _appSettingsService.GetAppSettings();
		if (string.IsNullOrWhiteSpace(appSettings?.EmzameConfigJson))
		{
			throw new CustomException("تنظیمات امضامی ثبت نشده است.");
		}
		AppSettingsModel appSettingsModel = new AppSettingsModel(appSettings);
		if (string.IsNullOrWhiteSpace(appSettingsModel?.EmzameConfig?.Password) || string.IsNullOrWhiteSpace(appSettingsModel?.EmzameConfig?.NationalCode))
		{
			throw new CustomException("تنظیمات امضامی ثبت نشده است.");
		}
		SendEmzameContractResult emzameContract = await new EmzameService(appSettingsModel.EmzameConfig).GetContractAsync(mdl.Contract._id, _hostingEnvironment.EnvironmentName == "Development");
		if (emzameContract == null)
		{
			throw new CustomException("قرارداد در سامانه امضامی یافت نشد.");
		}
		if (emzameContract.errorCode == 0)
		{
			entity.ContractJson = JsonSerializer.Serialize(emzameContract);
			await Update(entity);
		}
		return emzameContract;
	}
}
