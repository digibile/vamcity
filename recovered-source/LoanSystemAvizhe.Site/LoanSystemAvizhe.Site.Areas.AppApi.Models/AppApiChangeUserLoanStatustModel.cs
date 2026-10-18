using System;
using LoanSystem.Services.Models;

namespace LoanSystemAvizhe.Site.Areas.AppApi.Models;

public class AppApiChangeUserLoanStatustModel : BaseModel
{
    public new Guid Id { get; set; }

    public string Status { get; set; }
}
