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

public class UserLoanRequestAuthorizationItemService : BaseService, IUserLoanRequestAuthorizationItemService
{
	private record BankAccount(string AccountNumber, string Bank, string Branch);

	private readonly ICustomerService _customerService;

	private readonly IAppSmsService _appSmsService;

	public UserLoanRequestAuthorizationItemService(AppDbContext context, ICurrentUserService currentUserService, ICustomerService customerService, IAppSmsService appSmsService)
		: base(context, currentUserService)
	{
		_customerService = customerService;
		_appSmsService = appSmsService;
	}

	public Task<UserLoanRequestAuthorizationItem> GetById(Guid entityId, params string[] includs)
	{
		return GetById<UserLoanRequestAuthorizationItem>(entityId, includs);
	}

	public async Task<string> ReplaceDataByTags(UserLoanRequestAuthorizationItemModel model, Dictionary<string, string> manualTags, string pattern)
	{
		List<string> list = new List<string>
		{
			"{نام مشتری}", "{نام پدر مشتری}", "{کد ملی مشتری}", "{آدرس مشتری}", "{شماره موبایل مشتری}", "{شماره حساب مشتری}", "{نام بانک مشتری}", "{نام فروشگاه}", "{نام تجاری نماینده}", "{کد نماینده}",
			"{آدرس نماینده}", "{شماره درخواست وام}", "{نام طرح وام}", "{مبلغ درخواست وام}", "{بازپرداخت با بهره}", "{تعداد اقساط}", "{مبلغ هر قسط}", "{تاریخ درخواست}", "{تاریخ امروز}"
		};
		foreach (string tag in list)
		{
			switch (tag)
			{
			case "{نام مشتری}":
				pattern = pattern.Replace(tag, model.UserLoanRequest?.MemberModel?.FullName);
				break;
			case "{نام پدر مشتری}":
				pattern = pattern.Replace(tag, model.UserLoanRequest?.MemberModel?.FatherName);
				break;
			case "{کد ملی مشتری}":
				pattern = pattern.Replace(tag, model.UserLoanRequest?.MemberModel?.NationalCode);
				break;
			case "{آدرس مشتری}":
				pattern = pattern.Replace(tag, model.UserLoanRequest?.MemberModel?.Address);
				break;
			case "{شماره موبایل مشتری}":
				pattern = pattern.Replace(tag, model.UserLoanRequest?.MemberModel?.Mobile);
				break;
			case "{شماره حساب مشتری}":
				if (!string.IsNullOrWhiteSpace(model.UserLoanRequest?.MemberModel?.BankAccounts))
				{
					pattern = pattern.Replace(tag, JsonSerializer.Deserialize<List<BankAccount>>(model.UserLoanRequest?.MemberModel?.BankAccounts, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					})?.FirstOrDefault()?.AccountNumber);
				}
				break;
			case "{نام بانک مشتری}":
				if (!string.IsNullOrWhiteSpace(model.UserLoanRequest?.MemberModel?.BankAccounts))
				{
					pattern = pattern.Replace(tag, JsonSerializer.Deserialize<List<BankAccount>>(model.UserLoanRequest?.MemberModel?.BankAccounts, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					})?.FirstOrDefault()?.Bank);
				}
				break;
			case "{نام فروشگاه}":
				pattern = pattern.Replace(tag, (await _customerService.GetById(_currentUserService.CustomerId.Value))?.Name);
				break;
			case "{نام تجاری نماینده}":
				pattern = pattern.Replace(tag, model.UserLoanRequest?.AgentModel?.BusinessName);
				break;
			case "{کد نماینده}":
				pattern = pattern.Replace(tag, model.UserLoanRequest?.AgentModel?.Mobile);
				break;
			case "{آدرس نماینده}":
				pattern = pattern.Replace(tag, model.UserLoanRequest?.AgentModel?.Address);
				break;
			case "{شماره درخواست وام}":
				pattern = pattern.Replace(tag, model.UserLoanRequest?.Code.ToString());
				break;
			case "{نام طرح وام}":
				pattern = pattern.Replace(tag, model.UserLoanRequest?.LoanModel?.Title);
				break;
			case "{مبلغ وام}":
				pattern = pattern.Replace(tag, model.UserLoanRequest?.Amount.ToString("N0"));
				break;
			case "{تعداد اقساط}":
				pattern = pattern.Replace(tag, model.UserLoanRequest?.InstallmentCount.ToString());
				break;
			case "{مبلغ هر قسط}":
				pattern = pattern.Replace(tag, model.UserLoanRequest?.Installment.ToString("N0"));
				break;
			case "{تاریخ درخواست}":
				pattern = pattern.Replace(tag, model.UserLoanRequest?.RequestDate_Persian.Date);
				break;
			case "{تاریخ امروز}":
				pattern = pattern.Replace(tag, DateTime.Now.ToPersianDateObject().Date);
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
			string text3 = (manualTags.ContainsKey(key) ? manualTags[key] : pattern.Substring(num, num3 + text2.Length - num));
			pattern = pattern.Substring(0, num) + text3 + pattern.Substring(num3 + text2.Length);
		}
		return pattern;
	}

	public async Task<string> ValidateOtpAndSignContract(UserLoanRequestAuthorizationItem authorizeItem, string otp)
	{
		if (authorizeItem.Status == LoanRequestAuthorizationStatus.تایید_شده_توسط_مدیر)
		{
			return "قرارداد قبلا امضا شده است.";
		}
		string mobile = authorizeItem?.UserLoanRequest?.User?.Mobile;
		if (string.IsNullOrWhiteSpace(mobile))
		{
			return "شماره موبایل مشتری یافت نشد.";
		}
		List<AppSms> list = await _appSmsService.GetByCondition((AppSms x) => x.To == mobile && (int)x.AppSmsType == 18 && x.Date >= DateTime.Now.AddMinutes(-2.0) && x.Tag == otp, false);
		if (list == null || !list.Any())
		{
			return "کد تائیدیه صحیح نمی باشد.";
		}
		authorizeItem.Status = LoanRequestAuthorizationStatus.تایید_شده_توسط_مدیر;
		authorizeItem.LastChangeDate = DateTime.Now;
		authorizeItem.Tag = "این قرارداد در تاریخ <b>" + DateTime.Now.ToPersianDateObject().Date + "</b> در ساعت <b>" + DateTime.Now.ToPersianDateObject().Time + "</b> توسط آقای/خانم <b>" + authorizeItem?.UserLoanRequest?.User?.FullName + "</b> با کد ملی <b>" + authorizeItem?.UserLoanRequest?.User?.NationalCode + "</b> به عنوان خریدار از طریق امضای الکترونیکی مبتنی بر رمز یکبار مصرف (OTP) ارسال شده به شماره تلفن همراه <b>" + mobile + "</b> متعلق به امضا کننده با علم و اراده و مطالعه تمامی صفحات قرارداد امضا و تایید گردید. این امضا وفق قانون تجارت الکترونیکی معتبر و لازم الاجرا است.";
		Update(authorizeItem, true);
		await Save();
		return "";
	}

	public async Task<string> ValidateOtpAndVerify(UserLoanRequestAuthorizationItem entity, long memberId, string otp)
	{
		User member = await _context.Users.FirstOrDefaultAsync((User x) => x.Id == memberId);
		if (member == null)
		{
			return "مشتری یافت نشد";
		}
		List<AppSms> list = await _appSmsService.GetByCondition((AppSms x) => x.To == member.Mobile && (int)x.AppSmsType == 8 && x.Date >= DateTime.Now.AddMinutes(-2.0) && x.Tag == otp, false);
		if (list == null || !list.Any())
		{
			return "کد تائیدیه صحیح نمی باشد.";
		}
		member.MobileVerified = true;
		entity.Status = LoanRequestAuthorizationStatus.تایید_شده_توسط_مدیر;
		entity.LastChangeDate = DateTime.Now;
		Update(entity, true);
		await _context.SaveChangesAsync();
		return "";
	}

	public Task<List<UserLoanRequestAuthorizationItem>> GetByCondition(Expression<Func<UserLoanRequestAuthorizationItem, bool>> condition, params string[] includs)
	{
		IQueryable<UserLoanRequestAuthorizationItem> source = Table<UserLoanRequestAuthorizationItem>();
		foreach (string navigationPropertyPath in includs)
		{
			source = source.Include(navigationPropertyPath);
		}
		source = source.Where(condition);
		return source.AsSplitQuery().ToListAsync();
	}

	public async Task Insert(UserLoanRequestAuthorizationItem entity)
	{
		entity.CustomerId = _currentUserService.CustomerId.Value;
		Insert<UserLoanRequestAuthorizationItem>(entity);
		await Save();
	}

	public async Task InsertBatch(List<UserLoanRequestAuthorizationItem> entities)
	{
		entities.ForEach((UserLoanRequestAuthorizationItem x) =>
		{
			x.CustomerId = _currentUserService.CustomerId.Value;
		});
		_context.UserLoanRequestAuthorizationItems.AddRange(entities);
		try
		{
			await Save();
		}
		catch (Exception)
		{
			throw;
		}
	}

	public Task Update(UserLoanRequestAuthorizationItem entity)
	{
		entity.CustomerId = _currentUserService.CustomerId.Value;
		Update(entity, true);
		return Save();
	}

	public Task Delete(UserLoanRequestAuthorizationItem entity)
	{
		Delete<UserLoanRequestAuthorizationItem>(entity);
		return Save();
	}
}
