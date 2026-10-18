using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using LoanSystem.Core;
using LoanSystem.Data.Entities;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Services.Models;

public class UserLoanModel : BaseModel
{
    public int Code { get; set; }

    public Guid? LoanId { get; set; }

    public Loan2Model LoanModel { get; set; }

    public long? MemberId { get; set; }

    public Member2Model MemberModel { get; set; }

    public AgentModel AgentModel { get; set; }

    public DateTime? AgentDueDate { get; set; }

    public PersianDate AgentDueDate_Persian
    {
        get
        {
            if (!AgentDueDate.HasValue)
            {
                return new PersianDate();
            }
            return AgentDueDate.Value.ToPersianDateObject();
        }
    }

    public AgentModel SubAgentModel { get; set; }

    public DateTime? SubAgentDueDate { get; set; }

    public PersianDate SubAgentDueDate_Persian
    {
        get
        {
            if (!SubAgentDueDate.HasValue)
            {
                return new PersianDate();
            }
            return SubAgentDueDate.Value.ToPersianDateObject();
        }
    }

    public DateTime? DepositChargeDueDate { get; set; }

    [Range(0.0, double.MaxValue, ErrorMessage = "مبلغ پیش پرداخت نمی تواند کمتر از صفر باشد")]
    public decimal PreDepositAmount { get; set; }

    public decimal? CustomProfitPercent { get; set; }

    public PersianDate DepositChargeDueDate_Persian
    {
        get
        {
            if (!DepositChargeDueDate.HasValue)
            {
                return new PersianDate();
            }
            return DepositChargeDueDate.Value.ToPersianDateObject();
        }
    }

    public DateTime StartDate { get; set; }

    public PersianDate StartDate_Persian => StartDate.ToPersianDateObject();

    public decimal PayableAmount { get; set; }

    [Range(0.0, double.MaxValue, ErrorMessage = "سهم نماینده نمی تواند کمتر از صفر باشد")]
    public decimal AgentPrice { get; set; }

    [Range(0.0, double.MaxValue, ErrorMessage = "سهم نماینده نمی تواند کمتر از صفر باشد")]
    public decimal SubAgentPrice { get; set; }

    [Range(0.0, double.MaxValue, ErrorMessage = "مبلغ وام نمی تواند کمتر از صفر باشد")]
    public decimal Amount { get; set; }

    [Range(0.0, double.MaxValue, ErrorMessage = "مبلغ قسط نمی تواند کمتر از صفر باشد")]
    public decimal Installment { get; set; }

    public int Month { get; set; }

    public int MonthGap { get; set; } = 1;

    public decimal Paid { get; set; }

    [Range(0.0, double.MaxValue, ErrorMessage = "مبلغ تخفیف نمی تواند کمتر از صفر باشد")]
    public decimal Discount { get; set; }

    public decimal Remain
    {
        get
        {
            if (IsCheckouted.HasValue && IsCheckouted.Value)
            {
                return 0m;
            }
            return PayableAmount + Delays - Discount - Paid;
        }
    }

    public decimal PaidCount { get; set; }

    public bool? IsCheckouted { get; set; }

    public DateTime? CheckoutDate { get; set; }

    public string Description { get; set; }

    public string Status { get; set; }

    public bool IsCanceled { get; set; }

    public UserLoanRequestModel UserLoanRequest { get; set; }

    public PersianDate CheckoutDate_Persian
    {
        get
        {
            if (!CheckoutDate.HasValue)
            {
                return new PersianDate();
            }
            return CheckoutDate.Value.ToPersianDateObject();
        }
    }

    public List<LoanPaymentModel> LoanPayments { get; set; }

    public decimal Delays
    {
        get
        {
            if (LoanPayments == null)
            {
                return 0m;
            }
            return Math.Round(LoanPayments.Sum((LoanPaymentModel x) => x.DelayPrice * (decimal)((x.DelayDay >= 0) ? 1 : (-1))));
        }
    }

    public decimal SumSponsorPayments { get; set; }

