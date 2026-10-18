using System;
using LoanSystem.Core;

namespace LoanSystem.Services.Models;

public class RegisterOtpModel
{
    public string Email { get; set; }

    public string Mobile { get; set; }

    public string Code { get; set; }

    public DateTime Date { get; set; }

    public PersianDate PersianDate => Date.ToPersianDateObject();
}
