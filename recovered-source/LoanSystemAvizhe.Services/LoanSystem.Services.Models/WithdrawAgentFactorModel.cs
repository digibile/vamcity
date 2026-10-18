using System;
using LoanSystem.Core;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Models;

public class WithdrawAgentFactorModel : BaseModel
{
    public Guid AgentFactorId { get; set; }

    public int AgentFactorCode { get; set; }

    public string AgentFactorUserCode { get; set; }

    public DateTime AgentFactorCreateAt { get; set; }

    public PersianDate CreateAt_Persian => AgentFactorCreateAt.ToPersianDateObject();

    public decimal AgentFactorAmount { get; set; }

    public TransactionModel Withdraw { get; set; }

    public WithdrawAgentFactorModel()
    {
    }

    public WithdrawAgentFactorModel(WithdrawAgentFactor withdrawAgentFactor)
    {
        Id = withdrawAgentFactor.Id;
        if (withdrawAgentFactor.AgentFactor != null)
        {
            AgentFactorCode = withdrawAgentFactor.AgentFactor.Code;
            AgentFactorUserCode = withdrawAgentFactor.AgentFactor.UserCode;
            AgentFactorAmount = withdrawAgentFactor.AgentFactor.Amount;
            AgentFactorCreateAt = withdrawAgentFactor.AgentFactor.CreateAt;
        }
        if (withdrawAgentFactor.Withdraw != null)
        {
            Withdraw = new TransactionModel(withdrawAgentFactor.Withdraw);
        }
    }
}
