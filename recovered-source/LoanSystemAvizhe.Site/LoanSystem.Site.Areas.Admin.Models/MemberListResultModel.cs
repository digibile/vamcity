using System.Collections.Generic;
using LoanSystem.Services.Models;

namespace LoanSystem.Site.Areas.Admin.Models;

public class MemberListResultModel
{
    public List<Member3Model> Members { get; set; }

    public int CountAll { get; set; }

    public int CountActives { get; set; }
}
