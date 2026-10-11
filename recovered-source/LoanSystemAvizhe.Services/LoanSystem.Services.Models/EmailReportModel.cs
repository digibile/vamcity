namespace LoanSystem.Services.Models;

public class EmailReportModel
{
	public EmailPropsReportModel Daily { get; set; } = new EmailPropsReportModel();

	public EmailPropsReportModel Weekly { get; set; } = new EmailPropsReportModel();

	public EmailPropsReportModel Monthly { get; set; } = new EmailPropsReportModel();
}
