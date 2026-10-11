using System.Collections.Generic;

namespace LoanSystem.Services.Models;

public class BackupEntityModel
{
	public string EntityName { get; set; }

	public List<dynamic> Rows { get; set; }
}
