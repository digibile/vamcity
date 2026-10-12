using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Models;
using LoanSystem.Services.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace LoanSystem.Site.Helpers.Security;

public class JwtTokenService
{
	private UserManager userManager;

	private RoleManager<Role> roleManager;

	private SymmetricSecurityKey signingKey;

	public JwtTokenService(UserManager userManager, RoleManager<Role> roleManager)
	{
		this.userManager = userManager;
		this.roleManager = roleManager;
	}

	public async Task<ClaimsIdentity> GetUserClaims(User user, bool silent = false)
	{
		List<Claim> claims = new List<Claim>();
		IList<Claim> userClaims = await userManager.GetClaimsAsync(user);
		IList<string> list = await userManager.GetRolesAsync(user);
		claims.AddRange(userClaims);
		foreach (string item in list)
		{
			claims.Add(new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", item));
			Role role = await roleManager.FindByNameAsync(item);
			if (role != null)
			{
				List<Claim> list2 = claims;
				list2.AddRange(await roleManager.GetClaimsAsync(role));
			}
		}
		claims.Add(new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", user.UserName));
		claims.Add(new Claim("JETAQSAAT_APP_CID", user.CustomerId.ToString()));
		claims.Add(new Claim("JETAQSAAT_APP_UID", user.Id.ToString()));
		if (silent)
		{
			claims.Add(new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/anonymous", "true"));
		}
		JsonSerializerOptions options = new JsonSerializerOptions
		{
			WriteIndented = true,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		};
		UserModel userModel = new UserModel(user);
		userModel.IsSuperAdmin = list.Any(roleName => string.Equals(roleName, "SuperAdmin", StringComparison.OrdinalIgnoreCase));
		userModel.Claims = userClaims.Select((Claim x) => new UserClaimModel
		{
			Type = x.Type,
			Value = x.Value
		}).ToList();
		claims.Add(new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/userdata", JsonSerializer.Serialize(userModel, options)));
		return new ClaimsIdentity(claims.ToArray(), "Bearer");
	}

	public async Task<JwtToken> GetJwtToken(ClaimsIdentity claimIdentity, TokenProviderOptions options)
	{
		DateTime now = DateTime.UtcNow;
		List<Claim> claims = claimIdentity.Claims.ToList();
		List<Claim> list = claims;
		Claim claim = new Claim("sub", claimIdentity.GetUserName());
		string value = await options.NonceGenerator();
		list.AddRange(new Claim[3]
		{
			claim,
			new Claim("jti", value),
			new Claim("iat", ToUnixEpochDate(now).ToString(), "http://www.w3.org/2001/XMLSchema#integer64")
		});
		JwtSecurityToken token = new JwtSecurityToken(options.Issuer, options.Audience, claims.ToArray(), now, now.Add(options.Expiration), options.SigningCredentials);
		string accessToken = new JwtSecurityTokenHandler().WriteToken(token);
		return new JwtToken
		{
			AccessToken = accessToken,
			ExpiredIn = (int)options.Expiration.TotalSeconds,
			UserData = claimIdentity.GetUserData()
		};
	}

	public static long ToUnixEpochDate(DateTime date)
	{
		return new DateTimeOffset(date).ToUniversalTime().ToUnixTimeSeconds();
	}
}
