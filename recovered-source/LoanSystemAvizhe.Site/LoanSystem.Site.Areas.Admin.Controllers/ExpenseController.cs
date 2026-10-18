using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Helpers.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoanSystem.Site.Areas.Admin.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Admin")]
public class ExpenseController : BaseAdminController
{
    private readonly IExpenseService _expenseService;

    public ExpenseController(ILogger<ExpenseController> logger, IExpenseService expenseService)
        : base(logger)
    {
        _expenseService = expenseService;
    }

    [HttpGet("[action]")]
    public Task<ResponseState<List<ExpenseModel>>> List()
    {
        return TryCatch(async () =>
        {
            List<ExpenseModel> data = (await _expenseService.GetAllExpenses()).Select((Expense x) => new ExpenseModel(x)).ToList();
            return Success(data);
        });
    }

    [HttpGet("[action]/{id}")]
    public Task<ResponseState<ExpenseModel>> Get(Guid id)
    {
        return TryCatch(async () =>
        {
            if (id == Guid.Empty)
            {
                return Success(new ExpenseModel
                {
                    SaveDate = DateTime.Now
                });
            }
            Expense expense = await _expenseService.GetById(id);
            if (expense == null)
            {
                return Error<ExpenseModel>(null, "اطلاعات مورد نظر یافت نشد");
            }
            ExpenseModel data = new ExpenseModel(expense);
            return Success(data);
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<Guid>> Create([FromBody] ExpenseModel model)
    {
        return TryCatch(async () =>
        {
            Expense entity = new Expense
            {
                Id = Guid.Empty,
                Amount = model.Amount,
                Comment = model.Comment,
                Title = model.Title,
                CostDate = model.CostDate,
                SaveDate = DateTime.Now,
                SavedBy = HttpContext.User.Identity.Name
            };
            await _expenseService.Insert(entity);
            return Success(entity.Id, "عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<Guid>> Edit([FromBody] ExpenseModel model)
    {
        return TryCatch(async () =>
        {
            Expense entity = await _expenseService.GetById(model.Id);
            entity.Amount = model.Amount;
            entity.Comment = model.Comment;
            entity.Title = model.Title;
            entity.CostDate = model.CostDate;
            await _expenseService.Update(entity);
            return Success(entity.Id, "عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]/{id}")]
    public Task<DefaultResponseState> Delete(Guid id)
    {
        return TryCatch(async () =>
        {
            Expense expense = await _expenseService.GetById(id);
            if (expense == null)
            {
                return Error("امکان حذف وجود ندارد. لطفا بعدا دوباره تلاش نمایید");
            }
            await _expenseService.Delete(expense);
            return Success();
        });
    }
}
