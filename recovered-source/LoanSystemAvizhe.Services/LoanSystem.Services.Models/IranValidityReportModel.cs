using System;
using LoanSystem.Core;
using LoanSystem.Data.Entities;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Services.Models;

public class IranValidityReportModel : BaseModel
{
    public DateTime Date { get; set; }

    public PersianDate Date_Persian => Date.ToPersianDateObject();

    public string Result { get; set; }

    public int GradeNumber { get; set; }

    public decimal Price { get; set; }

    public string Url { get; set; }

    public string AttachmentFileName { get; set; }

    public string ReportPdfFileName { get; set; }

    public IranValidityReportStatus Status { get; set; }

    public string Status_Str => Status.ToString().Replace("_", " ");

    public string IranValidityReportJson { get; set; }

    public string Messages { get; set; }

    public string ReportID { get; set; }

    public Member3Model Member { get; set; }

    public Guid CustomerId { get; set; }

    public IranValidityReportModel()
    {
    }

    public IranValidityReportModel(IranValidityReport iranValidityiceReport)
    {
        Id = iranValidityiceReport.Id;
        Date = iranValidityiceReport.Date;
        Result = iranValidityiceReport.Result;
        GradeNumber = iranValidityiceReport.GradeNumber;
        Price = iranValidityiceReport.Price;
        Url = iranValidityiceReport.Url;
        Status = iranValidityiceReport.Status;
        ReportPdfFileName = iranValidityiceReport.ReportPdfFileName;
        AttachmentFileName = iranValidityiceReport.AttachmentFileName;
        IranValidityReportJson = iranValidityiceReport.IranValidityReportJson;
        Messages = iranValidityiceReport.Messages;
        ReportID = iranValidityiceReport.ReportID;
        if (iranValidityiceReport.Member != null)
        {
            Member = new Member3Model(iranValidityiceReport.Member);
        }
    }
}
