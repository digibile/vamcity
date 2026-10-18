using Microsoft.AspNetCore.Http;

namespace LoanSystem.Services.Models;

public class RestoreBackupModel : BaseModel
{
    public IFormFile File { get; set; }

    public string FileName { get; set; }
}
