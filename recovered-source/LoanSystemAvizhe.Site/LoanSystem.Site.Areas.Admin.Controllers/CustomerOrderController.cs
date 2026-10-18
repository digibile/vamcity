using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Site.Areas.SuperAdmin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoanSystem.Site.Areas.Admin.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Admin")]
public class CustomerOrderController : BaseAdminController
{
    private readonly ICustomerOrderService _customerOrderService;

    public CustomerOrderController(ILogger<CustomerOrderController> logger, ICustomerOrderService consultantService)
        : base(logger)
    {
        _customerOrderService = consultantService;
    }

    [HttpGet("[action]/{all}")]
    public Task<ResponseState<LoanSystemAvizhe.Site.Areas.SuperAdmin.Models.CustomerOrder>> List(bool all)
    {
        return TryCatch(async () =>
        {
            List<LoanSystem.Data.Entities.CustomerOrder> entities = await _customerOrderService.GetAll(all ? ((bool?)null) : new bool?(false));
            int countAll = (all ? entities.Count : (await _customerOrderService.GetCount(null)));
            List<CustomerOrderModel> customerOrders = (from x in entities
                                                       select new CustomerOrderModel(x) into x
                                                       orderby x.SaveDate descending
                                                       select x).ToList();
            LoanSystemAvizhe.Site.Areas.SuperAdmin.Models.CustomerOrder data = new LoanSystemAvizhe.Site.Areas.SuperAdmin.Models.CustomerOrder
            {
                CustomerOrders = customerOrders,
                CountAll = countAll
            };
            return Success(data);
        });
    }

    [HttpGet("[action]/{id}")]
    public Task<ResponseState<CustomerOrderModel>> Get(Guid id)
    {
        return TryCatch(async () =>
        {
            CustomerOrderModel data = new CustomerOrderModel();
            if (id != Guid.Empty)
            {
                LoanSystem.Data.Entities.CustomerOrder customerOrder = await _customerOrderService.GetById(id);
                if (customerOrder == null)
                {
                    return Error<CustomerOrderModel>(null, "اطلاعات مورد نظر یافت نشد");
                }
                data = new CustomerOrderModel(customerOrder);
            }
            return Success(data);
        });
    }

    [HttpPost("[action]")]
    public Task<DefaultResponseState> Edit([FromBody] CustomerOrderModel model)
    {
        return TryCatch(async () =>
        {
            LoanSystem.Data.Entities.CustomerOrder customerOrder = await _customerOrderService.GetById(model.Id);
            if (customerOrder == null)
            {
                return Error("اطلاعات مورد نظر یافت نشد.");
            }
            customerOrder.IsSeen = model.IsSeen;
            customerOrder.CallResult = model.CallResult;
            await _customerOrderService.Update(customerOrder);
            return Success("عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]/{id}")]
    public Task<DefaultResponseState> Delete(Guid id)
    {
        return TryCatch(async () =>
        {
            LoanSystem.Data.Entities.CustomerOrder customerOrder = await _customerOrderService.GetById(id);
            if (customerOrder == null)
            {
                return Error("امکان حذف وجود ندارد. لطفا بعدا دوباره تلاش نمایید");
            }
            await _customerOrderService.Delete(customerOrder);
            return Success();
        });
    }
}
