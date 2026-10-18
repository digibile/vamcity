using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Services.Enums;
using LoanSystemAvizhe.Services.FileHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LoanSystem.Site.Areas.Admin.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Admin")]
public class LoanController : BaseAdminController
{
    private readonly IFileHelper _fileHelper;

    private readonly ILoanService _loanService;

    private readonly ICurrentUserService _currentUserService;

    public LoanController(IFileHelper fileHelper, ILogger<LoanController> logger, ILoanService loanService, ICurrentUserService currentUserService)
        : base(logger)
    {
        _loanService = loanService;
        _fileHelper = fileHelper;
        _currentUserService = currentUserService;
    }

    [HttpGet("[action]/{includeContacts?}")]
    public Task<ResponseState<List<LoanModel>>> List(bool includeContacts = false)
    {
        return TryCatch(async () =>
        {
            new List<Loan>();
            List<Loan> source = ((!includeContacts) ? (await _loanService.GetAllLoans()) : (await _loanService.GetAllLoans("LoanContacts.Contact")));
            List<LoanModel> data = source.Select((Loan x) => new LoanModel(x)).ToList();
            return Success(data);
        });
    }

    [HttpGet("[action]/{id}")]
    public Task<ResponseState<LoanModel>> Get(Guid id)
    {
        return TryCatch(async () =>
        {
            LoanModel loanModel = new LoanModel();
            if (id != Guid.Empty)
            {
                Loan loan = await _loanService.GetById(id, "LoanTags.Tag");
                if (loan == null)
                {
                    return Error<LoanModel>(null, "اطلاعات مورد نظر یافت نشد");
                }
                loanModel = new LoanModel(loan);
                loanModel.FillImageBase64Prop(_fileHelper);
            }
            return Success(loanModel);
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<Guid>> Create(LoanModel model)
    {
        return TryCatch(async () =>
        {
            if (await _loanService.GetByTitle(model.Title) != null)
            {
                return Error("نام وارد شده تکراری است.");
            }
            int num = (await (from x in _loanService.GetAllLoansAsQueryable()
                              orderby x.Code descending
                              select x).FirstOrDefaultAsync())?.Code ?? 0;
            model.Code = num + 1;
            if (await _loanService.GetByCode(model.Code) != null)
            {
                return Error("شماره وارد شده تکراری است.");
            }
            string fileName = "";
            if (model.File != null)
            {
                if (model.File.Length == 0L)
                {
                    return Error("فایل خالی است.");
                }
                _ = model.File.Length;
                if (!_fileHelper.CheckFileIsImage(model.File))
                {
                    return Error("فقط فایل تصویر مجاز می باشد.");
                }
                Path.GetExtension(model.File.FileName);
                fileName = await _fileHelper.UploadFile(model.File, FileUploadDirectory.Loan, _currentUserService.CustomerId.ToString());
            }
            Loan entity = new Loan
            {
                Id = model.Id,
                Title = model.Title,
                Code = model.Code,
                LoanProfitType = model.LoanProfitType,
                ProfitPercent = model.ProfitPercent,
                DelayPercent = model.DelayPercent,
                EarlyPercent = model.EarlyPercent,
                IsSponsored = model.IsSponsored,
                IsSmsActive = model.IsSmsActive,
                MaxAmount = model.MaxAmount.GetValueOrDefault(),
                MaxMonthGap = model.MaxMonthGap.GetValueOrDefault(),
                IncludePlan = model.IncludePlan,
                PlanHtml = model.PlanHtml?.Replace("<script", ""),
                PlanShortDescription = model.PlanShortDescription,
                PlanTitle = model.PlanTitle,
                FileName = fileName,
                CostsJson = model.CostsJson,
                AllowedInstallments = (model.AllowedInstallments ?? ""),
                Disable = model.Disable,
                HasSponsore = model.HasSponsore,
                DisableText = model.DisableText,
                DefaultSponsoreCheckoutDays = model.DefaultSponsoreCheckoutDays,
                LoanTags = (model.Tags?.Select((TagModel tag) => new LoanTag
                {
                    TagId = tag.Id,
                    LoanId = model.Id
                }).ToList() ?? new List<LoanTag>())
            };
            await _loanService.Insert(entity);
            return Success(entity.Id, "عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<Guid>> Edit(LoanModel model)
    {
        return TryCatch(async () =>
        {
            Loan loan = await _loanService.GetByTitle(model.Title);
            if (loan != null && loan.Id != model.Id)
            {
                return Error("نام وارد شده تکراری است.");
            }
            loan = await _loanService.GetByCode(model.Code);
            if (loan != null && loan.Id != model.Id)
            {
                return Error("شماره وارد شده تکراری است.");
            }
            Loan entity = new Loan
            {
                Id = model.Id,
                Title = model.Title,
                Code = model.Code,
                LoanProfitType = model.LoanProfitType,
                ProfitPercent = model.ProfitPercent,
                DelayPercent = model.DelayPercent,
                EarlyPercent = model.EarlyPercent,
                IsSponsored = model.IsSponsored,
                MaxAmount = model.MaxAmount.GetValueOrDefault(),
                MaxMonthGap = model.MaxMonthGap.GetValueOrDefault(),
                IsSmsActive = model.IsSmsActive,
                IncludePlan = model.IncludePlan,
                PlanHtml = model.PlanHtml?.Replace("<script", ""),
                PlanShortDescription = model.PlanShortDescription,
                PlanTitle = model.PlanTitle,
                AllowedInstallments = (model.AllowedInstallments ?? ""),
                CostsJson = model.CostsJson,
                Disable = model.Disable,
                HasSponsore = model.HasSponsore,
                DisableText = model.DisableText,
                DefaultSponsoreCheckoutDays = model.DefaultSponsoreCheckoutDays,
                FileName = model.FileName,
                LoanTags = (model.Tags?.Select((TagModel tag) => new LoanTag
                {
                    TagId = tag.Id,
                    LoanId = model.Id
                }).ToList() ?? new List<LoanTag>())
            };
            if (model.File != null)
            {
                if (model.File.Length == 0L)
                {
                    return Error("فایل خالی است.");
                }
                if (!_fileHelper.CheckFileIsImage(model.File))
                {
                    return Error("فقط فایل تصویر مجاز می باشد.");
                }
                _fileHelper.DeleteFile(loan.FileName, Directories.Loan(_currentUserService.CustomerId.ToString()));
                entity.FileName = await _fileHelper.UploadFile(model.File, FileUploadDirectory.Loan, _currentUserService.CustomerId.ToString());
            }
            else if (!string.IsNullOrEmpty(loan.FileName) && string.IsNullOrEmpty(model.FileName))
            {
                _fileHelper.DeleteFile(loan.FileName, Directories.Loan(_currentUserService.CustomerId.ToString()));
            }
            try
            {
                await _loanService.Update(entity);
            }
            catch (Exception)
            {
                throw;
            }
            return Success(entity.Id, "عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]/{id}")]
    public Task<DefaultResponseState> Delete(Guid id)
    {
        return TryCatch(async () =>
        {
            Loan loan = await _loanService.GetById(id);
            if (loan == null)
            {
                return Error("امکان حذف وجود ندارد. لطفا بعدا دوباره تلاش نمایید");
            }
            _fileHelper.DeleteFile(loan.FileName, Directories.Loan(_currentUserService.CustomerId.ToString()));
            await _loanService.Delete(loan);
            return Success();
        });
    }
}
