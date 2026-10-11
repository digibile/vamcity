using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using LoanSystem.Core;
using LoanSystem.Data.Entities;
using LoanSystemAvizhe.Data.Enums;
using LoanSystemAvizhe.Services.FileHelper;
using Microsoft.AspNetCore.Http;

namespace LoanSystem.Services.Models;

public class AgentFactorTransactionModel : BaseModel
{
	[Range(0.0, double.MaxValue, ErrorMessage = "مبلغ نمی تواند کمتر از صفر باشد")]
	public decimal Amount { get; set; }

	public string FileName { get; set; }

	public string Base64 { get; set; }

	public IFormFile File { get; set; }

	public string FileName2 { get; set; }

	public string Base642 { get; set; }

	public IFormFile File2 { get; set; }

	public string FileName3 { get; set; }

	public string Base643 { get; set; }

	public IFormFile File3 { get; set; }

	public DateTime SaveDate { get; set; }

	public PersianDate SaveDate_Persian => SaveDate.ToPersianDateObject();

	public PaymentMethods PaymentMethod { get; set; }

	public bool IsUnknown { get; set; }

	public string Description { get; set; }

	public string SavedBy { get; set; }

	public Guid? SuccessfullOnlineMoneyTransferId { get; set; }

	public AgentModel Agent { get; set; }

	public List<IdCode> AgentFactors { get; set; }

	public AgentFactorTransactionModel()
	{
	}

	public AgentFactorTransactionModel(Transaction transaction)
	{
		Id = transaction.Id;
		Amount = transaction.Amount;
		SaveDate = transaction.SaveDate;
		PaymentMethod = transaction.PaymentMethod;
		IsUnknown = transaction.IsUnknown;
		Description = transaction.Description;
		SavedBy = transaction.SavedBy;
		SuccessfullOnlineMoneyTransferId = transaction.SuccessfullOnlineMoneyTransferId;
		FileName = transaction.FileName;
		FileName2 = transaction.FileName2;
		FileName3 = transaction.FileName3;
		if (transaction.Agent != null)
		{
			Agent = new AgentModel(transaction.Agent);
		}
	}

	public void FillImageBase64Prop(IFileHelper fileHelper, Guid customerId)
	{
		if (customerId == Guid.Empty)
		{
			return;
		}
		if (!string.IsNullOrEmpty(FileName))
		{
			string relativePath = Path.Combine(Directories.Transaction(customerId.ToString()), FileName);
			string text = fileHelper.ReadFileAsBase64(relativePath);
			if (!string.IsNullOrEmpty(text))
			{
				Base64 = "data:image/jpeg;base64," + text;
			}
		}
		if (!string.IsNullOrEmpty(FileName2))
		{
			string relativePath2 = Path.Combine(Directories.Transaction(customerId.ToString()), FileName2);
			string text2 = fileHelper.ReadFileAsBase64(relativePath2);
			if (!string.IsNullOrEmpty(text2))
			{
				Base642 = "data:image/jpeg;base64," + text2;
			}
		}
		if (!string.IsNullOrEmpty(FileName3))
		{
			string relativePath3 = Path.Combine(Directories.Transaction(customerId.ToString()), FileName3);
			string text3 = fileHelper.ReadFileAsBase64(relativePath3);
			if (!string.IsNullOrEmpty(text3))
			{
				Base643 = "data:image/jpeg;base64," + text3;
			}
		}
	}
}
