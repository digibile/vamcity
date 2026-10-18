using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using LoanSystem.Core;
using LoanSystem.Data.Entities;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Services.Models;

public class LoanPaymentModel : BaseModel
{
    public int Code { get; set; }

    public UserLoanModel UserLoanModel { get; set; }

    public int InstallmentNumber { get; set; }

    public decimal InstallmentAmount { get; set; }

    [Range(0.0, double.MaxValue, ErrorMessage = "مجموع مبالغ نمی تواند کمتر از صفر باشد")]
    public decimal SumAmount { get; set; }

    [Range(0.0, double.MaxValue, ErrorMessage = "مبلغ نمی تواند کمتر از صفر باشد")]
    public decimal Amount { get; set; }

    public int DelayDay { get; set; }

    public decimal DelayPrice { get; set; }

    public string SavedBy { get; set; }

    public DateTime SaveDate { get; set; }

    public PersianDate SaveDate_Persian => SaveDate.ToPersianDateObject();

    public DateTime TargetMonth { get; set; }

    public PersianDate TargetMonth_Persian => TargetMonth.ToPersianDateObject();

    public PaymentMethods PaymentMethod { get; set; }

    public double GoldWeight { get; set; }

    public double GoldRate { get; set; }

    public bool IsGoldCheckouted { get; set; }

    public string Description { get; set; }

    public bool SendSms { get; set; }

    public Guid? UserLoanPaymentRequestId { get; set; }

    public List<AppSmsModel> AppSmsList { get; set; }

    public virtual List<MemberPurchaseModel> MemberPurchases { get; set; }

    public virtual List<LoanPaymentOtherModel> LoanPaymentOthers { get; set; }

    public LoanPaymentModel()
    {
    }

    public LoanPaymentModel(UserLoanPayment userLoanPayment)
    {
        Id = userLoanPayment.Id;
        Code = userLoanPayment.Code;
        UserLoanModel = new UserLoanModel(userLoanPayment.UserLoan);
        SumAmount = userLoanPayment.SumAmount;
        Amount = userLoanPayment.Amount;
        SavedBy = userLoanPayment.SavedBy;
        SaveDate = userLoanPayment.SaveDate;
        TargetMonth = userLoanPayment.TargetMonth;
        InstallmentAmount = userLoanPayment.InstallmentAmount;
        InstallmentNumber = userLoanPayment.InstallmentNumber;
        DelayDay = userLoanPayment.DelayDay;
        DelayPrice = userLoanPayment.DelayPrice;
        PaymentMethod = userLoanPayment.PaymentMethod;
        GoldRate = userLoanPayment.GoldRate;
        GoldWeight = userLoanPayment.GoldWeight;
        Description = userLoanPayment.Description;
        IsGoldCheckouted = userLoanPayment.IsGoldCheckouted;
        AppSmsList = new List<AppSmsModel>();
        ICollection<MemberPurchase> memberPurchases = userLoanPayment.MemberPurchases;
        if (memberPurchases != null && memberPurchases.Any())
        {
            MemberPurchases = userLoanPayment.MemberPurchases.Select((MemberPurchase x) => new MemberPurchaseModel(x)).ToList();
        }
        ICollection<UserLoanPaymentOther> userLoanPaymentOthers = userLoanPayment.UserLoanPaymentOthers;
        if (userLoanPaymentOthers != null && userLoanPaymentOthers.Any())
        {
            LoanPaymentOthers = userLoanPayment.UserLoanPaymentOthers.Select((UserLoanPaymentOther x) => new LoanPaymentOtherModel(x)).ToList();
        }
        UserLoanPaymentRequestId = userLoanPayment.UserLoanPaymentRequests?.FirstOrDefault()?.Id;
    }
}
