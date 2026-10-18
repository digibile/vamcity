using System;

namespace LoanSystem.Services.Models.Ice;

public class IceAddIdentificationFromFirmResultData
{
    public DateTime ExpireDate { get; set; }

    public string ReportUrl { get; set; }

    public string ReportID { get; set; } = "";

    public string NationalCode { get; set; } = "";

    public string ResponseMessage { get; set; } = "";

    public string VerifyUrl { get; set; } = "";

    public ResponseStateEnum ResponseState { get; set; }
}
