using System.Collections.Generic;

namespace LoanSystem.Services.Models;

public class SavedMoneyImportSubmitModel : BaseModel
{
	public List<SavedMoneyImportPreviewModel> ImportPreviewModels { get; set; }
}
