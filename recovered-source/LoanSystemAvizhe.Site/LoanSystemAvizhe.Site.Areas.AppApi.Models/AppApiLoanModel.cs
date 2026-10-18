using System.ComponentModel.DataAnnotations;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Models;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystemAvizhe.Site.Areas.AppApi.Models;

public class AppApiLoanModel : BaseModel
{
    [Required(ErrorMessage = "عنوان وام الزامی است.")]
    public string Title { get; set; }

    [Required(ErrorMessage = "شماره وام الزامی است.")]
    public int Code { get; set; }

    [Range(0.0, double.MaxValue, ErrorMessage = "درصد بهره نمی تواند کمتر از صفر باشد")]
    public double ProfitPercent { get; set; }

    public LoanProfitType LoanProfitType { get; set; }

    [Range(0, 100, ErrorMessage = "درصد زودکرد نمی تواند کمتر از صفر باشد")]
    public double EarlyPercent { get; set; }

    [Range(0, 100, ErrorMessage = "درصد دیرکرد نمی تواند کمتر از صفر باشد")]
    public double DelayPercent { get; set; }

    public bool IsSponsored { get; set; }

    public bool IsSmsActive { get; set; }

    [Range(0.0, double.MaxValue, ErrorMessage = "حداکثر تعداد فاصله بین اقساط نمی تواند کمتر از صفر باشد")]
    public int? MaxMonthGap { get; set; }

    [Range(0.0, double.MaxValue, ErrorMessage = "حداکثر مبلغ وام نمی تواند کمتر از صفر باشد")]
    public decimal? MaxAmount { get; set; }

    public bool IncludePlan { get; set; }

    [StringLength(100, MinimumLength = 0, ErrorMessage = "طول کاراکترهای عنوان نباید بیشتر از 100 باشد")]
    public string PlanTitle { get; set; }

    [StringLength(1000, MinimumLength = 0, ErrorMessage = "طول کاراکترهای توضیحات کوتاه نباید بیشتر از 1000 باشد")]
    public string PlanShortDescription { get; set; } = "";

    public bool Disable { get; set; }

    public bool HasSponsore { get; set; }

    public int DefaultSponsoreCheckoutDays { get; set; }

    public string DisableText { get; set; }

    public AppApiLoanModel()
    {
    }

    public AppApiLoanModel(Loan loan)
    {
        Id = loan.Id;
        Title = loan.Title;
        Code = loan.Code;
        ProfitPercent = loan.ProfitPercent;
        LoanProfitType = loan.LoanProfitType;
        EarlyPercent = loan.EarlyPercent;
        DelayPercent = loan.DelayPercent;
        IsSponsored = loan.IsSponsored;
        IsSmsActive = loan.IsSmsActive;
        MaxAmount = loan.MaxAmount;
        MaxMonthGap = loan.MaxMonthGap;
        IncludePlan = loan.IncludePlan;
        PlanTitle = loan.PlanTitle;
        PlanShortDescription = loan.PlanShortDescription ?? "";
        Disable = loan.Disable;
        HasSponsore = loan.HasSponsore;
    }
}
