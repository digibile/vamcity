using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading.Tasks;
using LoanSystem.Core;
using LoanSystem.Data;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Models;
using LoanSystemAvizhe.Core;
using Microsoft.EntityFrameworkCore;
using Spm.Services.Infrastructure;

namespace LoanSystem.Services.Base;

public class ThirdPersonAssuranceAuthorizationItemService : BaseService, IThirdPersonAssuranceAuthorizationItemService
{
	private record BankAccount(string AccountNumber, string Bank, string Branch);

	private readonly ICustomerService _customerService;

	private readonly IAppSmsService _appSmsService;

	public ThirdPersonAssuranceAuthorizationItemService(AppDbContext context, ICurrentUserService currentUserService, ICustomerService customerService, IAppSmsService appSmsService)
		: base(context, currentUserService)
	{
		_customerService = customerService;
		_appSmsService = appSmsService;
	}

	public Task<ThirdPersonAssuranceAuthorizationItem> GetById(Guid entityId, params string[] includs)
	{
		return GetById<ThirdPersonAssuranceAuthorizationItem>(entityId, includs);
	}

	public async Task<ThirdPersonAssuranceAuthorizationItem> GetByIdIncludeRelations(Guid entityId, params string[] includs)
	{
		return await Table<ThirdPersonAssuranceAuthorizationItem>().Include((ThirdPersonAssuranceAuthorizationItem x) => x.User).Include((ThirdPersonAssuranceAuthorizationItem x) => x.AuthorizationItem).Include((ThirdPersonAssuranceAuthorizationItem x) => x.UserLoanRequestAuthorizationItem)
			.ThenInclude((UserLoanRequestAuthorizationItem x) => x.UserLoanRequest)
			.ThenInclude((UserLoanRequest x) => x.Agent)
			.Include((ThirdPersonAssuranceAuthorizationItem x) => x.UserLoanRequestAuthorizationItem)
			.ThenInclude((UserLoanRequestAuthorizationItem x) => x.UserLoanRequest)
			.ThenInclude((UserLoanRequest x) => x.Loan)
			.Include((ThirdPersonAssuranceAuthorizationItem x) => x.UserLoanRequestAuthorizationItem)
			.ThenInclude((UserLoanRequestAuthorizationItem x) => x.UserLoanRequest)
			.ThenInclude((UserLoanRequest x) => x.User)
			.Include((ThirdPersonAssuranceAuthorizationItem x) => x.UserLoan)
			.ThenInclude((UserLoan x) => x.User)
			.Include((ThirdPersonAssuranceAuthorizationItem x) => x.UserLoan)
			.ThenInclude((UserLoan x) => x.Loan)
			.Include((ThirdPersonAssuranceAuthorizationItem x) => x.UserLoan)
			.ThenInclude((UserLoan x) => x.Agent)
			.AsSplitQuery()
			.FirstOrDefaultAsync();
	}

	public async Task<string> ReplaceDataByTags(ThirdPersonAssuranceAuthorizationItemModel model, Dictionary<string, string> manualTags, string pattern)
	{
		List<string> list = new List<string>
		{
			"{نام مشتری}", "{نام پدر مشتری}", "{کد ملی مشتری}", "{آدرس مشتری}", "{شماره موبایل مشتری}", "{شماره حساب مشتری}", "{نام بانک مشتری}", "{نام فروشگاه}", "{نام تجاری نماینده}", "{کد نماینده}",
			"{آدرس نماینده}", "{شماره درخواست وام}", "{نام طرح وام}", "{مبلغ درخواست وام}", "{بازپرداخت با بهره}", "{تعداد اقساط}", "{مبلغ هر قسط}", "{تاریخ درخواست}", "{تاریخ امروز}", "{نام و نام خانوادگی ضامن}",
			"{کد ملی ضامن}", "{شماره موبایل ضامن}"
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

	public async Task<string> ValidateOtpAndSignContract(ThirdPersonAssuranceAuthorizationItem authorizeItem, string otp)
	{
		if (authorizeItem.IsSigned)
		{
			return "قرارداد قبلا امضا شده است.";
		}
		string mobile = authorizeItem?.User?.Mobile;
		if (string.IsNullOrWhiteSpace(mobile))
		{
			return "شماره موبایل ضامن یافت نشد.";
		}
		List<AppSms> list = await _appSmsService.GetByCondition((AppSms x) => x.To == mobile && (int)x.AppSmsType == 24 && x.Date >= DateTime.Now.AddMinutes(-2.0) && x.Tag == otp, false);
		if (list == null || !list.Any())
		{
			return "کد تائیدیه صحیح نمی باشد.";
		}
		authorizeItem.IsSigned = true;
		authorizeItem.LastChangeDate = DateTime.Now;
		authorizeItem.Tag = "این قرارداد در تاریخ <b>" + DateTime.Now.ToPersianDateObject().Date + "</b> در ساعت <b>" + DateTime.Now.ToPersianDateObject().Time + "</b> توسط آقای/خانم <b>" + authorizeItem?.User?.FullName + "</b> با کد ملی <b>" + authorizeItem?.User?.NationalCode + "</b> به عنوان ضامن از طریق امضای الکترونیکی مبتنی بر رمز یکبار مصرف (OTP) ارسال شده به شماره تلفن همراه <b>" + mobile + "</b> متعلق به امضا کننده با علم و اراده و مطالعه تمامی صفحات قرارداد امضا و تایید گردید. این امضا وفق قانون تجارت الکترونیکی معتبر و لازم الاجرا است.";
		Update(authorizeItem, true);
		await Save();
		return "";
	}

	public Task<List<ThirdPersonAssuranceAuthorizationItem>> GetByCondition(Expression<Func<ThirdPersonAssuranceAuthorizationItem, bool>> condition, params string[] includs)
	{
		IQueryable<ThirdPersonAssuranceAuthorizationItem> source = Table<ThirdPersonAssuranceAuthorizationItem>();
		foreach (string navigationPropertyPath in includs)
		{
			source = source.Include(navigationPropertyPath);
		}
		source = source.Where(condition);
		return source.AsSplitQuery().ToListAsync();
	}

	public Task<int> GetCountByCondition(Expression<Func<ThirdPersonAssuranceAuthorizationItem, bool>> condition)
	{
		return Table<ThirdPersonAssuranceAuthorizationItem>().Where(condition).CountAsync();
	}

	public async Task Insert(ThirdPersonAssuranceAuthorizationItem entity)
	{
		entity.CustomerId = _currentUserService.CustomerId.Value;
		Insert<ThirdPersonAssuranceAuthorizationItem>(entity);
		await Save();
	}

	public async Task InsertBatch(List<ThirdPersonAssuranceAuthorizationItem> entities)
	{
		entities.ForEach((ThirdPersonAssuranceAuthorizationItem x) =>
		{
			x.CustomerId = _currentUserService.CustomerId.Value;
		});
		_context.ThirdPersonAssuranceAuthorizationItems.AddRange(entities);
		try
		{
			await Save();
		}
		catch (Exception)
		{
			throw;
		}
	}

	public Task Update(ThirdPersonAssuranceAuthorizationItem entity)
	{
		entity.CustomerId = _currentUserService.CustomerId.Value;
		Update(entity, true);
		return Save();
	}

	public Task Delete(ThirdPersonAssuranceAuthorizationItem entity)
	{
		Delete<ThirdPersonAssuranceAuthorizationItem>(entity);
		return Save();
	}
}
