using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Data.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoanSystem.Site.Areas.Admin.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Admin")]
public class TagController : BaseAdminController
{
    private readonly ITagService _TagService;

    private readonly ICurrentUserService _currentUserService;

    public TagController(ILogger<TagController> logger, ITagService TagService, ICurrentUserService currentUserService)
        : base(logger)
    {
        _currentUserService = currentUserService;
        _TagService = TagService;
    }

    [HttpGet("[action]/{id}")]
    public Task<ResponseState<TagModel>> Get(Guid id)
    {
        return TryCatch(async () =>
        {
            Tag tag = await _TagService.GetById(id);
            return Success(new TagModel(tag));
        });
    }

    [HttpGet("[action]/{category?}")]
    public Task<ResponseState<List<TagModel>>> List(TagCategory? category)
    {
        return TryCatch(async () =>
        {
            List<TagModel> data = (await _TagService.GetAll(_currentUserService.CustomerId, category)).Select((Tag x) => new TagModel(x)).ToList();
            return Success(data);
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<Guid>> Create([FromBody] TagModel model)
    {
        return TryCatch(async () =>
        {
            List<Tag> list = await _TagService.GetByCondition((Tag x) => x.Title == model.Title);
            if (list != null && list.Any())
            {
                return Error("عنوان وارد شده تکراری است.");
            }
            Tag entity = new Tag
            {
                Id = Guid.Empty,
                IsPublic = model.IsPublic,
                TagCategory = model.TagCategory,
                Title = model.Title
            };
            await _TagService.Insert(entity);
            return Success(entity.Id, "عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<Guid>> Edit([FromBody] TagModel model)
    {
        return TryCatch(async () =>
        {
            List<Tag> list = await _TagService.GetByCondition((Tag x) => x.Id != model.Id && x.Title == model.Title);
            if (list != null && list.Any())
            {
                return Error("عنوان وارد شده تکراری است.");
            }
            Tag entity = await _TagService.GetById(model.Id);
            entity.Title = model.Title;
            entity.IsPublic = model.IsPublic;
            entity.TagCategory = model.TagCategory;
            await _TagService.Update(entity);
            return Success(entity.Id, "عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]/{id}")]
    public Task<DefaultResponseState> Delete(Guid id)
    {
        return TryCatch(async () =>
        {
            Tag tag = await _TagService.GetById(id);
            if (tag == null)
            {
                return Error("امکان حذف وجود ندارد. لطفا بعدا دوباره تلاش نمایید");
            }
            await _TagService.Delete(tag);
            return Success();
        });
    }
}
