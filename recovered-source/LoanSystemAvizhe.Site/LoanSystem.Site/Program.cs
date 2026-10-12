using System;
using LoanSystem.Data;
using LoanSystem.Services.Users;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Web;

namespace LoanSystem.Site;

public class Program
{
	public static void Main(string[] args)
	{
		NLogBuilder.ConfigureNLog("nlog.config");
		IWebHost webHost = CreateWebHostBuilder(args).Build();
		using (IServiceScope serviceScope = webHost.Services.CreateScope())
		{
			try
			{
				AppDbContext service = serviceScope.ServiceProvider.GetService<AppDbContext>();
				UserManager service2 = serviceScope.ServiceProvider.GetService<UserManager>();
				service.Database.Migrate();
				DBInitializer.Initialize(service);
			}
			catch (Exception exception)
			{
				ILogger<Startup> requiredService = serviceScope.ServiceProvider.GetRequiredService<ILogger<Startup>>();
				requiredService.LogCritical(exception, "Database migration or initialization failed. Refusing to start with an unverified schema.");
				throw new InvalidOperationException("VamCity database migration or initialization failed; application startup was aborted.", exception);
			}
		}
		try
		{
			webHost.Run();
		}
		finally
		{
			LogManager.Shutdown();
		}
	}

	public static IWebHostBuilder CreateWebHostBuilder(string[] args)
	{
		return WebHost.CreateDefaultBuilder(args).UseStartup<Startup>().UseNLog()
			.UseDefaultServiceProvider((ServiceProviderOptions options) =>
			{
				options.ValidateScopes = false;
			});
	}
}
