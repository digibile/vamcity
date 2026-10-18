namespace LoanSystem.Services.Models.IranValidity;

public class IranValidityStatusResultData
{
    public string StatusTitle { get; set; }

    public string Status { get; set; }

    public string ReportLink { get; set; }

    public string OTPMessageStatusTitle { get; set; }

    public MessageStatus OTPMessageStatus { get; set; }
}
