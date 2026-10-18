using System.Collections.Generic;
using System.Threading.Tasks;
using LoanSystem.Services.Models;

namespace LoanSystem.Services.Base;

public interface IExcelImportService
{
    Task<List<MemberImportPreviewModel>> PreviewImportMembers(ImportFileModel model);

    Task<MemberImportResultModel> SubmitImportMembers(MemberImportSubmitModel submitModel);

    Task<List<InstallmentImportPreviewModel>> PreviewImportInstallments(ImportFileModel model);

    Task<InstallmentImportResultModel> SubmitImportInstallments(InstallmentImportSubmitModel model);

    Task<List<SavedMoneyImportPreviewModel>> PreviewImportSavedMoney(ImportFileModel model);

    Task<SavedMoneyImportResultModel> SubmitImportSavedMoney(SavedMoneyImportSubmitModel model);

    Task<List<CompareRemainingsPreviewModel>> PreviewCompareRemainings(ImportFileModel importModel);

    Task ImportStarter();
}
