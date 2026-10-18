using System.Collections.Generic;
using System.Threading.Tasks;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Helpers.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoanSystem.Site.Areas.Admin.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Admin")]
public class ImportController : BaseAdminController
{
    private readonly IExcelImportService _importService;

    public ImportController(ILogger<UserLoanController> logger, IExcelImportService importService)
        : base(logger)
    {
        _importService = importService;
    }

    [HttpPost("[action]")]
    public Task<ResponseState<List<InstallmentImportPreviewModel>>> InstallmentImportPreview(ImportFileModel model)
    {
        return TryCatch(async () =>
        {
            if (model.File == null || model.File.Length == 0L)
            {
                return Error<List<InstallmentImportPreviewModel>>(null, "فایل خالی است.");
            }
            List<InstallmentImportPreviewModel> data = await _importService.PreviewImportInstallments(model);
            return Success(data);
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<InstallmentImportResultModel>> InstallmentImportSubmit([FromBody] InstallmentImportSubmitModel model)
    {
        return TryCatch(async () =>
        {
            InstallmentImportResultModel data = await _importService.SubmitImportInstallments(model);
            return Success(data);
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<List<SavedMoneyImportPreviewModel>>> SavedMoneyImportPreview(ImportFileModel model)
    {
        return TryCatch(async () =>
        {
            if (model.File == null || model.File.Length == 0L)
            {
                return Error<List<SavedMoneyImportPreviewModel>>(null, "فایل خالی است.");
            }
            List<SavedMoneyImportPreviewModel> data = await _importService.PreviewImportSavedMoney(model);
            return Success(data);
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<SavedMoneyImportResultModel>> SavedMoneyImportSubmit([FromBody] SavedMoneyImportSubmitModel model)
    {
        return TryCatch(async () =>
        {
            SavedMoneyImportResultModel data = await _importService.SubmitImportSavedMoney(model);
            return Success(data);
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<List<MemberImportPreviewModel>>> MemberImportPreview(ImportFileModel model)
    {
        return TryCatch(async () =>
        {
            if (model.File == null || model.File.Length == 0L)
            {
                return Error<List<MemberImportPreviewModel>>(null, "فایل خالی است.");
            }
            List<MemberImportPreviewModel> data = await _importService.PreviewImportMembers(model);
            return Success(data);
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<MemberImportResultModel>> MemberImportSubmit([FromBody] MemberImportSubmitModel model)
    {
        return TryCatch(async () =>
        {
            MemberImportResultModel data = await _importService.SubmitImportMembers(model);
            return Success(data);
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<List<CompareRemainingsPreviewModel>>> CompareRemainingImportPreview(ImportFileModel model)
    {
        return TryCatch(async () =>
        {
            if (model.File == null || model.File.Length == 0L)
            {
                return Error<List<CompareRemainingsPreviewModel>>(null, "فایل خالی است.");
            }
            List<CompareRemainingsPreviewModel> data = await _importService.PreviewCompareRemainings(model);
            return Success(data);
        });
    }
}
