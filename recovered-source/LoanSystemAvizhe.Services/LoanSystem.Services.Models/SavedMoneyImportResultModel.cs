using System.Collections.Generic;

namespace LoanSystem.Services.Models;

public class SavedMoneyImportResultModel : BaseModel
{
    public int SavedMoneyNew { get; set; }

    public int SavedMoneyUpdate { get; set; }

    public int Failed { get; set; }

    public List<string> Errors { get; set; } = new List<string>();
}
