using System.Collections.Generic;
using LoanSystem.Services.Models;

namespace LoanSystemAvizhe.Site.Areas.SuperAdmin.Models;

public class SuperAdminConsultant
{
    public List<ConsultantModel> Consultants { get; set; }

    public int CountAll { get; set; }
}
