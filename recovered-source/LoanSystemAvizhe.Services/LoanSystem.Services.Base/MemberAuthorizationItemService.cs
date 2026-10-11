using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoanSystem.Data;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Models;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Data.Enums;
using LoanSystemAvizhe.Services.Inquiry.Models;
using Microsoft.EntityFrameworkCore;
using Spm.Services.Infrastructure;

namespace LoanSystem.Services.Base;

public class MemberAuthorizationItemService : BaseService, IMemberAuthorizationItemService
{
	private record BankAccount(string AccountNumber, string Bank, string Branch);

	private readonly ICustomerService _customerService;

	private readonly IAppSmsService _appSmsService;

	private readonly IntegratedInquiryService _integratedInquiryService;

	private readonly IBusinessAuthorizationItemService _businessAuthorizationItemService;

	public MemberAuthorizationItemService(AppDbContext context, ICurrentUserService currentUserService, ICustomerService customerService, IAppSmsService appSmsService, IntegratedInquiryService integratedInquiryService, IBusinessAuthorizationItemService businessAuthorizationItemService)
		: base(context, currentUserService)
	{
		_customerService = customerService;
		_appSmsService = appSmsService;
		_integratedInquiryService = integratedInquiryService;
		_businessAuthorizationItemService = businessAuthorizationItemService;
	}

	public Task<MemberAuthorizationItem> GetById(Guid entityId, params string[] includs)
	{
		return GetById<MemberAuthorizationItem>(entityId, includs);
	}

	public Task<List<MemberAuthorizationItem>> GetByCondition(Expression<Func<MemberAuthorizationItem, bool>> condition, params string[] includs)
	{
		IQueryable<MemberAuthorizationItem> source = Table<MemberAuthorizationItem>();
		foreach (string navigationPropertyPath in includs)
		{
			source = source.Include(navigationPropertyPath);
		}
		source = source.Where(condition);
		return source.AsSplitQuery().ToListAsync();
	}

	public async Task Insert(MemberAuthorizationItem entity)
	{
		entity.CustomerId = _currentUserService.CustomerId.Value;
		Insert<MemberAuthorizationItem>(entity);
		await Save();
	}

	public async Task InsertBatch(List<MemberAuthorizationItem> entities)
	{
		entities.ForEach((MemberAuthorizationItem x) =>
		{
			x.CustomerId = _currentUserService.CustomerId.Value;
		});
		_context.MemberAuthorizationItems.AddRange(entities);
		try
		{
			await Save();
		}
		catch (Exception)
		{
			throw;
		}
	}

	public Task Update(MemberAuthorizationItem entity)
	{
		entity.CustomerId = _currentUserService.CustomerId.Value;
		Update(entity, true);
		return Save();
	}

	public Task Delete(MemberAuthorizationItem entity)
	{
		Delete<MemberAuthorizationItem>(entity);
		return Save();
	}

