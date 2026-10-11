using System;
using System.Collections.Generic;
using System.Linq;
using LoanSystem.Core;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Models;

public class IceReportModel : BaseModel
{
	public DateTime Date { get; set; }

	public PersianDate Date_Persian => Date.ToPersianDateObject();

	public string Result { get; set; }

	public decimal Price { get; set; }

	public string Url { get; set; }

	public string AttachmentFileName { get; set; }

	public string ReportPdfFileName { get; set; }

	public string Status { get; set; }

	public string IceReportJson { get; set; }

	public string Errors { get; set; }

	public string ErrorsText
	{
		get
		{
			if (string.IsNullOrEmpty(Errors))
			{
				return "";
			}
			List<string> values = (from x in Errors.Split(',')
				select x switch
				{
					"1000" => "خطای پردازش در سرور آیس", 
					"1002" => "کاربر غیرفعال شده است", 
					"1003" => "حساب کاربری در آیس تایید نشده است", 
					"1005" => "شماره موبایل صحیح یا معتبر نمی باشد", 
					"1008" => "شارژ حساب در آیس کافی نمی باشد", 
					"1009" => "گزارش اعتبارسنجی طی 24 ساعت گذشته ثبت شده است", 
					"1010" => "درخواست اعتبارسنجی قبلا ثبت شده است", 
					"1011" => "رابط API غیر فعال است", 
					"1012" => "شماره ملی شرکت صحیح نمی باشد", 
					_ => x, 
				}).ToList();
			return string.Join(',', values);
		}
	}

	public string ReportID { get; set; }

	public Member3Model Member { get; set; }

	public Guid CustomerId { get; set; }

	public IceReportModel()
	{
	}

	public IceReportModel(IceReport iceReport)
	{
		Id = iceReport.Id;
		Date = iceReport.Date;
		Result = iceReport.Result;
		Price = iceReport.Price;
		Url = iceReport.Url;
		Status = iceReport.Status;
		ReportPdfFileName = iceReport.ReportPdfFileName;
		AttachmentFileName = iceReport.AttachmentFileName;
		IceReportJson = iceReport.IceReportJson;
		Errors = iceReport.Errors;
		ReportID = iceReport.ReportID;
		if (iceReport.Member != null)
		{
			Member = new Member3Model(iceReport.Member);
		}
	}
}
