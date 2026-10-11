using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace LoanSystem.Site.Helpers.Middlewares;

public class WhiteListMiddleware
{
	private readonly RequestDelegate _next;

	private readonly IConfiguration _configuration;

	public WhiteListMiddleware(IConfiguration configuration, RequestDelegate next)
	{
		_configuration = configuration;
		_next = next;
	}

	public async Task Invoke(HttpContext context)
	{
		if (context.Request.Method == HttpMethod.Get.Method)
		{
			HttpClient httpClient = new HttpClient();
			httpClient.BaseAddress = new Uri(_configuration.GetSection("AppConfig:BaseUrl").Value);
			if ((await httpClient.PostAsync("/Site/CheckIpAccess", null)).StatusCode == HttpStatusCode.Forbidden)
			{
				context.Response.StatusCode = 403;
				return;
			}
		}
		await _next(context);
	}
}
