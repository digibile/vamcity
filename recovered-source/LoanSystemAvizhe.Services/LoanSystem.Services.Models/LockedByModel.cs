using System;
using LoanSystem.Core;

namespace LoanSystem.Services.Models;

public class LockedByModel
{
    public long UserId { get; set; }

    public string User_FullName { get; set; }

    public DateTime Date { get; set; }

    public PersianDate PersianDate => Date.ToPersianDateObject();
}
