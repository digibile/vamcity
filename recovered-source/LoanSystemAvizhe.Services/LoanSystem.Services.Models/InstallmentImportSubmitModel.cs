using System.Collections.Generic;

namespace LoanSystem.Services.Models;

public class InstallmentImportSubmitModel : BaseModel
{
	public List<InstallmentImportPreviewModel> InstallmentImportPreviewModels { get; set; }
}
