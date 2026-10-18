namespace LoanSystem.Services.Models.Ice;

public class IceGetStatusResultData
{
    public string ReportLink { get; set; }

    public string PDFLink { get; set; }

    public ReportStateEnum ReportState { get; set; }
}
