using System;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Data.Entities;

public class IranValidityReport : BaseEntity
{
    public DateTime Date { get; set; }

    public string Result { get; set; }

    public int GradeNumber { get; set; }

    public decimal Price { get; set; }

    public string Url { get; set; }

    public string ReportPdfFileName { get; set; }

    public string AttachmentFileName { get; set; }

    public string ReportID { get; set; }

    public IranValidityReportStatus Status { get; set; }

    public string IranValidityReportJson { get; set; }

    public string Messages { get; set; }

    public long MemberId { get; set; }

    public virtual User Member { get; set; }

    public Guid CustomerId { get; set; }

    public virtual Customer Customer { get; set; }
}
