using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using LoanSystem.Core;
using LoanSystem.Data.Entities;
using LoanSystemAvizhe.Data.Enums;
using LoanSystemAvizhe.Services.FileHelper;
using Microsoft.AspNetCore.Http;

namespace LoanSystem.Services.Models;

public class SubAgentTransactionModel : BaseModel
{
	[Range(0.0, double.MaxValue, ErrorMessage = "مبلغ نمی تواند کمتر از صفر باشد")]
	public decimal Amount { get; set; }

	public string FileName { get; set; }

	public string Base64 { get; set; }

	public IFormFile File { get; set; }

	public DateTime SaveDate { get; set; }

	public PersianDate SaveDate_Persian => SaveDate.ToPersianDateObject();

	public PaymentMethods PaymentMethod { get; set; }

	public bool IsWithdraw { get; set; }

	public string Description { get; set; }

	public string SavedBy { get; set; }

	public UserLoanModel UserLoan { get; set; }

	public SubAgentTransactionModel()
	{
	}

	public SubAgentTransactionModel(SubAgentTransaction transaction)
	{
		Id = transaction.Id;
		Amount = transaction.Amount;
		SaveDate = transaction.SaveDate;
		PaymentMethod = transaction.PaymentMethod;
		IsWithdraw = transaction.IsWithdraw;
		Description = transaction.Description;
		SavedBy = transaction.SavedBy;
		FileName = transaction.FileName;
		if (transaction.UserLoan != null)
		{
			UserLoan = new UserLoanModel(transaction.UserLoan);
		}
	}

	public void FillImageBase64Prop(IFileHelper fileHelper, Guid customerId)
	{
		if (!string.IsNullOrEmpty(FileName) && !(customerId == Guid.Empty))
		{
			string relativePath = Path.Combine(Directories.SubAgentTransaction(customerId.ToString()), FileName);
			string text = fileHelper.ReadFileAsBase64(relativePath);
			if (!string.IsNullOrEmpty(text))
			{
				Base64 = "data:image/jpeg;base64," + text;
			}
		}
	}
}
