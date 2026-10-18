using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoanSystem.Site.Areas.Admin.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Admin")]
public class LoanContactController : BaseAdminController
{
    private readonly ILoanContactService _loanContactService;

    private readonly ICurrentUserService _currentUserService;

    public LoanContactController(ILogger<LoanContactController> logger, ILoanContactService loanContactService, ICurrentUserService currentUserService)
        : base(logger)
    {
        _loanContactService = loanContactService;
        _currentUserService = currentUserService;
    }

    [HttpGet("[action]/{id}")]
    public Task<ResponseState<List<LoanContactModel>>> ListByLoanId(Guid id)
    {
        return TryCatch(async () =>
        {
            List<LoanContactModel> data = (await _loanContactService.GetByCondition((LoanContact x) => x.LoanId == id, "Loan", "Contact")).Select((LoanContact x) => new LoanContactModel(x)).ToList();
            return Success(data);
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<Guid>> Create([FromBody] LoanContactModel model)
    {
        return TryCatch(async () =>
        {
            LoanContact entity = new LoanContact
            {
                LoanId = model.Loan.Id,
                ContactId = model.Contact.Id
            };
            await _loanContactService.Insert(entity);
            return Success(entity.Id, "عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<Guid>> Edit([FromBody] LoanContactModel model)
    {
        return TryCatch(async () =>
        {
            if ((await _loanContactService.GetByCondition((LoanContact x) => x.LoanId == model.Loan.Id && x.Id != model.Id)).Sum((LoanContact x) => x.Share) + model.Share > 100m)
            {
                return Error("مجموع درصد سهم ها نباید بیشتر از 100 درصد باشد.");
            }
            LoanContact entity = (await _loanContactService.GetByCondition((LoanContact x) => x.Id == model.Id)).FirstOrDefault();
            if (entity == null)
            {
                return Error("آیتم یافت نشد.");
            }
            entity.ContactId = model.Contact.Id;
            entity.Share = model.Share;
            await _loanContactService.Update(entity);
            return Success(entity.Id, "عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]/{id}")]
    public Task<DefaultResponseState> Delete(Guid id)
    {
        return TryCatch(async () =>
        {
            LoanContact loanContact = (await _loanContactService.GetByCondition((LoanContact x) => x.Id == id)).FirstOrDefault();
            if (loanContact == null)
            {
                return Error("امکان حذف وجود ندارد. لطفا بعدا دوباره تلاش نمایید");
            }
            await _loanContactService.Delete(loanContact);
            return Success();
        });
    }
}
