using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Core.Configuration;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LoanSystem.Site.Controllers;

[Route("[controller]")]
public class AssuranceRequirementController : BaseDefaultController
{
    private readonly AppConfig _appConfig;

    private readonly ICustomerService _customerService;

    private readonly IAssuranceRequirementService _assuranceRequirementService;

    private readonly ICurrentUserService _currentUserService;

    public AssuranceRequirementController(ILogger<AssuranceRequirementController> logger, IOptions<AppConfig> appConfig, ICustomerService customerService, IAssuranceRequirementService assuranceRequirementService, ICurrentUserService currentUserService)
        : base(logger)
    {
        _appConfig = appConfig.Value;
        _customerService = customerService;
        _assuranceRequirementService = assuranceRequirementService;
        _currentUserService = currentUserService;
    }

    [HttpGet("[action]/{customerCode}")]
    public Task<ResponseState<List<AssuranceRequirementModel>>> List(int customerCode)
    {
        return TryCatch(async () =>
        {
            if (_appConfig.IsCloud)
            {
                Customer customer = await _customerService.GetByCode(customerCode);
                if (customer == null)
                {
                    return Error<List<AssuranceRequirementModel>>(null, "پارامتر مشتری صحیح نمی باشد.");
                }
                _currentUserService.CustomerId = customer.Id;
            }
            else
            {
                Customer customer2 = (await _customerService.GetAllCustomers()).FirstOrDefault();
                _currentUserService.CustomerId = customer2.Id;
            }
            List<AssuranceRequirementModel> data = (await _assuranceRequirementService.GetByCondition((AssuranceRequirement _) => true, "AssuranceRequirementItems")).Select((AssuranceRequirement x) => new AssuranceRequirementModel(x)).ToList();
            return Success(data);
        });
    }
}
