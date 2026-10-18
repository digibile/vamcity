using System;

namespace LoanSystem.Data.Entities;

public class IceReport : BaseEntity
{
    public DateTime Date { get; set; }

    public string Result { get; set; }

    public decimal Price { get; set; }

    public string Url { get; set; }

    public string ReportPdfFileName { get; set; }

    public string AttachmentFileName { get; set; }

    public string ReportID { get; set; }

    public string Status { get; set; }

    public string IceReportJson { get; set; }

    public string Errors { get; set; }

    public long MemberId { get; set; }

    public virtual User Member { get; set; }

    public Guid CustomerId { get; set; }

    public virtual Customer Customer { get; set; }
}
