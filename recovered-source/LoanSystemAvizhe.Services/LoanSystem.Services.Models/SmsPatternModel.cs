using System.ComponentModel.DataAnnotations;
using LoanSystem.Data.Entities;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Services.Models;

public class SmsPatternModel : BaseModel
{
    [Required(ErrorMessage = "عنوان الگو الزامی است.")]
    public string Title { get; set; }

    [Required(ErrorMessage = "قالب الگو الزامی است.")]
    public string Pattern { get; set; }

    public bool IsSystem { get; set; }

    public SmsPatternStatus Status { get; set; }

    public SmsPatternCategory Category { get; set; }

    public SmsPanel SmsPatternPanel { get; set; }

    public bool IsPatternBase { get; set; }

    public string IppanelKeysJson { get; set; }

    public string PatternBaseCode { get; set; }

    public string RejectReason { get; set; }

    public CustomerModel Customer { get; set; }

    public SmsPatternModel()
    {
    }

    public SmsPatternModel(SmsPattern smsPattern)
    {
        Id = smsPattern.Id;
        Title = smsPattern.Title;
        Pattern = smsPattern.Pattern;
        IsSystem = smsPattern.IsSystem;
        Status = smsPattern.Status;
        Category = smsPattern.Category;
        RejectReason = smsPattern.RejectReason;
        SmsPatternPanel = smsPattern.SmsPatternPanel;
        IsPatternBase = smsPattern.IsPatternBase;
        PatternBaseCode = smsPattern.PatternBaseCode;
        IppanelKeysJson = smsPattern.IppanelKeysJson;
        if (smsPattern.Customer != null)
        {
            Customer = new CustomerModel(smsPattern.Customer);
        }
    }
}
