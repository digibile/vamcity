using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using LoanSystem.Data;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Models;
using LoanSystemAvizhe.Data.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LoanSystem.Services.Users;

public class UserManager : UserManager<User>
{
	private readonly AppDbContext _context;

	private readonly RoleManager<Role> _roleManager;

	public UserManager(IUserStore<User> store, IOptions<IdentityOptions> optionsAccessor, IPasswordHasher<User> passwordHasher, IEnumerable<IUserValidator<User>> userValidators, IEnumerable<IPasswordValidator<User>> passwordValidators, ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<User>> logger, RoleManager<Role> roleManager, AppDbContext context)
		: base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
	{
		_roleManager = roleManager;
		_context = context;
	}

	public async Task<IdentityResult> Insert(User user, string password, List<string> roleNames, List<Claim> claims = null)
	{
		IdentityResult identityResult = ((!string.IsNullOrEmpty(password)) ? (await CreateAsync(user, password)) : (await CreateAsync(user)));
		IdentityResult result = identityResult;
		if (result.Succeeded)
		{
			await AddToRolesAsync(user, roleNames);
			if (claims?.Any() ?? false)
			{
				await AddClaimsAsync(user, claims);
			}
		}
		return result;
	}

	public async Task<IdentityResult> Update(User user)
	{
		IdentityResult identityResult = await UpdateAsync(user);
		if (identityResult.Succeeded)
		{
			identityResult = await UpdateSecurityStampAsync(user);
		}
		return identityResult;
	}

	public async Task<IdentityResult> Update(User user, List<string> roles, bool updateSensitiveFields)
	{
		IdentityResult identityResult = null;
		User mainUser = await GetByIdWithRoles(user.Id);
		if (mainUser == null)
		{
			return IdentityResult.Failed(new IdentityError
			{
				Code = "UserNotFound"
			});
		}
		using (IDbContextTransaction trans = await _context.Database.BeginTransactionAsync(IsolationLevel.ReadUncommitted))
		{
			mainUser.Email = user.Email;
			mainUser.FirstName = user.FirstName;
			mainUser.LastName = user.LastName;
			mainUser.Gender = user.Gender;
			mainUser.Address = user.Address;
			mainUser.PhoneNumber = user.PhoneNumber;
			mainUser.UserName = user.UserName;
			if (updateSensitiveFields)
			{
				mainUser.IsActive = user.IsActive;
				foreach (string item in await GetRolesAsync(mainUser))
				{
					await RemoveFromRoleAsync(mainUser, item);
				}
				foreach (string role in roles)
				{
					await AddToRoleAsync(mainUser, role);
				}
			}
			identityResult = await UpdateAsync(mainUser);
			if (identityResult.Succeeded)
			{
				identityResult = await UpdateSecurityStampAsync(mainUser);
			}
			trans.Commit();
		}
		return identityResult;
	}

	public async Task<IdentityResult> ChangePasswordAndCookie(User user, string oldPassword, string newPassword)
	{
		IdentityResult result = await ChangePasswordAsync(user, oldPassword, newPassword);
		if (result.Succeeded)
		{
			await UpdateSecurityStampAsync(user).ConfigureAwait(continueOnCapturedContext: false);
		}
		return result;
	}

	public async Task<List<User>> GetAll()
	{
		List<User> users = await Users.Where((User m) => m.UserName.ToLower() != "admin").ToListAsync();
		await ConvertUserRoleToRole(users);
		return users;
	}

	public IQueryable<User> GetAllAsQueryable()
	{
		return Users;
	}

	public async Task<List<User>> GetAllUser()
	{
		return await Users.ToListAsync();
	}

	public List<UserClaimModel> GetClaims()
	{
		List<UserClaimModel> list = new List<UserClaimModel>();
		foreach (object value in Enum.GetValues(typeof(UserPermissionClaims)))
		{
			list.Add(new UserClaimModel
			{
				Type = "Permission",
				Value = value.ToString()
			});
		}
		return list;
	}

	public async Task<User> GetById(long id, bool ignoreQueryFilter = false)
	{
		IQueryable<User> source = Users;
		if (ignoreQueryFilter)
		{
			source = source.IgnoreQueryFilters();
		}
		return await source.FirstOrDefaultAsync((User m) => m.Id == id);
	}

	public async Task<User> FindByUserName(string username)
	{
		return await Users.Include((User x) => x.Customer).Include((User x) => x.AgentMetaData).IgnoreQueryFilters()
			.FirstOrDefaultAsync((User m) => m.UserName == username);
	}

	public async Task<List<User>> GetByMobile(string mobile)
	{
		return await (from m in Users.Include((User x) => x.Customer).Include((User x) => x.AgentMetaData)
			where m.Mobile == mobile
			select m).ToListAsync();
	}

	public async Task<User> FindByAffiliateCode(string affiliateCode, Guid? customerId = null)
	{
		IIncludableQueryable<User, Customer> source = Users.Include((User x) => x.Customer);
		if (customerId.HasValue)
		{
			return await source.FirstOrDefaultAsync((User m) => m.CustomerId == ((Guid?)customerId).Value && m.AffiliateCode == affiliateCode);
		}
		return await source.IgnoreQueryFilters().FirstOrDefaultAsync((User m) => m.AffiliateCode == affiliateCode);
	}

	public async Task<User> FindByAffiliateCode(string affiliateCode)
	{
		return await Users.Include((User x) => x.Customer).IgnoreQueryFilters().FirstOrDefaultAsync((User m) => m.AffiliateCode == affiliateCode);
	}

	public bool CheckUserProfileCompletion(User user)
	{
		if (string.IsNullOrEmpty(user.FirstName))
		{
			return false;
		}
		if (string.IsNullOrEmpty(user.LastName))
		{
			return false;
		}
		if (string.IsNullOrEmpty(user.FatherName))
		{
			return false;
		}
		if (string.IsNullOrEmpty(user.Address))
		{
			return false;
		}
		if (string.IsNullOrEmpty(user.Mobile))
		{
			return false;
		}
		if (string.IsNullOrEmpty(user.PostalCode))
		{
			return false;
		}
		if (string.IsNullOrEmpty(user.City))
		{
			return false;
		}
		if (string.IsNullOrEmpty(user.BankAccountsJson))
		{
			return false;
		}
		return true;
	}

	public async Task<User> GetByIdWithRoles(long id)
	{
		User user = await GetById(id);
		await ConvertUserRoleToRole(new List<User> { user });
		return user;
	}

	public async Task ConvertUserRoleToRole(List<User> users)
	{
		foreach (User user2 in users)
		{
			User user = user2;
			user.UserRoleNames = (await GetRolesAsync(user2)) as List<string>;
		}
	}
}
