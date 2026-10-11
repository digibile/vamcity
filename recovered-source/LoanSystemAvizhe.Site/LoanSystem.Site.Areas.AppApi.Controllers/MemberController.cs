using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Core.Configuration;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Users;
using LoanSystem.Site.Areas.AppApi.Models;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Services.Models.DynamicQueryModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NikanBase.Services.Security;

namespace LoanSystem.Site.Areas.AppApi.Controllers;

[Route("[area]/[controller]")]
public class MemberController : BaseApiController
{
	private readonly IMemberService _memberService;

	private readonly ICurrentUserService _currentUserService;

	private readonly UserManager _userManager;

	private readonly ICustomerService _customerService;

	private readonly IEncryptionService _encryptionService;

	private readonly AppConfig _appConfig;

	public MemberController(ILogger<MemberController> logger, IOptions<AppConfig> appConfig, IMemberService memberService, ICurrentUserService currentUserService, UserManager userManager, ICustomerService customerService, IEncryptionService encryptionService)
		: base(logger)
	{
		_memberService = memberService;
		_currentUserService = currentUserService;
		_appConfig = appConfig.Value;
		_userManager = userManager;
		_customerService = customerService;
		_encryptionService = encryptionService;
		if (!_currentUserService.TrySetCustomerIdFromToken())
		{
			throw new Exception("Token is not valid, please try get token again.");
		}
	}

	[HttpPost("[action]")]
	public Task<ResponseState<MemberListApiResultModel>> List([FromBody] DynamicQueryForApiModel query)
	{
		return TryCatch(async () =>
		{
			IQueryable<User> memberQuery = await _memberService.AsQueryable();
			if (!string.IsNullOrWhiteSpace(query.NationalCode))
			{
				memberQuery = memberQuery.Where((User x) => x.NationalCode.Contains(query.NationalCode));
			}
			if (!string.IsNullOrWhiteSpace(query.Email))
			{
				memberQuery = memberQuery.Where((User x) => x.Email.ToLower().Contains(query.Email.ToLower()));
			}
			if (!string.IsNullOrWhiteSpace(query.Mobile))
			{
				memberQuery = memberQuery.Where((User x) => x.Mobile.Contains(query.Mobile) || x.PhoneNumber.Contains(query.Mobile));
			}
			int countAll = await memberQuery.CountAsync();
			memberQuery = memberQuery.OrderByDescending((User x) => x.Id).Skip((query.Page - 1) * query.ItemsPerPage).Take(query.ItemsPerPage);
			List<AppApiMemberModel> members = (from x in await memberQuery.ToListAsync()
				select new AppApiMemberModel
				{
					Id = x.Id,
					UserName = x.UserName,
					FirstName = x.FirstName,
					LastName = x.LastName,
					Mobile = x.Mobile,
					Email = x.Email,
					NationalCode = x.NationalCode,
					Address = x.Address,
					FatherName = x.FatherName,
					IsActive = x.IsActive,
					PostalCode = x.PostalCode
				} into x
				orderby x.Id descending
				select x).ToList();
			MemberListApiResultModel data = new MemberListApiResultModel
			{
				Members = members,
				CountAll = countAll
			};
			return Success(data);
		});
	}

