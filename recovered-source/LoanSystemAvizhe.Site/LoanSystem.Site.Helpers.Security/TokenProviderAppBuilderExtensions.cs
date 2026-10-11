using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

namespace LoanSystem.Site.Helpers.Security;

public static class TokenProviderAppBuilderExtensions
{
	public static IApplicationBuilder UseSimpleTokenProvider(this IApplicationBuilder app, TokenProviderOptions options)
	{
		if (app == null)
		{
			throw new ArgumentNullException("app");
		}
		if (options == null)
		{
			throw new ArgumentNullException("options");
		}
		return app.UseMiddleware<TokenProviderMiddleware>(new object[1] { Options.Create(options) });
	}
}
