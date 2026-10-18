using System;
using LoanSystem.Services.Models;

namespace LoanSystemAvizhe.Site.Areas.AppApi.Models;

public class AppApiUserLoanResponseModel : BaseModel
{
    public new Guid Id { get; set; }

    public int Code { get; set; }
}
