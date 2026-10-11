using System;
using LoanSystem.Core;
using LoanSystem.Data.Entities;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Services.Models;

public class AppSmsModel : BaseModel
{
	public DateTime Date { get; set; }

	public PersianDate Date_Persian => Date.ToPersianDateObject();

	public string Text { get; set; }

	public decimal Price { get; set; }

	public AppSmsType AppSmsType { get; set; }

	public string AppSmsTypeText => AppSmsType.ToString();

	public string SmsPatternTitle { get; set; }

	public string To { get; set; }

	public SmsPanel SmsPanel { get; set; }

	public string SMSKey { get; set; }

	public string SmsPanelText => SmsPanel.ToString();

	public AppSmsStatus Status { get; set; }

	public string StatusText => Status.ToString();

	public Guid EntityId { get; set; }

	public string EntityType { get; set; }

	public string Tag { get; set; }

	public CustomerModel Customer { get; set; }

	public AppSmsModel()
	{
	}

	public AppSmsModel(AppSms appSms)
	{
		Id = appSms.Id;
		Date = appSms.Date;
		Text = appSms.Text;
		Price = appSms.Price;
		AppSmsType = appSms.AppSmsType;
		To = appSms.To;
		SmsPatternTitle = appSms.SmsPatternTitle;
		Status = appSms.Status;
		SMSKey = appSms.SMSKey;
		EntityId = appSms.EntityId;
		EntityType = appSms.EntityType;
		SmsPanel = appSms.SmsPanel;
		Tag = appSms.Tag;
		if (appSms.Customer != null)
		{
			Customer = new CustomerModel(appSms.Customer);
		}
	}
}
