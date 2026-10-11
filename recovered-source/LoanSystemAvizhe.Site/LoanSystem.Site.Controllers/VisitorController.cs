using System;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Core.Configuration;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using UAParser;

namespace LoanSystem.Site.Controllers;

[Route("[controller]")]
public class VisitorController : BaseDefaultController
{
	private readonly ICustomerMarketerService _customerMarketerService;

	private readonly IVisitorLogService _visitorLogService;

	private readonly ICustomerService _customerService;

	private readonly ICurrentUserService _currentUserService;

	private readonly AppConfig _appConfig;

	public VisitorController(ILogger<VisitorController> logger, IOptions<AppConfig> appConfig, IVisitorLogService visitorLogService, ICustomerService customerService, ICustomerMarketerService customerMarketerService, ICurrentUserService currentUserService)
		: base(logger)
	{
		_appConfig = appConfig.Value;
		_visitorLogService = visitorLogService;
		_customerService = customerService;
		_customerMarketerService = customerMarketerService;
		_currentUserService = currentUserService;
	}

	[HttpPost("[action]")]
	public Task LogVisitor(int customerCode, string mrkCode)
	{
		return TryCatch(async () =>
		{
			Customer customer = ((!_appConfig.IsCloud) ? (await _customerService.GetAllCustomers()).FirstOrDefault() : (await _customerService.GetByCode(customerCode)));
			if (customer == null)
			{
				return (DefaultResponseState)null;
			}
			_currentUserService.CustomerId = customer.Id;
			if (await _customerMarketerService.GetByAffliateCode(mrkCode, customer.Id) == null)
			{
				return (DefaultResponseState)null;
			}
			StringValues stringValues = HttpContext.Request.Headers["User-Agent"];
			Parser parser = Parser.GetDefault();
			ClientInfo clientInfo = parser.Parse(stringValues);
			VisitorLog entity = new VisitorLog
			{
				Date = DateTime.Now,
				IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
				Mrk = mrkCode,
				UserAgent = ExtractReadableDataFromClientInfo(clientInfo)
			};
			await _visitorLogService.Insert(entity);
			return (DefaultResponseState)null;
		});
	}

	private string ExtractReadableDataFromClientInfo(ClientInfo clientInfo)
	{
		return clientInfo.UA.Family + "(" + clientInfo.UA.Major + "-" + clientInfo.UA.Minor + ")," + clientInfo.OS.Family + "(" + clientInfo.OS.Major + "-" + clientInfo.OS.Minor + ")," + clientInfo.Device.Family;
	}
}
