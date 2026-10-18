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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LoanSystemAvizhe.Site.Areas.SuperAdmin.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "SuperAdmin")]
public class PurchaseController : BaseSuperAdminController
{
    private readonly IPurchaseService _purchaseService;

    public PurchaseController(ILogger<PurchaseController> logger, IPurchaseService purchaseService)
        : base(logger)
    {
        _purchaseService = purchaseService;
    }

    [HttpGet("[action]")]
    public Task<ResponseState<List<PurchaseModel>>> List(DateTime? start, DateTime? end, Guid? customerId)
    {
        return TryCatch(async () =>
        {
            IQueryable<Purchase> allAsQueryable = _purchaseService.GetAllAsQueryable();
            allAsQueryable = allAsQueryable.Include((Purchase x) => x.Customer);
            if (start.HasValue)
            {
                allAsQueryable = allAsQueryable.Where((Purchase x) => x.StartDate.Date >= ((DateTime?)start).Value.Date);
            }
            if (end.HasValue)
            {
                allAsQueryable = allAsQueryable.Where((Purchase x) => x.StartDate.Date <= ((DateTime?)end).Value.Date);
            }
            if (customerId.HasValue)
            {
                allAsQueryable = allAsQueryable.Where((Purchase x) => x.CustomerId == customerId);
            }
            List<PurchaseModel> data = (from x in await allAsQueryable.ToListAsync()
                                        select new PurchaseModel(x) into x
                                        orderby x.StartDate descending
                                        select x).ToList();
            return Success(data);
        });
    }
}
