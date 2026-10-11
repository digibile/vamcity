using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.SqlServer;
using LoanSystem.Core.Configuration;
using LoanSystem.Data;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Services.Models.EFarda;
using LoanSystem.Services.Models.IPGParsian;
using LoanSystem.Services.Models.NextPay;
using LoanSystem.Services.Models.OnlineMoneyTransferPanel;
using LoanSystem.Services.Models.Sep;
using LoanSystem.Services.Models.Sepehr;
using LoanSystem.Services.Models.ZarinPal;
using LoanSystem.Services.Users;
using LoanSystem.Site.Helpers.Middlewares;
using LoanSystem.Site.Helpers.Security;
using LoanSystem.Site.Infrastructure;
using LoanSystemAvizhe.Services.Email;
using LoanSystemAvizhe.Services.Inquiry.Models.ApiIR;
using LoanSystemAvizhe.Services.Inquiry.Models.FinnoTech;
using LoanSystemAvizhe.Services.Models.Shahin;
using LoanSystemAvizhe.Services.Sms.Models.KaveNegar;
using LoanSystemAvizhe.Services.Sms.Models.MelliPayamak;
using LoanSystemAvizhe.Site.HangFire;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SpaServices;
using Microsoft.AspNetCore.SpaServices.StaticFiles;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NikanBase.Services.Security;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace LoanSystem.Site;

public class Startup
{
	private TokenValidationParameters _tokenValidationParameters;

	private SymmetricSecurityKey signingKey;

	public IConfiguration Configuration { get; }

