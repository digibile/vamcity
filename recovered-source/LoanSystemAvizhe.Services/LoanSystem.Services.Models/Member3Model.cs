using System;
using System.Collections.Generic;
using LoanSystem.Core;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Models;

public class Member3Model
{
    public long Id { get; set; }

    public string UserCode { get; set; }

    public string UserName { get; set; }

    public string FullName { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string Mobile { get; set; }

    public string NationalCode { get; set; }

    public decimal TotalLoans { get; set; }

    public decimal TotalRemain { get; set; }

    public decimal TotalSavedMoney { get; set; }

    public int LoanCount { get; set; }

    public DateTime RegDate2 { get; set; }

    public PersianDate RegDate2_Persian => RegDate2.ToPersianDateObject();

    public string SpreadInstallments { get; set; }

    public string SpreadLoanCodes { get; set; }

    public long? AgentId { get; set; }

    public string AgentName { get; set; }

    public bool MobileVerified { get; set; }

    public string MarketerName { get; set; }

    public ReliabilityModel Reliability { get; set; }

    public List<AppSmsModel> AppSmsList { get; set; } = new List<AppSmsModel>();

    public Member3Model()
    {
    }

    public Member3Model(User user)
    {
        Id = user.Id;
        UserCode = user.UserCode;
        UserName = user.UserName;
        FullName = user.FullName;
        FirstName = user.FirstName;
        LastName = user.LastName;
        NationalCode = user.NationalCode;
        Mobile = user.Mobile;
        TotalSavedMoney = user.TotalSavedMoney;
        RegDate2 = user.RegDate2;
        MobileVerified = user.MobileVerified;
        if (user.Reliability != null)
        {
            Reliability = new ReliabilityModel(user.Reliability);
        }
    }
}
