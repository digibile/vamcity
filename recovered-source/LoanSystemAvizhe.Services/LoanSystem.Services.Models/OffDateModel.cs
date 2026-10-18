using System;
using LoanSystem.Core;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Models;

public class OffDateModel : BaseModel
{
    public DateTime Date { get; set; }

    public string Title { get; set; }

    public PersianDate Date_Persian => Date.ToPersianDateObject();

    public Guid CustomerId { get; set; }

    public OffDateModel()
    {
    }

    public OffDateModel(OffDate offDate)
    {
        Id = offDate.Id;
        Date = offDate.Date;
        Title = offDate.Title;
        CustomerId = offDate.CustomerId;
    }
}