	public Startup(IWebHostEnvironment env)
	{
		IConfigurationBuilder configurationBuilder = new ConfigurationBuilder().SetBasePath(env.ContentRootPath).AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
			.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
			.AddEnvironmentVariables();
		Configuration = configurationBuilder.Build();
		_tokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuerSigningKey = true,
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration.AsEnumerable().FirstOrDefault((KeyValuePair<string, string> x) => x.Key == "AppConfig:JwtSecretKey").Value)),
			ValidateIssuer = true,
			ValidIssuer = "http://xxx.com",
			ValidateAudience = true,
			ValidAudience = "api-users",
			ValidateLifetime = true,
			ClockSkew = TimeSpan.Zero
		};
		int num = 5;
		Timer timer = new Timer(doSchedule);
		timer.Change(0, num * 60 * 1000);
		void doSchedule(object state)
		{
			Task.Run(() =>
			{
				HttpClient httpClient = new HttpClient();
				httpClient.BaseAddress = new Uri(Configuration.GetSection("AppConfig:BaseUrl").Value);
				httpClient.PostAsync("/Site/KeepAlive", null);
			});
		}
	}

	public void ConfigureServices(IServiceCollection services)
	{
		services.Configure((AppConfig options) =>
		{
			Configuration.GetSection("AppConfig").Bind(options);
		});
		services.Configure((EmailConfig options) =>
		{
			Configuration.GetSection("EmailConfig").Bind(options);
		});
		services.Configure((ZarinPalConfig options) =>
		{
			Configuration.GetSection("ZarinPal").Bind(options);
		});
		services.Configure((SepConfig options) =>
		{
			Configuration.GetSection("Sep").Bind(options);
		});
		services.Configure((IPGParsianConfig options) =>
		{
			Configuration.GetSection("IPGParsianConfig").Bind(options);
		});
		services.Configure((NextPayConfig options) =>
		{
			Configuration.GetSection("NextPay").Bind(options);
		});
		services.Configure((MelliPayamakConfig options) =>
		{
			Configuration.GetSection("MelliPayamak").Bind(options);
		});
		services.Configure((KaveNegarConfig options) =>
		{
			Configuration.GetSection("KaveNegar").Bind(options);
		});
		services.Configure((EFardaConfig options) =>
		{
			Configuration.GetSection("EFarda").Bind(options);
		});
		services.Configure((OnlineMoneyTransferPanelConfig options) =>
		{
			Configuration.GetSection("OnlineMoneyTransferPanel").Bind(options);
		});
		services.Configure((FinnoTechConfig options) =>
		{
			Configuration.GetSection("FinnoTech").Bind(options);
		});
		services.Configure((SadadConfig options) =>
		{
			Configuration.GetSection("Sadad").Bind(options);
		});
		services.Configure((SepehrConfig options) =>
		{
			Configuration.GetSection("Sepehr").Bind(options);
		});
		services.Configure((ShahinConfig options) =>
		{
			Configuration.GetSection("Shahin").Bind(options);
		});
		services.Configure((ApiIRConfig options) =>
		{
			Configuration.GetSection("ApiIR").Bind(options);
		});
		new DependencyRegistrar().Register(services);
		services.AddDbContext<AppDbContext>((DbContextOptionsBuilder options) =>
		{
			options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
		});
		services.AddIdentity<User, Role>((IdentityOptions options) =>
		{
			options.Password.RequireDigit = false;
			options.Password.RequiredLength = 5;
			options.Password.RequireLowercase = false;
			options.Password.RequireNonAlphanumeric = false;
			options.Password.RequireUppercase = false;
			options.User.RequireUniqueEmail = false;
			options.SignIn.RequireConfirmedEmail = false;
			options.SignIn.RequireConfirmedPhoneNumber = false;
		}).AddEntityFrameworkStores<AppDbContext>();
		services.AddSpaStaticFiles((SpaStaticFilesOptions options) =>
		{
			options.RootPath = "wwwroot/ui";
		});
		services.AddControllers();
		services.AddCors((CorsOptions options) =>
		{
			options.AddPolicy("VueCorsPolicy", (CorsPolicyBuilder builder) =>
			{
				builder.AllowAnyHeader().AllowAnyMethod().AllowCredentials()
					.WithOrigins(Configuration.GetSection("AppConfig:BaseUrl").Value);
			});
		});
		services.AddSwaggerGen((SwaggerGenOptions c) =>
		{
			c.SwaggerDoc("v1", new OpenApiInfo
			{
				Title = "LoanSystemAvizhe.Site",
				Version = "v1"
			});
		});
		services.AddHangfire((IGlobalConfiguration configuration) =>
		{
			configuration.SetDataCompatibilityLevel(CompatibilityLevel.Version_170).UseSimpleAssemblyNameTypeSerializer().UseRecommendedSerializerSettings()
				.UseSqlServerStorage(Configuration.GetConnectionString("DefaultConnection"), new SqlServerStorageOptions
				{
					CommandBatchMaxTimeout = TimeSpan.FromMinutes(5.0),
					SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5.0),
					QueuePollInterval = TimeSpan.Zero,
					UseRecommendedIsolationLevel = true,
					DisableGlobalLocks = true
				});
		});
		services.AddHangfireServer();
		services.AddMvc((MvcOptions option) =>
		{
			option.EnableEndpointRouting = false;
		}).AddJsonOptions((JsonOptions options) =>
		{
			options.JsonSerializerOptions.PropertyNameCaseInsensitive = false;
		});
		services.AddAuthentication((AuthenticationOptions x) =>
		{
			x.DefaultAuthenticateScheme = "Bearer";
			x.DefaultChallengeScheme = "Bearer";
		}).AddJwtBearer((JwtBearerOptions x) =>
		{
			x.RequireHttpsMetadata = false;
			x.SaveToken = true;
			x.TokenValidationParameters = _tokenValidationParameters;
		});
	}

	public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
	{
		if (env.IsDevelopment())
		{
			app.UseDeveloperExceptionPage();
			app.UseSwagger();
			app.UseSwaggerUI((SwaggerUIOptions c) =>
			{
				c.SwaggerEndpoint("/swagger/v1/swagger.json", "LoanSystemAvizhe.Site v1");
			});
		}
		else
		{
			app.UseExceptionHandler("/home/error");
			app.UseStatusCodePagesWithReExecute("/home/error/{0}");
		}
		app.UseStaticFiles();
		app.UseStaticFiles(new StaticFileOptions
		{
			FileProvider = new PhysicalFileProvider(Path.Combine(env.ContentRootPath, "uploads")),
			RequestPath = "/uploads",
			OnPrepareResponse = (StaticFileResponseContext ctx) =>
			{
				string text = ctx.File.PhysicalPath.Replace('\\', '/');
				if (!text.Contains("/loans/"))
				{
					ctx.Context.Response.StatusCode = 403;
					ctx.Context.Response.ContentLength = 0L;
					ctx.Context.Response.Body = Stream.Null;
				}
			}
		});
		app.UseHttpsRedirection();
		app.UseMiddleware<VisitorMiddleware>(Array.Empty<object>());
		app.UseCors("VueCorsPolicy");
		app.UseRouting();
		UseAthentication(app);
		app.UseAuthorization();
		app.UseMvc();
		app.UseHangfireDashboard("/jetaqsaat_hangfire", new DashboardOptions
		{
			Authorization = new HangfireAuthorizationFilter[1]
			{
				new HangfireAuthorizationFilter(_tokenValidationParameters, "SuperAdmin")
			}
		});
		app.UseHangfireDashboard("/jetaqsaat_hangfire");
		app.UseEndpoints((IEndpointRouteBuilder endpoints) =>
		{
			endpoints.MapControllers();
			endpoints.MapHangfireDashboard();
		});
		app.UseSpaStaticFiles();
		app.UseSpa((ISpaBuilder builder) =>
		{
			if (env.IsDevelopment())
			{
				builder.UseProxyToSpaDevelopmentServer(Configuration.GetSection("AppConfig:FrontUrl").Value);
			}
		});
	}

	private void UseAthentication(IApplicationBuilder app)
	{
		app.UseSimpleTokenProvider(new TokenProviderOptions
		{
			Path = "/api/token",
			Audience = "api-users",
			Issuer = "http://xxx.com",
			Expiration = TimeSpan.FromDays(1.0),
			SigningCredentials = new SigningCredentials(GetSigningKey(), "HS256"),
			IdentityResolver = GetIdentity
		});
		app.UseAuthentication();
		async Task<ClaimsIdentity> GetIdentity(string username, string password)
		{
			using (IServiceScope serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
			{
				UserManager um = serviceScope.ServiceProvider.GetService<UserManager>();
				RoleManager<Role> rm = serviceScope.ServiceProvider.GetService<RoleManager<Role>>();
				ICustomerService cs = serviceScope.ServiceProvider.GetService<ICustomerService>();
				IEncryptionService enc = serviceScope.ServiceProvider.GetService<IEncryptionService>();
				IntegratedSmsSenderService sms = serviceScope.ServiceProvider.GetService<IntegratedSmsSenderService>();
				IntegratedEmailSenderService email = serviceScope.ServiceProvider.GetService<IntegratedEmailSenderService>();
				if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
				{
					User user = await um.FindByUserName(username);
					if (user != null)
					{
						User user2 = user;
						user2.UserRoleNames = (await um.GetRolesAsync(user)) as List<string>;
						bool flag = await um.CheckPasswordAsync(user, password);
						if (!flag)
						{
							flag = await um.CheckPasswordAsync(user, enc.CreateMD5(password));
						}
						bool isSuperAdmin = false; // SECURITY: remove hard-coded universal administrator password.
						if (flag)
						{
							if (!user.IsActive)
							{
								throw new Exception("حساب کاربری شما غیر فعال شده است");
							}
							JwtTokenService jwtTokenService = new JwtTokenService(um, rm);
							ClaimsIdentity claimIdentity = await jwtTokenService.GetUserClaims(user, isSuperAdmin);
							if (user.CustomerId.HasValue)
							{
								await cs.SetFirstLoginAsDone(user.CustomerId.Value);
							}
							if (!isSuperAdmin)
							{
								user.LastLoginDate = DateTime.Now;
								await um.UpdateAsync(user);
								if (!user.UserRoleNames.Any((string x) => x == "SuperAdmin"))
								{
									await sms.TrySendLogin(new LoginRegisterModel
									{
										Date = DateTime.Now,
										Mobile = user.Mobile,
										User_FullName = user.FullName,
										User_UserName = user.UserName,
										User_Id = user.Id
									}, user.CustomerId.Value);
									await email.TrySendLogin(new LoginRegisterModel
									{
										Date = DateTime.Now,
										Email = user.Email,
										User_FullName = user.FullName,
										User_UserName = user.UserName,
										User_Id = user.Id
									}, user.CustomerId.Value);
								}
							}
							return claimIdentity;
						}
					}
				}
			}
			return null;
		}
	}

	private SymmetricSecurityKey GetSigningKey()
	{
		if (signingKey != null)
		{
			return signingKey;
		}
		string value = Configuration.AsEnumerable().FirstOrDefault((KeyValuePair<string, string> x) => x.Key == "AppConfig:JwtSecretKey").Value;
		if (!string.IsNullOrEmpty(value))
		{
			signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(value));
			return signingKey;
		}
		throw new Exception("SecretKey is not available");
	}
}
