using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using LoanSystem.Core;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Models.EFarda;
using LoanSystemAvizhe.Data.Enums;
using LoanSystemAvizhe.Services.Models.Shahin;

namespace LoanSystem.Services.Models;

public class OnlineMoneyTransferModel : BaseModel
{
	[Range(1000.0, double.MaxValue, ErrorMessage = "مبلغ نمی تواند کمتر از 1000 باشد")]
	public decimal Amount { get; set; }

	public DateTime SaveDate { get; set; }

	public PersianDate SaveDate_Persian => SaveDate.ToPersianDateObject();

	public OnlineMoneyTransferSteps OnlineMoneyTransferStep { get; set; }

	public string From { get; set; }

	public string To { get; set; }

	public string ToName { get; set; }

	public string OTP { get; set; }

	public DateTime OTPGenerateDateTime { get; set; }

	public string OurTraceNumber { get; set; }

	public string Cif { get; set; }

	public bool IsSuccess { get; set; }

	public OnlineMoneyTransferProviders Provider { get; set; }

	public ShahinTransferRequestModel Shahin { get; set; }

	public ShahinTransferResult ShahinResult { get; set; }

	public EFardaResult EFardaResult { get; set; }

	public Guid? OnlineMoneyTransferPanelId { get; set; }

	public Transaction2Model Transaction { get; set; }

	public OnlineMoneyTransferModel()
	{
	}

	public OnlineMoneyTransferModel(OnlineMoneyTransfer onlineMoneyTransfer)
	{
		Id = onlineMoneyTransfer.Id;
		Amount = onlineMoneyTransfer.Amount;
		OnlineMoneyTransferStep = onlineMoneyTransfer.OnlineMoneyTransferStep;
		SaveDate = onlineMoneyTransfer.SaveDate;
		From = onlineMoneyTransfer.From;
		To = onlineMoneyTransfer.To;
		ToName = onlineMoneyTransfer.ToName;
		OTP = onlineMoneyTransfer.OTP;
		OTPGenerateDateTime = onlineMoneyTransfer.OTPGenerateDateTime;
		OurTraceNumber = onlineMoneyTransfer.OurTraceNumber;
		Cif = onlineMoneyTransfer.Cif;
		IsSuccess = onlineMoneyTransfer.IsSuccess;
		Provider = onlineMoneyTransfer.Provider;
		if (!string.IsNullOrEmpty(onlineMoneyTransfer.EFardaJson))
		{
			EFardaResult = JsonSerializer.Deserialize<EFardaResult>(onlineMoneyTransfer.EFardaJson);
		}
		if (!string.IsNullOrEmpty(onlineMoneyTransfer.ShahinJson))
		{
			Shahin = JsonSerializer.Deserialize<ShahinTransferRequestModel>(onlineMoneyTransfer.ShahinJson);
		}
		if (!string.IsNullOrEmpty(onlineMoneyTransfer.ShahinResultJson))
		{
			ShahinResult = JsonSerializer.Deserialize<ShahinTransferResult>(onlineMoneyTransfer.ShahinResultJson);
		}
		if (onlineMoneyTransfer.Transaction != null)
		{
			Transaction = new Transaction2Model(onlineMoneyTransfer.Transaction);
		}
	}
}
