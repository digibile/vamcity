namespace LoanSystem.Site.Areas.Admin.Models;

public enum CreateTicketTargets
{
    AllMembers = 1,
    ActiveMembers,
    SpecificMembers,
    AllAgents,
    SpecificAgents,
    SuperAdmin
}
