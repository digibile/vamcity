using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Areas.SuperAdmin.Controllers;
using LoanSystem.Site.Helpers.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoanSystemAvizhe.Site.Areas.SuperAdmin.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "SuperAdmin")]
public class AppEmailController : BaseSuperAdminController
{
    private readonly IAppEmailService _appEmailService;

    public AppEmailController(ILogger<AppEmailController> logger, IAppEmailService appEmailService)
        : base(logger)
    {
        _appEmailService = appEmailService;
    }

    [HttpGet("[action]")]
    public Task<ResponseState<List<AppEmailModel>>> List(DateTime? start, DateTime? end, string email)
    {
        return TryCatch(async () =>
        {
            List<AppEmailModel> data = (from x in await _appEmailService.GetByCondition((AppEmail x) => (((DateTime?)start).HasValue ? (x.Date.Date >= ((DateTime?)start).Value.Date) : true) && (((DateTime?)end).HasValue ? (x.Date.Date <= ((DateTime?)end).Value.Date) : true) && ((!string.IsNullOrEmpty(email)) ? (x.To == email) : true), true, "Customer")
                                        select new AppEmailModel(x) into x
                                        orderby x.Date descending
                                        select x).ToList();
            return Success(data);
        });
    }
}
