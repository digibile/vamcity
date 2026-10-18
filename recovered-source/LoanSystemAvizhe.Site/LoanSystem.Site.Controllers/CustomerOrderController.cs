using System;
using System.Linq;
using System.Threading.Tasks;
using Ganss.Xss;
using LoanSystem.Core.Configuration;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Services.Users;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LoanSystem.Site.Controllers;

[ApiController]
[Route("[controller]")]
public class CustomerOrderController : BaseDefaultController
{
    private readonly UserManager _userManager;

    private readonly ICustomerOrderService _customerOrderService;

    private readonly IntegratedSmsSenderService _integratedSmsSenderService;

    private readonly IntegratedEmailSenderService _integratedEmailSenderService;

    private readonly AppConfig _appConfig;

    private readonly ICurrentUserService _currentUserService;

    private readonly ICustomerService _customerService;

    public CustomerOrderController(ILogger<AccountController> logger, UserManager userManager, ICustomerOrderService consultantService, IntegratedSmsSenderService integratedSmsSenderService, IntegratedEmailSenderService integratedEmailSenderService, ICurrentUserService currentUserService, IOptions<AppConfig> options, ICustomerService customerService)
        : base(logger)
    {
        _userManager = userManager;
        _customerOrderService = consultantService;
        _integratedSmsSenderService = integratedSmsSenderService;
        _integratedEmailSenderService = integratedEmailSenderService;
        _appConfig = options.Value;
        _currentUserService = currentUserService;
        _customerService = customerService;
    }

    [HttpPost("[action]")]
    public Task<ResponseState<Guid>> Create([FromBody] CustomerOrderModel model)
    {
        return TryCatch(async () =>
        {
            if (!CheckMobileFormatValidity(model.Phone))
            {
                return Error<Guid>(default, "شماره موبایل وارد شده صحیح نمی باشد.");
            }
            Customer customer;
            if (_appConfig.IsCloud)
            {
                if (model.CustomerCode == 0)
                {
                    return Error<Guid>(default, "پارامترهای مشتری معتبر نمی باشد.");
                }
                customer = await _customerService.GetByCode(model.CustomerCode);
                if (customer == null)
                {
                    return Error<Guid>(default, "پارامترهای مشتری معتبر نمی باشد.");
                }
            }
            else
            {
                customer = (await _customerService.GetAllCustomers()).FirstOrDefault();
            }
            if (customer == null)
            {
                return Error<Guid>(default, "پارامترهای مشتری معتبر نمی باشد.");
            }
            _currentUserService.CustomerId = customer.Id;
            HtmlSanitizer htmlSanitizer = new HtmlSanitizer();
            CustomerOrder order = new CustomerOrder
            {
                Name = htmlSanitizer.Sanitize(model.Name),
                NationalCode = htmlSanitizer.Sanitize(model.NationalCode),
                Phone = htmlSanitizer.Sanitize(model.Phone),
                ProductName = htmlSanitizer.Sanitize(model.ProductName),
                Description = htmlSanitizer.Sanitize(model.Description),
                SaveDate = DateTime.Now
            };
            await _customerOrderService.Insert(order);
            try
            {
                await _integratedSmsSenderService.SendNewCustomerOrderNotification(model, customer.Id);
            }
            catch (Exception)
            {
            }
            return Success(order.Id, "عملیات با موفقیت انجام شد.");
        });
    }

    private bool CheckMobileFormatValidity(string mobile)
    {
        if (string.IsNullOrEmpty(mobile))
        {
            return false;
        }
        long result2;
        if (!mobile.StartsWith("0"))
        {
            if (mobile.Length != 10 || !long.TryParse(mobile, out var _))
            {
                return false;
            }
        }
        else if (mobile.Length != 11 || !long.TryParse(mobile, out result2))
        {
            return false;
        }
        return true;
    }
}