	[HttpPost("[action]")]
	public Task<ResponseState<long>> Create([FromBody] AppApiMemberModel model)
	{
		return TryCatch(async () =>
		{
			Customer customer = await _customerService.GetById(_currentUserService.CustomerId.Value);
			if (string.IsNullOrEmpty(model.NationalCode))
			{
				return Error(0L, "کد ملی وارد نشده است.");
			}
			model.UserName = GenerateUserName(customer.Code, model);
			if (await _userManager.FindByUserName(model.UserName) != null)
			{
				return Error(0L, "نام کاربری وارد شده تکراری است.");
			}
			User user = await _memberService.GetByNationalCode(model.NationalCode);
			if (user != null && user.CustomerId == customer.Id)
			{
				return Error(0L, "کد ملی وارد شده تکراری است.");
			}
			User entity = new User
			{
				Id = model.Id,
				UserName = model.UserName,
				FirstName = model.FirstName,
				LastName = model.LastName,
				Address = model.Address,
				PostalCode = model.PostalCode,
				Gender = true,
				LastLoginDate = DateTime.MinValue,
				Mobile = model.Mobile,
				Phone = "",
				NationalCode = model.NationalCode,
				Email = model.Email,
				RegDate = DateTime.Now,
				RegDate2 = DateTime.Now,
				Familiar1FirstName = "",
				Familiar1LastName = "",
				Familiar1Mobile = "",
				Familiar2FirstName = "",
				Familiar2LastName = "",
				Familiar2Mobile = "",
				FatherName = model.FatherName,
				City = model.City,
				BankAccountsJson = "",
				IsActive = true
			};
			entity.CustomerId = _currentUserService.CustomerId.Value;
			entity.UserRoleNames = new List<string> { "Member" };
			PasswordHasher<User> passwordHasher = new PasswordHasher<User>();
			string password = (string.IsNullOrEmpty(model.EncryptedPassword) ? ((!string.IsNullOrEmpty(model.Password)) ? model.Password : model.NationalCode) : _encryptionService.DigibileSimpleDecrypt(model.EncryptedPassword));
			string passwordHash = passwordHasher.HashPassword(entity, password);
			entity.PasswordHash = passwordHash;
			IdentityResult identityResult = await _userManager.Insert(entity, password, entity.UserRoleNames);
			return identityResult.Succeeded ? Success(entity.Id, "عملیات با موفقیت انجام شد.") : Error(500L, string.Join(".", identityResult.Errors.Select((IdentityError x) => x.Description)));
		});
	}

	[HttpPost("[action]")]
	public Task<ResponseState<long>> Edit([FromBody] AppApiMemberModel model)
	{
		return TryCatch(async () =>
		{
			Customer customer = await _customerService.GetById(_currentUserService.CustomerId.Value);
			if (string.IsNullOrEmpty(model.NationalCode))
			{
				return Error(0L, "کد ملی وارد نشده است.");
			}
			model.UserName = GenerateUserName(customer.Code, model);
			User user = await _userManager.FindByUserName(model.UserName);
			if (user != null && user.Id != model.Id)
			{
				return Error(0L, "نام کاربری وارد شده تکراری است.");
			}
			user = await _memberService.GetByNationalCode(model.NationalCode);
			if (user != null && user.Id != model.Id && user.CustomerId == customer.Id)
			{
				return Error(0L, "کد ملی وارد شده تکراری است.");
			}
			User entity = await _memberService.GetById(model.Id);
			entity.FirstName = model.FirstName;
			entity.LastName = model.LastName;
			entity.FatherName = model.FatherName;
			entity.NationalCode = model.NationalCode;
			entity.Mobile = model.Mobile;
			entity.Email = model.Email;
			entity.City = model.City;
			entity.Address = model.Address;
			entity.PostalCode = model.PostalCode;
			if (!string.IsNullOrEmpty(model.Password) || !string.IsNullOrEmpty(model.EncryptedPassword))
			{
				string password = model.Password;
				if (!string.IsNullOrEmpty(model.EncryptedPassword))
				{
					password = _encryptionService.DigibileSimpleDecrypt(model.EncryptedPassword);
				}
				PasswordHasher<User> passwordHasher = new PasswordHasher<User>();
				string passwordHash = passwordHasher.HashPassword(entity, password);
				entity.PasswordHash = passwordHash;
			}
			await _userManager.UpdateAsync(entity);
			return Success(entity.Id, "عملیات با موفقیت انجام شد.");
		});
	}

	private string GenerateUserName(int customerCode, AppApiMemberModel model)
	{
		if (_appConfig.IsCloud)
		{
			return $"{customerCode}-{model.NationalCode}";
		}
		return model.NationalCode;
	}
}
