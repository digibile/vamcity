using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using LoanSystem.Core;
using LoanSystem.Data.Entities;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Services.Models;

public class AgentFactorModel : BaseModel
{
    public int Code { get; set; }

    public string UserCode { get; set; }

    [Required(ErrorMessage = "مشتری الزامی است.")]
    public Member2Model Member { get; set; }

    [Required(ErrorMessage = "نماینده الزامی است.")]
    public AgentModel Agent { get; set; }

    public DateTime CreateAt { get; set; }

    public PersianDate CreateAt_Persian => CreateAt.ToPersianDateObject();

    public AgentFactorCheckoutTypes CheckoutType { get; set; }

    public string Str_CheckoutType => CheckoutType.ToString().Replace("_", " ");

    [Range(0.0, double.MaxValue, ErrorMessage = "مبلغ نمی تواند کمتر از صفر باشد")]
    public decimal Amount { get; set; }

    [StringLength(1000, MinimumLength = 0, ErrorMessage = "طول کاراکترهای توضیحات نباید بیشتر از 1000 باشد")]
    public string Description { get; set; }

    [StringLength(1000, MinimumLength = 0, ErrorMessage = "طول کاراکترهای توضیحات مدیر نباید بیشتر از 1000 باشد")]
    public string AdminDescription { get; set; }

    public AdminAcceptStatus AdminAcceptStatus { get; set; }

    public string Str_AdminAcceptStatus => AdminAcceptStatus.ToString().Replace("_", " ");

    public MemberPurchaseModel MemberPurchase { get; set; }

    public string MobileOtp { get; set; }

    public List<WithdrawAgentFactorModel> Withdraws { get; set; }

    public decimal SumWithdraws
    {
        get
        {
            decimal val = Withdraws?.Sum((WithdrawAgentFactorModel x) => x.Withdraw.Amount) ?? 0m;
            decimal val2;
            if (MemberPurchase != null)
            {
                val2 = MemberPurchase.Amount - Commission;
            }
            else
            {
                val2 = ((CheckoutType != AgentFactorCheckoutTypes.آنلاین) ? (Amount - Commission) : 0m);
            }
            return Math.Min(val, val2);
        }
    }

    public decimal Commission { get; set; }

    public DateTime CheckoutRequestDateTime { get; set; }

    public AgentFactorOnlinePaymentMethod AgentFactorOnlinePaymentMethod { get; set; }

    public List<AgentFactorItemModel> AgentFactorItems { get; set; }

    public AgentFactorModel()
    {
    }

    public AgentFactorModel(AgentFactor agentFactor)
    {
        Id = agentFactor.Id;
        Code = agentFactor.Code;
        UserCode = agentFactor.UserCode;
        CheckoutType = agentFactor.CheckoutType;
        Amount = agentFactor.Amount;
        CreateAt = agentFactor.CreateAt;
        Amount = agentFactor.Amount;
        Description = agentFactor.Description;
        AdminDescription = agentFactor.AdminDescription;
        AdminAcceptStatus = agentFactor.AdminAcceptStatus;
        Commission = agentFactor.Commission;
        CheckoutRequestDateTime = agentFactor.CheckoutRequestDateTime;
        AgentFactorOnlinePaymentMethod = agentFactor.AgentFactorOnlinePaymentMethod;
        if (agentFactor.Member != null)
        {
            Member = new Member2Model(agentFactor.Member);
        }
        if (agentFactor.Agent != null)
        {
            Agent = new AgentModel(agentFactor.Agent);
        }
        if (agentFactor.MemberPurchase != null)
        {
            MemberPurchase = new MemberPurchaseModel(agentFactor.MemberPurchase);
        }
        ICollection<WithdrawAgentFactor> withdrawAgentFactors = agentFactor.WithdrawAgentFactors;
        if (withdrawAgentFactors != null && withdrawAgentFactors.Any())
        {
            Withdraws = agentFactor.WithdrawAgentFactors.Select((WithdrawAgentFactor x) => new WithdrawAgentFactorModel(x)).ToList();
        }
        ICollection<AgentFactorItem> agentFactorItems = agentFactor.AgentFactorItems;
        if (agentFactorItems != null && agentFactorItems.Any())
        {
            AgentFactorItems = agentFactor.AgentFactorItems.Select((AgentFactorItem x) => new AgentFactorItemModel(x)).ToList();
        }
    }
}
