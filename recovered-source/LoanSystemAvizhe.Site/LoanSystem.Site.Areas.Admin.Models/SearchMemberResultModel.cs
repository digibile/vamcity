using System;

namespace LoanSystem.Site.Areas.Admin.Models;

public class SearchMemberResultModel
{
    public Guid Id { get; set; }

    public string FullName { get; set; }

    public string NationalCode { get; set; }
}
