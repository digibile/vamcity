using System.Collections.Generic;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Services.Models;

public class SendThirdPartyTransferModel
{
    public string EntityType { get; set; }

    public List<string> EntityIds { get; set; }

    public ThirdPart ThirdParty { get; set; }
}
