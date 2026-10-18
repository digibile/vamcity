using System;
using LoanSystem.Core;

namespace LoanSystem.Services.Models;

public class LoginRegisterModel
{
    public string Mobile { get; set; }

    public string Email { get; set; }

    public string User_FullName { get; set; }

    public string User_UserName { get; set; }

    public long User_Id { get; set; }

    public string Agent_BusinessName { get; set; }

    public DateTime Date { get; set; }

    public PersianDate PersianDate => Date.ToPersianDateObject();
}