    public decimal SumParentAgentPayments { get; set; }

    public AgentLoanRequestChargeTypes ChargeType { get; set; }

    public string Str_ChargeType => ChargeType.ToString().Replace("_", " ");

    public DateTime CreateAt { get; set; }

    public long RefahBetaInquiryAmount { get; set; }

    public long RefahBetaOrderId { get; set; }

    public string SecondaryCode { get; set; }

    public List<UserLoanContactModel> UserLoanContacts { get; set; } = new List<UserLoanContactModel>();

    public List<UserLoanSponsoreDueDateModel> UserLoanSponsoreDueDates { get; set; } = new List<UserLoanSponsoreDueDateModel>();

    public List<AppSmsModel> AppSmsList { get; set; }

    public UserLoanModel()
    {
    }

    public UserLoanModel(UserLoan userLoan)
    {
        Id = userLoan.Id;
        Code = userLoan.Code;
        if (userLoan.Loan != null)
        {
            LoanModel = new Loan2Model
            {
                Id = userLoan.LoanId,
                LoanProfitType = userLoan.Loan.LoanProfitType,
                Title = userLoan.Loan?.Title,
                DelayPercent = userLoan.Loan.DelayPercent,
                EarlyPercent = userLoan.Loan.EarlyPercent,
                ProfitPercent = userLoan.Loan.ProfitPercent,
                IsSponsored = userLoan.Loan.IsSponsored,
                AllowedInstallments = userLoan.Loan.AllowedInstallments
            };
        }
        if (userLoan.User != null)
        {
            MemberModel = new Member2Model(userLoan.User);
        }
        if (userLoan.UserLoanRequest != null)
        {
            UserLoanRequest = new UserLoanRequestModel(userLoan.UserLoanRequest);
        }
        if (userLoan.Agent != null)
        {
            AgentModel = new AgentModel(userLoan.Agent);
        }
        AgentPrice = userLoan.AgentPrice;
        AgentDueDate = userLoan.AgentDueDate;
        if (userLoan.SubAgent != null)
        {
            SubAgentModel = new AgentModel(userLoan.SubAgent);
        }
        LoanId = userLoan.LoanId;
        MemberId = userLoan.UserId;
        SubAgentPrice = userLoan.SubAgentPrice;
        SubAgentDueDate = userLoan.SubAgentDueDate;
        CustomProfitPercent = userLoan.CustomProfitPercent;
        StartDate = userLoan.StartDate;
        PayableAmount = userLoan.PayableAmount;
        Amount = userLoan.Amount;
        Installment = userLoan.Installment;
        Month = userLoan.Month;
        MonthGap = userLoan.MonthGap;
        Paid = userLoan.Paid;
        Discount = userLoan.Discount;
        IsCheckouted = userLoan.IsCheckouted;
        PreDepositAmount = userLoan.PreDepositAmount;
        CheckoutDate = userLoan.CheckoutDate;
        DepositChargeDueDate = userLoan.DepositChargeDueDate;
        Description = userLoan.Description;
        Status = userLoan.Status;
        SecondaryCode = userLoan.SecondaryCode;
        CreateAt = userLoan.CreateAt;
        IsCanceled = userLoan.IsCanceled;
        RefahBetaInquiryAmount = userLoan.RefahBetaInquiryAmount;
        RefahBetaOrderId = userLoan.RefahBetaOrderId;
        UserLoanContacts = userLoan.UserLoanContacts.Select((UserLoanContact x) => new UserLoanContactModel(x)).ToList();
        UserLoanSponsoreDueDates = userLoan.UserLoanSponsoreDueDates.Select((UserLoanSponsoreDueDate x) => new UserLoanSponsoreDueDateModel(x)).ToList();
        AppSmsList = new List<AppSmsModel>();
    }

    public decimal CalcPayableAmount()
    {
        if (LoanModel == null)
        {
            throw new NullReferenceException("LoanModel");
        }
        return (decimal)Month * Installment;
    }
}
