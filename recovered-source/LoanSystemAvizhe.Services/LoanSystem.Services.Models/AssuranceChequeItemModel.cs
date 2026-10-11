using System;
using System.IO;
using LoanSystem.Core;
using LoanSystem.Data.Entities;
using LoanSystemAvizhe.Services.FileHelper;
using Microsoft.AspNetCore.Http;

namespace LoanSystem.Services.Models;

public class AssuranceChequeItemModel : BaseModel
{
	public bool IsReturned { get; set; }

	public bool IsCollected { get; set; }

	public bool IsChanged { get; set; }

	public IFormFile File { get; set; }

	public string FileName { get; set; }

	public string Base64 { get; set; }

	public DateTime DueDate { get; set; }

	public PersianDate DueDate_Persian => DueDate.ToPersianDateObject();

	public string ChequeNumber { get; set; }

	public string SayadiId { get; set; }

	public decimal Price { get; set; }

	public Guid AssuranceId { get; set; }

	public AssuranceChequeItemModel()
	{
	}

	public AssuranceChequeItemModel(AssuranceChequeItem assuranceChequeItem)
	{
		Id = assuranceChequeItem.Id;
		DueDate = assuranceChequeItem.DueDate;
		ChequeNumber = assuranceChequeItem.ChequeNumber;
		SayadiId = assuranceChequeItem.SayadiId;
		Price = assuranceChequeItem.Price;
		AssuranceId = assuranceChequeItem.AssuranceId;
		FileName = assuranceChequeItem.FileName;
		IsCollected = assuranceChequeItem.IsCollected;
		IsReturned = assuranceChequeItem.IsReturned;
	}

	public void FillImageBase64Prop(IFileHelper fileHelper, Guid customerId)
	{
		if (!string.IsNullOrEmpty(FileName) && !(customerId == Guid.Empty))
		{
			string relativePath = Path.Combine(Directories.AssuranceCheque(customerId.ToString()), FileName);
			string text = fileHelper.ReadFileAsBase64(relativePath);
			if (!string.IsNullOrEmpty(text))
			{
				Base64 = "data:image/jpeg;base64," + text;
			}
		}
	}
}
