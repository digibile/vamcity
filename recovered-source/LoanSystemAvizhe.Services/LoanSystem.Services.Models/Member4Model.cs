using System;
using LoanSystem.Core;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Models;

public class Member4Model
{
    public long Id { get; set; }

    public string UserName { get; set; }

    public string FullName { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string Mobile { get; set; }

    public string NationalCode { get; set; }

    public decimal Credit { get; set; }

    public bool WalletIsLock { get; set; }

    public bool MobileVerified { get; set; }

    public DateTime RegDate2 { get; set; }

    public PersianDate RegDate2_Persian => RegDate2.ToPersianDateObject();

    public Member4Model()
    {
    }

    public Member4Model(User user)
    {
        Id = user.Id;
        UserName = user.UserName;
        FullName = user.FullName;
        FirstName = user.FirstName;
        LastName = user.LastName;
        NationalCode = user.NationalCode;
        Mobile = user.Mobile;
        MobileVerified = user.MobileVerified;
        RegDate2 = user.RegDate2;
        Credit = user.Credit;
        WalletIsLock = user.WalletIsLock;
    }
}
