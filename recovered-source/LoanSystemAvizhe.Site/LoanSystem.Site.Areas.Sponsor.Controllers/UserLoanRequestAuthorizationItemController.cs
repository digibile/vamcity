using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Services.FileHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoanSystem.Site.Areas.Sponsor.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Sponsor")]
public class UserLoanRequestAuthorizationItemController : BaseSponsorController
{
    private readonly IUserLoanRequestAuthorizationItemService _userLoanRequestAuthorizationItemService;

    private readonly IFileHelper _fileHelper;

    public UserLoanRequestAuthorizationItemController(ILogger<UserLoanRequestAuthorizationItemController> logger, IUserLoanRequestAuthorizationItemService userLoanRequestAuthorizationItemService, IFileHelper fileHelper)
        : base(logger)
    {
        _userLoanRequestAuthorizationItemService = userLoanRequestAuthorizationItemService;
        _fileHelper = fileHelper;
    }

    [HttpGet("[action]/{id}")]
    public Task<ResponseState<UserLoanRequestAuthorizationItemModel>> Get(Guid id)
    {
        return TryCatch(async () =>
        {
            UserLoanRequestAuthorizationItemModel model = new UserLoanRequestAuthorizationItemModel(await _userLoanRequestAuthorizationItemService.GetById(id, "ThirdPersonAssuranceAuthorizationItem.User", "AuthorizationItem", "Attachments"));
            if (model.Attachments?.Any() ?? false)
            {
                model.Attachments.ForEach((UserLoanRequestAuthorizationItemAttachmentModel a) =>
                {
                    a.FillFileProps(_fileHelper, model.CustomerId);
                });
            }
            else
            {
                model.Attachments.Add(new UserLoanRequestAuthorizationItemAttachmentModel
                {
                    RowState = "new"
                });
            }
            return Success(model);
        });
    }

    [HttpGet("[action]/{id}")]
    public Task<ResponseState<List<UserLoanRequestAuthorizationItemModel>>> ListByUserLoanRequestId(Guid id)
    {
        return TryCatch(async () =>
        {
            List<UserLoanRequestAuthorizationItemModel> list = (await _userLoanRequestAuthorizationItemService.GetByCondition((UserLoanRequestAuthorizationItem x) => x.UserLoanRequestId == id, "AuthorizationItem", "ThirdPersonAssuranceAuthorizationItem", "Attachments")).Select((UserLoanRequestAuthorizationItem x) => new UserLoanRequestAuthorizationItemModel(x)).ToList();
            foreach (UserLoanRequestAuthorizationItemModel model in list)
            {
                if (model.Attachments?.Any() ?? false)
                {
                    model.Attachments.ForEach((UserLoanRequestAuthorizationItemAttachmentModel a) =>
                    {
                        a.FillFileProps(_fileHelper, model.CustomerId);
                    });
                }
                else
                {
                    model.Attachments.Add(new UserLoanRequestAuthorizationItemAttachmentModel
                    {
                        RowState = "new"
                    });
                }
            }
            return Success(list);
        });
    }
}
