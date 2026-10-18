using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Core.Configuration;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Services.FileHelper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LoanSystem.Site.Controllers;

[Route("[controller]")]
public class LoanController : BaseDefaultController
{
    private readonly ILoanService _loanService;

    private readonly ICurrentUserService _currentUserService;

    private readonly AppConfig _appConfig;

    private readonly ICustomerService _customerService;

    private readonly ILoanAssuranceRequirementService _loanAssuranceRequirementService;

    private readonly IFileHelper _fileHelper;

    public LoanController(ILogger<LoanController> logger, ILoanService loanService, ICustomerService customerService, ICurrentUserService currentUserService, IOptions<AppConfig> appConfig, IFileHelper fileHelper, ILoanAssuranceRequirementService loanAssuranceRequirementService)
        : base(logger)
    {
        _loanService = loanService;
        _currentUserService = currentUserService;
        _customerService = customerService;
        _appConfig = appConfig.Value;
        _fileHelper = fileHelper;
        _loanAssuranceRequirementService = loanAssuranceRequirementService;
    }

    [HttpGet("[action]/{customerCode}/{includeImage?}")]
    public Task<ResponseState<List<LoanModel>>> List(int customerCode, bool includeImage = false)
    {
        return TryCatch(async () =>
        {
            new List<Loan>();
            IQueryable<Loan> query = _loanService.GetAllLoansAsQueryable().Include((Loan x) => x.LoanAuthorizationItems).Include((Loan x) => x.LoanTags)
                .ThenInclude((LoanTag x) => x.Tag)
                .IgnoreQueryFilters();
            if (_appConfig.IsCloud)
            {
                Customer customer = await _customerService.GetByCode(customerCode);
                if (customer == null)
                {
                    return Success(new List<LoanModel>());
                }
                query = query.Where((Loan x) => x.CustomerId == customer.Id);
            }
            List<Loan> source = (await query.ToListAsync()).Where((Loan x) => x.IncludePlan).ToList();
            List<LoanModel> list = (from x in source
                                    orderby x.Disable
                                    select new LoanModel(x)).ToList();
            if (includeImage)
            {
                list.ForEach((LoanModel x) =>
                {
                    x.FillImageUrlProp(_fileHelper);
                });
            }
            return Success(list);
        });
    }

    [HttpGet("[action]/{customerCode}/{code}")]
    public Task<ResponseState<LoanModel>> Get(int customerCode, int code)
    {
        return TryCatch(async () =>
        {
            Loan loan;
            if (_appConfig.IsCloud)
            {
                Customer customer = await _customerService.GetByCode(customerCode);
                if (customer == null)
                {
                    return Success<LoanModel>();
                }
                _currentUserService.CustomerId = customer.Id;
                IQueryable<Loan> allLoansAsQueryable = _loanService.GetAllLoansAsQueryable();
                loan = await (from x in allLoansAsQueryable.Include((Loan x) => x.LoanAuthorizationItems).ThenInclude((LoanAuthorizationItem x) => x.AuthorizationItem)
                              where x.Code == code
                              select x).FirstOrDefaultAsync();
            }
            else
            {
                IQueryable<Loan> allLoansAsQueryable2 = _loanService.GetAllLoansAsQueryable();
                loan = await (from x in allLoansAsQueryable2.Include((Loan x) => x.LoanAuthorizationItems).ThenInclude((LoanAuthorizationItem x) => x.AuthorizationItem).IgnoreQueryFilters()
                              where x.Code == code
                              select x).FirstOrDefaultAsync();
            }
            if (loan == null)
            {
                return Error<LoanModel>(null, "طرح وام یافت نشد");
            }
            LoanModel loanModel = new LoanModel(loan);
            loanModel.FillImageUrlProp(_fileHelper);
            return Success(loanModel);
        });
    }

    [HttpPost("[action]/{customerCode}/{includeImage}")]
    public Task<ResponseState<List<LoanModel>>> ListByAssuranceRequirement(int customerCode, bool includeImage, [FromBody] Dictionary<Guid, Guid?> dic)
    {
        return TryCatch(async () =>
        {
            if (_appConfig.IsCloud)
            {
                Customer customer = await _customerService.GetByCode(customerCode);
                if (customer == null)
                {
                    return Error<List<LoanModel>>(null, "پارامتر مشتری صحیح نمی باشد.");
                }
                _currentUserService.CustomerId = customer.Id;
            }
            else
            {
                Customer customer2 = (await _customerService.GetAllCustomers()).FirstOrDefault();
                _currentUserService.CustomerId = customer2.Id;
            }
            List<Guid> matchedIds = await _loanAssuranceRequirementService.GetMatchLoanIdsByAssuranceRequirement(dic);
            List<LoanModel> list = new List<LoanModel>();
            if (matchedIds?.Any() ?? false)
            {
                new List<Loan>();
                IIncludableQueryable<Loan, ICollection<LoanAuthorizationItem>> source = _loanService.GetAllLoansAsQueryable().Include((Loan x) => x.LoanAuthorizationItems);
                list = (from x in await source.Where((Loan x) => matchedIds.Contains(x.Id) && x.IncludePlan).ToListAsync()
                        orderby x.Disable
                        select new LoanModel(x)).ToList();
                if (includeImage)
                {
                    list.ForEach((LoanModel x) =>
                    {
                        x.FillImageUrlProp(_fileHelper);
                    });
                }
            }
            return Success(list);
        });
    }
}
