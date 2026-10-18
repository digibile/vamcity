using System.Collections.Generic;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystemAvizhe.Services.Models.DynamicQueryModel;

public class DynamicQueryForThirdPartyModel
{
    public string EntityType { get; set; }

    public string EntityId { get; set; }

    public ThirdPart ThirdParty { get; set; }

    public List<string> SortBy { get; set; }

    public List<bool> SortDesc { get; set; }

    public int Page { get; set; }

    public int ItemsPerPage { get; set; }

    public string Search { get; set; }
}
