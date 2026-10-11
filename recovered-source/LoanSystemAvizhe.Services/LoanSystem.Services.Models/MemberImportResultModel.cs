using System.Collections.Generic;

namespace LoanSystem.Services.Models;

public class MemberImportResultModel : BaseModel
{
	public int MemberNew { get; set; }

	public int MemberUpdate { get; set; }

	public int Failed { get; set; }

	public List<string> Errors { get; set; } = new List<string>();
}
