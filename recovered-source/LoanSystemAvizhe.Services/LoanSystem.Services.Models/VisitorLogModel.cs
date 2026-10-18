using System;
using LoanSystem.Core;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Models;

public class VisitorLogModel : BaseModel
{
    public DateTime Date { get; set; }

    public PersianDate Date_Persian => Date.ToPersianDateObject();

    public string IpAddress { get; set; }

    public string UserAgent { get; set; }

    public string Mrk { get; set; }

    public VisitorLogModel(VisitorLog visitorLog)
    {
        Id = visitorLog.Id;
        Date = visitorLog.Date;
        IpAddress = visitorLog.IpAddress;
        UserAgent = visitorLog.UserAgent;
        Mrk = visitorLog.Mrk;
    }
}
