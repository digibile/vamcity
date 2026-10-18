using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using LoanSystem.Core;
using LoanSystem.Data.Entities;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Services.Models;

public class UserLoanRequestModel : BaseModel
{
    public int Code { get; set; }

    [Required(ErrorMessage = "نوع وام الزامی است.")]
    public Loan2Model LoanModel { get; set; }

    public Member2Model MemberModel { get; set; }

    public AgentModel AgentModel { get; set; }

    public SponsorModel SponsorModel { get; set; }

    public DateTime RequestDate { get; set; }

    public PersianDate RequestDate_Persian => RequestDate.ToPersianDateObject();

    public DateTime SponsoreDueDate { get; set; }

    public PersianDate SponsoreDueDate_Persian => SponsoreDueDate.ToPersianDateObject();

    public decimal SponsoreAmount { get; set; }

    [Range(0.0, double.MaxValue, ErrorMessage = "مبلغ وام نمی تواند کمتر از صفر باشد")]
    public decimal Amount { get; set; }

    public decimal Installment { get; set; }

    [Range(1.0, double.MaxValue, ErrorMessage = "تعداد اقساط نمی تواند کمتر از یک باشد")]
    public int InstallmentCount { get; set; }

    [Range(1.0, double.MaxValue, ErrorMessage = "فاصله بین اقساط نمی تواند کمتر از یک باشد")]
    public int MonthGap { get; set; } = 1;

    [StringLength(1000, MinimumLength = 0, ErrorMessage = "طول کاراکترهای توضیحات نباید بیشتر از 1000 باشد")]
    public string Comment { get; set; }

    [StringLength(1000, MinimumLength = 0, ErrorMessage = "طول کاراکترهای توضیحات نباید بیشتر از 1000 باشد")]
    public string AdminDescription { get; set; }

    [StringLength(1000, MinimumLength = 0, ErrorMessage = "طول کاراکترهای توضیحات نباید بیشتر از 1000 باشد")]
    public string AdminDescriptionPrivate { get; set; }

    public LoanRequestStatusModel Status { get; set; } = new LoanRequestStatusModel();

    public string Str_Status => Status?.Title ?? "";

    public LoanRequestStatusModel AgentStatus { get; set; }

    public string Str_AgentStatus => AgentStatus?.Title ?? "";

    public bool IsPhysicalDocsDelivered { get; set; }

    public int Month { get; set; }

    public int Year { get; set; }

    public UserLoanModel UserLoan { get; set; }

    public long UserId { get; set; }

    public List<UserLoanRequestAuthorizationItemModel> UserLoanRequestAuthorizationItems { get; set; }

    public AgentLoanRequestChargeTypes ChargeType { get; set; }

    public string Str_ChargeType => ChargeType.ToString().Replace("_", " ");

    public OtherPercent OtherPercent { get; set; }

    public LockedByModel LockedBy { get; set; }

    public long RefahBetaInquiryAmount { get; set; }

    public long RefahBetaOrderId { get; set; }

    public string RefahOtp { get; set; }

    public string Products { get; set; }

    public UserLoanSponsorStatuses UserLoanSponsorStatus { get; set; }

    public string Str_UserLoanSponsorStatus => UserLoanSponsorStatus.ToString().Replace("_", " ");

    public string LastSponsorComment { get; set; }

    public int SponsorHistoryCount { get; set; }

    public UserLoanRequestModel()
    {
    }

    public UserLoanRequestModel(UserLoanRequest userLoanRequest)
    {
        Id = userLoanRequest.Id;
        Code = userLoanRequest.Code;
        if (userLoanRequest.Loan != null)
        {
            LoanModel = new Loan2Model
            {
                Id = userLoanRequest.LoanId,
                Title = userLoanRequest.Loan?.Title,
                DelayPercent = userLoanRequest.Loan.DelayPercent,
                EarlyPercent = userLoanRequest.Loan.EarlyPercent,
                AllowedInstallments = userLoanRequest.Loan.AllowedInstallments,
                ProfitPercent = userLoanRequest.Loan.ProfitPercent,
                Disable = userLoanRequest.Loan.Disable,
                DisableText = userLoanRequest.Loan.DisableText,
                IsSponsored = userLoanRequest.Loan.IsSponsored
            };
        }
        if (userLoanRequest.User != null)
        {
            MemberModel = new Member2Model(userLoanRequest.User);
        }
        RequestDate = userLoanRequest.RequestDate;
        Amount = userLoanRequest.Amount;
        Installment = userLoanRequest.Installment;
        Comment = userLoanRequest.Comment;
        AdminDescription = userLoanRequest.AdminDescription;
        AdminDescriptionPrivate = userLoanRequest.AdminDescriptionPrivate;
        Month = userLoanRequest.Month;
        Year = userLoanRequest.Year;
        InstallmentCount = userLoanRequest.InstallmentCount;
        MonthGap = userLoanRequest.MonthGap;
        IsPhysicalDocsDelivered = userLoanRequest.IsPhysicalDocsDelivered;
        SponsoreDueDate = userLoanRequest.SponsoreDueDate;
        SponsoreAmount = userLoanRequest.SponsoreAmount;
        ChargeType = userLoanRequest.ChargeType;
        RefahBetaInquiryAmount = userLoanRequest.RefahBetaInquiryAmount;
        RefahBetaOrderId = userLoanRequest.RefahBetaOrderId;
        Products = userLoanRequest.Products;
        LastSponsorComment = userLoanRequest.LastSponsorComment;
        UserLoanSponsorStatus = userLoanRequest.UserLoanSponsorStatus;
        if (!string.IsNullOrEmpty(userLoanRequest.LockedBy))
        {
            LockedBy = JsonSerializer.Deserialize<LockedByModel>(userLoanRequest.LockedBy);
        }
        if (userLoanRequest.Agent != null)
        {
            AgentModel = new AgentModel(userLoanRequest.Agent);
        }
        if (userLoanRequest.Sponsor != null)
        {
            SponsorModel = new SponsorModel(userLoanRequest.Sponsor);
        }
        if (userLoanRequest.Status != null)
        {
            Status = new LoanRequestStatusModel(userLoanRequest.Status);
        }
        if (userLoanRequest.AgentStatus != null)
        {
            AgentStatus = new LoanRequestStatusModel(userLoanRequest.AgentStatus);
        }
        OtherPercent = ((userLoanRequest.OtherPercent_CheckoutDays > 0 && userLoanRequest.OtherPercent_Percent > 0m) ? new OtherPercent
        {
            CheckoutDays = userLoanRequest.OtherPercent_CheckoutDays,
            Percent = userLoanRequest.OtherPercent_Percent
        } : null);
    }
}
