using System.Collections.Generic;

namespace LoanSystem.Services.Models;

public class MemberImportSubmitModel : BaseModel
{
    public List<MemberImportPreviewModel> ImportPreviewModels { get; set; }
}
