using System;

namespace LoanSystemAvizhe.Services.Models.Shahin;

public class ShahinTransferValidationResult
{
	public string transactionState { get; set; }

	public long transactionTime { get; set; }

	public Guid uuid { get; set; }

	public ShahinTransferValidationRespObject respObject { get; set; } = new ShahinTransferValidationRespObject();
}