	public async Task<string> ValidateOtpAndVerify(MemberAuthorizationItem entity, long memberId, string otp)
	{
		if (entity.MemberId != memberId)
		{
			return "شناسه مشتری معتبر نمی باشد.";
		}
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

	public async Task<string> InquiryRequest(InquiryRequestModel inquiryRequestModel)
	{
		MemberAuthorizationItem entity = await GetById(inquiryRequestModel.MemberAuthorizationItemId.Value);
		if (entity == null)
		{
			return "آیتم احراز هویت یافت نشد.";
		}
		BusinessAuthorizationItem businessAuth = (await _businessAuthorizationItemService.GetByCondition((BusinessAuthorizationItem x) => x.AuthorizationItemId == entity.AuthorizationItemId)).FirstOrDefault();
		if (businessAuth == null)
		{
			return "گام احراز هویت سیستمی یافت نشد.";
		}
		User member = await _context.Users.FirstOrDefaultAsync((User x) => x.Id == inquiryRequestModel.MemberId);
		if (businessAuth == null)
		{
			return "مشتری یافت نشد.";
		}
		if (member.Credit < businessAuth.Cost)
		{
			return "اعتبار کیف پول کافی نمی باشد. حداقل اعتبار لازم برای این استعلام " + businessAuth.Cost.ToString("N0") + " تومان می باشد.";
		}
		AppInquiry appInquiry = await _integratedInquiryService.InquiryRequest(inquiryRequestModel);
		if (appInquiry != null && appInquiry.IsSuccess)
		{
			if (new AppInquiryModel(appInquiry).AcceptanceDetection())
			{
				entity.Status = LoanRequestAuthorizationStatus.تایید_شده_توسط_مدیر;
			}
			else
			{
				entity.Status = LoanRequestAuthorizationStatus.در_انتطار_بررسی_کارشناس;
			}
			Update(entity, true);
			appInquiry.MemberPrice = businessAuth.Cost;
			Update(appInquiry, true);
			member.Credit -= businessAuth.Cost;
			await _context.SaveChangesAsync();
		}
		return "";
	}

	public async Task<List<MemberAuthorizationItemModel>> SyncAndGetMemberAuthorizationsItems(long memberId)
	{
		List<BusinessAuthorizationItem> businessItems = await _businessAuthorizationItemService.GetByCondition((BusinessAuthorizationItem x) => x.CustomerId == _currentUserService.CustomerId, "AuthorizationItem");
		List<MemberAuthorizationItem> memberAuthorizations = await GetByCondition((MemberAuthorizationItem x) => x.MemberId == memberId, "AuthorizationItem", "Attachments");
		User member = await _context.Users.FirstOrDefaultAsync((User x) => x.Id == memberId);
		List<MemberAuthorizationItemModel> result = new List<MemberAuthorizationItemModel>();
		foreach (BusinessAuthorizationItem businessItem in businessItems)
		{
			MemberAuthorizationItem ma = memberAuthorizations.FirstOrDefault((MemberAuthorizationItem x) => x.AuthorizationItemId == businessItem.AuthorizationItemId);
			if (ma == null)
			{
				MemberAuthorizationItem memberAuthorizationItem = new MemberAuthorizationItem
				{
					CreateDate = DateTime.Now
				};
				MemberAuthorizationItem memberAuthorizationItem2 = memberAuthorizationItem;
				memberAuthorizationItem2.LastChangeBy = (await _context.Users.AsNoTracking().FirstOrDefaultAsync((User x) => (long?)x.Id == _currentUserService.UserId))?.FullName;
				memberAuthorizationItem.AuthorizationItemId = businessItem.AuthorizationItemId;
				memberAuthorizationItem.Status = LoanRequestAuthorizationStatus.در_انتظار_ثبت_توسط_کاربر;
				memberAuthorizationItem.MemberId = memberId;
				memberAuthorizationItem.CustomerId = _currentUserService.CustomerId.Value;
				memberAuthorizationItem.MemberDescription = "";
				ma = memberAuthorizationItem;
				await Insert(ma);
			}
			if (businessItem.AuthorizationItem.SystemType == AuthorizationItemSystemType.MobileVerification && ma.Status != LoanRequestAuthorizationStatus.تایید_شده_توسط_مدیر && member.MobileVerified)
			{
				ma.Status = LoanRequestAuthorizationStatus.تایید_شده_توسط_مدیر;
				Update(ma, true);
			}
			if (new List<AuthorizationItemSystemType>
			{
				AuthorizationItemSystemType.Shahkar1,
				AuthorizationItemSystemType.Shahkar2,
				AuthorizationItemSystemType.Sana,
				AuthorizationItemSystemType.UnpaidCheque
			}.Contains(businessItem.AuthorizationItem.SystemType) && ma.Status != LoanRequestAuthorizationStatus.تایید_شده_توسط_مدیر)
			{
				AppInquiryType? appInquiryType = AppInquiryModel.MapAuthorizationItemSystemTypeToInquiryType(businessItem.AuthorizationItem.SystemType);
				if (!appInquiryType.HasValue)
				{
					continue;
				}
				AppInquiry appInquiry = await (from x in _context.AppInquirys
					where x.MemberId == (long?)memberId && (int?)x.AppInquiryType == (int?)appInquiryType
					orderby x.Date descending
					select x).FirstOrDefaultAsync();
				ma.Status = ((appInquiry != null) ? (new AppInquiryModel(appInquiry).AcceptanceDetection() ? LoanRequestAuthorizationStatus.تایید_شده_توسط_مدیر : LoanRequestAuthorizationStatus.در_انتطار_بررسی_کارشناس) : LoanRequestAuthorizationStatus.در_انتظار_ثبت_توسط_کاربر);
				Update(ma, true);
			}
			result.Add(new MemberAuthorizationItemModel(ma));
		}
		await _context.SaveChangesAsync();
		return result;
	}
}
