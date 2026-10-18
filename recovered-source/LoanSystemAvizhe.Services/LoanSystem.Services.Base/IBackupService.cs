using System.Collections.Generic;
using System.Threading.Tasks;
using LoanSystem.Services.Models;

namespace LoanSystem.Services.Base;

public interface IBackupService
{
    Task<string> Create();

    Task<List<dynamic>> GetBackupList();

    byte[] Download(string fileName);

    Task Restore(RestoreBackupModel model);
}
