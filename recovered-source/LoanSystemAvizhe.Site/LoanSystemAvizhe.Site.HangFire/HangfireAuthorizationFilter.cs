using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace LoanSystemAvizhe.Site.HangFire;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
	private static readonly string HangFireCookieName = "HangFireCookie";

	private static readonly int CookieExpirationMinutes = 60;

	private TokenValidationParameters tokenValidationParameters;

	private string role;

	public HangfireAuthorizationFilter(TokenValidationParameters tokenValidationParameters, string role = null)
	{
		this.tokenValidationParameters = tokenValidationParameters;
		this.role = role;
	}

	public bool Authorize(DashboardContext context)
	{
		HttpContext httpContext = context.GetHttpContext();
		string empty = string.Empty;
		if (httpContext.Request.Cookies.ContainsKey(HangFireCookieName))
		{
			empty = httpContext.Request.Cookies[HangFireCookieName];
			if (string.IsNullOrEmpty(empty))
			{
				return false;
			}
			try
			{
				SecurityToken validatedToken = null;
				JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
				ClaimsPrincipal claimsPrincipal = jwtSecurityTokenHandler.ValidateToken(empty, tokenValidationParameters, out validatedToken);
				if (!string.IsNullOrEmpty(role) && !claimsPrincipal.IsInRole(role))
				{
					return false;
				}
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}
		return false;
	}
}
