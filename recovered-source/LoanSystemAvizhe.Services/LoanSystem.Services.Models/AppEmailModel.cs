using System;
using LoanSystem.Core;
using LoanSystem.Data.Entities;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Services.Models;

public class AppEmailModel : BaseModel
{
    public DateTime Date { get; set; }

    public PersianDate Date_Persian => Date.ToPersianDateObject();

    public string Text { get; set; }

    public AppSmsType AppEmailType { get; set; }

    public string AppEmailTypeText => AppEmailType.ToString();

    public string PatternTitle { get; set; }

    public string To { get; set; }

    public EmailPanel EmailPanel { get; set; }

    public string EmailPanelText => EmailPanel.ToString();

    public Guid EntityId { get; set; }

    public string EntityType { get; set; }

    public string Tag { get; set; }

    public CustomerModel Customer { get; set; }

    public AppEmailModel()
    {
    }

    public AppEmailModel(AppEmail appEmail)
    {
        Id = appEmail.Id;
        Date = appEmail.Date;
        Text = appEmail.Text;
        AppEmailType = appEmail.AppEmailType;
        To = appEmail.To;
        PatternTitle = appEmail.PatternTitle;
        EntityId = appEmail.EntityId;
        EntityType = appEmail.EntityType;
        EmailPanel = appEmail.EmailPanel;
        Tag = appEmail.Tag;
        if (appEmail.Customer != null)
        {
            Customer = new CustomerModel(appEmail.Customer);
        }
    }
}
