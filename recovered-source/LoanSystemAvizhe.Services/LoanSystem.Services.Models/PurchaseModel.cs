using System;
using System.ComponentModel.DataAnnotations;
using LoanSystem.Core;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Models.ZarinPal;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Services.Models;

public class PurchaseModel : BaseModel
{
    public ProductTypes ProductType { get; set; }

    public PaymentSteps PaymentStep { get; set; }

    public PurchasePaymentMethods PaymentMethod { get; set; }

    public User2Model User { get; set; }

    public CustomerModel Customer { get; set; }

    [Range(0.0, double.MaxValue, ErrorMessage = "مبلغ نمی تواند کمتر از صفر باشد")]
    public decimal Amount { get; set; }

    public string Description { get; set; }

    public string ZarinPalAuthority { get; set; }

    public string ZarinPalRefId { get; set; }

    public int ZarinPalStatus { get; set; }

    public bool IsPaymentSuccess { get; set; }

    public DateTime StartDate { get; set; }

    public PersianDate StartDate_Persian => StartDate.ToPersianDateObject();

    public string AffCode { get; set; }

    public int LoanCount { get; set; }

    public ZarinPalPurchase ZarinPalPurchase { get; set; }

    public string OnlinePaymentGatewayText
    {
        get
        {
            if (PaymentMethod == PurchasePaymentMethods.ZarinPal)
            {
                return "زرین پال";
            }
            return "نامشخص";
        }
    }

    public PurchaseModel()
    {
    }

    public PurchaseModel(Purchase purchase)
    {
        Id = purchase.Id;
        Amount = purchase.Amount;
        ProductType = purchase.ProductType;
        PaymentStep = purchase.PaymentStep;
        PaymentMethod = purchase.PaymentMethod;
        if (purchase.Customer != null)
        {
            Customer = new CustomerModel
            {
                Id = purchase.CustomerId,
                Name = purchase.Customer.Name
            };
        }
        if (purchase.User != null)
        {
            User = new User2Model
            {
                Id = purchase.UserId
            };
        }
        if (purchase.PaymentMethod == PurchasePaymentMethods.ZarinPal)
        {
            ZarinPalPurchase = new ZarinPalPurchase
            {
                Authority = purchase.ZarinPalAuthority,
                RefId = purchase.ZarinPalRefId,
                VerifyStatus = purchase.ZarinPalStatus
            };
        }
        Description = purchase.Description;
        ZarinPalAuthority = purchase.ZarinPalAuthority;
        ZarinPalRefId = purchase.ZarinPalRefId;
        IsPaymentSuccess = purchase.IsPaymentSuccess;
        ZarinPalStatus = purchase.ZarinPalStatus;
        StartDate = purchase.StartDate;
    }
}
