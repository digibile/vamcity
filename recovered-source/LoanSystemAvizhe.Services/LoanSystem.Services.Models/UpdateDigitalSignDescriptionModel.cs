using System;
using System.Collections.Generic;

namespace LoanSystem.Services.Models;

public class UpdateDigitalSignDescriptionModel : BaseModel
{
    public Guid ThirdPersonAssuranceAuthorizationItemId { get; set; }

    public Guid UserLoanRequestAuthorizationItemId { get; set; }

    public Guid EmzameContractAuthorizationItemId { get; set; }

    public Dictionary<string, string> ManualFields { get; set; } = new Dictionary<string, string>();
}
