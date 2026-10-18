using LoanSystem.Data.Entities;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Services.Models;

public class InquirySettingsModel : BaseModel
{
    public AppInquiryType AppInquiryType { get; set; }

    public decimal ApiIR_PriceFrom { get; set; }

    public decimal ApiIR_PriceTo { get; set; }

    public bool ApiIR_IsActive { get; set; }

    public InquirySettingsModel()
    {
    }

    public InquirySettingsModel(InquirySettings inquirySettings)
    {
        Id = inquirySettings.Id;
        AppInquiryType = inquirySettings.AppInquiryType;
        ApiIR_IsActive = inquirySettings.ApiIR_IsActive;
        ApiIR_PriceTo = inquirySettings.ApiIR_PriceTo;
        ApiIR_PriceFrom = inquirySettings.ApiIR_PriceFrom;
    }
}
