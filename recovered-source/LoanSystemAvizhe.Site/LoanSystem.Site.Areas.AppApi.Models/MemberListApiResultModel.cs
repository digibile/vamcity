using System.Collections.Generic;

namespace LoanSystem.Site.Areas.AppApi.Models;

public class MemberListApiResultModel
{
	public List<AppApiMemberModel> Members { get; set; }

	public int CountAll { get; set; }
}
