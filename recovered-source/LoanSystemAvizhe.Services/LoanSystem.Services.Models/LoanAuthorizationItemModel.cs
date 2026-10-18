using System.ComponentModel.DataAnnotations;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Models;

public class LoanAuthorizationItemModel : BaseModel
{
    [Required(ErrorMessage = "نوع مدرک الزامی است.")]
    public AuthorizationItemModel AuthorizationItem { get; set; }

    [Required(ErrorMessage = "وام الزامی است.")]
    public Loan2Model Loan { get; set; }

    public bool IsPrimary { get; set; }

    public CustomerModel Customer { get; set; }

    public LoanAuthorizationItemModel()
    {
    }

    public LoanAuthorizationItemModel(AgentLoanAuthorizationItem agentLoanAuthorizationItem)
    {
        Id = agentLoanAuthorizationItem.Id;
        if (agentLoanAuthorizationItem.AuthorizationItem != null)
        {
            AuthorizationItem = new AuthorizationItemModel(agentLoanAuthorizationItem.AuthorizationItem);
        }
        if (agentLoanAuthorizationItem.Loan != null)
        {
            Loan = new Loan2Model
            {
                Id = agentLoanAuthorizationItem.LoanId,
                Title = agentLoanAuthorizationItem.Loan.Title,
                DelayPercent = agentLoanAuthorizationItem.Loan.DelayPercent,
                EarlyPercent = agentLoanAuthorizationItem.Loan.EarlyPercent,
                ProfitPercent = agentLoanAuthorizationItem.Loan.ProfitPercent,
                IsSponsored = agentLoanAuthorizationItem.Loan.IsSponsored,
                AllowedInstallments = agentLoanAuthorizationItem.Loan.AllowedInstallments
            };
        }
        if (agentLoanAuthorizationItem.Customer != null)
        {
            Customer = new CustomerModel(agentLoanAuthorizationItem.Customer);
        }
    }

    public LoanAuthorizationItemModel(LoanAuthorizationItem loanAuthorizationItem)
    {
        Id = loanAuthorizationItem.Id;
        IsPrimary = loanAuthorizationItem.IsPrimary;
        if (loanAuthorizationItem.AuthorizationItem != null)
        {
            AuthorizationItem = new AuthorizationItemModel(loanAuthorizationItem.AuthorizationItem);
        }
        if (loanAuthorizationItem.Loan != null)
        {
            Loan = new Loan2Model
            {
                Id = loanAuthorizationItem.LoanId,
                Title = loanAuthorizationItem.Loan.Title,
                DelayPercent = loanAuthorizationItem.Loan.DelayPercent,
                EarlyPercent = loanAuthorizationItem.Loan.EarlyPercent,
                ProfitPercent = loanAuthorizationItem.Loan.ProfitPercent,
                IsSponsored = loanAuthorizationItem.Loan.IsSponsored,
                AllowedInstallments = loanAuthorizationItem.Loan.AllowedInstallments
            };
        }
        if (loanAuthorizationItem.Customer != null)
        {
            Customer = new CustomerModel(loanAuthorizationItem.Customer);
        }
    }
}
