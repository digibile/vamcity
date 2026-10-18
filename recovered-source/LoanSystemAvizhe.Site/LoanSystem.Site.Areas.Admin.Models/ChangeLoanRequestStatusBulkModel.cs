using System;
using System.Collections.Generic;

namespace LoanSystem.Site.Areas.Admin.Models;

public class ChangeLoanRequestStatusBulkModel
{
    public List<Guid> Ids { get; set; }

    public Guid AgentStatusId { get; set; }

    public Guid StatusId { get; set; }
}
