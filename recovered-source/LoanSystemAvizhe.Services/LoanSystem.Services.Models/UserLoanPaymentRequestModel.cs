using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using LoanSystem.Core;
using LoanSystem.Data.Entities;
using LoanSystemAvizhe.Data.Enums;
using LoanSystemAvizhe.Services.FileHelper;
using Microsoft.AspNetCore.Http;

namespace LoanSystem.Services.Models;

public class UserLoanPaymentRequestModel : BaseModel
{
    [Required(ErrorMessage = "شماره قسط الزامی است.")]
    public LoanPaymentModel LoanPayment { get; set; }

    public DateTime RequestDate { get; set; }

    public PersianDate RequestDate_Persian => RequestDate.ToPersianDateObject();

    [Required(ErrorMessage = "تاریخ واریز الزامی است.")]
    public DateTime PaymentDate { get; set; }

    public PersianDate PaymentDate_Persian => PaymentDate.ToPersianDateObject();

    [Range(0.0, double.MaxValue, ErrorMessage = "مبلغ نمی تواند کمتر از صفر باشد")]
    public decimal Amount { get; set; }

    public string MemberDescription { get; set; }

    public string AdminDescription { get; set; }

    public UserModel AdminAcceptor { get; set; }

    public DateTime AdminAcceptDate { get; set; }

    public PersianDate AdminAcceptDate_Persian => AdminAcceptDate.ToPersianDateObject();

    public string TrackNumber { get; set; }

    public PaymentMethods PaymentMethod { get; set; }

    public UserLoanPaymentRequestStatus Status { get; set; }

    public string Str_Status => Status.ToString().Replace("_", " ");

    public bool AffectOnMainPayment { get; set; }

    public bool SendSms { get; set; }

    public string FileName { get; set; }

    public string Base64 { get; set; }

    public IFormFile File { get; set; }

    public UserLoanPaymentRequestModel()
    {
    }

    public UserLoanPaymentRequestModel(UserLoanPaymentRequest userLoanPaymentRequest)
    {
        Id = userLoanPaymentRequest.Id;
        LoanPayment = new LoanPaymentModel(userLoanPaymentRequest.UserLoanPayment);
        RequestDate = userLoanPaymentRequest.RequestDate;
        PaymentDate = userLoanPaymentRequest.PaymentDate;
        Amount = userLoanPaymentRequest.Amount;
        MemberDescription = userLoanPaymentRequest.MemberDescription;
        AdminDescription = userLoanPaymentRequest.AdminDescription;
        Status = userLoanPaymentRequest.Status;
        PaymentMethod = userLoanPaymentRequest.PaymentMethod;
        TrackNumber = userLoanPaymentRequest.TrackNumber;
        AdminAcceptDate = userLoanPaymentRequest.AdminAcceptDate;
        AffectOnMainPayment = userLoanPaymentRequest.AffectOnMainPayment;
        FileName = userLoanPaymentRequest.FileName;
        if (userLoanPaymentRequest.AdminAcceptorId.HasValue)
        {
            AdminAcceptor = new UserModel
            {
                Id = userLoanPaymentRequest.AdminAcceptorId.Value,
                Email = userLoanPaymentRequest.AdminAcceptor?.Email,
                FirstName = userLoanPaymentRequest.AdminAcceptor?.FirstName
            };
        }
    }

    public void FillImageBase64Prop(IFileHelper fileHelper, Guid customerId)
    {
        if (!string.IsNullOrEmpty(FileName) && !(customerId == Guid.Empty))
        {
            string relativePath = Path.Combine(Directories.UserLoanPaymentRequest(customerId.ToString()), FileName);
            string text = fileHelper.ReadFileAsBase64(relativePath);
            if (!string.IsNullOrEmpty(text))
            {
                Base64 = "data:image/jpeg;base64," + text;
            }
        }
    }
}
