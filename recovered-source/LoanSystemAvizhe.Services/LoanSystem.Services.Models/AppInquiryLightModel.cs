using System;
using LoanSystem.Core;
using LoanSystem.Data.Entities;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Services.Models;

public class AppInquiryLightModel : BaseModel
{
    public DateTime Date { get; set; }

    public PersianDate Date_Persian => Date.ToPersianDateObject();

    public AppInquiryType AppInquiryType { get; set; }

    public string AppInquiryTypeText => AppInquiryType.ToString();

    public bool IsSuccess { get; set; }

    public string PanelResultJson { get; set; }

    public string Result { get; set; }

    public AppInquiryLightModel()
    {
    }

    public AppInquiryLightModel(AppInquiry appInquiry)
    {
        Id = appInquiry.Id;
        Date = appInquiry.Date;
        AppInquiryType = appInquiry.AppInquiryType;
        IsSuccess = appInquiry.IsSuccess;
        PanelResultJson = appInquiry.PanelResultJson;
        Result = appInquiry.Result;
    }
}
