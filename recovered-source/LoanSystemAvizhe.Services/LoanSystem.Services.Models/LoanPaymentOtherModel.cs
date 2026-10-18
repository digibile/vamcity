using System;
using System.ComponentModel.DataAnnotations;
using LoanSystem.Core;
using LoanSystem.Data.Entities;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Services.Models;

public class LoanPaymentOtherModel : BaseModel
{
    [Range(0.0, double.MaxValue, ErrorMessage = "مبلغ نمی تواند کمتر از صفر باشد")]
    public decimal Amount { get; set; }

    public string SavedBy { get; set; }

    public DateTime SaveDate { get; set; }

    public PersianDate SaveDate_Persian => SaveDate.ToPersianDateObject();

    public PaymentMethods PaymentMethod { get; set; }

    public double GoldWeight { get; set; }

    public double GoldRate { get; set; }

    public bool IsGoldCheckouted { get; set; }

    public string Description { get; set; }

    public bool SendSms { get; set; }

    public Guid UserLoanPaymentId { get; set; }

    public MemberPurchaseModel MemberPurchase { get; set; }

    public LoanPaymentOtherModel()
    {
    }

    public LoanPaymentOtherModel(UserLoanPaymentOther userLoanPaymentOther)
    {
        Id = userLoanPaymentOther.Id;
        Amount = userLoanPaymentOther.Amount;
        SavedBy = userLoanPaymentOther.SavedBy;
        SaveDate = userLoanPaymentOther.SaveDate;
        PaymentMethod = userLoanPaymentOther.PaymentMethod;
        GoldRate = userLoanPaymentOther.GoldRate;
        GoldWeight = userLoanPaymentOther.GoldWeight;
        Description = userLoanPaymentOther.Description;
        IsGoldCheckouted = userLoanPaymentOther.IsGoldCheckouted;
        if (userLoanPaymentOther.MemberPurchase != null)
        {
            MemberPurchase = new MemberPurchaseModel(userLoanPaymentOther.MemberPurchase);
        }
        UserLoanPaymentId = userLoanPaymentOther.UserLoanPaymentId;
    }
}
