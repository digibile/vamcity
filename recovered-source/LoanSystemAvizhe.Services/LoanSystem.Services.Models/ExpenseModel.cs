using System;
using LoanSystem.Core;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Models;

public class ExpenseModel : BaseModel
{
    public decimal Amount { get; set; }

    public string SavedBy { get; set; }

    public string Title { get; set; }

    public string Comment { get; set; }

    public DateTime SaveDate { get; set; }

    public PersianDate SaveDate_Persian => SaveDate.ToPersianDateObject();

    public DateTime CostDate { get; set; }

    public PersianDate CostDate_Persian => CostDate.ToPersianDateObject();

    public ExpenseModel()
    {
    }

    public ExpenseModel(Expense expense)
    {
        Id = expense.Id;
        Amount = expense.Amount;
        SavedBy = expense.SavedBy;
        Title = expense.Title;
        Comment = expense.Comment;
        SaveDate = expense.SaveDate;
        CostDate = expense.CostDate;
    }
}
