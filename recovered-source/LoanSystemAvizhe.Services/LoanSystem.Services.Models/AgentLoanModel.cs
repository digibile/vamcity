using System;
using System.Collections.Generic;
using System.Text.Json;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Models;

public class AgentLoanModel
{
    public Guid Id { get; set; }

    public long AgentId { get; set; }

    public Guid LoanId { get; set; }

    public decimal Percent { get; set; }

    public byte DefaultCheckoutDays { get; set; }

    public decimal ProductPercent { get; set; }

    public byte ProductCheckoutDays { get; set; }

    public List<OtherPercent> OtherPercents { get; set; } = new List<OtherPercent>();

    public decimal ContactPercent { get; set; }

    public ContactModel Contact { get; set; }

    public AgentLoanModel()
    {
    }

    public AgentLoanModel(AgentLoan agentLoan)
    {
        Id = agentLoan.Id;
        AgentId = agentLoan.AgentId;
        LoanId = agentLoan.LoanId;
        Percent = agentLoan.Percent;
        DefaultCheckoutDays = agentLoan.DefaultCheckoutDays;
        ProductPercent = agentLoan.ProductPercent;
        ProductCheckoutDays = agentLoan.ProductCheckoutDays;
        ContactPercent = agentLoan.ContactPercent;
        if (agentLoan.Contact != null)
        {
            Contact = new ContactModel(agentLoan.Contact);
        }
        if (!string.IsNullOrWhiteSpace(agentLoan.OtherPercentsJson))
        {
            OtherPercents = JsonSerializer.Deserialize<List<OtherPercent>>(agentLoan.OtherPercentsJson);
        }
    }
}
