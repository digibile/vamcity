using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Core.Configuration;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Areas.Admin.Models;
using LoanSystem.Site.Controllers;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Services.FileHelper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LoanSystemAvizhe.Site.Controllers;

[Route("[controller]")]
public class SiteContentController : BaseDefaultController
{
    private readonly ISiteContentService _SiteContentService;

    private readonly AppConfig _appConfig;

    private readonly ICurrentUserService _currentUserService;

    private readonly ITransactionService _transactionService;

    private readonly ICustomerService _customerService;

    private readonly IFileHelper _fileHelper;

    public SiteContentController(ILogger<SiteContentController> logger, ISiteContentService SiteContentService, ICurrentUserService currentUserService, ITransactionService transactionService, ICustomerService customerService, IOptions<AppConfig> appConfig, IFileHelper fileHelper)
        : base(logger)
    {
        _SiteContentService = SiteContentService;
        _currentUserService = currentUserService;
        _transactionService = transactionService;
        _customerService = customerService;
        _fileHelper = fileHelper;
        _appConfig = appConfig.Value;
    }

    [HttpPost("[action]/{customerCode}")]
    public Task<ResponseState<List<SiteContentModel>>> ListbyKeys([FromBody] StringList model, int customerCode)
    {
        return TryCatch(async () =>
        {
            Customer customer;
            if (customerCode != 0)
            {
                customer = await _customerService.GetByCode(customerCode);
            }
            else
            {
                if (_appConfig.IsCloud)
                {
                    return Success(new List<SiteContentModel>());
                }
                customer = (await _customerService.GetAllCustomers()).FirstOrDefault();
            }
            if (customer == null)
            {
                return Success(new List<SiteContentModel>());
            }
            _currentUserService.CustomerId = customer.Id;
            List<SiteContentModel> data = (await _SiteContentService.GetByCondition((SiteContent _) => model.Keys.Contains(_.Key))).Select((SiteContent x) => new SiteContentModel(x)).ToList();
            return Success(data);
        });
    }
}
