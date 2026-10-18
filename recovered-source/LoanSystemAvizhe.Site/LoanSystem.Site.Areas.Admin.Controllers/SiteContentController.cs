using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Areas.Admin.Models;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Services.Enums;
using LoanSystemAvizhe.Services.FileHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoanSystem.Site.Areas.Admin.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Admin")]
public class SiteContentController : BaseAdminController
{
    private readonly ISiteContentService _SiteContentService;

    private readonly ICurrentUserService _currentUserService;

    private readonly ITransactionService _transactionService;

    private readonly ICustomerService _customerService;

    private readonly IFileHelper _fileHelper;

    public SiteContentController(ILogger<SiteContentController> logger, ISiteContentService SiteContentService, ICurrentUserService currentUserService, ITransactionService transactionService, ICustomerService customerService, IFileHelper fileHelper)
        : base(logger)
    {
        _SiteContentService = SiteContentService;
        _currentUserService = currentUserService;
        _transactionService = transactionService;
        _customerService = customerService;
        _fileHelper = fileHelper;
    }

    [HttpGet("[action]/{id}")]
    public Task<ResponseState<SiteContentModel>> Get(Guid id)
    {
        return TryCatch(async () =>
        {
            SiteContentModel data = new SiteContentModel();
            if (id != Guid.Empty)
            {
                SiteContent siteContent = await _SiteContentService.GetById(id);
                if (siteContent == null)
                {
                    return Error<SiteContentModel>(null, "اطلاعات مورد نظر یافت نشد");
                }
                data = new SiteContentModel(siteContent);
            }
            return Success(data);
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<List<SiteContentModel>>> ListbyKeys([FromBody] StringList model)
    {
        return TryCatch(async () =>
        {
            List<SiteContentModel> data = (await _SiteContentService.GetByCondition((SiteContent _) => model.Keys.Contains(_.Key))).Select((SiteContent x) => new SiteContentModel(x)).ToList();
            return Success(data);
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<string>> CreateUpdateImage(CreateUpdateImageModel model)
    {
        return TryCatch(async () =>
        {
            string data = "";
            if (model.File != null)
            {
                if (model.File.Length == 0L)
                {
                    return Error<string>(null, "فایل خالی است.");
                }
                if (!_fileHelper.CheckFileIsImage(model.File))
                {
                    return Error<string>(null, "فقط فایل تصویر مجاز می باشد.");
                }
                Customer customer = await _customerService.GetById(_currentUserService.CustomerId.Value);
                if (!string.IsNullOrEmpty(model.FileName))
                {
                    _fileHelper.DeleteFile(model.FileName, Directories.SiteContent(customer.Code.ToString()));
                }
                data = await _fileHelper.UploadFile(model.File, FileUploadDirectory.SiteContent, customer.Code.ToString());
            }
            return Success(data, "عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<bool>> DeleteImage([FromBody] DeleteImageModel model)
    {
        return TryCatch(async () =>
        {
            Customer customer = await _customerService.GetById(_currentUserService.CustomerId.Value);
            if (!string.IsNullOrEmpty(model.FileName))
            {
                _fileHelper.DeleteFile(model.FileName, Directories.SiteContent(customer.Code.ToString()));
            }
            return Success(data: true, "عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<Guid>> CreateUpdate([FromBody] SiteContentModel model)
    {
        return TryCatch(async () =>
        {
            SiteContent entity = (await _SiteContentService.GetByCondition((SiteContent x) => x.Key == model.Key)).FirstOrDefault();
            if (entity == null)
            {
                entity = new SiteContent
                {
                    Content = model.Content,
                    Key = model.Key
                };
                await _SiteContentService.Insert(entity);
            }
            else
            {
                entity.Content = model.Content;
                await _SiteContentService.Update(entity);
            }
            return Success(entity.Id, "عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]/{id}")]
    public Task<DefaultResponseState> Delete(Guid id)
    {
        return TryCatch(async () =>
        {
            SiteContent siteContent = (await _SiteContentService.GetByCondition((SiteContent x) => x.Id == id)).FirstOrDefault();
            if (siteContent == null)
            {
                return Error("امکان حذف وجود ندارد. لطفا بعدا دوباره تلاش نمایید");
            }
            await _SiteContentService.Delete(siteContent);
            return Success();
        });
    }
}
