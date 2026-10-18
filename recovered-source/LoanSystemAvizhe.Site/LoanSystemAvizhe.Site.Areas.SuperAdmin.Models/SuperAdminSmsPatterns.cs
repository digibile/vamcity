using System.Collections.Generic;
using LoanSystem.Services.Models;

namespace LoanSystemAvizhe.Site.Areas.SuperAdmin.Models;

public class SuperAdminSmsPatterns
{
    public List<SmsPatternModel> SmsPatterns { get; set; }

    public int CountAll { get; set; }
}
