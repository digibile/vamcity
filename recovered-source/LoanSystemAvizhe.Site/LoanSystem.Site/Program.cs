using System;
using LoanSystem.Data;
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

		// Database schema changes are deliberately opt-in. This app has a real
		// loan/financial database, so merely starting a new Linux release must
		// never run migrations or seed data against an unspecified connection.
		bool autoMigrate = string.Equals(
			Environment.GetEnvironmentVariable("VAMCITY_AUTO_MIGRATE"),
			"true",
			StringComparison.OrdinalIgnoreCase);

		using (IServiceScope serviceScope = webHost.Services.CreateScope())
		{
			ILogger<Startup> logger = serviceScope.ServiceProvider.GetRequiredService<ILogger<Startup>>();
			if (autoMigrate)
			{
				try
				{
					AppDbContext db = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();
					db.Database.Migrate();
					DBInitializer.Initialize(db);
					logger.LogInformation("VamCity database migrations and initialization completed because VAMCITY_AUTO_MIGRATE=true.");
				}
				catch (Exception exception)
				{
					logger.LogCritical(exception, "Database migration or initialization failed. Refusing to start with an unverified schema.");
					throw new InvalidOperationException("VamCity database migration or initialization failed; application startup was aborted.", exception);
				}
			}
			else
			{
				logger.LogWarning("Automatic database migration is disabled. Set VAMCITY_AUTO_MIGRATE=true only for an explicitly approved migration against a verified backup and the intended database.");
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
